using System;
using System.Threading.Tasks;

namespace XiaoTianQuanServer.Services
{
    public interface IVendingJobQueue
    {
        Task EnqueueVendingMachineUnlockMessageAsync(Guid machineId, int delay);
        Task<bool> RemoveVendingMachineUnlockMessageAsync(Guid machineId);

        Task EnqueuePaymentExpiryMessageAsync(Guid transactionId, int delay);
        Task<bool> RemovePaymentExpiryMessageAsync(Guid transactionId);

        Task EnqueueProductUnreleasedRefundMessageAsync(Guid transactionId, int delay);
        Task<bool> RemoveProductUnreleasedRefundMessageAsync(Guid transactionId);
    }
}