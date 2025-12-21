using MiniServerProject.Infrastructure;
using System.Collections.Concurrent;
using System.Text.Json;

namespace MiniServerProject.Tests.TestHelpers
{
    public sealed class MemoryIdempotencyCache : IIdempotencyCache
    {
        private readonly ConcurrentDictionary<string, (string Json, DateTimeOffset ExpireAt)> _store = new();
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public Task<T?> GetAsync<T>(string key)
        {
            if (_store.TryGetValue(key, out var entry))
            {
                if (entry.ExpireAt > DateTimeOffset.UtcNow)
                    return Task.FromResult(JsonSerializer.Deserialize<T>(entry.Json, JsonOptions));
                _store.TryRemove(key, out _);
            }

            return Task.FromResult<T?>(default);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan ttl)
        {
            var json = JsonSerializer.Serialize(value, JsonOptions);
            _store[key] = (json, DateTimeOffset.UtcNow.Add(ttl));
            return Task.CompletedTask;
        }
    }
}
