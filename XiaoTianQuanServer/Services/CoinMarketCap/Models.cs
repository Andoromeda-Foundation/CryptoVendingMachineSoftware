using System.Collections.Generic;

namespace XiaoTianQuanServer.Services.CoinMarketCap
{
    public class Status
    {
        public int ErrorCode { get; set; }
    }

    public class Currency
    {
        public int Id { get; set; }
        public string Symbol { get; set; }
        public string Slug { get; set; }
        public Dictionary<string, Quote> Quote { get; set; }
    }

    public class Quote
    {
        public double Price { get; set; }
    }

    public class QuoteResponse
    {
        public Status Status { get; set; }
        public Dictionary<string, Currency> Data { get; set; }
    }
}