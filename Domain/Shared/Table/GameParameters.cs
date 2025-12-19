using MiniServerProject.Domain.Table;

namespace MiniServerProject.Domain.Shared.Table
{
    public class GameParameters : ITable
    {
        public uint StaminaRecoverCycleSec = 60 * 6; // TODO: 테이블 읽을 때 0 이상인 것을 보장해줘야 함

        public void Initialize()
        {
            // TODO: 로컬 테이블에서 데이터를 읽어오도록 수정 필요
            // 다른 테이블들과는 달리 Key와 Data 쌍으로 이루어지지 않은 테이블에 대한 처리도 필요할 듯
        }
    }
}
