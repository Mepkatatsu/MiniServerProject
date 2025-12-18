using Microsoft.EntityFrameworkCore;
using MiniServerProject.Infrastructure.Persistence;

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
