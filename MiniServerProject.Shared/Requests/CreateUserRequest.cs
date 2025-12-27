namespace MiniServerProject.Shared.Requests
{
    public sealed class CreateUserRequest
    {
        public string AccountId { get; init; } = null!;
        public string Nickname { get; init; } = null!;

        public CreateUserRequest(string accountId, string nickname)
        {
            AccountId = accountId;
            Nickname = nickname;
        }
    }
}
