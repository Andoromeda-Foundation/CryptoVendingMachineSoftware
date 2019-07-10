using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using XiaoTianQuanProtocols;
using XiaoTianQuanServer.Data;

namespace XiaoTianQuanServer.Services.Implementations
{
    public class TransactionManager : ITransactionManager
    {
        private readonly ApplicationDbContext _context;

        public TransactionManager(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> CreateTransactionAsync(Guid machineId, PaymentType paymentType)
        {
            var machine = await _context.VendingMachines.SingleAsync(vm => vm.MachineId == machineId);

            var transaction = new DataModels.Transaction
            {
                VendingMachine = machine,
                PaymentType = paymentType,
                TransactionCreated = DateTime.UtcNow
            };

            _context.Add(transaction);
            await _context.SaveChangesAsync();

            return transaction.Id;
        }
    }
}
