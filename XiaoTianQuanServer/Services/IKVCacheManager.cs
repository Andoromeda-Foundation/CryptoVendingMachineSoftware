using System.Threading.Tasks;

namespace XiaoTianQuanServer.Services
{
    public interface IKvCacheManager
    {
        Task<bool> SetAsync(string topic, string key, string value);
        Task<bool> SetAsync(string topic, string key, int value);
        Task<bool> SetAsync(string topic, string key, uint value);
        Task<bool> SetAsync(string topic, string key, double value);
        Task<bool> SetAsync(string topic, string key, byte[] value);
        Task<bool> SetAsync(string topic, string key, bool value);
        Task<bool> SetAsync(string topic, string key, long value);
        Task<bool> SetAsync(string topic, string key, ulong value);
        Task<bool> SetAsync(string topic, string key, float value);

        Task<string> GetStringAsync(string topic, string key);
        Task<int?> GetIntAsync(string topic, string key);
        Task<uint?> GetUintAsync(string topic, string key);
        Task<double?> GetDoubleAsync(string topic, string key);
        Task<byte[]> GetByteArrayAsync(string topic, string key);
        Task<bool?> GetBoolAsync(string topic, string key);
        Task<long?> GetLongAsync(string topic, string key);
        Task<ulong?> GetUlongAsync(string topic, string key);
        Task<float?> GetFloatAsync(string topic, string key);

        Task<string> GetDeleteStringAsync(string topic, string key);
        Task<int?> GetDeleteIntAsync(string topic, string key);
        Task<uint?> GetDeleteUintAsync(string topic, string key);
        Task<double?> GetDeleteDoubleAsync(string topic, string key);
        Task<byte[]> GetDeleteByteArrayAsync(string topic, string key);
        Task<bool?> GetDeleteBoolAsync(string topic, string key);
        Task<long?> GetDeleteLongAsync(string topic, string key);
        Task<ulong?> GetDeleteUlongAsync(string topic, string key);
        Task<float?> GetDeleteFloatAsync(string topic, string key);
    }
}