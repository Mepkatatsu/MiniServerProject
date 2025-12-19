namespace MiniServerProject.Controllers.Request
{
    public sealed class EnterStageRequest
    {
        public ulong UserId { get; init; }
        public required string RequestId { get; init; }
    }
}
