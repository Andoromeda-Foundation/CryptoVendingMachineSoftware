using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
    public abstract class LightningNetworkServiceBase
    {
        protected static HttpClientHandler HttpClientHandler { get; private set; }
        protected static HttpClient HttpClient { get; private set; }

        protected readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };

        protected ILogger _logger;

        protected LightningNetworkServiceBase(IOptions<LndSettings> settings, ILogger logger)
        {
            _logger = logger;

            if (HttpClient == null)
            {
                HttpClientHandler = new HttpClientHandler();

                var lndSettings = settings.Value;

                if (!lndSettings.CheckCertificate)
                {
                    HttpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                }

                HttpClient = new HttpClient(HttpClientHandler)
                {
                    BaseAddress = lndSettings.RestfulEndpoint
                };

                var macaroon = Convert.FromBase64String(lndSettings.Macaroon);
                var macString = BitConverter.ToString(macaroon).Replace("-", string.Empty);
                HttpClient.DefaultRequestHeaders.Add("Grpc-Metadata-macaroon", macString);
            }
        }

        protected async Task<TResponse> RequestWrapperAsync<TResponse>(
            Func<Task<HttpResponseMessage>> requestAction,
            Func<HttpResponseMessage, Task<TResponse>> responseParser = null)
            where TResponse : class
        {
            if (responseParser == null)
            {
                responseParser = ParseJsonResponse<TResponse>;
            }

            try
            {
                using var result = await requestAction();

                if (result.IsSuccessStatusCode)
                {
                    return await responseParser(result);
                }
                else
                {
                    _logger.LogError($"lnd request failed, status {result.StatusCode}");
                    return null;
                }
            }
            catch (HttpRequestException e)
            {
                _logger.LogError($"failed to request lnd: {e.GetInnerMessages()}");
                return null;
            }
        }

        protected async Task<TResponse> ParseJsonResponse<TResponse>(HttpResponseMessage result) where TResponse : class
        {
            var contents = await result.Content.ReadAsStringAsync();
            try
            {
                var responseObject = JsonConvert.DeserializeObject<TResponse>(contents, JsonSerializerSettings);

                return responseObject;
            }
            catch (JsonReaderException)
            {
                _logger.LogError($"lnd server returned invalid json: {contents}");
                return null;
            }
        }
    }
}
