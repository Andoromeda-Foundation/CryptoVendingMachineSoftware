using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using XiaoTianQuanProtocols.HubProxies;
using XiaoTianQuanServer.Authorizations;
using XiaoTianQuanServer.Extensions;

namespace XiaoTianQuanServer.Hubs
{
    [Authorize(Policy = Policies.VendingMachine)]
    public class VendingMachine : Hub<IVendingMachineProxy>
    {
        private readonly ILogger<VendingMachine> _logger;

        public VendingMachine(ILogger<VendingMachine> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            var machineId = this.GetMachineId();
            _logger.LogInformation($"machine {machineId} connected to hub");
            await Groups.AddToGroupAsync(Context.ConnectionId, machineId.ToString());
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
            var machineId = this.GetMachineId();
            _logger.LogInformation($"machine {machineId} disconnected to hub");
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, machineId.ToString());
        }
    }
}
