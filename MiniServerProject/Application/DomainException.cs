namespace MiniServerProject.Application
{
    public enum ErrorType
    {
        InvalidRequest,
        UserNotFound,
        StageNotFound,
        RewardNotFound,
        UserAlreadyInStage,
        UserNotInThisStage,
        NotEnoughStamina,
        RequestIdUsedForDifferentStage,
        IdempotencyMissingAfterUniqueViolation
    }

    public sealed class DomainException : Exception
    {
        public ErrorType ErrorType { get; }
        public object? Details { get; }

        public DomainException(ErrorType errorType, object? details = null) : base(errorType.ToString())
        {
            ErrorType = errorType;
            Details = details;
        }
    }
}
