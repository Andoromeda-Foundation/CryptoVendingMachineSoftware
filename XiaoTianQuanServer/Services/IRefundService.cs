using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XiaoTianQuanServer.Services
{
    public interface IRefundService
    {
        Task AppendToRefundQueueAsync(Guid transactionId);
    }
}
