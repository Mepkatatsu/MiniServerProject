namespace MiniServerProject.Shared.Responses
{
    public class ClearStageResponse
    {
        public string RequestId { get; set; } = null!;
        public string StageId { get; set; } = null!;
        public string RewardId { get; set; } = null!;
        public ulong GainGold { get; set; }
        public ulong GainExp { get; set; }
        public ulong AfterGold { get; set; }
        public ulong AfterExp { get; set; }

        // Deserialize용 생성자
        public ClearStageResponse() { }
    }
}
