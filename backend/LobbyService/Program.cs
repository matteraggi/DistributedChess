using DistributedChess.LobbyService.Game;
using LobbyService.Handlers;
using Shared.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ConnectionManager>();
builder.Services.AddSingleton<WebSocketHandler>();
builder.Services.AddSingleton<LobbyManager>();
builder.Services.AddSingleton<GameManager>();
builder.Services.AddSingleton<IMessageHandler, LobbyHandler>();
builder.Services.AddSingleton<IMessageHandler, CreateGameHandler>();
builder.Services.AddSingleton<IMessageHandler, JoinGameHandler>();
builder.Services.AddSingleton<IMessageHandler, LeaveGameHandler>();
builder.Services.AddSingleton<IMessageHandler, ReadyGameHandler>();

builder.Services.AddSingleton<MessageRouter>();
builder.Services.AddSingleton<WebSocketHandler>();

builder.Services.AddSingleton(new RedisService("localhost:6379"));


var app = builder.Build();

app.UseWebSockets();

app.Map("/ws", async (HttpContext ctx, WebSocketHandler handler) =>
{
    if (ctx.WebSockets.IsWebSocketRequest)
        await handler.HandleAsync(ctx);
    else
        ctx.Response.StatusCode = 400;
});

app.Run();
