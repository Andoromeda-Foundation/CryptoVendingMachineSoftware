using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendingMachineKiosk.Services
{
    public class VendingLifecycleManager
    {
        private readonly ServerRequester _requester;

        public Guid LockToken { get; set; }

        public VendingLifecycleManager(ServerRequester requester)
        {
            _requester = requester;
        }

        public async Task LockVendingMachineAsync()
        {
            LockToken = await _requester.LockVendingMachine();
        }

        public async Task UnlockVendingMachineAsync()
        {
            await _requester.UnlockVendingMachine();

            LockToken = Guid.Empty;
        }
    }
}
