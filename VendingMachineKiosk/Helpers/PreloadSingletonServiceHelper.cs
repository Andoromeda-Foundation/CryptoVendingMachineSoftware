using VendingMachineKiosk.Services;

namespace VendingMachineKiosk.Helpers
{
    public class PreloadSingletonServiceHelper
    {
        public ServerRequester Requester { get; }
        public IVendingMachineControlService MachineControlService { get; }

        public PreloadSingletonServiceHelper(ServerRequester serverRequester, IVendingMachineControlService vendingMachineControlService)
        {
            Requester = serverRequester;
            MachineControlService = vendingMachineControlService;
        }
    }
}
