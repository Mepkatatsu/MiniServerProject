using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MiniServerProject.Infrastructure.Persistence
{
    public sealed class GameDbContextFactory : IDesignTimeDbContextFactory<GameDbContext>
    {
        public GameDbContext CreateDbContext(string[] args)
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

            var cs = configuration.GetConnectionString("GameDb") ?? throw new InvalidOperationException("Connection string 'GameDb' not found.");

            var optionsBuilder = new DbContextOptionsBuilder<GameDbContext>();
            optionsBuilder.UseMySql(cs, ServerVersion.AutoDetect(cs));

            return new GameDbContext(optionsBuilder.Options);
        }
    }
}
