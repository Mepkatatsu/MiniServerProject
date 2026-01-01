using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MiniServerProject.Application;
using MiniServerProject.Application.Stages;
using MiniServerProject.Application.Users;
using MiniServerProject.Domain.Entities;
using MiniServerProject.Infrastructure;
using MiniServerProject.Infrastructure.Persistence;
using MiniServerProject.Shared.Tables;
using MiniServerProject.Tests.TestHelpers;

namespace MiniServerProject.Tests.Test
{
    public class StageServiceTests
    {
        private static StageService CreateService(GameDbContext db, IIdempotencyCache? cache = null)
        {
            cache ??= new FakeIdempotencyCache();
            var logger = NullLogger<StageService>.Instance;

            return new StageService(
                db,
                cache,
                logger
            );
        }

        private static async Task<ulong> SeedUserAsync(GameDbContext db, string accountId = "stage-test-account", string nickname = "stage-test-nickname")
        {
            var userService = new UserService(db, new FakeIdempotencyCache(), NullLogger<UserService>.Instance);
            var created = await userService.CreateAsync(accountId, nickname, CancellationToken.None);
            return created.UserId;
        }

        [Fact]
        public async Task EnterStage_ShouldSetCurrentStage_AndConsumeStamina()
        {
            var (db, cleanUp) = await TestDbFactory.CreateMySqlDbAsync();

            try
            {
                var service = CreateService(db);

                var userId = await SeedUserAsync(db);
                var stageDataKeyValue = TableHolder.GetTable<StageTable>().GetIdFirstOrDefault();
                var stageId = stageDataKeyValue.Key;
                var stageData = stageDataKeyValue.Value;

                var requestId = "req-enter-001";

                Assert.NotEmpty(stageId);
                Assert.NotNull(stageData);

                var user = await FindUserAsNoTracking(db, userId);
                Assert.NotNull(user);

                var expectedStamina = user.Stamina - stageData.NeedStamina;

                // Act
                var result = await service.EnterAsync(userId, requestId, stageId, CancellationToken.None);

                // Assert
                Assert.Equal(requestId, result.RequestId);
                Assert.Equal(stageId, result.StageId);
                Assert.Equal(stageData.NeedStamina, result.ConsumedStamina);

                user = await FindUserAsNoTracking(db, userId);
                Assert.NotNull(user);
                Assert.Equal(stageId, user.CurrentStageId);
                Assert.Equal(expectedStamina, user.Stamina);

                Assert.Equal(1, db.StageEnterLogs.Count(x => x.UserId == userId && x.RequestId == requestId));
            }
            finally
            {
                await cleanUp();
            }
        }

        [Fact]
        public async Task EnterStage_WithSameRequestId_ShouldReturnSameResponse_AndNotDuplicatedLogs()
        {
            var (db, cleanUp) = await TestDbFactory.CreateMySqlDbAsync();
            try
            {
                var cache = new MemoryIdempotencyCache();
                var service = CreateService(db, cache);

                var userId = await SeedUserAsync(db);
                var stageDataKeyValue = TableHolder.GetTable<StageTable>().GetIdFirstOrDefault();
                var stageId = stageDataKeyValue.Key;

                var requestId = "req-enter-dup-001";

                Assert.NotEmpty(stageId);

                // Act
                var first = await service.EnterAsync(userId, requestId, stageId, CancellationToken.None);
                var second = await service.EnterAsync(userId, requestId, stageId, CancellationToken.None);

                // Assert
                Assert.Equal(first.RequestId, second.RequestId);
                Assert.Equal(first.StageId, second.StageId);
                Assert.Equal(first.ConsumedStamina, second.ConsumedStamina);
                Assert.Equal(first.AfterStamina, second.AfterStamina);

                Assert.Equal(1, db.StageEnterLogs.Count(x => x.UserId == userId && x.RequestId == requestId));
            }
            finally
            {
                await cleanUp();
            }
        }

        [Fact]
        public async Task ClearStage_AfterEnter_ShouldRewardAndExitStage()
        {
            var (db, cleanUp) = await TestDbFactory.CreateMySqlDbAsync();

            try
            {
                var service = CreateService(db);

                var userId = await SeedUserAsync(db);
                var stageDataKeyValue = TableHolder.GetTable<StageTable>().GetIdFirstOrDefault();
                var stageId = stageDataKeyValue.Key;
                var stageData = stageDataKeyValue.Value;

                var enterRequestId = "req-enter-001";
                var clearRequestId = "req-clear-001";

                Assert.NotEmpty(stageId);
                Assert.NotNull(stageData);

                var rewardData = TableHolder.GetTable<RewardTable>().Get(stageData.RewardId);
                Assert.NotNull(rewardData);

                var user = await FindUserAsNoTracking(db, userId);
                Assert.NotNull(user);

                var expectedGold = user.Gold + rewardData.Gold;
                var expectedExp = user.Exp + rewardData.Exp;

                // Act
                await service.EnterAsync(userId, enterRequestId, stageId, CancellationToken.None);
                var result = await service.ClearAsync(userId, clearRequestId, stageId, CancellationToken.None);

                // Assert
                user = await FindUserAsNoTracking(db, userId);
                Assert.NotNull(user);
                Assert.Null(user.CurrentStageId);
                Assert.Equal(expectedGold, user.Gold);
                Assert.Equal(expectedExp, user.Exp);

                Assert.Equal(1, db.StageClearLogs.Count(x => x.UserId == userId && x.StageId == stageId));
            }
            finally
            {
                await cleanUp();
            }
        }

