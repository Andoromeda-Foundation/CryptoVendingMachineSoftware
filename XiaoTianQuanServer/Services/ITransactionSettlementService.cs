using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace XiaoTianQuanServer.Services
{
    public interface ITransactionSettlementService : IHostedService
    {
        Task AddLightningNetworkTransactionAsync(string paymentHash);
    }
}