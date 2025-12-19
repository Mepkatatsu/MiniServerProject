namespace MiniServerProject.Domain.ServerLogs
{
    public class StageEnterLog
    {
        public ulong StageEnterLogId { get; private set; }

        public ulong UserId { get; private set; }
        public string StageId { get; private set; } = null!;
        public string RequestId { get; private set; } = null!;
        public DateTime EnteredDateTime { get; private set; }

        protected StageEnterLog() { }

        public StageEnterLog(ulong userId, string stageId, string requestId, DateTime enteredDateTime)
        {
            if (string.IsNullOrWhiteSpace(stageId))
                throw new ArgumentException("stageId is required.");

            if (string.IsNullOrWhiteSpace(requestId))
                throw new ArgumentException("requestId is required.");

            UserId = userId;
            StageId = stageId;
            RequestId = requestId;
            EnteredDateTime = enteredDateTime;
        }
    }
}
