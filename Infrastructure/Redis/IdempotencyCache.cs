using StackExchange.Redis;
using System.Text.Json;

namespace MiniServerProject.Infrastructure.Redis
{
    public sealed class IdempotencyCache
    {
        private readonly IDatabase _redisDb;
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public IdempotencyCache(IConnectionMultiplexer mux)
        {
            _redisDb = mux.GetDatabase();
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
                Console.WriteLine($"[Redis] GET failed. key={key}, ex={ex.GetType().Name}: {ex.Message}");
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
                Console.WriteLine($"[Redis] SET failed. key={key}, ex={ex.GetType().Name}: {ex.Message}");
            }
        }
    }
}
