using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
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

        public VendingMachineControlService(LoggingChannel logger, ServerRequester serverRequester)
        {
            _logger = logger;
            _serverRequester = serverRequester;
            serverRequester.ReleaseProduct += ServerRequester_ReleaseProduct;
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

        public async Task<bool> ReleaseItemAsync(string slot)
        {
            // TODO: release item
            await Task.Delay(4000);
            return true;
        }

        public event Action<MachineHardwareStatus> MachineStatusChanged;
        public MachineHardwareStatus MachineHardwareStatus { get; private set; }

        private readonly HashSet<Guid> _pendingTransactions = new HashSet<Guid>();
    }
}
