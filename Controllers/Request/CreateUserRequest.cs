namespace MiniServerProject.Controllers.Request
{
    public sealed class CreateUserRequest
    {
        public required string AccountId { get; init; }
        public required string Nickname { get; init; }
    }
}
