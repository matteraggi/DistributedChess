using DistributedChess.LobbyService.Game;
using Shared.Messages;
using System.Net.WebSockets;
using System.Text.Json;

namespace LobbyService.Handlers;

public class CreateGameHandler : BaseHandler, IMessageHandler
{
    public MessageType Type => MessageType.CreateGame;

    public CreateGameHandler(ConnectionManager c, LobbyManager l, GameManager g)
        : base(c, l, g) { }

    public async Task HandleAsync(string socketId, WebSocket socket, BaseMessage baseMsg, string rawJson)
    {
        var msg = JsonSerializer.Deserialize<CreateGameMessage>(rawJson, JsonOptions);
        if (msg == null)
        {
            await SendError(socket, "Invalid CreateGame message");
            return;
        }

        await HandleCreateGame(socketId, socket, msg);
    }

    private async Task HandleCreateGame(string socketId, WebSocket socket, CreateGameMessage msg)
    {
        var room = Games.CreateGame(msg.GameName);

        var playerName = Lobby.GetPlayerName(socketId);

        room.AddPlayer(socketId, playerName);

        var gameCreated = new GameCreatedMessage
        {
            GameId = room.GameId,
            GameName = room.GameName
        };

        // broadcast lista partite (tutta la lobby)
        foreach (var ws in Connections.AllSockets)
            await SendJson(ws, gameCreated);

        // notifica join nella room (solo al creatore, per ora)
        var joinedMsg = new PlayerJoinedGameMessage
        {
            GameId = room.GameId,
            PlayerId = socketId,
            PlayerName = playerName
        };

        await SendJson(socket, joinedMsg);
    }
}
