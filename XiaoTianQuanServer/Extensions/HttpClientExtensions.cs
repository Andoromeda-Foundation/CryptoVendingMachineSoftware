using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace XiaoTianQuanServer.Extensions
{
    public static class HttpClientExtensions
    {
        public static Task<HttpResponseMessage> PostJsonAsync<T>(this HttpClient client, string requestUri, T payload, JsonSerializerSettings jsonSerializerSettings = null)
        {
            var json = jsonSerializerSettings == null
                ? JsonConvert.SerializeObject(payload)
                : JsonConvert.SerializeObject(payload, jsonSerializerSettings);
            return client.PostAsync(requestUri, new StringContent(json, System.Text.Encoding.UTF8, "application/json"));
        }
    }
}
