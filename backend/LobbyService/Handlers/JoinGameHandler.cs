using DistributedChess.LobbyService.Game;
using Shared.Messages;
using Shared.Models;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Numerics;
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

        await HandleJoinGame(socket, socketId, msg);
    }
    private async Task HandleJoinGame(WebSocket socket, string socketId, JoinGameMessage msg)
    {
        var room = await Games.GetGameAsync(msg.GameId);

        if (room == null)
        {
            await socket.SendErrorAsync("Game not found");
            return;
        }

        // Usa playerId dal messaggio se presente, altrimenti fallback a socketId
        var playerId = string.IsNullOrEmpty(msg.PlayerId) ? socketId : msg.PlayerId;

        // Recupera il nome del giocatore dalla lobby
        var player = await Lobby.GetPlayerAsync(playerId);
        if (player == null)
        {
            await socket.SendErrorAsync("Player not in lobby");
            return;
        }

        // Aggiungi il player alla partita (evita duplicati)
        if (!room.Players.Any(p => p.PlayerId == playerId))
            await Games.AddPlayerAsync(room.GameId, msg.PlayerId, player.PlayerName);

        // 1️⃣ Invia stato completo solo al nuovo entrato
        var stateMsg = new GameStateMessage
        {
            GameId = room.GameId,
            Players = room.Players.Select(p => new Player
            {
                PlayerId = p.PlayerId,
                PlayerName = p.PlayerName
            }).ToList()
        };

        // 2️⃣ Notifica tutti gli altri giocatori già dentro che è arrivato un nuovo player
        var joinedMsg = new PlayerJoinedGameMessage
        {
            GameId = room.GameId,
            PlayerId = playerId,
            PlayerName = player.PlayerName
        };

        if (socket.State == WebSocketState.Open)
        {
            await socket.SendJsonAsync(joinedMsg);
            await socket.SendJsonAsync(stateMsg);
        }


        foreach (var p in room.Players)
        {
            if (p.PlayerId == playerId) continue; // non duplicare al nuovo entrato

            var ws = Connections.GetSocket(p.PlayerId);
            if (ws != null && ws.State == WebSocketState.Open)
                await ws.SendJsonAsync(joinedMsg);
        }
    }
}
