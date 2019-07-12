using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XiaoTianQuanProtocols;
using XiaoTianQuanProtocols.VendingMachineRequests;
using XiaoTianQuanServer.Authorizations;
using XiaoTianQuanServer.Extensions;
using XiaoTianQuanServer.Services;
using XiaoTianQuanServer.Services.LightningNetwork;

namespace XiaoTianQuanServer.Controllers
{
    [Route("api/transaction")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly ILogger<TransactionController> _logger;
        private readonly IVendingMachineDataService _vmService;
        private readonly ICurrencyExchangeService _exchangeService;
        private readonly IMachineConfigurationService _machineConfiguration;
        private readonly IVendingJobQueue _vendingJobQueue;
        private readonly ITransactionManager _transactionManager;

        public TransactionController(ILogger<TransactionController> logger,
            IVendingMachineDataService vmService, ICurrencyExchangeService exchangeService,
            IMachineConfigurationService machineConfiguration, IVendingJobQueue vendingJobQueue,
            ITransactionManager transactionManager)
        {
            _logger = logger;
            _vmService = vmService;
            _exchangeService = exchangeService;
            _machineConfiguration = machineConfiguration;
            _vendingJobQueue = vendingJobQueue;
            _transactionManager = transactionManager;
        }

        [HttpPost]
        [Route("create")]
        [Authorize(Policy = Policies.VendingMachine)]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var machineId = this.GetMachineId();


            var response = new CreateTransactionResponse
            {
                Status = ResponseStatus.Ok
            };

            // Have inventory?
            var inventory = await _vmService.GetVendingMachineSlotInfoAsync(machineId, request.Slot);
            if (inventory.Quantity <= 0)
            {
                response.Status = ResponseStatus.OutOfOrder;
                _logger.LogWarning(
                    $"machine {machineId} requested transaction with slot {request.Slot} but product is out of stock");
                return Ok(response);
            }


            var tp = await _machineConfiguration.GetPaymentTimeoutAsync(machineId);
            var td = await _machineConfiguration.GetDisplayTimeoutAsync(machineId);

            var transaction = await _transactionManager.CreateTransactionAsync(inventory.Id, td);
            if (transaction == null)
            {
                _logger.LogError($"Transaction manager failed to create transaction");
                response.Status = ResponseStatus.TransactionGenerationFailed;
                return Ok(response);
            }

            // Enqueue payment expiry
            await _vendingJobQueue.EnqueuePaymentExpiryMessageAsync(transaction.Id, tp);

            response.TransactionId = transaction.Id;
            response.PaymentDisplayTimeout = transaction.TransactionExpiry;
            return Ok(response);
        }

        [HttpPost]
        [Route("getpaymentinstruction")]
        [Authorize(Policy = Policies.VendingMachine)]
        public async Task<ActionResult> GetPaymentInstruction([FromBody]GetPaymentInstructionRequest request)
        {
            var machineId = this.GetMachineId();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = new GetPaymentInstructionResponse();
            DataModels.Transaction transaction = await _transactionManager.GetTransactionAsync(request.TransactionId);

            if (transaction == null || transaction.Inventory.VendingMachine.MachineId != machineId)
            {
                response.Status = ResponseStatus.TransactionNotFound;
                return Ok(response);
            }

            var basePrice = transaction.BasePrice;
            var slot = transaction.Inventory.Slot;
            
            // Generate payment instruction
            switch (request.PaymentType)
            {
                case PaymentType.LightningNetwork:
                    var satoshiIfNotCreated = _exchangeService.ConvertToSatoshi(basePrice);
                    var memo = $"{transaction.Id}, sell product in {slot} for {satoshiIfNotCreated} on {DateTime.UtcNow}";
                    var result = await _transactionManager.GetLightningNetworkPaymentInstruction(transaction.Id, memo, satoshiIfNotCreated);

                    if (result == null)
                    {
                        response.Status = ResponseStatus.LndGenerateInvoiceFailed;
                        _logger.LogError(
                            $"machine {machineId} requested transaction with slot {slot} lnd service failed");
                        return Ok(response);
                    }

                    response.PaymentCode = result.PaymentRequest;
                    response.Amount = result.Amount;
                    return Ok(response);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [HttpPost]
        [Route("transactioncomplete")]
        [Authorize(Policy = Policies.VendingMachine)]
        public async Task<IActionResult> TransactionComplete([FromBody]TransactionCompleteRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var machineId = this.GetMachineId();
            var response = new TransactionCompleteResponse
            {
                Status = ResponseStatus.Ok
            };

            var completed = await _transactionManager.CompleteTransactionAsync(request.TransactionId, machineId);
            if (!completed)
            {
                response.Status = ResponseStatus.TransactionCompleteFailed;
            }
            return Ok(response);
        }
    }
}
