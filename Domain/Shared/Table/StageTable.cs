using MiniServerProject.Domain.Shared.Table;

namespace MiniServerProject.Domain.Table
{
    public class StageTable : TableBase<string, StageData>
    {
        public override void Initialize()
        {
            // TODO: 로컬 테이블에서 데이터를 읽어오도록 수정 필요

            StageData testStage1Data = new()
            {
                RewardId = "TEST-001-NORNAL-REWARD",
                NeedStamina = 10,
            };

            StageData testStage2Data = new()
            {
                RewardId = "TEST-002-NORNAL-REWARD",
                NeedStamina = 10,
            };

            datas.Add("TEST-001-NORNAL", testStage1Data);
            datas.Add("TEST-002-NORNAL", testStage2Data);
        }
    }

    public class StageData
    {
        public string RewardId = string.Empty;
        public ushort NeedStamina;
    }
}
