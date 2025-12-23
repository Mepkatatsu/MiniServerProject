using MiniServerProject.Shared.Responses;
using MiniServerProject.Shared.Tables;

namespace MiniServerProject.Domain.Entities
{
    public class User
    {
        public string AccountId { get; private set; } = null!;
        public ulong UserId { get; private set; }
        public string Nickname { get; private set; } = null!;

        public ushort Level { get; private set; } = 1;
        public ushort Stamina { get; private set; }
        public ulong Gold { get; private set; }
        public ulong Exp { get; private set; }

        public DateTime CreateDateTime { get; private set; }
        public DateTime LastStaminaUpdateTime { get; private set; }

        public string? CurrentStageId { get; private set; }

        private ushort MaxRecoverableStamina => TableHolder.GetTable<StaminaTable>().Get(Level)?.MaxRecoverableStamina ?? 0;

        protected User() { }

        public User(string accountId, UserResponse userResponse)
        {
            AccountId = accountId;
            UserId = userResponse.UserId;
            Nickname = userResponse.Nickname;
            Level = userResponse.Level;
            Stamina = userResponse.Stamina;
            Gold = userResponse.Gold;
            Exp = userResponse.Exp;
            CreateDateTime = userResponse.CreateDateTime;
            LastStaminaUpdateTime = userResponse.LastStaminaUpdateTime;
            CurrentStageId = userResponse.CurrentStageId;
        }

        public User(string accountId, string nickname)
        {
            if (string.IsNullOrWhiteSpace(accountId))
                throw new ArgumentException("AccountId is required");
            if (string.IsNullOrWhiteSpace(nickname))
                throw new ArgumentException("Nickname is required");

            // TODO: 닉네임에 부적절한 문자가 있는지 체크?

            ushort initialStamina = TableHolder.GetTable<StaminaTable>().Get(Level)?.MaxRecoverableStamina ?? 0;

            AccountId = accountId;
            Nickname = nickname.Trim();
            Stamina = initialStamina;
            CreateDateTime = DateTime.UtcNow;
            LastStaminaUpdateTime = DateTime.UtcNow;
        }

        public UserResponse CreateResponse()
        {
            var response = new UserResponse()
            {
                Nickname = Nickname,
                UserId = UserId,
                Level = Level,
                Stamina = Stamina,
                Gold = Gold,
                Exp = Exp,
                CreateDateTime = CreateDateTime,
                LastStaminaUpdateTime = LastStaminaUpdateTime,
                CurrentStageId = CurrentStageId,
            };

            return response;
        }

        public bool UpdateStaminaByDateTime(DateTime currentDateTime)
        {
            if (Stamina >= MaxRecoverableStamina)
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
            if (rawRecoverCount > MaxRecoverableStamina - Stamina)
                finalStamina = MaxRecoverableStamina;
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

        public bool AddStamina(ushort amount, DateTime currentDateTime)
        {
            UpdateStaminaByDateTime(currentDateTime);

            // 기존에 or 회복으로 인해 최대치가 되었다면 회복 시작 시간을 현재 시간으로 세팅
            if (Stamina >= MaxRecoverableStamina)
                LastStaminaUpdateTime = currentDateTime;

            Stamina += amount;
            return true;
        }

        public bool ConsumeStamina(ushort amount, DateTime currentDateTime)
        {
            UpdateStaminaByDateTime(currentDateTime);

            if (!HasStamina(amount))
                return false;

            // 기존에 or 회복으로 인해 최대치가 되었다면 회복 시작 시간을 현재 시간으로 세팅
            if (Stamina >= MaxRecoverableStamina)
                LastStaminaUpdateTime = currentDateTime;

            Stamina -= amount;
            return true;
        }

        public void AddGold(ulong amount)
        {
            Gold += amount;
        }

        public void AddExp(ulong amount)
        {
            Exp += amount;
        }

        public void SetCurrentStage(string stageId)
        {
            CurrentStageId = stageId;
        }

        public void ClearCurrentStage(string stageId)
        {
            if (CurrentStageId != stageId)
                throw new InvalidOperationException($"User is not in this stage. CurrentStageId: {CurrentStageId ?? "null"}, stageId: {stageId}");

            CurrentStageId = null;
        }
    }
}
