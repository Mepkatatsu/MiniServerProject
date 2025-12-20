using Microsoft.EntityFrameworkCore;
using MiniServerProject.Infrastructure.Persistence;
using MiniServerProject.Infrastructure.Redis;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);

// DbContext 등록
var cs = builder.Configuration.GetConnectionString("GameDb") ?? throw new InvalidOperationException("Connection string 'GameDb' not found.");
builder.Services.AddDbContext<GameDbContext>(options =>
{
    options.UseMySql(cs, ServerVersion.AutoDetect(cs));
});

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    var opt = ConfigurationOptions.Parse("localhost:6379");
    opt.AbortOnConnectFail = false;
    opt.ConnectTimeout = 200;
    opt.SyncTimeout = 200;
    opt.AsyncTimeout = 200;

    return ConnectionMultiplexer.Connect(opt);
});

builder.Services.AddSingleton<IdempotencyCache>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
