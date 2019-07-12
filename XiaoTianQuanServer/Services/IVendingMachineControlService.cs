using System;
using System.Threading.Tasks;

namespace XiaoTianQuanServer.Services
{
    public interface IVendingMachineControlService
    {
        Task SendReleaseProductMessageAsync(Guid machine, string slot, Guid transactionId);
    }
}