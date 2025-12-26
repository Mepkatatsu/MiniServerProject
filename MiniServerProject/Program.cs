using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MiniServerProject.Api.Middleware;
using MiniServerProject.Application.Stages;
using MiniServerProject.Application.Users;
using MiniServerProject.Infrastructure;
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

builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection("Redis"));

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var opt = sp.GetRequiredService<IOptions<RedisOptions>>().Value;

    var config = ConfigurationOptions.Parse(opt.ConnectionString);
    config.AbortOnConnectFail = opt.AbortOnConnectFail;
    config.ConnectTimeout = opt.ConnectTimeout;
    config.SyncTimeout = opt.SyncTimeout;
    config.AsyncTimeout = opt.AsyncTimeout;

    return ConnectionMultiplexer.Connect(config);
});

builder.Services.AddSingleton<IIdempotencyCache, RedisCache>();
builder.Services.AddScoped<IStageService, StageService>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
