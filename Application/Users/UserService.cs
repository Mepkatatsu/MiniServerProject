using Microsoft.EntityFrameworkCore;
using MiniServerProject.Controllers.Response;
using MiniServerProject.Domain.Entities;
using MiniServerProject.Domain.ServerLogs;
using MiniServerProject.Infrastructure.Persistence;
using MiniServerProject.Infrastructure.Redis;

namespace MiniServerProject.Application.Users
{
    public class UserService : IUserService
    {
        private readonly GameDbContext _db;
        private readonly IdempotencyCache _idemCache;
        private readonly ILogger<UserService> _logger;

        public UserService(GameDbContext db, IdempotencyCache idemCache, ILogger<UserService> logger)
        {
            _db = db;
            _idemCache = idemCache;
            _logger = logger;
        }

        public async Task<UserResponse> CreateAsync(string accountId, string nickname, CancellationToken ct)
        {
            accountId = accountId.Trim();
            nickname = nickname.Trim();

            // 1) Redis 캐시 조회
            var cacheKey = $"idem:users:{accountId}";
            var response = await _idemCache.GetAsync<UserResponse>(cacheKey);
            if (response != null)
            {
                _logger.LogInformation(
                    "Idempotent response served from Redis. accountId={AccountId} cacheKey={CacheKey}",
                    accountId, cacheKey);
                return response;
            }

            // 2) DB user 선조회
            var user = await FindUserAsync(accountId, ct);
            if (user != null)
            {
                response = new UserResponse(user);
                await _idemCache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));
                return response;
            }

            // 3) 실제 처리
            user = new User(accountId, nickname);

            try
            {
                var now = DateTime.UtcNow;

                _db.Users.Add(user);

                var log = new UserCreateLog(accountId, user, nickname, now);
                _db.UserCreateLogs.Add(log);

                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                // 이미 처리된 요청
                user = await FindUserAsync(accountId, ct);
                if (user == null)
                {
                    _logger.LogError(
                        ex,
                        "user missing after unique violation. accountId={AccountId}",
                        accountId);

                    throw new DomainException(ErrorType.IdempotencyMissingAfterUniqueViolation);
                }

                response = new UserResponse(user);
                await _idemCache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));
                return response;
            }

            response = new UserResponse(user);
            await _idemCache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));
            return response;
        }

        public async Task<UserResponse> GetAsync(ulong userId, CancellationToken ct)
        {
            var user = await FindUserAsync(userId, ct)
                ?? throw new DomainException(ErrorType.UserNotFound);

            var response = new UserResponse(user);
            return response;
        }

        private async Task<User?> FindUserAsync(string accountId, CancellationToken ct)
        {
            return await _db.Users
                .AsNoTracking()
                .Where(x => x.AccountId == accountId)
                .FirstOrDefaultAsync(ct);
        }

        private async Task<User?> FindUserAsync(ulong userId, CancellationToken ct)
        {
            return await _db.Users
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .FirstOrDefaultAsync(ct);
        }

        private static bool IsUniqueViolation(DbUpdateException ex)
        {
            return ex.InnerException is MySqlConnector.MySqlException mysqlException
                && mysqlException.ErrorCode == MySqlConnector.MySqlErrorCode.DuplicateKeyEntry;
        }
    }
}
