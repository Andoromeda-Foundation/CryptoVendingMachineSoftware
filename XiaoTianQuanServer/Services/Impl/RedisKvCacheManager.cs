using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace XiaoTianQuanServer.Services.Impl
{
    public class RedisKvCacheManager : IKvCacheManager
    {
        private readonly IDatabase _db;

        public RedisKvCacheManager(IConnectionMultiplexer multiplexer)
        {
            _db = multiplexer.GetDatabase();
        }

        private string GetKey(string topic, string key)
        {
            return $"{topic}##{key}";
        }

        public Task<bool> SetAsync(string topic, string key, string value)
        {
            return _db.StringSetAsync(GetKey(topic, key), value);
        }

        public Task<bool> SetAsync(string topic, string key, int value)
        {
            return _db.StringSetAsync(GetKey(topic, key), value);
        }

        public Task<bool> SetAsync(string topic, string key, uint value)
        {
            return _db.StringSetAsync(GetKey(topic, key), value);
        }

        public Task<bool> SetAsync(string topic, string key, double value)
        {
            return _db.StringSetAsync(GetKey(topic, key), value);
        }

        public Task<bool> SetAsync(string topic, string key, byte[] value)
        {
            return _db.StringSetAsync(GetKey(topic, key), value);
        }

        public Task<bool> SetAsync(string topic, string key, bool value)
        {
            return _db.StringSetAsync(GetKey(topic, key), value);
        }

        public Task<bool> SetAsync(string topic, string key, long value)
        {
            return _db.StringSetAsync(GetKey(topic, key), value);
        }

        public Task<bool> SetAsync(string topic, string key, ulong value)
        {
            return _db.StringSetAsync(GetKey(topic, key), value);
        }

        public Task<bool> SetAsync(string topic, string key, float value)
        {
            return _db.StringSetAsync(GetKey(topic, key), value);
        }


        public async Task<string> GetStringAsync(string topic, string key)
        {
            var val = await _db.StringGetAsync(GetKey(topic, key));
            return val.HasValue ? (string) val : null;
        }

        public async Task<int?> GetIntAsync(string topic, string key)
        {
            var val = await _db.StringGetAsync(GetKey(topic, key));
            return val.HasValue ? (int?) val : null;
        }

        public async Task<uint?> GetUintAsync(string topic, string key)
        {
            var val = await _db.StringGetAsync(GetKey(topic, key));
            return val.HasValue ? (uint?)val : null;
        }

        public async Task<double?> GetDoubleAsync(string topic, string key)
        {
            var val = await _db.StringGetAsync(GetKey(topic, key));
            return val.HasValue ? (double?)val : null;
        }

        public async Task<byte[]> GetByteArrayAsync(string topic, string key)
        {
            var val = await _db.StringGetAsync(GetKey(topic, key));
            return val.HasValue ? (byte[])val : null;
        }

        public async Task<bool?> GetBoolAsync(string topic, string key)
        {
            var val = await _db.StringGetAsync(GetKey(topic, key));
            return val.HasValue ? (bool?)val : null;
        }

        public async Task<long?> GetLongAsync(string topic, string key)
        {
            var val = await _db.StringGetAsync(GetKey(topic, key));
            return val.HasValue ? (long?)val : null;
        }

        public async Task<ulong?> GetUlongAsync(string topic, string key)
        {
            var val = await _db.StringGetAsync(GetKey(topic, key));
            return val.HasValue ? (ulong?)val : null;
        }

        public async Task<float?> GetFloatAsync(string topic, string key)
        {
            var val = await _db.StringGetAsync(GetKey(topic, key));
            return val.HasValue ? (float?)val : null;
        }



        private Task<RedisValue> GetDeleteAsync(string topic, string key)
        {
            var tran = _db.CreateTransaction();
            var result = tran.StringGetAsync(GetKey(topic, key));
            tran.KeyDeleteAsync(key);
            tran.Execute();
            return result;
        }

        public async Task<string> GetDeleteStringAsync(string topic, string key)
        {
            var val = await GetDeleteAsync(topic, key);
            return val.HasValue ? (string) val : null;
        }

        public async Task<int?> GetDeleteIntAsync(string topic, string key)
        {
            var val = await GetDeleteAsync(topic, key);
            return val.HasValue ? (int?)val : null;
        }

        public async Task<uint?> GetDeleteUintAsync(string topic, string key)
        {
            var val = await GetDeleteAsync(topic, key);
            return val.HasValue ? (uint?)val : null;
        }

        public async Task<double?> GetDeleteDoubleAsync(string topic, string key)
        {
            var val = await GetDeleteAsync(topic, key);
            return val.HasValue ? (double?)val : null;
        }

        public async Task<byte[]> GetDeleteByteArrayAsync(string topic, string key)
        {
            var val = await GetDeleteAsync(topic, key);
            return val.HasValue ? (byte[])val : null;
        }

        public async Task<bool?> GetDeleteBoolAsync(string topic, string key)
        {
            var val = await GetDeleteAsync(topic, key);
            return val.HasValue ? (bool?)val : null;
        }

        public async Task<long?> GetDeleteLongAsync(string topic, string key)
        {
            var val = await GetDeleteAsync(topic, key);
            return val.HasValue ? (long?)val : null;
        }

        public async Task<ulong?> GetDeleteUlongAsync(string topic, string key)
        {
            var val = await GetDeleteAsync(topic, key);
            return val.HasValue ? (ulong?)val : null;
        }

        public async Task<float?> GetDeleteFloatAsync(string topic, string key)
        {
            var val = await GetDeleteAsync(topic, key);
            return val.HasValue ? (float?)val : null;
        }
    }
}
