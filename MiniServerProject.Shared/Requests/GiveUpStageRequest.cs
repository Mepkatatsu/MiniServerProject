namespace MiniServerProject.Shared.Requests
{
    public sealed class GiveUpStageRequest
    {
        public ulong UserId { get; init; }
        public string RequestId { get; init; } = null!;

        public GiveUpStageRequest(ulong userId, string requestId)
        {
            UserId = userId;
            RequestId = requestId;
        }
    }
}
