using System;
using System.Threading.Tasks;

namespace XiaoTianQuanServer.Services
{
    public interface IMachineConfigurationService
    {
        Task<int> GetMachineLockTimeoutAsync(Guid machineId);
        Task<int> GetPaymentTimeoutAsync(Guid machineId);
        Task<int> GetDisplayTimeoutAsync(Guid machineId);
        Task<int> GetReleaseProductTimeoutAsync(Guid machineId);
    }
}