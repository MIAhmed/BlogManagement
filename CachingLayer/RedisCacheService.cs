using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using StackExchange.Redis;

namespace CachingLayer
{
    public class RedisCacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly IDatabase _cache;

        public RedisCacheService(string redisConnectionString)
        {
            _connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
            _cache = _connectionMultiplexer.GetDatabase();
        }

        public async Task<T> GetAsync<T>(string key)
        {
            try
            {
                var cachedData = await _cache.StringGetAsync(key);
                if (cachedData.IsNull)
                {
                    //throw new KeyNotFoundException();
                    return default(T);
                }
                return JsonSerializer.Deserialize<T>(cachedData);
            }
            catch (Exception ex)
            {
                throw ex;
                //return default(T);
            }

        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            try
            {
                var data = JsonSerializer.Serialize(value);
                await _cache.StringSetAsync(key, data, expiration);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task RemoveAsync<T>(string key)
        {
            try
            {
                await _cache.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
