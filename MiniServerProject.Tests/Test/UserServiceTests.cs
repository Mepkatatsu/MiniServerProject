using Microsoft.Extensions.Logging.Abstractions;
using MiniServerProject.Application;
using MiniServerProject.Application.Users;
using MiniServerProject.Controllers.Response;
using MiniServerProject.Infrastructure;
using MiniServerProject.Infrastructure.Persistence;
using MiniServerProject.Tests.TestHelpers;

namespace MiniServerProject.Tests.Users
{
    public class UserServiceTests
    {
        private static UserService CreateService(GameDbContext db, IIdempotencyCache? cache = null)
        {
            cache ??= new FakeIdempotencyCache();
            var logger = NullLogger<UserService>.Instance;

            return new UserService(
                db,
                cache,
                logger
            );
        }

        [Fact]
        public async Task CreateUser_ShouldCreateNewUser()
        {
            using var db = TestDbFactory.CreateInMemoryDb(nameof(CreateUser_ShouldCreateNewUser));
            var service = CreateService(db);

            var accountId = "test-create-001";
            var nickname = "create-001";

            // Act
            var result = await service.CreateAsync(accountId, nickname, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.UserId > 0);

            var user = await db.Users.FindAsync(result.UserId);
            Assert.NotNull(user);
            Assert.Equal(accountId, user!.AccountId);
        }

        [Fact]
        public async Task CreateUser_WithSameAccountId_ShouldReturnSameUser()
        {
            using var db = TestDbFactory.CreateInMemoryDb(nameof(CreateUser_WithSameAccountId_ShouldReturnSameUser));
            var service = CreateService(db);

            var accountId = "test-duplicate-001";
            var nickname1 = "duplicate-001";
            var nickname2 = "duplicate-002";

            // Act
            var first = await service.CreateAsync(accountId, nickname1, CancellationToken.None);
            var second = await service.CreateAsync(accountId, nickname2, CancellationToken.None);

            // Assert
            Assert.Equal(first.UserId, second.UserId);
            Assert.Equal(nickname1, first.Nickname);
            Assert.Equal(nickname1, second.Nickname);
            Assert.NotEqual(nickname2, second.Nickname);
            Assert.Equal(1, db.Users.Count(u => u.AccountId == accountId));
        }

        [Fact]
        public async Task CreateUser_WhenCacheHasResponse_ShouldNotAccessDb()
        {
            using var db = TestDbFactory.CreateInMemoryDb(nameof(CreateUser_WhenCacheHasResponse_ShouldNotAccessDb));
            var cache = new MemoryIdempotencyCache();
            var service = CreateService(db, cache);

            var testUserId = ulong.MaxValue;
            var accountId = "test-cache-preload-001";
            var nickname = "cache-preload";
            var cacheKey = IdempotencyKeyFactory.CreateUser(accountId);

            var response = new UserResponse(new Domain.Entities.User(accountId, nickname))
            {
                UserId = testUserId
            };

            // Act: 캐시에 이미 완료된 응답을 미리 심어둠
            await cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

            var result = await service.CreateAsync(accountId, nickname, CancellationToken.None);

            // Asset: DB를 안 탔음을 증명
            Assert.Equal(testUserId, result.UserId);
            Assert.Equal(0, db.Users.Count());
            Assert.Equal(0, db.UserCreateLogs.Count());
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CreateUser_WithInvalidAccountId_ShouldThrow(string accountId)
        {
            using var db = TestDbFactory.CreateInMemoryDb(nameof(CreateUser_WithInvalidAccountId_ShouldThrow));
            var service = CreateService(db);
            var nickname = "invalid-001";

            // Act
            var exception = await Assert.ThrowsAsync<DomainException>(() =>
                service.CreateAsync(accountId, nickname, CancellationToken.None));

            // Assert
            Assert.Equal(ErrorType.InvalidRequest, exception.ErrorType);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CreateUser_WithInvalidNickname_ShouldThrow(string nickname)
        {
            using var db = TestDbFactory.CreateInMemoryDb(nameof(CreateUser_WithInvalidNickname_ShouldThrow));
            var service = CreateService(db);
            var accountId = "test-invalid-002";

            // Act
            var exception = await Assert.ThrowsAsync<DomainException>(() =>
                service.CreateAsync(accountId, nickname, CancellationToken.None));

            // Assert
            Assert.Equal(ErrorType.InvalidRequest, exception.ErrorType);
        }

        [Fact]
        public async Task GetUser_WhenNotExists_ShouldThrowNotFound()
        {
            // Arrange
            using var db = TestDbFactory.CreateInMemoryDb(nameof(GetUser_WhenNotExists_ShouldThrowNotFound));
            var service = CreateService(db);

            // Act
            var exception = await Assert.ThrowsAsync<DomainException>(async () =>
            {
                await service.GetAsync(ulong.MaxValue, CancellationToken.None);
            });

            // Assert
            Assert.Equal(ErrorType.UserNotFound, exception.ErrorType);
        }
    }
}