        [Fact]
        public async Task ClearStage_WithSameRequestId_ShouldReturnSameResponse_AndNotDuplicateLogs()
        {
            var (db, cleanUp) = await TestDbFactory.CreateMySqlDbAsync();

            try
            {
                var cache = new MemoryIdempotencyCache();
                var service = CreateService(db, cache);

                var userId = await SeedUserAsync(db);
                var stageDataKeyValue = TableHolder.GetTable<StageTable>().GetIdFirstOrDefault();
                var stageId = stageDataKeyValue.Key;
                var stageData = stageDataKeyValue.Value;

                var requestId = "req-clear-002";

                Assert.NotEmpty(stageId);
                Assert.NotNull(stageData);

                // Act
                await service.EnterAsync(userId, requestId, stageId, CancellationToken.None);

                var first = await service.ClearAsync(userId, requestId, stageId, CancellationToken.None);
                var second = await service.ClearAsync(userId, requestId, stageId, CancellationToken.None);

                // Assert
                Assert.Equal(first.RequestId, second.RequestId);
                Assert.Equal(first.StageId, second.StageId);
                Assert.Equal(first.RewardId, second.RewardId);
                Assert.Equal(first.GainGold, second.GainGold);
                Assert.Equal(first.GainExp, second.GainExp);
                Assert.Equal(first.AfterGold, second.AfterGold);
                Assert.Equal(first.AfterExp, second.AfterExp);

                Assert.Equal(1, db.StageClearLogs.Count(x => x.UserId == userId && x.RequestId == requestId));
            }
            finally
            {
                await cleanUp();
            }
        }

        [Fact]
        public async Task GiveUpStage_AfterEnter_ShouldRefundStamina_AndExitStage()
        {
            var (db, cleanUp) = await TestDbFactory.CreateMySqlDbAsync();

            try
            {
                var service = CreateService(db);

                var userId = await SeedUserAsync(db);

                var stageDataKeyValue = TableHolder.GetTable<StageTable>().GetIdFirstOrDefault();
                var stageId = stageDataKeyValue.Key;
                var stageData = stageDataKeyValue.Value;

                Assert.NotEmpty(stageId);
                Assert.NotNull(stageData);

                var user = await FindUserAsNoTracking(db, userId);
                Assert.NotNull(user);

                var beforeStamina = user.Stamina;

                // Act: Enter 먼저 해서 스태미너 소비 + 스테이지 입장 상태 만들기
                var enterRequestId = "req-enter-for-giveup-001";
                await service.EnterAsync(userId, enterRequestId, stageId, CancellationToken.None);

                user = await FindUserAsNoTracking(db, userId);
                Assert.NotNull(user);

                // Assert
                var afterEnterStamina = user.Stamina;
                Assert.Equal(beforeStamina - stageData.NeedStamina, afterEnterStamina);
                Assert.Equal(stageId, user.CurrentStageId);

                // GiveUp 시 반환 스태미너 계산
                var expectedRefund = TableHolder.GetTable<GameParameters>().GetRefundStamina(stageData.NeedStamina);
                var expectedStamina = afterEnterStamina + expectedRefund;

                // Act
                var giveUpRequestId = "req-giveup-001";
                var result = await service.GiveUpAsync(userId, giveUpRequestId, stageId, CancellationToken.None);

                // Assert
                Assert.Equal(giveUpRequestId, result.RequestId);
                Assert.Equal(stageId, result.StageId);
                Assert.Equal(expectedRefund, result.RefundStamina);
                Assert.Equal(expectedStamina, result.AfterStamina);

                user = await FindUserAsNoTracking(db, userId);
                Assert.NotNull(user);
                Assert.Null(user.CurrentStageId);
                Assert.Equal(expectedStamina, user.Stamina);

                Assert.Equal(1, db.StageGiveUpLogs.Count(x => x.UserId == userId && x.RequestId == giveUpRequestId));
            }
            finally
            {
                await cleanUp();
            }
        }

