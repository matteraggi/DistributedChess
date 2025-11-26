// CreateGameHandler.cs
using DistributedChess.LobbyService.Game;
using Shared.Models;
using Shared.Messages;
using System.Net.WebSockets;
using System.Text.Json;

namespace LobbyService.Handlers;

public class CreateGameHandler : BaseHandler, IMessageHandler
{
    public MessageType Type => MessageType.CreateGame;

    public CreateGameHandler(ConnectionManager c, LobbyManager l, GameManager g)
        : base(c, l, g)
    {
    }

    public async Task HandleAsync(string socketId, WebSocket socket, BaseMessage baseMsg, string rawJson)
    {
        var msg = JsonSerializer.Deserialize<CreateGameMessage>(rawJson, WebSocketExtensions.JsonOptions);
        if (msg == null)
        {
            await socket.SendErrorAsync("Invalid CreateGame message");
            return;
        }

        await HandleCreateGame(socket, msg);
    }

    private async Task HandleCreateGame(WebSocket socket, CreateGameMessage msg)
    {
        // 1. Crea stanza su Redis
        var room = await Games.CreateGameAsync(msg.GameName);
        
        // 2. Recupera il giocatore dalla lobby
        var player = await Lobby.GetPlayerAsync(msg.PlayerId);
        if (player == null) return;

        // 3. Aggiungi giocatore alla stanza
        await Games.AddPlayerAsync(room.GameId, msg.PlayerId, player.PlayerName);
        await Games.UpdateGameAsync(room);

        // 4. Messaggi WebSocket
        var joinedMsg = new PlayerJoinedGameMessage
        {
            GameId = room.GameId,
            PlayerId = msg.PlayerId,
            PlayerName = player.PlayerName
        };
        if (socket.State == WebSocketState.Open)
            await socket.SendJsonAsync(joinedMsg);

        var gameCreated = new GameCreatedMessage
        {
            GameId = room.GameId,
            GameName = room.GameName,
        };
        foreach (var ws in Connections.AllSockets)
        {
            if (ws.State == WebSocketState.Open && ws != socket)
                await ws.SendJsonAsync(gameCreated);
        }

        var players = await Games.GetPlayersAsync(room.GameId);
        var stateMsg = new GameStateMessage
        {
            GameId = room.GameId,
            Players = players.Select(p => new Player
            {
                PlayerId = p.PlayerId,
                PlayerName = p.PlayerName
            }).ToList()
        };

        if (socket.State == WebSocketState.Open)
            await socket.SendJsonAsync(stateMsg);
    }

}
