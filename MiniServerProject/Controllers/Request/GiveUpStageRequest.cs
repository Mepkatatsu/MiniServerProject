namespace MiniServerProject.Controllers.Request
{
    public sealed class GiveUpStageRequest
    {
        public ulong UserId { get; init; }
        public required string RequestId { get; init; }
    }
}
