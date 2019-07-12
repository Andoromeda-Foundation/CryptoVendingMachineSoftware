using System;
using System.Threading.Tasks;
using XiaoTianQuanProtocols;
using XiaoTianQuanServer.DataModels;

namespace XiaoTianQuanServer.Services
{
    public interface ITransactionManager
    {
        Task<Transaction> CreateTransactionAsync(int inventoryId, int timeout);
        Task<Transaction> GetTransactionAsync(Guid transactionId);

        Task<LightningNetworkTransaction> GetLightningNetworkPaymentInstruction(Guid transactionId, string memo, int amount);
        Task<Guid> GetLightningNetworkTransactionId(string paymentHash);
        Task<bool> SettleLightningNetworkTransactionAsync(string paymentHash);

        Task<bool> CompleteTransactionAsync(Guid transactionId, Guid machineId);
    }
}