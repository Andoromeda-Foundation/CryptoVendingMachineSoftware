using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VendingMachineKiosk.Services
{
    public enum MachineHardwareStatus
    {
        Ok,
    }

    public interface IVendingMachineControlService
    {
        void AddPendingTransaction(Guid transactionId);
        void RemovePendingTransaction(Guid transactionId);

        Task<bool> ReleaseItemAsync(string slot);

        event Action<MachineHardwareStatus> MachineStatusChanged;

        MachineHardwareStatus MachineHardwareStatus { get; }
    }
}