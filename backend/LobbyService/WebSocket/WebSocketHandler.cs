using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Shared.Messages;



public class WebSocketHandler
{
    private readonly ConnectionManager _manager;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public WebSocketHandler(ConnectionManager manager)
    {
        _manager = manager;
    }

    public async Task HandleAsync(HttpContext context)
    {
        var socket = await context.WebSockets.AcceptWebSocketAsync();

        var id = Guid.NewGuid().ToString();
        _manager.AddSocket(id, socket);

        await Listen(id, socket);
    }

    private async Task Listen(string id, WebSocket socket)
    {
        var buffer = new byte[2048];

        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(buffer, CancellationToken.None);
            if (result.CloseStatus.HasValue)
                break;

            var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
            Console.WriteLine($"Received raw: {json}");

            BaseMessage? baseMsg;
            try
            {
                baseMsg = JsonSerializer.Deserialize<BaseMessage>(json, JsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Deserialization error: {ex.Message}");
                await SendError(socket, "Invalid JSON");
                continue;
            }

            if (baseMsg is null)
            {
                await SendError(socket, "Invalid message format");
                continue;
            }

            switch (baseMsg.Type)
            {
                case MessageType.Hello:
                    {
                        var hello = JsonSerializer.Deserialize<HelloMessage>(json, JsonOptions);
                        if (hello is null)
                        {
                            await SendError(socket, "Invalid Hello message");
                            break;
                        }

                        await HandleHello(socket, hello);
                        break;
                    }

                default:
                    await SendError(socket, $"Unknown message type: {baseMsg.Type}");
                    break;
            }
        }
    }

    private Task SendJson(WebSocket socket, object obj)
    {
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(obj));
        return socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private Task SendError(WebSocket socket, string error)
    {
        var msg = new ErrorMessage
        {
            Error = error
        };

        return SendJson(socket, msg);
    }

    private Task HandleHello(WebSocket socket, HelloMessage hello)
    {
        var pong = new PongMessage
        {
            Message = $"Hello {hello.ClientId}"
        };

        return SendJson(socket, pong);
    }

}
