namespace MiniServerProject.Domain.ServerLogs
{
    public class StageClearLog
    {
        public ulong StageClearLogId { get; private set; }

        public ulong UserId { get; private set; }
        public string StageId { get; private set; } = null!;
        public string RequestId { get; private set; } = null!;
        public string RewardId { get; private set; } = null!;
        public ulong GainGold { get; private set; }
        public ulong GainExp { get; private set; }
        public ulong AfterGold { get; private set; }
        public ulong AfterExp { get; private set; }
        public DateTime ClearedDateTime { get; private set; }

        protected StageClearLog() { }

        public StageClearLog(ulong userId, string stageId, string requestId, string rewardId, ulong gainGold, ulong gainExp, ulong afterGold, ulong afterExp, DateTime clearedDateTime)
        {
            if (string.IsNullOrWhiteSpace(stageId))
                throw new ArgumentException("stageId is required.");

            if (string.IsNullOrWhiteSpace(requestId))
                throw new ArgumentException("requestId is required.");

            UserId = userId;
            StageId = stageId;
            RequestId = requestId;
            RewardId = rewardId;
            GainGold = gainGold;
            GainExp = gainExp;
            AfterGold = afterGold;
            AfterExp = afterExp;
            ClearedDateTime = clearedDateTime;
        }
    }
}
