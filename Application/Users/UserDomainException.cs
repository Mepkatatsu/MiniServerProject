namespace MiniServerProject.Application.Users
{
    public enum UserError
    {
        UserNotFound,
        UserMissingAfterUniqueViolation
    }

    public sealed class UserDomainException : Exception
    {
        public UserError Error { get; }
        public object? Details { get; }

        public UserDomainException(UserError error, object? details = null) : base(error.ToString())
        {
            Error = error;
            Details = details;
        }
    }
}
