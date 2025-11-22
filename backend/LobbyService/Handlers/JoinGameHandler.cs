using DistributedChess.LobbyService.Game;
using Shared.Messages;
using System.Net.WebSockets;
using System.Text.Json;

namespace LobbyService.Handlers;

public class JoinGameHandler : BaseHandler, IMessageHandler
{
    public MessageType Type => MessageType.JoinGame;

    public JoinGameHandler(ConnectionManager c, LobbyManager l, GameManager g)
        : base(c, l, g) { }

    public async Task HandleAsync(string socketId, WebSocket socket, BaseMessage baseMsg, string rawJson)
    {
        var msg = JsonSerializer.Deserialize<JoinGameMessage>(rawJson, JsonOptions);
        if (msg == null)
        {
            await SendError(socket, "Invalid JoinGame message");
            return;
        }

        await HandleJoinGame(socketId, socket, msg);
    }

    private async Task HandleJoinGame(string socketId, WebSocket socket, JoinGameMessage msg)
    {
        var room = Games.GetGame(msg.GameId);
        if (room == null)
        {
            await SendError(socket, "Game not found");
            return;
        }

        var playerName = Lobby.GetPlayerName(socketId);
        if (playerName == null)
        {
            await SendError(socket, "Player not in lobby");
            return;
        }

        room.AddPlayer(socketId, playerName);

        var joined = new PlayerJoinedGameMessage
        {
            GameId = room.GameId,
            PlayerId = socketId,
            PlayerName = playerName
        };

        foreach (var p in room.Players)
        {
            var ws = Connections.GetSocket(p.PlayerId);
            if (ws != null)
                await SendJson(ws, joined);
        }
    }
}
