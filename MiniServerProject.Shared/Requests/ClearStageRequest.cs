namespace MiniServerProject.Shared.Requests
{
    public class ClearStageRequest
    {
        public ulong UserId { get; init; }
        public string RequestId { get; init; } = null!;

        public ClearStageRequest(ulong userId, string requestId)
        {
            UserId = userId;
            RequestId = requestId;
        }
    }
}
