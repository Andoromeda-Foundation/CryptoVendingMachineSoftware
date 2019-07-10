using System;
using System.Threading.Tasks;

namespace XiaoTianQuanServer.Services
{
    public interface IPaymentInstructionCacheManager
    {
        Task<DataModels.LightningNetworkTransaction> RetrieveOrCreateLightningNetwork(Guid transactionId, string memo, int amount);
    }
}