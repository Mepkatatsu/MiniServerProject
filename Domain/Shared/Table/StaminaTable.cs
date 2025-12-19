
using MiniServerProject.Domain.Table;

namespace MiniServerProject.Domain.Shared.Table
{
    public class StaminaTable : TableBase<ushort, StaminaData>
    {
        public override void Initialize()
        {
            // TODO: 로컬 테이블에서 데이터를 읽어오도록 수정 필요

            StaminaData StaminaData1 = new()
            {
                MaxRecoverableStamina = 24,
            };

            StaminaData StaminaData2 = new()
            {
                MaxRecoverableStamina = 28,
            };

            datas.Add(1, StaminaData1);
            datas.Add(2, StaminaData2);
        }
    }

    public class StaminaData
    {
        public ushort MaxRecoverableStamina;
    }
}