        [Fact]
        public async Task GiveUpStage_WithSameRequestId_ShouldReturnSameResponse_AndNotDuplicateLogs()
        {
            var (db, cleanUp) = await TestDbFactory.CreateMySqlDbAsync();

            try
            {
                var cache = new MemoryIdempotencyCache();
                var service = CreateService(db, cache);

                var userId = await SeedUserAsync(db);

                var stageDataKeyValue = TableHolder.GetTable<StageTable>().GetIdFirstOrDefault();
                var stageId = stageDataKeyValue.Key;

                Assert.NotEmpty(stageId);

                // Act
                var enterRequestId = "req-enter-for-giveup-dup-001";
                await service.EnterAsync(userId, enterRequestId, stageId, CancellationToken.None);

                var giveUpRequestId = "req-giveup-dup-001";
                var first = await service.GiveUpAsync(userId, giveUpRequestId, stageId, CancellationToken.None);
                var second = await service.GiveUpAsync(userId, giveUpRequestId, stageId, CancellationToken.None);

                // Assert
                Assert.Equal(first.RequestId, second.RequestId);
                Assert.Equal(first.StageId, second.StageId);
                Assert.Equal(first.RefundStamina, second.RefundStamina);
                Assert.Equal(first.AfterStamina, second.AfterStamina);

                Assert.Equal(1, db.StageGiveUpLogs.Count(x => x.UserId == userId && x.RequestId == giveUpRequestId));
            }
            finally
            {
                await cleanUp();
            }
        }

        [Fact]
        public async Task EnterStage_WhenNotEnoughStamina_ShouldThrow()
        {
            var (db, cleanUp) = await TestDbFactory.CreateMySqlDbAsync();
            try
            {
                var service = CreateService(db);

                var userId = await SeedUserAsync(db, "acc-low-sta", "low-sta");
                var user = await FindUser(db, userId);

                // Act: 스태미너를 일부러 부족하게 만들기
                Assert.NotNull(user);
                user.ConsumeStamina(user.Stamina, DateTime.UtcNow);
                await db.SaveChangesAsync();

                var stageDataKeyValue = TableHolder.GetTable<StageTable>().GetIdFirstOrDefault();
                var stageId = stageDataKeyValue.Key;
                var requestId = "req-enter-fail-001";

                Assert.NotEmpty(stageId);

                var ex = await Assert.ThrowsAsync<DomainException>(() =>
                    service.EnterAsync(userId, requestId, stageId, CancellationToken.None));

                // Assert
                Assert.Equal(ErrorType.NotEnoughStamina, ex.ErrorType);
            }
            finally
            {
                await cleanUp();
            }
        }

        [Fact]
        public async Task ClearStage_WithoutEnter_ShouldThrow()
        {
            var (db, cleanUp) = await TestDbFactory.CreateMySqlDbAsync();
            try
            {
                var service = CreateService(db);

                var userId = await SeedUserAsync(db);

                var stageDataKeyValue = TableHolder.GetTable<StageTable>().GetIdFirstOrDefault();
                var stageId = stageDataKeyValue.Key;
                var requestId = "req-clear-fail-001";

                Assert.NotEmpty(stageId);

                // Act
                var ex = await Assert.ThrowsAsync<DomainException>(() =>
                    service.ClearAsync(userId, requestId, stageId, CancellationToken.None));

                // Assert
                Assert.Equal(ErrorType.UserNotInThisStage, ex.ErrorType);
            }
            finally
            {
                await cleanUp();
            }            
        }

        [Fact]
        public async Task GiveUpStage_WithoutEnter_ShouldThrow()
        {
            var (db, cleanUp) = await TestDbFactory.CreateMySqlDbAsync();

            try
            {
                var service = CreateService(db);

                var userId = await SeedUserAsync(db);

                var stageDataKeyValue = TableHolder.GetTable<StageTable>().GetIdFirstOrDefault();
                var stageId = stageDataKeyValue.Key;
                var requestId = "req-giveup-fail-001";

                Assert.NotEmpty(stageId);

                // Act
                var ex = await Assert.ThrowsAsync<DomainException>(() =>
                    service.GiveUpAsync(userId, requestId, stageId, CancellationToken.None));

                // Assert
                Assert.Equal(ErrorType.UserNotInThisStage, ex.ErrorType);
            }
            finally
            {
                await cleanUp();
            }
        }

        [Fact]
        public async Task GiveUpStage_WhenStageNotFound_ShouldSucceess()
        {
            var (db, cleanUp) = await TestDbFactory.CreateMySqlDbAsync();

            try
            {
                var service = CreateService(db);

                var userId = await SeedUserAsync(db);

                // 존재하지 않는 stageId를 일부러 사용
                var stageId = "stage-does-not-exist";
                var requestId = "req-giveup-stage-missing-001";

                var user = await FindUser(db, userId);

                // Act: 유저를 해당 stageId에 입장한 상태로 만듬
                Assert.NotNull(user);
                user.SetCurrentStage(stageId);
                await db.SaveChangesAsync();

                var expectedRefund = TableHolder.GetTable<GameParameters>().GetRefundStamina(0);

                var response = await service.GiveUpAsync(userId, requestId, stageId, CancellationToken.None);

                // Assert
                Assert.Equal(expectedRefund, response.RefundStamina);
                Assert.Equal(1, db.StageGiveUpLogs.Count(x => x.UserId == userId && x.RequestId == requestId));
            }
            finally
            {
                await cleanUp();
            }
        }

        private async Task<User?> FindUser(GameDbContext db, ulong userId)
        {
            return await db.Users.SingleAsync(u => u.UserId == userId);
        }

        private async Task<User?> FindUserAsNoTracking(GameDbContext db, ulong userId)
        {
            return await db.Users.AsNoTracking().SingleAsync(u => u.UserId == userId);
        }
    }
}
