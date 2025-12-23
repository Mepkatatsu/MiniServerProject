namespace MiniServerProject.Shared.Requests
{
    public sealed class EnterStageRequest
    {
        public ulong UserId { get; init; }
        public required string RequestId { get; init; }
    }
}
