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
                Gold = 1,
                Exp = 1,
            };

            RewardData rewardData2 = new()
            {
                Gold = 2,
                Exp = 2,
            };

            datas.Add("TEST-001-NORMAL-REWARD", rewardData1);
            datas.Add("TEST-002-NORMAL-REWARD", rewardData2);
        }
    }

    public class RewardData
    {
        public ulong Gold;
        public ulong Exp;
    }
}
