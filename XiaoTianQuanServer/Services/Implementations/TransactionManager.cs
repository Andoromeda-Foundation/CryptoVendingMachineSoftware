using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XiaoTianQuanProtocols;
using XiaoTianQuanServer.Data;
using XiaoTianQuanServer.DataModels;

namespace XiaoTianQuanServer.Services.Implementations
{
    public class TransactionManager : ITransactionManager
    {
        private readonly ILogger<TransactionManager> _logger;
        private readonly ApplicationDbContext _context;

        public TransactionManager(ILogger<TransactionManager> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<Transaction> CreateTransactionAsync(int inventoryId, int timeout)
        {
            var inventory = await _context.Inventories.Include(i => i.VendingMachine)
                .SingleOrDefaultAsync(i => i.Id == inventoryId);

            if (inventory == null)
            {
                _logger.LogError($"Invalid inventory id {inventoryId}");
                return null;
            }

            var transaction = new Transaction
            {
                Inventory = inventory,
                BasePrice = inventory.BasePrice,
                Settled = false,
                Active = true,
                TransactionCreated = DateTime.UtcNow,
                TransactionExpiry = DateTime.UtcNow.AddSeconds(timeout),
            };

            _context.Add(transaction);
            await _context.SaveChangesAsync();

            return transaction;
        }

        public Task<Transaction> GetTransactionAsync(Guid transactionId)
        {
            return _context.Transactions.Include(t => t.Inventory).ThenInclude(i => i.VendingMachine)
                .SingleOrDefaultAsync(t => t.Id == transactionId);
        }
    }
}
