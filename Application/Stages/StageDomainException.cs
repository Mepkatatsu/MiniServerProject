namespace MiniServerProject.Application.Stages
{
    public enum StageError
    {
        UserNotFound,
        StageNotFound,
        RewardNotFound,
        UserAlreadyInStage,
        UserNotInThisStage,
        NotEnoughStamina,
        RequestIdUsedForDifferentStage,
        IdempotencyLogMissingAfterUniqueViolation
    }

    public sealed class StageDomainException : Exception
    {
        public StageError Error { get; }
        public object? Details { get; }

        public StageDomainException(StageError error, object? details = null) : base(error.ToString())
        {
            Error = error;
            Details = details;
        }
    }
}
