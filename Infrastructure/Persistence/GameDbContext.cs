using Microsoft.EntityFrameworkCore;
using MiniServerProject.Domain.Entities;
using MiniServerProject.Domain.ServerLogs;

namespace MiniServerProject.Infrastructure.Persistence
{
    public sealed class GameDbContext : DbContext
    {
        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<UserCreateLog> UserCreateLogs => Set<UserCreateLog>();
        public DbSet<StageEnterLog> StageEnterLogs => Set<StageEnterLog>();
        public DbSet<StageClearLog> StageClearLogs => Set<StageClearLog>();
        public DbSet<StageGiveUpLog> StageGiveUpLogs => Set<StageGiveUpLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(GameDbContext).Assembly);
        }
    }
}
