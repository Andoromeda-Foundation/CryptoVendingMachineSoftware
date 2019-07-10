using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<VendingMachineController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly ICurrencyExchangeService _currencyExchangeService;
        private readonly IMachineConfigurationService _machineConfiguration;
        private readonly IVendingJobQueue _vendingJobQueue;
        private readonly IVendingMachineDataService _vmService;

        public VendingMachineController(ILogger<VendingMachineController> logger,
            ApplicationDbContext context,
            ICurrencyExchangeService currencyExchangeService,
            IMachineConfigurationService machineConfiguration,
            IVendingJobQueue vendingJobQueue,
            IVendingMachineDataService vmService)
        {
            _logger = logger;
            _context = context;
            _currencyExchangeService = currencyExchangeService;
            _machineConfiguration = machineConfiguration;
            _vendingJobQueue = vendingJobQueue;
            _vmService = vmService;
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
