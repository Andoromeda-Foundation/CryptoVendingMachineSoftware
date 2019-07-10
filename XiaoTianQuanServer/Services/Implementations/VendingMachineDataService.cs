using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using XiaoTianQuanServer.Data;
using XiaoTianQuanServer.DataModels;

namespace XiaoTianQuanServer.Services.Implementations
{
    public class VendingMachineDataService : IVendingMachineDataService
    {
        private readonly ApplicationDbContext _context;

        public VendingMachineDataService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IList<string>> GetVendingMachineSlotsAsync(Guid machineId)
        {
            var list = await _context.Inventories.Include(i => i.VendingMachine)
                .Where(i => i.VendingMachine.MachineId == machineId).Select(i => i.Slot).OrderBy(slot => slot)
                .ToListAsync();
            return list;
        }

        public async Task<Inventory> GetVendingMachineSlotInfoAsync(Guid machineId, string slot)
        {
            var inventory = await _context.Inventories.Include(i => i.VendingMachine)
                .SingleAsync(i => i.VendingMachine.MachineId == machineId && i.Slot == slot);

            return inventory;
        }

        public async Task<bool> IncreaseVendingMachineSlotInventoryQuantityAsync(Guid machineId, string slot, int amount, int retry = 3)
        {
            do
            {
                var inventory = await _context.Inventories.Include(i => i.VendingMachine)
                    .SingleAsync(i => i.VendingMachine.MachineId == machineId && i.Slot == slot);

                inventory.Quantity += amount;
                _context.Entry(inventory).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                    return true;
                }
                catch (DbUpdateConcurrencyException)
                {
                    --retry;
                }
            } while (retry > 0);

            return false;
        }

        public async Task<bool> DecreaseVendingMachineSlotInventoryQuantityAsync(Guid machineId, string slot, int amount, int retry = 3)
        {
            do
            {
                var inventory = await _context.Inventories.Include(i => i.VendingMachine)
                    .SingleAsync(i => i.VendingMachine.MachineId == machineId && i.Slot == slot);

                if (amount > inventory.Quantity)
                    return false;

                inventory.Quantity -= amount;
                _context.Entry(inventory).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                    return true;
                }
                catch (DbUpdateConcurrencyException)
                {
                    --retry;
                }
            } while (retry > 0);

            return false;
        }

    }
}
