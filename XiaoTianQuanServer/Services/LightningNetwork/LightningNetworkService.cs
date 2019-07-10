using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using XiaoTianQuanServer.Services.LightningNetwork.Models;
using XiaoTianQuanServer.Settings;

namespace XiaoTianQuanServer.Services.LightningNetwork
{
    public class LightningNetworkService
    {
        private readonly HttpClient _httpClient;

        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };

        public LightningNetworkService(IOptions<LndSettings> settings)
        {
            var handler = new HttpClientHandler();

            var lndSettings = settings.Value;

            if (!lndSettings.CheckCertificate)
            {
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            }

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = lndSettings.RestfulEndpoint
            };

            var macaroon = Convert.FromBase64String(lndSettings.Macaroon);
            var macString = BitConverter.ToString(macaroon).Replace("-", string.Empty);
            _httpClient.DefaultRequestHeaders.Add("Grpc-Metadata-macaroon", macString);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memo"></param>
        /// <param name="value"></param>
        /// <param name="expiry">Expiry in seconds</param>
        /// <returns></returns>
        public async Task<string> AddInvoiceAsync(string memo, int value, int expiry)
        {
            var invoiceRequest = new AddInvoiceRequest
            {
                Memo = memo,
                Expiry = expiry.ToString(),
                Value = value.ToString(),
            };

            using var result = await _httpClient.PostAsync(LightningNetworkEndpoints.Invoices,
                new StringContent(JsonConvert.SerializeObject(invoiceRequest, Formatting.None,
                    _jsonSerializerSettings)));

            if (!result.IsSuccessStatusCode)
                return null;

            var response = await result.Content.ReadAsStringAsync();

            try
            {
                var inv = JsonConvert.DeserializeObject<AddInvoiceResponse>(response, _jsonSerializerSettings);
                return inv.PaymentRequest;
            }
            catch (JsonReaderException)
            {
                return null;
            }
        }
    }
}
