using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using XiaoTianQuanProtocols.DataObjects;
using XiaoTianQuanProtocols.VendingMachineRequests;
using XiaoTianQuanServer.Authorizations;
using XiaoTianQuanServer.Data;
using XiaoTianQuanServer.Extensions;
using XiaoTianQuanServer.Services;

namespace XiaoTianQuanServer.Controllers
{
    [Route("api/vendingmachine")]
    [ApiController]
    public class VendingMachineController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrencyExchangeService _currencyExchangeService;
        private readonly IMachineConfigurationService _machineConfiguration;
        private readonly IVendingJobQueue _vendingJobQueue;
        private readonly IVendingMachineDataService _vmService;

        public VendingMachineController(ApplicationDbContext context,
            ICurrencyExchangeService currencyExchangeService,
            IMachineConfigurationService machineConfiguration,
            IVendingJobQueue vendingJobQueue,
            IVendingMachineDataService vmService)
        {
            _context = context;
            _currencyExchangeService = currencyExchangeService;
            _machineConfiguration = machineConfiguration;
            _vendingJobQueue = vendingJobQueue;
            _vmService = vmService;
        }

        [HttpPost]
        [Route("lock")]
        [Authorize(Policy = Policies.VendingMachine)]
        public async Task<ActionResult<Guid>> LockVendingMachine()
        {
            var machineId = this.GetMachineId();

            // Enqueue auto unlock
            var tl = await _machineConfiguration.GetMachineLockTimeoutAsync(machineId);
            await _vendingJobQueue.EnqueueVendingMachineUnlockMessageAsync(machineId, tl);

            // Is locked and lock the machine
            var lockToken = await _vmService.TryLockVendingMachineAsync(machineId);
            if (lockToken == Guid.Empty)
            {
                // Lock the machine fails, remove the unlock from queue
                await _vendingJobQueue.RemoveVendingMachineUnlockMessageAsync(machineId);
                return Conflict();
            }

            return lockToken;
        }

        [HttpDelete]
        [Route("lock")]
        [Authorize(Policy = Policies.VendingMachine)]
        public async Task<IActionResult> UnlockVendingMachine()
        {
            var machineId = this.GetMachineId();

            // Is locked and lock the machine
            var unlocked = await _vmService.UnlockVendingMachineAsync(machineId);
            if (unlocked && await _vendingJobQueue.RemoveVendingMachineUnlockMessageAsync(machineId))
            {
                return Ok();
            }
            else
                return StatusCode(503);
        }

        [HttpGet]
        [Route("products")]
        [Authorize(Policy = Policies.VendingMachine)]
        public async Task<ActionResult<ListProductsResponse>> GetProducts()
        {
            var machineId = this.GetMachineId();

            var inventories = await _context.Inventories.Include(i => i.VendingMachine)
                .Where(i => i.VendingMachine.MachineId == machineId).OrderBy(i => i.Slot).ToListAsync();
            var products = inventories.Select(i => new ProductInformation
            {
                Description = i.ProductDescription,
                Image = i.Picture == null ? null : new Uri(i.Picture),
                Name = i.ProductName,
                Quantity = i.Quantity,
                Slot = i.Slot,
                Prices = new Dictionary<XiaoTianQuanProtocols.PaymentType, double>
                {
                    { XiaoTianQuanProtocols.PaymentType.LightningNetwork, _currencyExchangeService.ConvertToSatoshi(i.BasePrice) }
                }
            }).ToList();

            var response = new ListProductsResponse
            {
                Products = products,
                Status = products.Count == 0 ? ResponseStatus.VendingMachineNotFound : ResponseStatus.Ok
            };

            return response;
        }
    }
}
