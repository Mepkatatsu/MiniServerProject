using Microsoft.EntityFrameworkCore;
using MiniServerProject.Shared.Responses;
using MiniServerProject.Domain.ServerLogs;
using MiniServerProject.Infrastructure;
using MiniServerProject.Infrastructure.Persistence;
using MiniServerProject.Shared.Tables;

namespace MiniServerProject.Application.Stages
{
    public sealed class StageService : IStageService
    {
        private readonly GameDbContext _db;
        private readonly IIdempotencyCache _idemCache;
        private readonly ILogger<StageService> _logger;

        public StageService(GameDbContext db, IIdempotencyCache idemCache, ILogger<StageService> logger)
        {
            _db = db;
            _idemCache = idemCache;
            _logger = logger;
        }

        public async Task<EnterStageResponse> EnterAsync(ulong userId, string requestId, string stageId, CancellationToken ct, int testDelayMs = 0)
        {
            // 1) Redis 캐시 조회
            var cacheKey = IdempotencyKeyFactory.StageEnter(userId, stageId, requestId);
            var response = await _idemCache.GetAsync<EnterStageResponse>(cacheKey);
            if (response != null)
            {
                _logger.LogInformation(
                    "Idempotent response served from Redis. userId={UserId} requestId={RequestId} stageId={StageId} cacheKey={CacheKey}",
                    userId, requestId, stageId, cacheKey);
                return response;
            }

            // 2) DB log 선조회
            var log = await FindEnterLogAsync(userId, requestId, ct);
            if (log != null)
            {
                if (log.StageId != stageId)
                    throw new DomainException(ErrorType.RequestIdUsedForDifferentStage);

                response = log.CreateResponse();
                await _idemCache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));
                return response;
            }

            var user = await _db.Users.FirstOrDefaultAsync(x => x.UserId == userId, ct)
                        ?? throw new DomainException(ErrorType.UserNotFound);

            if (user.CurrentStageId != null)
                throw new DomainException(ErrorType.UserAlreadyInStage);

            var stageData = TableHolder.GetTable<StageTable>().Get(stageId)
                            ?? throw new DomainException(ErrorType.StageNotFound);

            // Race Condition 테스트용 Delay
            if (testDelayMs > 0)
                await Task.Delay(testDelayMs);

            // 3) 실제 처리
            var now = DateTime.UtcNow;
            if (!user.ConsumeStamina(stageData.NeedStamina, now))
                throw new DomainException(ErrorType.NotEnoughStamina, new { current = user.Stamina, required = stageData.NeedStamina });

            var afterStamina = user.Stamina;

            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            var affected = await _db.Users
                                    .Where(u => u.UserId == userId && u.CurrentStageId == null)
                                    .ExecuteUpdateAsync(setters =>
                                        setters.SetProperty(u => u.CurrentStageId, stageId)
                                               .SetProperty(u => u.Stamina, afterStamina)
                                               .SetProperty(u => u.LastStaminaUpdateTime, user.LastStaminaUpdateTime)
                                    , ct);

            if (affected == 0)
                throw new DomainException(ErrorType.UserAlreadyInStage);

            try
            {
                // 멱등성 보장을 위해 로그 INSERT
                log = new StageEnterLog(user.UserId, stageId, requestId, stageData.NeedStamina, afterStamina, now);
                _db.StageEnterLogs.Add(log);

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                // 이미 처리된 요청
                log = await FindEnterLogAsync(userId, requestId, ct);
                if (log == null)
                {
                    _logger.LogError(
                        ex,
                        "Idempotency log missing after unique violation. userId={UserId} requestId={RequestId} stageId={StageId}",
                        userId, requestId, stageId);

                    throw new DomainException(ErrorType.IdempotencyMissingAfterUniqueViolation);
                }

                var resp = log.CreateResponse();
                await _idemCache.SetAsync(cacheKey, resp, TimeSpan.FromMinutes(10));
                return resp;
            }

