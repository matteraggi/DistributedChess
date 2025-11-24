using DistributedChess.LobbyService.Game;
using LobbyService.Models;
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
        var msg = JsonSerializer.Deserialize<JoinGameMessage>(rawJson, WebSocketExtensions.JsonOptions);
        if (msg == null)
        {
            await socket.SendErrorAsync("Invalid JoinGame message");
            return;
        }

        await HandleJoinGame(socketId, socket, msg);
    }

    private async Task HandleJoinGame(string socketId, WebSocket socket, JoinGameMessage msg)
    {
        var room = Games.GetGame(msg.GameId);
        if (room == null)
        {
            await socket.SendErrorAsync("Game not found");
            return;
        }

        var playerName = Lobby.GetPlayerName(socketId);
        if (playerName == null)
        {
            await socket.SendErrorAsync("Player not in lobby");
            return;
        }

        // Aggiungi il giocatore al gioco
        room.AddPlayer(socketId, playerName);

        // 1️⃣ Invio stato completo solo al nuovo entrato
        var stateMsg = new GameStateMessage
        {
            GameId = room.GameId,
            Players = room.Players.Select(p => new Player
            {
                PlayerId = p.PlayerId,
                PlayerName = p.PlayerName
            }).ToList()
        };

        if (socket.State == WebSocketState.Open)
            await socket.SendJsonAsync(stateMsg);

        // 2️⃣ Avvisa tutti gli altri giocatori già dentro che è arrivato un nuovo giocatore
        var joinedMsg = new PlayerJoinedGameMessage
        {
            GameId = room.GameId,
            PlayerId = socketId,
            PlayerName = playerName
        };

        // invia a tutti tranne duplicare
        foreach (var p in room.Players)
        {
            var ws = Connections.GetSocket(p.PlayerId);
            if (ws != null && ws.State == WebSocketState.Open)
                await ws.SendJsonAsync(joinedMsg);
        }
    }
}
