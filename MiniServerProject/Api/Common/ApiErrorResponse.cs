namespace MiniServerProject.Api.Common
{
    public sealed class ApiErrorResponse
    {
        public string Error { get; init; } = null!;
        public string Message { get; init; } = null!;
        public string TraceId { get; init; } = null!;
        public object? Details { get; init; }

        public static ApiErrorResponse Create(string error, string message, string traceId, object? details = null)
            => new()
            {
                Error = error,
                Message = message,
                TraceId = traceId,
                Details = details
            };
    }
}
