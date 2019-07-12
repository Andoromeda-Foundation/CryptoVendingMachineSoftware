using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XiaoTianQuanProtocols;
using XiaoTianQuanProtocols.VendingMachineRequests;
using XiaoTianQuanServer.Data;
using XiaoTianQuanServer.DataModels;
using XiaoTianQuanServer.Services.LightningNetwork;

namespace XiaoTianQuanServer.Services.Impl
{
    public class TransactionManager : ITransactionManager
    {
        private readonly ILogger<TransactionManager> _logger;
        private readonly ApplicationDbContext _context;
        private readonly LightningNetworkRequestService _lightningNetworkRequestService;
        private readonly IVendingJobQueue _vendingJobQueue;
        private readonly IMachineConfigurationService _configurationService;
        private readonly IVendingMachineControlService _vendingMachineControlService;
        private readonly IVendingMachineDataService _vendingMachineDataService;

        public TransactionManager(ILogger<TransactionManager> logger, ApplicationDbContext context,
            LightningNetworkRequestService lightningNetworkRequestService, IVendingJobQueue vendingJobQueue,
            IMachineConfigurationService configurationService, IVendingMachineControlService vendingMachineControlService,
            IVendingMachineDataService vendingMachineDataService)
        {
            _logger = logger;
            _context = context;
            _lightningNetworkRequestService = lightningNetworkRequestService;
            _vendingJobQueue = vendingJobQueue;
            _configurationService = configurationService;
            _vendingMachineControlService = vendingMachineControlService;
            _vendingMachineDataService = vendingMachineDataService;
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
                Fulfilled = false,
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

        public async Task<Guid> GetLightningNetworkTransactionId(string paymentHash)
        {
            var transaction =
                await _context.LightningNetworkTransactions.SingleOrDefaultAsync(t => t.PaymentHash == paymentHash);

            return transaction?.TransactionId ?? Guid.Empty;
        }

        public async Task<bool> SettleLightningNetworkTransactionAsync(string paymentHash)
        {
            var lndTransaction =
                await _context.LightningNetworkTransactions.Include(t => t.Transaction)
                    .ThenInclude(t => t.Inventory)
                    .ThenInclude(i => i.VendingMachine)
                    .SingleOrDefaultAsync(t => t.PaymentHash == paymentHash);

            if (lndTransaction == null)
            {
                _logger.LogError($"settle lightning network transaction {paymentHash} but transaction was not found");
                return false;
            }

            if (lndTransaction.Settled || lndTransaction.Transaction.Settled)
            {
                _logger.LogInformation($"lightning network transaction {paymentHash} was already settled");
                return true;
            }

            lndTransaction.Settled = true;
            lndTransaction.Transaction.Settled = true;
            _context.Entry(lndTransaction).State = EntityState.Modified;

            try
            {

                var machineId = lndTransaction.Transaction.Inventory.VendingMachine.MachineId;
                var slot = lndTransaction.Transaction.Inventory.Slot;
                var transactionId = lndTransaction.TransactionId;
                var inventoryId = lndTransaction.Transaction.Inventory.Id;

                var timeoutRelease = await _configurationService.GetReleaseProductTimeoutAsync(machineId);
                await _vendingJobQueue.EnqueueProductUnfulfilledRefundMessageAsync(transactionId, inventoryId, timeoutRelease);
                await _context.SaveChangesAsync();

                var inventoryUpdated = await _vendingMachineDataService.DecreaseVendingMachineSlotInventoryQuantityAsync(machineId, slot, 1);
                if (!inventoryUpdated)
                {
                    _logger.LogError(
                        $"failed to update inventory for settlement, machine {machineId}, transaction {transactionId}");
                    return false;
                }
                await _vendingMachineControlService.SendReleaseProductMessageAsync(machineId, slot, transactionId);

                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogInformation($"lightning network transaction {paymentHash} settle concurrency");
                return false;
            }
        }


        public async Task<bool> CompleteTransactionAsync(Guid transactionId, Guid machineId)
        {
            // 1. Check if transaction is valid
            var transaction = await _context.Transactions.Include(t => t.Inventory)
                .ThenInclude(i => i.VendingMachineId)
                .SingleOrDefaultAsync(t => t.Id == transactionId && t.Inventory.VendingMachine.MachineId == machineId);

            if (transaction == null)
            {
                _logger.LogError($"transaction {transactionId} not found for machine {machineId}");
                return false;
            }

            // Already fulfilled
            if (transaction.Fulfilled)
            {
                _logger.LogInformation($"transaction {transaction} is already fulfilled");
                return true;
            }

            var removed = await _vendingJobQueue.RemoveProductUnfulfilledRefundMessageAsync(transactionId);
            if (!removed)
            {
                _logger.LogError($"failed to remove product from unfulfilled queue, transaction {transactionId}");
                return false;
            }

            transaction.Fulfilled = true;
            transaction.TransactionFulfilled = DateTime.UtcNow;
            _context.Entry(transaction).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<LightningNetworkTransaction> GetLightningNetworkPaymentInstruction(Guid transactionId, string memo, int amount)
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

                var lndResponse = await _lightningNetworkRequestService.AddInvoiceAsync(memo, amount, expiry);
                if (lndResponse == null)
                    return null;

                lndTransaction = new DataModels.LightningNetworkTransaction
                {
                    Amount = amount,
                    PaymentRequest = lndResponse.PaymentRequest,
                    Transaction = transaction,
                    PaymentHash = lndResponse.RHash,
                    TransactionId = transaction.Id
                };

                _context.Add(lndTransaction);
                await _context.SaveChangesAsync();
            }

            return lndTransaction;
        }
    }
}
