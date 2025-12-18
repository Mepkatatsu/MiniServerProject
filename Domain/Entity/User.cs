namespace MiniServerProject.Domain.Entities
{
    public class User
    {
        public ulong UserId { get; private set; }
        public string Nickname { get; private set; } = string.Empty;

        public short Level { get; private set; } = 1;
        public short Stamina { get; private set; }
        public ulong Gold { get; private set; }
        public ulong Exp { get; private set; }

        public DateTime CreateDateTime { get; private set; }
        public DateTime LastStaminaUpdateTime { get; private set; }

        protected User() { }

        public User(string nickname, short initialStamina)
        {
            if (string.IsNullOrWhiteSpace(nickname))
                throw new ArgumentException("Nickname is required");

            // TODO: 닉네임에 부적절한 문자가 있는지 체크?

            Nickname = nickname.Trim();
            Stamina = initialStamina;
            CreateDateTime = DateTime.UtcNow;
            LastStaminaUpdateTime = DateTime.UtcNow;
        }

        public bool HasStamina(short amount)
        {
            return Stamina >= amount;
        }

        public void ConsumeStamina(short amount)
        {
            if (!HasStamina(amount))
                throw new InvalidOperationException("Not enough Stamina");

            Stamina -= amount;
        }

        public void AddGold(ulong amount)
        {
            Gold += amount;
        }

        public void AddExp(ulong amount)
        {
            Exp += amount;
        }
    }
}
