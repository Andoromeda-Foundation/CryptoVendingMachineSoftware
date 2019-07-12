using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using XiaoTianQuanProtocols.Extensions;
using XiaoTianQuanServer.Extensions;
using XiaoTianQuanServer.Services.LightningNetwork.Models;
using XiaoTianQuanServer.Settings;

namespace XiaoTianQuanServer.Services.LightningNetwork
{
    public class LightningNetworkRequestService : LightningNetworkServiceBase
    {

        public LightningNetworkRequestService(IOptions<LndSettings> settings,
            ILogger<LightningNetworkRequestService> logger) : base(settings, logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memo"></param>
        /// <param name="value"></param>
        /// <param name="expiry">Expiry in seconds</param>
        /// <returns></returns>
        public async Task<AddInvoiceResponse> AddInvoiceAsync(string memo, int value, int expiry)
        {
            var addInvoiceRequest = new AddInvoiceRequest
            {
                Memo = memo,
                Expiry = expiry.ToString(),
                Value = value.ToString(),
            };


            var result = await RequestWrapperAsync<AddInvoiceResponse>(() =>
                HttpClient.PostJsonAsync(LightningNetworkEndpoints.Invoices, addInvoiceRequest,
                    JsonSerializerSettings));

            return result;
        }

        public async Task<QueryInvoiceResponse> QueryInvoiceAsync(byte[] rHash)
        {
            var result = await RequestWrapperAsync<QueryInvoiceResponse>(() =>
                HttpClient.GetAsync(
                    $"{LightningNetworkEndpoints.Invoice}/{BitConverter.ToString(rHash).Replace("-", "")}"));

            return result;
        }

    }
}
