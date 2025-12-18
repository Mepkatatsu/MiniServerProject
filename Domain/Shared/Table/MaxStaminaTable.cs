
using MiniServerProject.Domain.Table;

namespace MiniServerProject.Domain.Shared.Table
{
    public class MaxStaminaTable : TableBase<short, MaxStaminaData>
    {
        public override void Initialize()
        {
            // TODO: 로컬 테이블에서 데이터를 읽어오도록 수정 필요

            MaxStaminaData maxStaminaData1 = new()
            {
                MaxStamina = 24,
            };

            MaxStaminaData maxStaminaData2 = new()
            {
                MaxStamina = 28,
            };

            datas.Add(1, maxStaminaData1);
            datas.Add(2, maxStaminaData2);
        }
    }

    public class MaxStaminaData
    {
        public short MaxStamina;
    }
}
