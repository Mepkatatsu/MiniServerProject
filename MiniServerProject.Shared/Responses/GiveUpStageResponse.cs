namespace MiniServerProject.Shared.Responses
{
    public class GiveUpStageResponse
    {
        public string RequestId { get; set; } = null!;
        public string StageId { get; set; } = null!;
        public ushort RefundStamina { get; set; }
        public ushort AfterStamina { get; set; }

        // Deserialize용 생성자
        public GiveUpStageResponse() { }
    }
}
