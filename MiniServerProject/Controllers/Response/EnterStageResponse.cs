using MiniServerProject.Domain.ServerLogs;

namespace MiniServerProject.Controllers.Response
{
    public class EnterStageResponse
    {
        public string RequestId { get; set; } = null!;
        public string StageId { get; set; } = null!;
        public ushort ConsumedStamina { get; set; }
        public ushort AfterStamina { get; set; }

        // Deserialize용 생성자
        protected EnterStageResponse() { }

        public EnterStageResponse(StageEnterLog stageClearLog)
        {
            RequestId = stageClearLog.RequestId;
            StageId = stageClearLog.StageId;
            ConsumedStamina = stageClearLog.ConsumedStamina;
            AfterStamina = stageClearLog.AfterStamina;
        }
    }
}
