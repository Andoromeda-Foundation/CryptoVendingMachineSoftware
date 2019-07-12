using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Diagnostics;
using Windows.Security.Cryptography.Certificates;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using VendingMachineKiosk.Exceptions;
using XiaoTianQuanProtocols.DataObjects;
using XiaoTianQuanProtocols.HubProxies;
using XiaoTianQuanProtocols.VendingMachineRequests;
using HttpClient = Windows.Web.Http.HttpClient;
using HttpResponseMessage = Windows.Web.Http.HttpResponseMessage;

namespace VendingMachineKiosk.Services
{
    public class ServerRequester
    {
        private readonly LoggingChannel _logger;
        private readonly HttpClient _client;
        private readonly Uri _endpoint = new Uri(Config.RequestEndpoint);
        private readonly HubConnection _hubConnection;

        public ServerRequester(LoggingChannel logger)
        {
            _logger = logger;

            CertificateQuery certQuery = new CertificateQuery
            {
                IssuerName = Config.CertificateIssuer
            };
            // This is the friendly name of the certificate that was just installed.
            IReadOnlyList<Certificate> certs = CertificateStores.FindAllAsync(certQuery).AsTask().Result;
            var clientCert = certs.FirstOrDefault();

            HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter();
            filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.Untrusted);
            filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.InvalidName);

            if (clientCert == null)
            {
                logger.LogMessage("Cannot find certificate", LoggingLevel.Error);
                throw new ConfigException("Cannot find certificate");
            }
            else
            {
                filter.ClientCertificate = clientCert;
            }

            _client = new HttpClient(filter);

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(GetUri(Endpoints.VendingMachineHub), opt =>
                    {
                        opt.ClientCertificates = new X509CertificateCollection { GetX509Certificate() };


                        // BUG: remove this in production
                        opt.HttpMessageHandlerFactory = handler =>
                        {
                            if (handler is HttpClientHandler clientHandler)
                            {
                                clientHandler.ServerCertificateCustomValidationCallback =
                                    (_, __, ___, ____) => true;
                            }
                            return handler;
                        };
                    })
                .WithAutomaticReconnect()
                .Build();

            SetUpHubHandlers();

