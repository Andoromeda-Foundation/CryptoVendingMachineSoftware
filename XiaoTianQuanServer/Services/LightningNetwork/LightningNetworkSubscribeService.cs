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
using XiaoTianQuanProtocols.Extensions;
using XiaoTianQuanServer.Services.LightningNetwork.Models;
using XiaoTianQuanServer.Settings;

namespace XiaoTianQuanServer.Services.LightningNetwork
{
    public class LightningNetworkSubscribeService : LightningNetworkServiceBase, IHostedService
    {
        private readonly ITransactionSettlementService _transactionSettlementService;
        private readonly HttpClient _httpSubscribeClient;
        private readonly Thread _subscribeInvoiceThread;

        public LightningNetworkSubscribeService(IOptions<LndSettings> settings,
            ILogger<LightningNetworkSubscribeService> logger,
            ITransactionSettlementService transactionSettlementService)
            : base(settings, logger)
        {
            _transactionSettlementService = transactionSettlementService;
            _httpSubscribeClient = new HttpClient(HttpClientHandler)
            {
                BaseAddress = settings.Value.RestfulEndpoint
            };

            var macaroon = Convert.FromBase64String(settings.Value.Macaroon);
            var macString = BitConverter.ToString(macaroon).Replace("-", string.Empty);
            _httpSubscribeClient.DefaultRequestHeaders.Add("Grpc-Metadata-macaroon", macString);
            _httpSubscribeClient.Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite);

            _subscribeInvoiceThread = new Thread(SubscribeHandler);
        }

        private async void SubscribeHandler()
        {
            try
            {
                while (true)
                {
                    await SubscribeInvoiceAsync();
                }
            }
            catch (Exception e)
            {
                if (e is StackOverflowException || e is OutOfMemoryException)
                    throw;
                _logger.LogError(e.Message);
                _logger.LogError(e.StackTrace);
            }
        }

        private async Task SubscribeInvoiceAsync()
        {
            try
            {
                await using var stream =
                    await _httpSubscribeClient.GetStreamAsync(LightningNetworkEndpoints.SubscribeInvoices);

                using var reader = new StreamReader(stream);
                while (!reader.EndOfStream)
                {
                    var json = await reader.ReadLineAsync();
                    try
                    {
                        var invoice = JsonConvert.DeserializeObject<InvoiceStream>(json, JsonSerializerSettings);
                        if ((invoice?.Result?.Settled ?? false) && invoice.Result?.RHash != null)
                        {
                            await _transactionSettlementService.AddLightningNetworkTransactionAsync(invoice.Result
                                .RHash);
                        }
                    }
                    catch (JsonReaderException)
                    {
                        _logger.LogError($"invalid invoice subscription streaming json: {json}");
                    }
                }
            }
            catch (HttpRequestException e)
            {
                _logger.LogError($"invoice subscription request failed {e.GetInnerMessages()}");
                await Task.Delay(5000); // wait for 5 seconds
            }

            await Task.Delay(1000);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (!_subscribeInvoiceThread.IsAlive)
            {
                _subscribeInvoiceThread.Start();
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
