namespace MiniServerProject.Shared.Responses
{
    public class EnterStageResponse
    {
        public string RequestId { get; set; } = null!;
        public string StageId { get; set; } = null!;
        public ushort ConsumedStamina { get; set; }
        public ushort AfterStamina { get; set; }

        // Deserialize용 생성자
        public EnterStageResponse() { }
    }
}
