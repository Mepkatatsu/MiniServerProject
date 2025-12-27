namespace MiniServerProject.Shared.Requests
{
    public sealed class EnterStageRequest
    {
        public ulong UserId { get; init; }
        public string RequestId { get; init; } = null!;

        public EnterStageRequest(ulong userId, string requestId)
        {
            UserId = userId;
            RequestId = requestId;
        }
    }
}
