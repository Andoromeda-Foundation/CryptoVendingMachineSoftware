using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using XiaoTianQuanServer.Settings;

namespace XiaoTianQuanServer.Services.Implementations
{
    public class MachineConfigurationService : IMachineConfigurationService
    {
        private readonly DefaultConfiguration _configuration;

        public MachineConfigurationService(IOptions<DefaultConfiguration> options)
        {
            _configuration = options.Value;
        }

        public Task<int> GetMachineLockTimeoutAsync(Guid machineId)
        {
            return Task.FromResult(_configuration.MachineLockTimeout);
        }

        public Task<int> GetPaymentTimeoutAsync(Guid machineId)
        {
            return Task.FromResult(_configuration.PaymentTimeout);
        }

        public Task<int> GetDisplayTimeoutAsync(Guid machineId)
        {
            return Task.FromResult(_configuration.DisplayTimeout);
        }

        public Task<int> GetReleaseProductTimeoutAsync(Guid machineId)
        {
            return Task.FromResult(_configuration.ReleaseProductTimeout);
        }
    }
}
