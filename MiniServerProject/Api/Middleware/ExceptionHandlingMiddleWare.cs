using MiniServerProject.Api.Common;
using MiniServerProject.Application;
using System.Text.Json;

namespace MiniServerProject.Api.Middleware
{
    public sealed class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (DomainException ex)
            {
                await HandleDomainException(context, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unhandled exception. traceId={TraceId} method={Method} path={Path}",
                    context.TraceIdentifier, context.Request.Method, context.Request.Path);

                await WriteErrorAsync(
                    context,
                    statusCode: StatusCodes.Status500InternalServerError,
                    error: "InternalServerError",
                    message: "An unexpected error occurred.");
            }
        }

        private async Task HandleDomainException(HttpContext context, DomainException ex)
        {
            var (status, message) = ex.ErrorType switch
            {
                ErrorType.UserNotFound => (StatusCodes.Status404NotFound, "User not found."),
                ErrorType.StageNotFound => (StatusCodes.Status404NotFound, "Stage not found."),
                ErrorType.RewardNotFound => (StatusCodes.Status404NotFound, "Reward not found."),
                ErrorType.IdempotencyMissingAfterUniqueViolation => (StatusCodes.Status500InternalServerError, "User missing after unique violation."),
                ErrorType.UserAlreadyInStage => (StatusCodes.Status400BadRequest, "User is already in a stage."),
                ErrorType.UserNotInThisStage => (StatusCodes.Status400BadRequest, "User is not in this stage."),
                ErrorType.NotEnoughStamina => (StatusCodes.Status400BadRequest, "Not enough stamina."),
                ErrorType.RequestIdUsedForDifferentStage => (StatusCodes.Status409Conflict, "RequestId already used for a different stage."),
                _ => (StatusCodes.Status500InternalServerError, "Unknown error.")
            };

            if (status >= 500)
            {
                _logger.LogError(ex,
                    "Domain exception. error={Error} traceId={TraceId}",
                    ex.ErrorType, context.TraceIdentifier);
            }
            else
            {
                _logger.LogWarning(
                    "Domain exception. error={Error} traceId={TraceId}",
                    ex.ErrorType, context.TraceIdentifier);
            }

            await WriteErrorAsync(context, status, ex.ErrorType.ToString(), message, ex.Details);
        }

        private static async Task WriteErrorAsync(
            HttpContext context,
            int statusCode,
            string error,
            string message,
            object? details = null)
        {
            if (context.Response.HasStarted)
                return;

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json; charset=utf-8";

            var payload = ApiErrorResponse.Create(
                error: error,
                message: message,
                traceId: context.TraceIdentifier,
                details: details);

            var json = JsonSerializer.Serialize(payload);

            await context.Response.WriteAsync(json);
        }
    }
}
