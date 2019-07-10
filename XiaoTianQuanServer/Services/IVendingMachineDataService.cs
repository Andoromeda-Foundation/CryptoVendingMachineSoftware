using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XiaoTianQuanServer.DataModels;

namespace XiaoTianQuanServer.Services
{
    public interface IVendingMachineDataService
    {
        Task<IList<string>> GetVendingMachineSlotsAsync(Guid machineId);

        Task<Inventory> GetVendingMachineSlotInfoAsync(Guid machineId, string slot);

        Task<bool> IncreaseVendingMachineSlotInventoryQuantityAsync(Guid machineId, string slot, int amount, int retry = 3);
        Task<bool> DecreaseVendingMachineSlotInventoryQuantityAsync(Guid machineId, string slot, int amount, int retry = 3);
    }
}