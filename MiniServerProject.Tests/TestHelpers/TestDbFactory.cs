using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MiniServerProject.Infrastructure.Persistence;
using MySqlConnector;

namespace MiniServerProject.Tests.TestHelpers
{
    public static class TestDbFactory
    {
        public static async Task<(Func<GameDbContext> CreateDb, Func<Task> Cleanup)> CreateMySqlDbAsync()
        {
            var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                ?? "Development";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("TEST_MYSQL_CS") ?? throw new InvalidOperationException("Connection string 'TEST_MYSQL_CS' not found.");

            var dbName = $"test_{Guid.NewGuid():N}";

            // 1) DB 생성
            await using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();
                await new MySqlCommand($"CREATE DATABASE `{dbName}`;", connection)
                    .ExecuteNonQueryAsync();
            }

            // 2) DbContext 생성
            var cs = $"{connectionString};Database={dbName}";
            var serverVersion = ServerVersion.AutoDetect(cs);

            GameDbContext CreateDb()
            {
                var options = new DbContextOptionsBuilder<GameDbContext>()
                    .UseMySql(cs, serverVersion)
                    .EnableSensitiveDataLogging()
                    .Options;

                return new GameDbContext(options);
            }

            await using (var db = CreateDb())
            {
                await db.Database.MigrateAsync();
            }

            async Task Cleanup()
            {
                await using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();
                await new MySqlCommand($"DROP DATABASE IF EXISTS `{dbName}`;", connection)
                    .ExecuteNonQueryAsync();
            }

            return (CreateDb, Cleanup);
        }
    }
}
