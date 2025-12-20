using MiniServerProject.Domain.ServerLogs;

namespace MiniServerProject.Controllers.Response
{
    public class ClearStageResponse
    {
        public string RequestId { get; set; } = null!;
        public string StageId { get; init; } = null!;
        public string RewardId { get; init; } = null!;
        public ulong GainGold { get; init; }
        public ulong GainExp { get; init; }
        public ulong AfterGold { get; init; }
        public ulong AfterExp { get; init; }

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
