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
    }
}