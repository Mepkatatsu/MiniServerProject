using StackExchange.Redis;
using System.Text.Json;

namespace MiniServerProject.Infrastructure.Redis
{
    public sealed class RedisCache : IIdempotencyCache
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly IDatabase _redisDb;
        private readonly ILogger _logger;

        public RedisCache(IConnectionMultiplexer mux, ILogger logger)
        {
            _redisDb = mux.GetDatabase();
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var value = await _redisDb.StringGetAsync(key);
                if (value.IsNullOrEmpty)
                    return default;

                return JsonSerializer.Deserialize<T>(value!, JsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"[Redis] GET failed. key={key}, ex={ex.GetType().Name}: {ex.Message}");
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan ttl)
        {
            try
            {
                var json = JsonSerializer.Serialize(value, JsonOptions);
                await _redisDb.StringSetAsync(key, json, ttl);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"[Redis] SET failed. key={key}, ex={ex.GetType().Name}: {ex.Message}");
            }
        }
    }
}
