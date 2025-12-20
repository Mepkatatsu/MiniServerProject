using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniServerProject.Controllers.Request;
using MiniServerProject.Domain.ServerLogs;
using MiniServerProject.Domain.Shared.Table;
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
                return NotFound($"User not found. UserId: {request.UserId}");

            if (user.CurrentStageId != null)
                return BadRequest($"User is already in a stage. CurrentStageId: {user.CurrentStageId}");

            var stageData = TableHolder.GetTable<StageTable>().Get(stageId);
            if (stageData == null)
                return NotFound($"Stage not found. stageId: {stageId}");

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

        // POST /stages/{stageId}/clear
        [HttpPost("{stageId}/clear")]
        public async Task<IActionResult> Clear(string stageId, [FromBody] ClearStageRequest request)
        {
            if (string.IsNullOrWhiteSpace(stageId))
                return BadRequest("stageId is required.");
            if (request.UserId == 0)
                return BadRequest("userId is required.");
            if (string.IsNullOrWhiteSpace(request.RequestId))
                return BadRequest("requestId is required.");

            var user = await _db.Users.FirstOrDefaultAsync(x => x.UserId == request.UserId);
            if (user == null)
                return NotFound($"User not found. userId: {request.UserId}");

            if (user.CurrentStageId != stageId)
                return BadRequest($"User is not in this stage. CurrentStageId: {user.CurrentStageId ?? "null"}, stageId: {stageId}");

            var stage = TableHolder.GetTable<StageTable>().Get(stageId);
            if (stage == null)
                return NotFound($"Stage not found. stageId: {stageId}");

            var reward = TableHolder.GetTable<RewardTable>().Get(stage.RewardId);
            if (reward == null)
                return NotFound($"Reward not found. rewardId: {stage.RewardId}");

            try
            {
                // 1) 보상 지급 + 상태 전이
                user.AddGold(reward.Gold);
                user.AddExp(reward.Exp);
                user.ClearCurrentStage(stageId);

                // 2) 멱등성 보장을 위해 로그 INSERT
                _db.StageClearLogs.Add(new StageClearLog(user.UserId, stageId, request.RequestId, DateTime.UtcNow));

                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                // 이미 처리된 requestId
                return Ok(new { stageId, requestId = request.RequestId, alreadyCleared = true });
            }

            return Ok(new
            {
                stageId,
                rewardId = stage.RewardId,
                rewardGold = reward.Gold,
                rewardExp = reward.Exp,
                user.Gold,
                user.Exp,
                user.CurrentStageId
            });
        }

        // POST /stages/{stageId}/give-up
        [HttpPost("{stageId}/give-up")]
        public async Task<IActionResult> GiveUp(string stageId, [FromBody] GiveUpStageRequest request)
        {
            if (string.IsNullOrWhiteSpace(stageId))
                return BadRequest("stageId is required.");
            if (request.UserId == 0)
                return BadRequest("userId is required.");
            if (string.IsNullOrWhiteSpace(request.RequestId))
                return BadRequest("requestId is required.");

            var user = await _db.Users.FirstOrDefaultAsync(x => x.UserId == request.UserId);
            if (user == null)
                return NotFound($"User not found. userId: {request.UserId}");

            if (user.CurrentStageId != stageId)
                return BadRequest($"User is not in this stage. CurrentStageId: {user.CurrentStageId ?? "null"}, stageId: {stageId}");

            // stageData가 삭제된 경우에 포기는 할 수 있도록 NotFound 처리 X
            var stage = TableHolder.GetTable<StageTable>().Get(stageId);
            ushort consumedStamina = stage?.NeedStamina ?? 0;
            ushort recoverStamina = TableHolder.GetTable<GameParameters>().GetRefundStamina(consumedStamina);

            try
            {
                var now = DateTime.UtcNow;

                user.SetCurrentStage(null);
                user.AddStamina(recoverStamina, now);

                _db.StageGiveUpLogs.Add(new StageGiveUpLog(user.UserId, stageId, request.RequestId, now));

                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                // 이미 처리된 requestId
                return Ok(new { stageId, requestId = request.RequestId, alreadyGiveUp = true });
            }

            return Ok(new
            {
                stageId,
                requestId = request.RequestId,
                recoverStamina,
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
