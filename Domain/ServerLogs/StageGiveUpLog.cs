namespace MiniServerProject.Domain.ServerLogs
{
    public class StageGiveUpLog
    {
        public ulong StageGiveUpLogId { get; private set; }

        public ulong UserId { get; private set; }
        public string StageId { get; private set; } = null!;
        public string RequestId { get; private set; } = null!;
        public ushort RefundStamina {  get; private set; }
        public ushort AfterStamina { get; private set; }
        public DateTime GaveUpDateTime { get; private set; }

        protected StageGiveUpLog() { }

        public StageGiveUpLog(ulong userId, string stageId, string requestId, ushort refundStamina, ushort afterStamina, DateTime gaveUpDateTime)
        {
            if (string.IsNullOrWhiteSpace(stageId))
                throw new ArgumentException("stageId is required.");

            if (string.IsNullOrWhiteSpace(requestId))
                throw new ArgumentException("requestId is required.");

            UserId = userId;
            StageId = stageId;
            RequestId = requestId;
            RefundStamina = refundStamina;
            AfterStamina = afterStamina;
            GaveUpDateTime = gaveUpDateTime;
        }
    }
}
