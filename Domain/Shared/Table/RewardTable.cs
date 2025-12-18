using MiniServerProject.Domain.Shared.Table;

namespace MiniServerProject.Domain.Table
{
    public class RewardTable : TableBase<string, RewardData>
    {
        public override void Initialize()
        {
            // TODO: 로컬 테이블에서 데이터를 읽어오도록 수정 필요

            RewardData rewardData1 = new()
            {
                gold = 1,
                exp = 1,
            };

            RewardData rewardData2 = new()
            {
                gold = 2,
                exp = 2,
            };

            datas.Add("stageReward1", rewardData1);
            datas.Add("stageReward2", rewardData2);
        }
    }

    public class RewardData
    {
        public ulong gold;
        public ulong exp;
    }
}
