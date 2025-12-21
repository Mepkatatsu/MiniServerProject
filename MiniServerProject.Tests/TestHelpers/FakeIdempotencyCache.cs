using MiniServerProject.Infrastructure;

namespace MiniServerProject.Tests.TestHelpers
{
    public sealed class FakeIdempotencyCache : IIdempotencyCache
    {
        public Task<T?> GetAsync<T>(string key)
            => Task.FromResult<T?>(default);

        public Task SetAsync<T>(string key, T value, TimeSpan ttl)
            => Task.CompletedTask;
    }
}
