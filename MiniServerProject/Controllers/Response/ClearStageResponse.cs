using MiniServerProject.Domain.ServerLogs;

namespace MiniServerProject.Controllers.Response
{
    public class ClearStageResponse
    {
        public string RequestId { get; set; } = null!;
        public string StageId { get; set; } = null!;
        public string RewardId { get; set; } = null!;
        public ulong GainGold { get; set; }
        public ulong GainExp { get; set; }
        public ulong AfterGold { get; set; }
        public ulong AfterExp { get; set; }

        // Deserialize용 생성자
        public ClearStageResponse()
        {

        }

        public ClearStageResponse(StageClearLog stageClearLog)
        {
            RequestId = stageClearLog.RequestId;
            StageId = stageClearLog.StageId;
            RewardId = stageClearLog.RewardId;
            GainGold = stageClearLog.GainGold;
            GainExp = stageClearLog.GainExp;
            AfterGold = stageClearLog.AfterGold;
            AfterExp = stageClearLog.AfterExp;
        }
    }
}
