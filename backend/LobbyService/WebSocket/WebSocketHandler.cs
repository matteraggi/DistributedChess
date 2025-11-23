using Shared.Messages;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

public class WebSocketHandler
{
    private readonly MessageRouter _router;
    private readonly ConnectionManager _connections;

    public WebSocketHandler(MessageRouter router, ConnectionManager connections)
    {
        _router = router;
        _connections = connections;
    }

    public async Task HandleAsync(HttpContext context)
    {
        var socket = await context.WebSockets.AcceptWebSocketAsync();
        var socketId = Guid.NewGuid().ToString();

        _connections.AddSocket(socketId, socket);

        await Listen(socketId, socket);
    }

    private async Task Listen(string socketId, WebSocket socket)
    {
        var buffer = new byte[4096];

        try
        {
            while (socket.State == WebSocketState.Open)
            {
                var res = await socket.ReceiveAsync(buffer, CancellationToken.None);
                if (res.CloseStatus.HasValue)
                    break;

                var raw = Encoding.UTF8.GetString(buffer, 0, res.Count);
                var baseMsg = JsonSerializer.Deserialize<BaseMessage>(raw);
                if (baseMsg != null)
                    await _router.RouteAsync(socketId, socket, baseMsg, raw);
            }
        }
        finally
        {
            await _connections.RemoveSocketAsync(socketId);
        }
    }

}
