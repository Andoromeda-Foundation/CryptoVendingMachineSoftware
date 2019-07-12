using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace XiaoTianQuanServer.Services.Impl
{
    public class DummyRefundService : IRefundService
    {
        private readonly ILogger<DummyRefundService> _logger;

        public DummyRefundService(ILogger<DummyRefundService> logger)
        {
            _logger = logger;
        }

        public Task AppendToRefundQueueAsync(Guid transactionId)
        {
            _logger.LogWarning($"received refund message for {transactionId} but I'm not goin' to refund");
            return Task.CompletedTask;
        }
    }
}
