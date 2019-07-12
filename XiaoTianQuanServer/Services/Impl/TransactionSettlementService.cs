using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XiaoTianQuanServer.Data;
using XiaoTianQuanServer.DataModels;
using XiaoTianQuanServer.Services.LightningNetwork;

namespace XiaoTianQuanServer.Services.Impl
{
    public class TransactionSettlementService : ITransactionSettlementService
    {
        private readonly ILogger<TransactionSettlementService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly LightningNetworkRequestService _lndRequestService;
        private readonly HashSet<string> _pendingLightningNetworkTransactions = new HashSet<string>();  // key is payment hash

        private Thread _lndPaymentProcessingThread;

        public TransactionSettlementService(ILogger<TransactionSettlementService> logger,
            IServiceProvider serviceProvider,
            LightningNetworkRequestService lndRequestService)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _lndRequestService = lndRequestService;
            _lndPaymentProcessingThread = new Thread(ProcessPendingLightningNetworkTransactionsLoop);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (!_lndPaymentProcessingThread.IsAlive)
            {
                _lndPaymentProcessingThread.Start();
            }
            else
            {
                _logger.LogError("lnd payment settling service already started");
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task AddLightningNetworkTransactionAsync(string paymentHash)
        {
            lock (_pendingLightningNetworkTransactions)
            {
                _pendingLightningNetworkTransactions.Add(paymentHash);
            }

            return Task.CompletedTask;
        }

        private async void ProcessPendingLightningNetworkTransactionsLoop()
        {
            while (true)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
                    var tm = scope.ServiceProvider.GetService<ITransactionManager>();

                    await ProcessPendingLightningNetworkTransactions(context, tm);
                }

                await Task.Delay(1000);
            }
        }

        private async Task ProcessPendingLightningNetworkTransactions(ApplicationDbContext context, ITransactionManager transactionManager)
        {
            var pendingTransactions = await context.LightningNetworkTransactions.Include(t => t.Transaction)
                .Where(t => t.Transaction.TransactionExpiry.AddSeconds(10) >= DateTime.UtcNow && t.Settled == false)
                .Select(t => t.PaymentHash)
                .ToListAsync();

            lock (_pendingLightningNetworkTransactions)
            {
                pendingTransactions.AddRange(_pendingLightningNetworkTransactions);
                _pendingLightningNetworkTransactions.Clear();
            }
            var hashset = new HashSet<string>(pendingTransactions);

            foreach (var paymentHash in hashset)
            {
                var hashArray = Convert.FromBase64String(paymentHash);
                try
                {
                    var result = await _lndRequestService.QueryInvoiceAsync(hashArray);
                    if (result == null)
                    {
                        _logger.LogError($"lnd payment hash {paymentHash} was not found");
                        continue;
                    }

                    if (result.Settled)
                    {
                        await transactionManager.SettleLightningNetworkTransactionAsync(paymentHash);
                    }
                }
                catch (FormatException)
                {
                    _logger.LogError($"invalid payment hash base64 received: {paymentHash}");
                }
            }
        }
    }
}
