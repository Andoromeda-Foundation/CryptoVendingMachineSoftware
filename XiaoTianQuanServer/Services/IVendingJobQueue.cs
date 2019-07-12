using System;
using System.Threading.Tasks;

namespace XiaoTianQuanServer.Services
{
    public interface IVendingJobQueue
    {
        Task EnqueuePaymentExpiryMessageAsync(Guid transactionId, int delay);
        Task<bool> RemovePaymentExpiryMessageAsync(Guid transactionId);

        Task EnqueueProductUnfulfilledRefundMessageAsync(Guid transactionId, int inventoryId, int timeoutRelease);
        Task<bool> RemoveProductUnfulfilledRefundMessageAsync(Guid transactionId);
    }
}