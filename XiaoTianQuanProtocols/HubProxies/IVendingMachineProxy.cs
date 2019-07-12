using System;
using System.Threading.Tasks;

namespace XiaoTianQuanProtocols.HubProxies
{
    public interface IVendingMachineProxy
    {
        Task ReleaseProduct(string slot, Guid transactionId);
    }
}
