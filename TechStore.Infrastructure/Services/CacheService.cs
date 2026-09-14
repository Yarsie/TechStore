using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using StackExchange.Redis;
using TechStore.Application.Services;

namespace TechStore.Infrastructure.Services
{
    public class CacheService : ICacheService
    {
        private readonly IDatabase _database;
        private readonly JsonSerializerOptions _jsonOptions;

        public CacheService(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await _database.StringGetAsync(key);
            if (value.IsNullOrEmpty)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(value.ToString(), _jsonOptions);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            var serialized = JsonSerializer.Serialize(value, _jsonOptions);
            Expiration expiry = expiration.HasValue ? expiration.Value : default;
            await _database.StringSetAsync(key, serialized, expiry);
        }   

        public async Task RemoveAsync(string key)
        {
            await _database.KeyDeleteAsync(key);
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            var endpoints = _database.Multiplexer.GetEndPoints();
            foreach (var endpoint in endpoints)
            {
                var server = _database.Multiplexer.GetServer(endpoint);
                var keys = server.Keys(pattern: pattern).ToArray();
                
                if (keys.Length > 0)
                {
                    await _database.KeyDeleteAsync(keys);
                }
            }
        }
    }
}