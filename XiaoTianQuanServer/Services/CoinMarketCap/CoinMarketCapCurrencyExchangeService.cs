using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using XiaoTianQuanProtocols;
using XiaoTianQuanProtocols.Extensions;

namespace XiaoTianQuanServer.Services.CoinMarketCap
{
    public class CoinMarketCapCurrencyExchangeService : ICurrencyExchangeService, IHostedService, IDisposable
    {
        private readonly ILogger<CoinMarketCapCurrencyExchangeService> _logger;
        private Timer _timer;

        private const int BtcId = 1;

        private const string EndpointQuotesLatest = "/v1/cryptocurrency/quotes/latest";

        private readonly HttpClient _httpClient = new HttpClient();

        private readonly Dictionary<PaymentType, double> _exchangeRates = new Dictionary<PaymentType, double>();

        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };

        public CoinMarketCapCurrencyExchangeService(ILogger<CoinMarketCapCurrencyExchangeService> logger, IOptions<Settings.CoinMarketCapSettings> settings)
        {
            _logger = logger;
            _httpClient.BaseAddress = settings.Value.RestfulEndpoint;
            _httpClient.DefaultRequestHeaders.Add("X-CMC_PRO_API_KEY", settings.Value.ApiKey);
        }

        private async void ExecuteRequestTask(object state)
        {
            try
            {
                await RequestExchangeRatesAsync();
            }
            catch (OperationCanceledException e)
            {
                _logger.LogError(e.GetInnerMessages());
            }
        }

        private async Task RequestExchangeRatesAsync()
        {
            var idList = new List<int>();
            var paymentTypes = Enum.GetValues(typeof(PaymentType));
            foreach (PaymentType paymentType in paymentTypes)
            {
                switch (paymentType)
                {
                    case PaymentType.LightningNetwork:
                        idList.Add(BtcId);  // BTC
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["id"] = string.Join(',', idList);
            queryString["convert"] = "CNY";

            try
            {

                var query = queryString.ToString();
                var response = await _httpClient.GetAsync($"{EndpointQuotesLatest}?{query}");

                if (!response.IsSuccessStatusCode)
                    return;

                var result = await response.Content.ReadAsStringAsync();
                var quoteResponse = JsonConvert.DeserializeObject<QuoteResponse>(result, _jsonSerializerSettings);

                if (quoteResponse.Status.ErrorCode != 0)
                    return;

                foreach ((var dummy, Currency currency) in quoteResponse.Data)
                {
                    if (currency.Quote.Count != 1)
                        continue;

                    var exchange = currency.Quote.First().Value.Price;

                    switch (currency.Id)
                    {
                        case BtcId:
                            _exchangeRates[PaymentType.LightningNetwork] = exchange / 1_000_000.00;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException($"response.data.obj.id");
                    }
                }
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e.GetInnerMessages());
            }
            catch (JsonReaderException e)
            {
                _logger.LogError(e.GetInnerMessages());
            }
        }

        public int ConvertToSatoshi(int baseUnit)
        {
            return (int)(baseUnit / _exchangeRates[PaymentType.LightningNetwork]);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(CoinMarketCapCurrencyExchangeService)} started to pull");
            _timer = new Timer(ExecuteRequestTask, null, TimeSpan.Zero, TimeSpan.FromSeconds(60));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(CoinMarketCapCurrencyExchangeService)} stopped to pull");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
