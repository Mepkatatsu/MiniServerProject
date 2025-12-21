using MiniServerProject.Domain.Entities;

namespace MiniServerProject.Domain.ServerLogs
{
    public class UserCreateLog
    {
        public ulong UserCreateLogId { get; set; }
        public string AccountId { get; set; } = null!;
        public ulong UserId { get; set; }       // Navigated by User
        public User User { get; set; } = null!; // Navigation for UserId
        public string Nickname { get; set; } = null!;
        public DateTime CreateDateTime { get; set; }

        protected UserCreateLog() { }

        public UserCreateLog(string accountId, User user, string nickname, DateTime createDateTime)
        {
            if (string.IsNullOrWhiteSpace(accountId))
                throw new ArgumentException("accountId is required.");

            if (user == null)
                throw new ArgumentException("user is required.");

            if (string.IsNullOrWhiteSpace(nickname))
                throw new ArgumentException("nickName is required.");

            AccountId = accountId;
            User = user;
            Nickname = nickname;
            CreateDateTime = createDateTime;
        }
    }
}
