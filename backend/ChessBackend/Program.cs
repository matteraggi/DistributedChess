using ChessBackend.Hubs;
using ChessBackend.Manager;
using Shared.Redis;
using StackExchange.Redis;
using GameEngine;
using ChessBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// redis connection string
string redisConn = builder.Configuration.GetConnectionString("Redis")
                   ?? "localhost:6379";

// logica
builder.Services.AddSingleton<LobbyManager>();
builder.Services.AddSingleton<GameManager>();
builder.Services.AddSingleton(new RedisService(redisConn));
builder.Services.AddSingleton<ChessLogic>();
builder.Services.AddHostedService<ProposalTimeoutService>();


// signalR con backplane Redis
builder.Services.AddSignalR()
    .AddStackExchangeRedis(redisConn, options => {
        options.Configuration.ChannelPrefix = RedisChannel.Literal("DistributedChess");
    });

// cors policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://localhost:4201") // Docker & Locale
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// middleware
app.UseCors("AllowFrontend");

app.MapHub<GameHub>("/ws");

app.Run();