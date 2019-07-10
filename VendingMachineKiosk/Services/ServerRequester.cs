using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Diagnostics;
using Windows.Security.Cryptography.Certificates;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Newtonsoft.Json;
using VendingMachineKiosk.Exceptions;
using XiaoTianQuanProtocols.DataObjects;
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
            //_client.DefaultRequestHeaders.Accept.Add(new Windows.Web.Http.Headers.HttpMediaTypeWithQualityHeaderValue("application/json"));
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
            var productResponse = await ExceptionWrapperAsync<ListProductsResponse, object>(_ =>
                _client.GetAsync(new Uri(_endpoint, Endpoints.GetProductList)));

            return productResponse.Products;
        }

        public async Task<CreateTransactionResponse> CreateTransaction(CreateTransactionRequest createTransactionRequest)
        {
            var transactionResponse = await ExceptionWrapperAsync<CreateTransactionResponse, CreateTransactionRequest>(
                req =>
                {
                    string jsonStr = JsonConvert.SerializeObject(req);
                    return _client.PostAsync(new Uri(_endpoint, Endpoints.CreateTransaction),
                        new HttpStringContent(jsonStr, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json"));
                }, createTransactionRequest);
            return transactionResponse;
        }

        public async Task UnlockVendingMachine()
        {
            await ExceptionWrapperAsync<object, object>(
                _ => _client.DeleteAsync(new Uri(_endpoint, Endpoints.LockVendingMachine)),
                null, _ => Task.FromResult(new object()));
        }

        public async Task<Guid> LockVendingMachine()
        {
            var lockToken = await ExceptionWrapperAsync<Guid, object>(
                _ => _client.PostAsync(new Uri(_endpoint, Endpoints.LockVendingMachine), null), null, async msg =>
                {
                    var str = await msg.Content.ReadAsStringAsync();
                    var ok = Guid.TryParse(str, out var token);
                    if (!ok)
                    {
                        throw new ServerInvalidResponseException($"Invalid lock token {str}");
                    }
                    return token;
                });

            return lockToken;
        }

        private async Task<TResponse> ExceptionWrapperAsync<TResponse, TRequest>(
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
                var result = await action(request);

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
            catch (Exception e)
            {
                throw new ServerRequestException(e.Message, e);
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
                            $"Server returned OK but status is {resp.Status.ToString()}");
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(TResponse), "Template is not a ResponseBase");
                }

            }
            catch (JsonReaderException e)
            {
                throw new ServerInvalidResponseException(e.Message, e);
            }
        }
    }
}
