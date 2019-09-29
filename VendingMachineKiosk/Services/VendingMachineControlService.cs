using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;
using Windows.Foundation.Diagnostics;
using Windows.UI.Core;
using GalaSoft.MvvmLight.Messaging;
using VendingMachineKiosk.Exceptions;
using XiaoTianQuanProtocols.Extensions;

namespace VendingMachineKiosk.Services
{
    public class VendingMachineControlService : IVendingMachineControlService
    {
        private readonly LoggingChannel _logger;
        private readonly ServerRequester _serverRequester;
        private readonly VendingMachineHardwareService _vendingMachine;

        public VendingMachineControlService(LoggingChannel logger, ServerRequester serverRequester, VendingMachineHardwareService vendingMachine)
        {
            _logger = logger;
            _serverRequester = serverRequester;
            _vendingMachine = vendingMachine;
            serverRequester.ReleaseProduct += ServerRequester_ReleaseProduct;
        }

        public event Action<MachineHardwareStatus> MachineStatusChanged;

        private MachineHardwareStatus _machineHardwareStatus;
        public MachineHardwareStatus MachineHardwareStatus
        {
            get => _machineHardwareStatus;
            private set
            {
                _machineHardwareStatus = value;
                MachineStatusChanged?.Invoke(value);
            }
        }

        private async Task<bool> ReleaseItemAsync(string slot)
        {
            if (!uint.TryParse(slot, out var slotId))
            {
                MachineHardwareStatus = MachineHardwareStatus.InvalidSlot;
                return false;
            }

            if (slotId > 0x8 * 0xF)
            {
                MachineHardwareStatus = MachineHardwareStatus.InvalidSlot;
                return false;
            }

            return await _vendingMachine.ReleaseItemAsync(slotId);
        }

        private async void ServerRequester_ReleaseProduct(string slot, Guid transactionId)
        {
            lock (_pendingTransactions)
            {
                if (_pendingTransactions.Contains(transactionId))
                {
                    _pendingTransactions.Remove(transactionId);
                }
                else
                {
                    _logger.LogMessage($"Release instruction received but transaction id {transactionId} is invalid");
                }
            }

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                () => Messenger.Default.Send(ViewModels.Messages.ProductReleasing));

            if (await ReleaseItemAsync(slot))
            {
                // TODO: resend when fail
                try
                {
                    await _serverRequester.TransactionCompleteAsync(transactionId);
                }
                catch (VendingMachineKioskException e)
                {
                    _logger.LogMessage(e.GetInnerMessages(), LoggingLevel.Error);
                }
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                    () => Messenger.Default.Send(ViewModels.Messages.ProductReleased));
            }
            else
            {
                _pendingTransactions.Add(transactionId);
            }
        }

        public void AddPendingTransaction(Guid transactionId)
        {
            lock (_pendingTransactions)
            {
                _pendingTransactions.Add(transactionId);
            }
        }

        public void RemovePendingTransaction(Guid transactionId)
        {
            lock (_pendingTransactions)
            {
                _pendingTransactions.Remove(transactionId);
            }
        }

        private readonly HashSet<Guid> _pendingTransactions = new HashSet<Guid>();
    }
}
