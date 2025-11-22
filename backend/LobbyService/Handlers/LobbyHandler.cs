using DistributedChess.LobbyService.Game;
using Shared.Messages;
using System.Net.WebSockets;
using System.Text.Json;

namespace LobbyService.Handlers;

public class LobbyHandler : BaseHandler, IMessageHandler
{
    public MessageType Type => MessageType.JoinLobby;

    public LobbyHandler(ConnectionManager c, LobbyManager l, GameManager g)
        : base(c, l, g) { }

    public async Task HandleAsync(string socketId, WebSocket socket, BaseMessage baseMsg, string rawJson)
    {
        var msg = JsonSerializer.Deserialize<JoinLobbyMessage>(rawJson, JsonOptions);
        if (msg == null)
        {
            await SendError(socket, "Invalid JoinLobby message");
            return;
        }

        Lobby.AddPlayer(socketId, msg.PlayerName);

        var broadcast = new PlayerJoinedLobbyMessage
        {
            PlayerId = socketId,
            PlayerName = msg.PlayerName
        };

        foreach (var ws in Connections.AllSockets)
            await SendJson(ws, broadcast);
    }
}
