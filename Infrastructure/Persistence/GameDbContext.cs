using Microsoft.EntityFrameworkCore;
using MiniServerProject.Domain.Entities;

namespace MiniServerProject.Infrastructure.Persistence
{
    public sealed class GameDbContext : DbContext
    {
        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(GameDbContext).Assembly);
        }
    }
}
