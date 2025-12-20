using MiniServerProject.Domain.ServerLogs;

namespace MiniServerProject.Controllers.Response
{
    public class GiveUpStageResponse
    {
        public string RequestId { get; set; } = null!;
        public string StageId { get; set; } = null!;
        public ushort RefundStamina { get; set; }
        public ushort AfterStamina { get; set; }

        // Deserialize용 생성자
        public GiveUpStageResponse()
        {

        }

        public GiveUpStageResponse(StageGiveUpLog stageGiveUpLog)
        {
            RequestId = stageGiveUpLog.RequestId;
            StageId = stageGiveUpLog.StageId;
            RefundStamina = stageGiveUpLog.RefundStamina;
            AfterStamina = stageGiveUpLog.AfterStamina;
        }
    }
}
