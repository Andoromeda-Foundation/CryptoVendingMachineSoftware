using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using XiaoTianQuanProtocols.HubProxies;
using XiaoTianQuanServer.Hubs;

namespace XiaoTianQuanServer.Services.Impl
{
    public class VendingMachineControlService : IVendingMachineControlService
    {
        private readonly IHubContext<VendingMachine, IVendingMachineProxy> _hubContext;

        public VendingMachineControlService(IHubContext<VendingMachine, IVendingMachineProxy> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task SendReleaseProductMessageAsync(Guid machine, string slot, Guid transactionId)
        {
            return _hubContext.Clients.Group(machine.ToString()).ReleaseProduct(slot, transactionId);
        }

    }
}
