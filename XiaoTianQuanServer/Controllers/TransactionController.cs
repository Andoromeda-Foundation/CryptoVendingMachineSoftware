using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        private readonly LightningNetworkService _lightningNetworkService;
        private readonly IVendingMachineDataService _vmService;
        private readonly ICurrencyExchangeService _exchangeService;
        private readonly IMachineConfigurationService _machineConfiguration;
        private readonly IVendingJobQueue _vendingJobQueue;
        private readonly ITransactionManager _transactionManager;

        public TransactionController(LightningNetworkService lightningNetworkService,
            IVendingMachineDataService vmService, ICurrencyExchangeService exchangeService,
            IMachineConfigurationService machineConfiguration, IVendingJobQueue vendingJobQueue,
            ITransactionManager transactionManager)
        {
            _lightningNetworkService = lightningNetworkService;
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

            var lockValid = await _vmService.CheckVendingMachineLockTokenAsync(machineId, request.LockToken);
            if (!lockValid)
            {
                response.Status = ResponseStatus.InvalidLockToken;
                return Ok(response);
            }

            // Have inventory?
            var inventory = await _vmService.GetVendingMachineSlotInfoAsync(machineId, request.Slot);
            if (inventory.Quantity <= 0)
            {
                response.Status = ResponseStatus.OutOfOrder;
                return Ok(response);
            }

            var basePrice = inventory.BasePrice;

            var tp = await _machineConfiguration.GetPaymentTimeoutAsync(machineId);

            // Generate payment instruction
            switch (request.PaymentType)
            {
                case PaymentType.LightningNetwork:
                    var tId = await _transactionManager.CreateTransactionAsync(machineId, PaymentType.LightningNetwork);

                    var satoshi = _exchangeService.ConvertToSatoshi(basePrice);
                    var result =
                        await _lightningNetworkService.AddInvoiceAsync(
                            $"{machineId} sell product in {request.Slot} for {satoshi}, transaction {tId} on {DateTime.UtcNow}", satoshi,
                            await _machineConfiguration.GetPaymentTimeoutAsync(machineId));

                    if (result == null)
                    {
                        response.Status = ResponseStatus.LndGenerateInvoiceFailed;
                        return Ok(response);
                    }

                    // Enqueue payment expiry
                    await _vendingJobQueue.EnqueuePaymentExpiryMessageAsync(tId, tp);

                    response.PaymentCode = result.PaymentRequest;
                    response.PaymentDisplayTimeout = await _machineConfiguration.GetDisplayTimeoutAsync(machineId);
                    response.TransactionId = tId;
                    return Ok(response);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}