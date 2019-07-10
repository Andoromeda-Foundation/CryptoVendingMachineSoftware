using System;
using System.Threading.Tasks;
using XiaoTianQuanProtocols;

namespace XiaoTianQuanServer.Services
{
    public interface ITransactionManager
    {
        Task<Guid> CreateTransactionAsync(Guid machineId, PaymentType paymentType);
    }
}