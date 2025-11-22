using DistributedChess.LobbyService.Game;
using Shared.Messages;
using System.Net.WebSockets;
using System.Text.Json;

namespace LobbyService.Handlers;

public class HelloHandler : BaseHandler, IMessageHandler
{
    public MessageType Type => MessageType.Hello;

    public HelloHandler(ConnectionManager c, LobbyManager l, GameManager g)
        : base(c, l, g) { }

    public async Task HandleAsync(string socketId, WebSocket socket, BaseMessage baseMsg, string rawJson)
    {
        var hello = JsonSerializer.Deserialize<HelloMessage>(rawJson, JsonOptions);

        var pong = new PongMessage
        {
            Message = $"Hello {hello?.ClientId}"
        };

        await SendJson(socket, pong);
    }
}
