namespace MiniServerProject.Shared.Requests
{
    public class ClearStageRequest
    {
        public ulong UserId { get; init; }
        public required string RequestId { get; init; }
    }
}
