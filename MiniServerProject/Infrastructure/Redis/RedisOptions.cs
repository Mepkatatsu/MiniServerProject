namespace MiniServerProject.Infrastructure.Redis
{
    public sealed class RedisOptions
    {
        public string ConnectionString { get; init; } = null!;
        public bool AbortOnConnectFail { get; init; } = false;
        public int ConnectTimeout { get; init; } = 200;
        public int SyncTimeout { get; init; } = 200;
        public int AsyncTimeout { get; init; } = 200;
    }
}
