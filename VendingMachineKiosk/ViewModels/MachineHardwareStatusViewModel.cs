using GalaSoft.MvvmLight;
using VendingMachineKiosk.Services;

namespace VendingMachineKiosk.ViewModels
{
    public class MachineHardwareStatusViewModel : ViewModelBase
    {
        private readonly IVendingMachineControlService _vendingMachineControlService;

        public MachineHardwareStatusViewModel(IVendingMachineControlService vendingMachineControlService)
        {
            _vendingMachineControlService = vendingMachineControlService;
            vendingMachineControlService.MachineStatusChanged += VendingMachineControlService_MachineStatusChanged;
        }

        private void VendingMachineControlService_MachineStatusChanged(MachineHardwareStatus _)
        {
            RaisePropertyChanged(nameof(MachineHardwareStatus));
        }

        public MachineHardwareStatus MachineHardwareStatus => _vendingMachineControlService.MachineHardwareStatus;
    }
}