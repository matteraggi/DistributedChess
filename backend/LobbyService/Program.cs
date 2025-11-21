var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ConnectionManager>();
builder.Services.AddSingleton<WebSocketHandler>();

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
