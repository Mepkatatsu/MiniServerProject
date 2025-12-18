using MiniServerProject.Domain.Shared.Table;

namespace MiniServerProject.Domain.Table
{
    public class StageTable : TableBase<string, StageData>
    {
        public override void Initialize()
        {
            // TODO: 로컬 테이블에서 데이터를 읽어오도록 수정 필요

            StageData stageData1 = new()
            {
                RewardId = "stageReward1",
                NeedStamina = 10,
            };

            StageData stageData2 = new()
            {
                RewardId = "stageReward2",
                NeedStamina = 10,
            };

            datas.Add("stageData1", stageData1);
            datas.Add("stageData2", stageData2);
        }
    }

    public class StageData
    {
        public string RewardId = string.Empty;
        public short NeedStamina;
    }
}