            response = log.CreateResponse();
            await _idemCache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));
            return response;
        }

        public async Task<ClearStageResponse> ClearAsync(ulong userId, string requestId, string stageId, CancellationToken ct, int testDelayMs = 0)
        {
            // 1) Redis 캐시 조회
            var cacheKey = IdempotencyKeyFactory.StageClear(userId, stageId, requestId);
            var response = await _idemCache.GetAsync<ClearStageResponse>(cacheKey);
            if (response != null)
            {
                _logger.LogInformation("Idempotent response served from Redis. userId={userId}, requestId={request.RequestId}, cacheKey={cacheKey}",
                    userId, requestId, cacheKey);
                return response;
            }

            // 2) DB log 선조회
            var log = await FindClearLogAsync(userId, requestId, ct);
            if (log != null)
            {
                if (log.StageId != stageId)
                    throw new DomainException(ErrorType.RequestIdUsedForDifferentStage);

                response = log.CreateResponse();
                await _idemCache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));
                return response;
            }

            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId, ct)
                        ?? throw new DomainException(ErrorType.UserNotFound);

            if (user.CurrentStageId != stageId)
                throw new DomainException(ErrorType.UserNotInThisStage, new { current = user.CurrentStageId });

            var stageData = TableHolder.GetTable<StageTable>().Get(stageId)
                            ?? throw new DomainException(ErrorType.StageNotFound);

            var reward = TableHolder.GetTable<RewardTable>().Get(stageData.RewardId)
                            ?? throw new DomainException(ErrorType.RewardNotFound);

            // Race Condition 테스트용 Delay
            if (testDelayMs > 0)
                await Task.Delay(testDelayMs);

            // 3) 실제 처리
            var afterGold = user.Gold + reward.Gold;
            var afterExp = user.Exp + reward.Exp;

            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            var affected = await _db.Users
                                    .Where(u => u.UserId == userId && u.CurrentStageId == stageId)
                                    .ExecuteUpdateAsync(setters =>
                                        setters.SetProperty(u => u.Gold, afterGold)
                                               .SetProperty(u => u.Exp, afterExp)
                                               .SetProperty(u => u.CurrentStageId, (string?)null)
                                    , ct);

            if (affected == 0)
                throw new DomainException(ErrorType.UserNotInThisStage);

            try
            {
                // 응답 멱등성 보장을 위해 로그 INSERT
                var now = DateTime.UtcNow;
                log = new StageClearLog(user.UserId, stageId, requestId, stageData.RewardId, reward.Gold, reward.Exp, afterGold, afterExp, now);
                _db.StageClearLogs.Add(log);

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                log = await FindClearLogAsync(userId, requestId, ct);
                if (log == null)
                {
                    _logger.LogError(
                        ex,
                        "Idempotency log missing after unique violation. userId={UserId} requestId={RequestId} stageId={StageId}",
                        userId, requestId, stageId);

                    throw new DomainException(ErrorType.IdempotencyMissingAfterUniqueViolation);
                }

                response = log.CreateResponse();
                await _idemCache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));
                return response;
            }

            response = log.CreateResponse();
            await _idemCache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));
            return response;
        }

        public async Task<GiveUpStageResponse> GiveUpAsync(ulong userId, string requestId, string stageId, CancellationToken ct, int testDelayMs = 0)
        {
            // 1) Redis 캐시 조회
            var cacheKey = IdempotencyKeyFactory.StageGiveUp(userId, stageId, requestId);
            var response = await _idemCache.GetAsync<GiveUpStageResponse>(cacheKey);
            if (response != null)
            {
                _logger.LogInformation("Idempotent response served from Redis. userId={userId}, requestId={request.RequestId}, cacheKey={cacheKey}",
                    userId, requestId, cacheKey);
                return response;
            }

            // 2) DB log 선조회
            var log = await FindGiveUpLogAsync(userId, requestId, ct);
            if (log != null)
            {
                if (log.StageId != stageId)
                    throw new DomainException(ErrorType.RequestIdUsedForDifferentStage);

                response = log.CreateResponse();
                await _idemCache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));
                return response;
            }

            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId, ct)
                        ?? throw new DomainException(ErrorType.UserNotFound);

            if (user.CurrentStageId != stageId)
                throw new DomainException(ErrorType.UserNotInThisStage, new { current = user.CurrentStageId });

            // stageData가 삭제된 경우에도 포기는 할 수 있도록 NotFound 처리 X
            var stage = TableHolder.GetTable<StageTable>().Get(stageId);
            ushort consumedStamina = stage?.NeedStamina ?? 0;
            ushort refundStamina = TableHolder.GetTable<GameParameters>().GetRefundStamina(consumedStamina);

            // Race Condition 테스트용 Delay
            if (testDelayMs > 0)
                await Task.Delay(testDelayMs);

            // 3) 실제 처리
            var now = DateTime.UtcNow;
            user.AddStamina(refundStamina, now);

            var afterStamina = user.Stamina;

            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            var affected = await _db.Users
                                    .Where(u => u.UserId == userId && u.CurrentStageId == stageId)
                                    .ExecuteUpdateAsync(setters =>
                                        setters.SetProperty(u => u.CurrentStageId, (string?)null)
                                               .SetProperty(u => u.Stamina, afterStamina)
                                               .SetProperty(u => u.LastStaminaUpdateTime, user.LastStaminaUpdateTime)
                                    , ct);

            if (affected == 0)
                throw new DomainException(ErrorType.UserNotInThisStage);

            try
            {
                // 멱등성 보장을 위해 로그 INSERT
                log = new StageGiveUpLog(user.UserId, stageId, requestId, refundStamina, afterStamina, now);
                _db.StageGiveUpLogs.Add(log);

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                // 이미 처리된 요청
                log = await FindGiveUpLogAsync(userId, requestId, ct);
                if (log == null)
                {
                    _logger.LogError(
                        ex,
                        "Idempotency log missing after unique violation. userId={UserId} requestId={RequestId} stageId={StageId}",
                        userId, requestId, stageId);

                    throw new DomainException(ErrorType.IdempotencyMissingAfterUniqueViolation);
                }

                response = log.CreateResponse();
                await _idemCache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));
                return response;
            }

            response = log.CreateResponse();
            await _idemCache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));
            return response;
        }

        private async Task<StageEnterLog?> FindEnterLogAsync(ulong userId, string requestId, CancellationToken ct)
        {
            return await _db.StageEnterLogs
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.RequestId == requestId)
                .FirstOrDefaultAsync(ct);
        }

        private async Task<StageClearLog?> FindClearLogAsync(ulong userId, string requestId, CancellationToken ct)
        {
            return await _db.StageClearLogs
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.RequestId == requestId)
                .FirstOrDefaultAsync(ct);
        }

        private async Task<StageGiveUpLog?> FindGiveUpLogAsync(ulong userId, string requestId, CancellationToken ct)
        {
            return await _db.StageGiveUpLogs
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.RequestId == requestId)
                .FirstOrDefaultAsync(ct);
        }

        private static bool IsUniqueViolation(DbUpdateException ex)
        {
            return ex.InnerException is MySqlConnector.MySqlException mysqlException
                && mysqlException.ErrorCode == MySqlConnector.MySqlErrorCode.DuplicateKeyEntry;
        }
    }
}
