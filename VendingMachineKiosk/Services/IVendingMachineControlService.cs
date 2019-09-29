using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VendingMachineKiosk.Services
{
    public enum MachineHardwareStatus
    {
        Ok,
        InvalidSlot,
        ReleaseFailed,
    }

    public interface IVendingMachineControlService
    {
        void AddPendingTransaction(Guid transactionId);
        void RemovePendingTransaction(Guid transactionId);

        event Action<MachineHardwareStatus> MachineStatusChanged;

        MachineHardwareStatus MachineHardwareStatus { get; }
    }
}