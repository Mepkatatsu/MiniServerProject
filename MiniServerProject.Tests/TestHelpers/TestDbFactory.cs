using Microsoft.EntityFrameworkCore;
using MiniServerProject.Infrastructure.Persistence;

namespace MiniServerProject.Tests.TestHelpers
{
    public static class TestDbFactory
    {
        public static GameDbContext CreateInMemoryDb(string dbName)
        {
            var options = new DbContextOptionsBuilder<GameDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .EnableSensitiveDataLogging()
                .Options;

            var db = new GameDbContext(options);
            db.Database.EnsureCreated();
            return db;
        }
    }

}
