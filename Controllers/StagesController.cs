using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniServerProject.Controllers.Request;
using MiniServerProject.Domain.ServerLogs;
using MiniServerProject.Domain.Table;
using MiniServerProject.Infrastructure.Persistence;

namespace MiniServerProject.Controllers
{
    [ApiController]
    [Route("stages")]
    public sealed class StagesController : ControllerBase
    {
        private readonly GameDbContext _db;

        public StagesController(GameDbContext db)
        {
            _db = db;
        }

        // POST stages/{stageId}/enter
        [HttpPost("{stageId}/enter")]
        public async Task<IActionResult> Enter(string stageId, [FromBody] EnterStageRequest request)
        {
            if (string.IsNullOrWhiteSpace(stageId))
                return BadRequest("stageId is required.");
            if (request.UserId == 0)
                return BadRequest("userId is required.");
            if (string.IsNullOrWhiteSpace(request.RequestId))
                return BadRequest("requestId is required.");

            var now = DateTime.UtcNow;

            var user = await _db.Users.FirstOrDefaultAsync(x => x.UserId == request.UserId);
            if (user == null)
                return NotFound("User not found.");

            if (user.CurrentStageId != null)
                return BadRequest("User is already in a stage.");

            var stageData = TableHolder.GetTable<StageTable>().Get(stageId);
            if (stageData == null)
                return NotFound("Stage not found.");

            try
            {
                // 1) 스태미너 체크/소모
                if (!user.ConsumeStamina(stageData.NeedStamina, now))
                    return BadRequest(new { error = "NotEnoughStamina", current = user.Stamina, required = stageData.NeedStamina });

                // 2) 멱등성 보장을 위해 로그 INSERT
                _db.StageEnterLogs.Add(new StageEnterLog(user.UserId, stageId, request.RequestId, now));

                user.SetCurrentStage(stageId);

                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                // 이미 처리된 requestId
                return Ok(new { stageId, requestId = request.RequestId, alreadyEntered = true });
            }

            return Ok(new
            {
                stageId,
                requestId = request.RequestId,
                consumedStamina = stageData.NeedStamina,
                user.Stamina,
                user.LastStaminaUpdateTime,
                user.CurrentStageId
            });
        }

        private static bool IsUniqueViolation(DbUpdateException ex)
        {
            return ex.InnerException?.Message.Contains("Duplicate", StringComparison.OrdinalIgnoreCase) == true;
        }
    }
}