            Task.Run(StartAsync);
        }

        public async Task StartAsync()
        {
            bool ok;
            do
            {
                try
                {
                    await _hubConnection.StartAsync();
                    ok = true;
                }
                catch (Exception e)
                {
                    _logger.LogMessage(e.Message, LoggingLevel.Error);
                    ok = false;
                }

                await Task.Delay(10000);    // wait for 10s then reconnect
            } while (!ok);
        }

        private X509Certificate2 GetX509Certificate()
        {
            foreach (StoreLocation loc in Enum.GetValues(typeof(StoreLocation)))
            {
                X509Store store = new X509Store(StoreName.My, loc);
                store.Open(OpenFlags.ReadOnly);
                foreach (var storeCertificate in store.Certificates)
                {
                    var issuerName = storeCertificate.GetNameInfo(X509NameType.SimpleName, true);
                    if (issuerName == Config.CertificateIssuer)
                        return storeCertificate;
                }
                store.Close();
            }
            return null;
        }

        private void SetUpHubHandlers()
        {
            _hubConnection.On<string, Guid>(nameof(IVendingMachineProxy.ReleaseProduct),
                (slot, trans) => ReleaseProduct?.Invoke(slot, trans));
        }

        public event Action<string, Guid> ReleaseProduct;

        private Uri GetUri(string endpoint)
        {
            return new Uri(_endpoint, endpoint);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ServerNonSuccessResponseException">When server returned HTTP error or OK but internal error code</exception>
        /// <exception cref="ServerRequestException">When failed to request server</exception>
        /// <exception cref="ServerInvalidResponseException">When server responsed invalid data</exception>
        public async Task<IList<ProductInformation>> GetProductList()
        {
            var productResponse = await RequestWrapperAsync<ListProductsResponse, object>(_ =>
                _client.GetAsync(new Uri(_endpoint, Endpoints.GetProductList)));

            return productResponse.Products;
        }

        public async Task<CreateTransactionResponse> CreateTransaction(CreateTransactionRequest createTransactionRequest)
        {
            var transactionResponse = await RequestWrapperAsync<CreateTransactionResponse, CreateTransactionRequest>(
                req =>
                {
                    string jsonStr = JsonConvert.SerializeObject(req);
                    return _client.PostAsync(GetUri(Endpoints.CreateTransaction),
                        new HttpStringContent(jsonStr, UnicodeEncoding.Utf8, "application/json"));
                }, createTransactionRequest);
            return transactionResponse;
        }

        public async Task<GetPaymentInstructionResponse> GetPaymentInstruction(GetPaymentInstructionRequest request)
        {
            var pir = await RequestWrapperAsync<GetPaymentInstructionResponse, GetPaymentInstructionRequest>(
                req =>
                {
                    string jsonStr = JsonConvert.SerializeObject(req);
                    return _client.PostAsync(GetUri(Endpoints.GetPaymentInstruction),
                        new HttpStringContent(jsonStr, UnicodeEncoding.Utf8, "application/json"));
                }, request);

            return pir;
        }


        private async Task<TResponse> RequestWrapperAsync<TResponse, TRequest>(
            Func<TRequest, IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress>> action,
            TRequest request = null, Func<HttpResponseMessage, Task<TResponse>> responseParser = null)
            where TRequest : class
        {
            if (responseParser == null)
            {
                responseParser = ParseJsonResponse<TResponse>;
            }

            try
            {
                using (var result = await action(request))
                {
                    if (result.IsSuccessStatusCode)
                    {
                        return await responseParser(result);
                    }
                    else
                    {
                        throw new ServerNonSuccessResponseException(
                            $"Server returned {result.StatusCode}: {result.ReasonPhrase}");
                    }
                }
            }
            catch (Exception e)
            {
                throw new ServerRequestException("Server request error", e);
            }
        }


        private async Task<TResponse> RequestWrapperAsync<TResponse>(
            Func<IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress>> action,
            Func<HttpResponseMessage, Task<TResponse>> responseParser = null)
        {
            if (responseParser == null)
            {
                responseParser = ParseJsonResponse<TResponse>;
            }

            try
            {
                using (var result = await action())
                {
                    if (result.IsSuccessStatusCode)
                    {
                        return await responseParser(result);
                    }
                    else
                    {
                        throw new ServerNonSuccessResponseException(
                            $"Server returned {result.StatusCode}: {result.ReasonPhrase}");
                    }
                }
            }
            catch (Exception e)
            {
                throw new ServerRequestException("Server request error", e);
            }
        }

        private async Task<TResponse> ParseJsonResponse<TResponse>(HttpResponseMessage result)
        {
            try
            {
                var contents = await result.Content.ReadAsStringAsync();

                var responseObject = JsonConvert.DeserializeObject<TResponse>(contents);

                if (responseObject is ResponseBase resp)
                {
                    if (resp.Status == ResponseStatus.Ok)
                        return responseObject;
                    else
                        throw new ServerNonSuccessResponseException(
                            $"Server responsed with {resp.Status.ToString()}");
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(TResponse), "Template is not a ResponseBase");
                }

            }
            catch (JsonReaderException e)
            {
                throw new ServerInvalidResponseException("Server returned invalid json", e);
            }
        }

        public async Task<bool> TransactionCompleteAsync(Guid transactionId)
        {
            var request = new TransactionCompleteRequest { TransactionId = transactionId };
            var response = await RequestWrapperAsync<TransactionCompleteResponse>(() => _client.PostAsync(GetUri(Endpoints.TransactionComplete),
                new HttpStringContent(JsonConvert.SerializeObject(request),
                    UnicodeEncoding.Utf8,
                    "application/json")));
            return response.Status == ResponseStatus.Ok;
        }
    }
}
