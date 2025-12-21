using Microsoft.AspNetCore.Mvc;

namespace MiniServerProject.Application.Stages
{
    public static class StageDomainExceptionExtensions
    {
        public static IActionResult ToActionResult(this StageDomainException ex)
        {
            return ex.Error switch
            {
                StageError.UserNotFound => new NotFoundObjectResult("User not found."),
                StageError.StageNotFound => new NotFoundObjectResult("Stage not found."),
                StageError.RewardNotFound => new NotFoundObjectResult("Reward not found."),
                StageError.UserAlreadyInStage => new BadRequestObjectResult("User is already in a stage."),
                StageError.UserNotInThisStage => new BadRequestObjectResult("User is not in this stage."),
                StageError.NotEnoughStamina => new BadRequestObjectResult(ex.Details ?? new { error = "NotEnoughStamina" }),
                StageError.RequestIdUsedForDifferentStage => new ConflictObjectResult("RequestId already used for a different stage."),
                StageError.IdempotencyLogMissingAfterUniqueViolation => new ObjectResult("Idempotency log missing after unique violation.") { StatusCode = 500 },
                _ => new ObjectResult("Unknown error.") { StatusCode = 500 }
            };
        }
    }
}
