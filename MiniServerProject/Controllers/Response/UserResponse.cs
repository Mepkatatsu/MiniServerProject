using MiniServerProject.Domain.Entities;

namespace MiniServerProject.Controllers.Response
{
    public class UserResponse
    {
        public string Nickname { get; set; } = null!;
        public ulong UserId { get; set; }
        public ushort Level { get; set; }
        public ushort Stamina { get; set; }
        public ulong Gold { get; set; }
        public ulong Exp { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime LastStaminaUpdateTime { get; set; }
        public string? CurrentStageId { get; set; }

        // Deserialize용 생성자
        public UserResponse() { }

        public UserResponse(User user)
        {
            Nickname = user.Nickname;
            UserId = user.UserId;
            Level = user.Level;
            Stamina = user.Stamina;
            Gold = user.Gold;
            Exp = user.Exp;
            CreateDateTime = user.CreateDateTime;
            LastStaminaUpdateTime = user.LastStaminaUpdateTime;
            CurrentStageId = user.CurrentStageId;
        }
    }
}
