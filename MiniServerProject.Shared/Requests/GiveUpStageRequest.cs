namespace MiniServerProject.Shared.Requests
{
    public sealed class GiveUpStageRequest
    {
        public ulong UserId { get; init; }
        public required string RequestId { get; init; }
    }
}
