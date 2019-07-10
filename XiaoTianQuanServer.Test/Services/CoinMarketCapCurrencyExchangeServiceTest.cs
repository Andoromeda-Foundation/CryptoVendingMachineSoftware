using System;
using System.Threading.Tasks;
using XiaoTianQuanServer.Services.CoinMarketCap;
using Xunit;

namespace XiaoTianQuanServer.Test.Services
{
    public class CoinMarketCapCurrencyExchangeServiceTest : IAsyncLifetime
    {
        private readonly CoinMarketCapCurrencyExchangeService _service;
        public CoinMarketCapCurrencyExchangeServiceTest()
        {
            _service = new CoinMarketCapCurrencyExchangeService(new Logger<CoinMarketCapCurrencyExchangeService>(),
                new Option<Settings.CoinMarketCapSettings>(new Settings.CoinMarketCapSettings
                {
                    ApiKey = "fa4803dc-7036-4917-8ae2-2cf8a6e2cf6f",
                    RestfulEndpoint = new System.Uri("https://pro-api.coinmarketcap.com")
                }));
        }

        [Fact]
        public void TestSatoshi()
        {
            var satoshi = _service.ConvertToSatoshi(100);
            Assert.True(satoshi > 0);
        }

        public async Task InitializeAsync()
        {
            await _service.StartAsync(new System.Threading.CancellationToken());
            await Task.Delay(TimeSpan.FromSeconds(5));
        }

        public Task DisposeAsync()
        {
            return _service.StopAsync(new System.Threading.CancellationToken());
        }
    }
}
