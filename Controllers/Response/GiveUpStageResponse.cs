using MiniServerProject.Domain.ServerLogs;

namespace MiniServerProject.Controllers.Response
{
    public class GiveUpStageResponse
    {
        public string RequestId { get; set; } = null!;
        public string StageId { get; init; } = null!;
        public ushort RefundStamina { get; set; }
        public ushort AfterStamina { get; set; }

        public GiveUpStageResponse(StageGiveUpLog stageGiveUpLog)
        {
            RequestId = stageGiveUpLog.RequestId;
            StageId = stageGiveUpLog.StageId;
            RefundStamina = stageGiveUpLog.RefundStamina;
            AfterStamina = stageGiveUpLog.AfterStamina;
        }
    }
}
