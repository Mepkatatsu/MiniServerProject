using Microsoft.AspNetCore.Mvc;

namespace MiniServerProject.Application.Users
{
    public static class UserDomainExceptionExtensions
    {
        public static IActionResult ToActionResult(this UserDomainException ex)
        {
            return ex.Error switch
            {
                UserError.UserNotFound => new NotFoundObjectResult("User not found."),
                UserError.UserMissingAfterUniqueViolation => new ObjectResult("Idempotency log missing after unique violation.") { StatusCode = 500 },
                _ => new ObjectResult("Unknown error.") { StatusCode = 500 }
            };
        }
    }
}
