using MiniServerProject.Domain.Shared.Table;
using MiniServerProject.Domain.Table;

namespace MiniServerProject.Domain.Entities
{
    public class User
    {
        public ulong UserId { get; private set; }
        public string Nickname { get; private set; } = null!;

        public ushort Level { get; private set; } = 1;
        public ushort Stamina { get; private set; }
        public ulong Gold { get; private set; }
        public ulong Exp { get; private set; }

        public DateTime CreateDateTime { get; private set; }
        public DateTime LastStaminaUpdateTime { get; private set; }

        public string? CurrentStageId { get; private set; }

        protected User() { }

        public User(string nickname)
        {
            if (string.IsNullOrWhiteSpace(nickname))
                throw new ArgumentException("Nickname is required");

            // TODO: 닉네임에 부적절한 문자가 있는지 체크?

            ushort initialStamina = TableHolder.GetTable<StaminaTable>().Get(Level)?.MaxRecoverableStamina ?? 0;

            Nickname = nickname.Trim();
            Stamina = initialStamina;
            CreateDateTime = DateTime.UtcNow;
            LastStaminaUpdateTime = DateTime.UtcNow;
        }

        public bool UpdateStaminaByDateTime(DateTime currentDateTime)
        {
            ushort maxRecoverableStamina = TableHolder.GetTable<StaminaTable>().Get(Level)?.MaxRecoverableStamina ?? 0;
            if (Stamina >= maxRecoverableStamina)
                return false;

            long elapsedSec = (long)(currentDateTime - LastStaminaUpdateTime).TotalSeconds;
            if (elapsedSec <= 0)
                return false;

            uint recoverCycleSec = TableHolder.GetTable<GameParameters>().StaminaRecoverCycleSec;
            if (recoverCycleSec == 0)
                throw new InvalidOperationException("StaminaRecoverCycleSec must be > 0");

            if (elapsedSec < recoverCycleSec)
                return false;

            uint rawRecoverCount = (uint)(elapsedSec / recoverCycleSec);
            
            ushort finalStamina;
            if (rawRecoverCount > maxRecoverableStamina - Stamina)
                finalStamina = maxRecoverableStamina;
            else
                finalStamina = (ushort)(Stamina + rawRecoverCount);

            ushort recoveredStamina = (ushort)(finalStamina - Stamina);

            long increasedSec = (long)recoveredStamina * recoverCycleSec;
            LastStaminaUpdateTime = LastStaminaUpdateTime.AddSeconds(increasedSec);
            Stamina = finalStamina;

            return true;
        }

        public bool HasStamina(ushort amount)
        {
            return Stamina >= amount;
        }

        public void ConsumeStamina(ushort amount)
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
