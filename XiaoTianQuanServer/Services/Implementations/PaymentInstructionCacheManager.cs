using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XiaoTianQuanServer.Data;
using XiaoTianQuanServer.Services.LightningNetwork;

namespace XiaoTianQuanServer.Services.Implementations
{
    public class PaymentInstructionCacheManager : IPaymentInstructionCacheManager
    {
        private readonly ILogger<PaymentInstructionCacheManager> _logger;
        private readonly LightningNetworkService _lightningNetworkService;
        private readonly ApplicationDbContext _context;

        public PaymentInstructionCacheManager(ILogger<PaymentInstructionCacheManager> logger,
            LightningNetworkService lightningNetworkService, ApplicationDbContext context)
        {
            _logger = logger;
            _lightningNetworkService = lightningNetworkService;
            _context = context;
        }

        public async Task<DataModels.LightningNetworkTransaction> RetrieveOrCreateLightningNetwork(Guid transactionId, string memo, int amount)
        {
            var transaction = await _context.Transactions.FindAsync(transactionId);

            var lndTransaction =
                _context.LightningNetworkTransactions.Include(t => t.Transaction)
                    .SingleOrDefault(t => t.Transaction.Id == transactionId);

            if (lndTransaction == null)
            {
                var expiry = (int)(transaction.TransactionExpiry - DateTime.UtcNow).TotalSeconds;
                if (expiry < 0)
                    return null;

                var lndResponse = await _lightningNetworkService.AddInvoiceAsync(memo, amount, expiry);
                if (lndResponse == null)
                    return null;

                lndTransaction = new DataModels.LightningNetworkTransaction
                {
                    Amount = amount,
                    PaymentRequest = lndResponse,
                    Transaction = transaction
                };

                _context.Add(lndTransaction);
                await _context.SaveChangesAsync();
            }

            return lndTransaction;
        }
    }
}