namespace MiniServerProject.Domain.ServerLogs
{
    public class StageClearLog
    {
        public ulong StageClearLogId { get; private set; }

        public ulong UserId { get; private set; }
        public string StageId { get; private set; } = null!;
        public string RequestId { get; private set; } = null!;
        public DateTime ClearedDateTime { get; private set; }

        protected StageClearLog() { }

        public StageClearLog(ulong userId, string stageId, string requestId, DateTime clearedDateTime)
        {
            if (string.IsNullOrWhiteSpace(stageId))
                throw new ArgumentException("stageId is required.");

            if (string.IsNullOrWhiteSpace(requestId))
                throw new ArgumentException("requestId is required.");

            UserId = userId;
            StageId = stageId;
            RequestId = requestId;
            ClearedDateTime = clearedDateTime;
        }
    }
}
