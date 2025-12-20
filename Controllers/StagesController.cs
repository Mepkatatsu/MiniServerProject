using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniServerProject.Controllers.Request;
using MiniServerProject.Controllers.Response;
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

            var log = await FindEnterLogAsync(request.UserId, request.RequestId);

            if (log != null)
            {
                if (log.StageId != stageId)
                    return Conflict("RequestId already used for a different stage.");

                return Ok(new EnterStageResponse(log));
            }

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
                var now = DateTime.UtcNow;
                if (!user.ConsumeStamina(stageData.NeedStamina, now))
                    return BadRequest(new { error = "NotEnoughStamina", current = user.Stamina, required = stageData.NeedStamina });

                // 2) 멱등성 보장을 위해 로그 INSERT
                log = new StageEnterLog(user.UserId, stageId, request.RequestId, stageData.NeedStamina, user.Stamina, now);
                _db.StageEnterLogs.Add(log);

                user.SetCurrentStage(stageId);

                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                log = await FindEnterLogAsync(request.UserId, request.RequestId);

                if (log == null)
                {
                    // TODO: 특별히 이상한 상황이라 서버 로그를 남기면 좋을 듯
                    return StatusCode(500, "Idempotency log missing after unique violation.");
                }

                return Ok(new EnterStageResponse(log));
            }

            return Ok(new EnterStageResponse(log));
        }

        private async Task<StageEnterLog?> FindEnterLogAsync(ulong userId, string requestId)
        {
            return await _db.StageEnterLogs
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.RequestId == requestId)
                .FirstOrDefaultAsync();
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

            var log = await FindClearLogAsync(request.UserId, request.RequestId);

            if (log != null)
            {
                if (log.StageId != stageId)
                    return Conflict("RequestId already used for a different stage.");

                return Ok(new ClearStageResponse(log));
            }

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
                var now = DateTime.UtcNow;
                log = new StageClearLog(user.UserId, stageId, request.RequestId, stage.RewardId, reward.Gold, reward.Exp, user.Gold, user.Exp, now);
                _db.StageClearLogs.Add(log);

                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                log = await FindClearLogAsync(request.UserId, request.RequestId);

                if (log == null)
                {
                    // TODO: 특별히 이상한 상황이라 서버 로그를 남기면 좋을 듯
                    return StatusCode(500, "Idempotency log missing after unique violation.");
                }

                return Ok(new ClearStageResponse(log));
            }

            return Ok(new ClearStageResponse(log));
        }

        private async Task<StageClearLog?> FindClearLogAsync(ulong userId, string requestId)
        {
            return await _db.StageClearLogs
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.RequestId == requestId)
                .FirstOrDefaultAsync();
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

            var log = await FindGiveUpLogAsync(request.UserId, request.RequestId);

            if (log != null)
            {
                if (log.StageId != stageId)
                    return Conflict("RequestId already used for a different stage.");

                return Ok(new GiveUpStageResponse(log));
            }

            var user = await _db.Users.FirstOrDefaultAsync(x => x.UserId == request.UserId);
            if (user == null)
                return NotFound($"User not found. userId: {request.UserId}");

            if (user.CurrentStageId != stageId)
                return BadRequest($"User is not in this stage. CurrentStageId: {user.CurrentStageId ?? "null"}, stageId: {stageId}");

            // stageData가 삭제된 경우에 포기는 할 수 있도록 NotFound 처리 X
            var stage = TableHolder.GetTable<StageTable>().Get(stageId);
            ushort consumedStamina = stage?.NeedStamina ?? 0;
            ushort refundStamina = TableHolder.GetTable<GameParameters>().GetRefundStamina(consumedStamina);

            try
            {
                var now = DateTime.UtcNow;

                user.ClearCurrentStage(user.CurrentStageId);
                user.AddStamina(refundStamina, now);

                log = new StageGiveUpLog(user.UserId, stageId, request.RequestId, refundStamina, user.Stamina, now);

                _db.StageGiveUpLogs.Add(log);

                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                log = await FindGiveUpLogAsync(request.UserId, request.RequestId);

                if (log == null)
                {
                    // TODO: 특별히 이상한 상황이라 서버 로그를 남기면 좋을 듯
                    return StatusCode(500, "Idempotency log missing after unique violation.");
                }

                return Ok(new GiveUpStageResponse(log));
            }

            return Ok(new GiveUpStageResponse(log));
        }

        private async Task<StageGiveUpLog?> FindGiveUpLogAsync(ulong userId, string requestId)
        {
            return await _db.StageGiveUpLogs
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.RequestId == requestId)
                .FirstOrDefaultAsync();
        }

        private static bool IsUniqueViolation(DbUpdateException ex)
        {
            return ex.InnerException is MySqlConnector.MySqlException mysqlException && mysqlException.ErrorCode == MySqlConnector.MySqlErrorCode.DuplicateKeyEntry;
        }
    }
}
