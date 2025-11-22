using DistributedChess.LobbyService.Game;
using Shared.Messages;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace LobbyService.Handlers;

public abstract class BaseHandler
{
    protected readonly ConnectionManager Connections;
    protected readonly LobbyManager Lobby;
    protected readonly GameManager Games;

    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    protected BaseHandler(ConnectionManager connections, LobbyManager lobby, GameManager games)
    {
        Connections = connections;
        Lobby = lobby;
        Games = games;
    }

    protected Task SendJson(WebSocket socket, object obj)
    {
        var json = JsonSerializer.Serialize(obj, JsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json);

        return socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    protected Task SendError(WebSocket socket, string error)
    {
        var msg = new ErrorMessage
        {
            Error = error
        };

        return SendJson(socket, msg);
    }
}
