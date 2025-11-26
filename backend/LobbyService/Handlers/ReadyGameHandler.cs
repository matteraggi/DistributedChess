using DistributedChess.LobbyService.Game;
using Shared.Models;
using Shared.Messages;
using System.Net.WebSockets;
using System.Text.Json;
using System.Linq;

namespace LobbyService.Handlers;

public class ReadyGameHandler : BaseHandler, IMessageHandler
{
    public MessageType Type => MessageType.ReadyGame;

    public ReadyGameHandler(ConnectionManager c, LobbyManager l, GameManager g)
        : base(c, l, g) { }

    public async Task HandleAsync(string socketId, WebSocket socket, BaseMessage baseMsg, string rawJson)
    {
        var msg = JsonSerializer.Deserialize<ReadyGameMessage>(rawJson, WebSocketExtensions.JsonOptions);
        if (msg == null)
        {
            await socket.SendErrorAsync("Invalid ReadyGame message");
            return;
        }

        // Prendi la stanza dal GameManager
        var room = await Games.GetGameAsync(msg.GameId);
        if (room == null)
        {
            await socket.SendErrorAsync("Game not found");
            return;
        }

        // Aggiorna lo stato ready del giocatore tramite GameManager/Redis
        await Games.SetPlayerReadyAsync(msg.GameId, msg.PlayerId, msg.IsReady);

        // Ricarica lo stato aggiornato dei giocatori
        room = await Games.GetGameAsync(msg.GameId);
        if (room == null) return; // fallback

        // Costruisci lista degli stati ready
        var readyStatus = room.Players
            .Select(p => new Player { PlayerId = p.PlayerId, IsReady = p.IsReady })
            .ToList();

        var statusMsg = new PlayerReadyStatusMessage
        {
            GameId = room.GameId,
            PlayersReady = readyStatus
        };

        // Invia a tutti i giocatori della partita
        foreach (var p in room.Players)
        {
            var ws = Connections.GetSocket(p.PlayerId);
            if (ws != null && ws.State == WebSocketState.Open)
                await ws.SendJsonAsync(statusMsg);
        }

        // Controlla se tutti i giocatori sono ready e se il numero di giocatori = capacity
        if (room.Players.All(p => p.IsReady) && room.Players.Count == room.Capacity)
        {
            var startMsg = new GameStartMessage { GameId = room.GameId };
            foreach (var p in room.Players)
            {
                var ws = Connections.GetSocket(p.PlayerId);
                if (ws != null && ws.State == WebSocketState.Open)
                    await ws.SendJsonAsync(startMsg);
            }
        }
    }
}
