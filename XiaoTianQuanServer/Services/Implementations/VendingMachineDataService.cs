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

        /// <summary>
        /// Atomic operation, checks whether already locked
        /// </summary>
        /// <param name="machineId"></param>
        /// <param name="retry"></param>
        /// <returns></returns>
        public async Task<Guid> TryLockVendingMachineAsync(Guid machineId, int retry)
        {
            do
            {
                var machine = await _context.VendingMachines.SingleAsync(vm => vm.MachineId == machineId);

                if (machine.ExclusiveUseLock != Guid.Empty)   // Lock was acquired by others
                    return Guid.Empty;

                machine.ExclusiveUseLock = Guid.NewGuid();
                _context.Entry(machine).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                    return machine.ExclusiveUseLock;
                }
                catch (DbUpdateConcurrencyException)
                {
                    --retry;
                }

            } while (retry > 0);

            return Guid.Empty; // Conflict
        }

        /// <summary>
        /// Does not check locked
        /// </summary>
        /// <param name="machineId"></param>
        /// <param name="retry"></param>
        /// <returns>true if locked, false on retry</returns>
        public async Task<Guid> LockVendingMachineAsync(Guid machineId, int retry)
        {
            do
            {
                var machine = await _context.VendingMachines.SingleAsync(vm => vm.MachineId == machineId);

                machine.ExclusiveUseLock = Guid.NewGuid();
                _context.Entry(machine).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                    return machine.ExclusiveUseLock;
                }
                catch (DbUpdateConcurrencyException)
                {
                    --retry;
                }
            } while (retry > 0);

            return Guid.Empty;
        }

        public async Task<bool> UnlockVendingMachineAsync(Guid machineId, int retry)
        {
            do
            {
                var machine = await _context.VendingMachines.SingleAsync(vm => vm.MachineId == machineId);

                machine.ExclusiveUseLock = Guid.Empty;
                _context.Entry(machine).State = EntityState.Modified;

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

        public async Task<bool> CheckVendingMachineLockTokenAsync(Guid machineId, Guid lockToken)
        {
            var machine = await _context.VendingMachines.SingleAsync(v => v.MachineId == machineId);
            return machine.ExclusiveUseLock == lockToken;
        }
    }
}
