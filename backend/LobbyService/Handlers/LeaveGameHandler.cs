using DistributedChess.LobbyService.Game;
using LobbyService.Models;
using Shared.Messages;
using System.Net.WebSockets;
using System.Text.Json;

namespace LobbyService.Handlers;

public class LeaveGameHandler : BaseHandler, IMessageHandler
{
    public MessageType Type => MessageType.LeaveGame;

    public LeaveGameHandler(ConnectionManager c, LobbyManager l, GameManager g)
        : base(c, l, g) { }

    public async Task HandleAsync(string socketId, WebSocket socket, BaseMessage baseMsg, string rawJson)
    {
        var msg = JsonSerializer.Deserialize<LeaveGameMessage>(rawJson, WebSocketExtensions.JsonOptions);
        if (msg == null)
        {
            await socket.SendErrorAsync("Invalid LeaveGame message");
            return;
        }

        await HandleLeaveGame(socketId, socket, msg);
    }

    private async Task HandleLeaveGame(string socketId, WebSocket socket, LeaveGameMessage msg)
    {
        var room = Games.GetGame(msg.GameId);
        if (room == null)
        {
            await socket.SendErrorAsync("Game not found");
            return;
        }

        var player = room.Players.FirstOrDefault(p => p.PlayerId == socketId);
        if (player == null)
        {
            await socket.SendErrorAsync("Player not in this game");
            return;
        }

        // rimuovi giocatore
        room.RemovePlayer(socketId);

        // messaggio agli altri
        var leftMsg = new PlayerLeftGameMessage
        {
            GameId = room.GameId,
            PlayerId = player.PlayerId,
            PlayerName = player.PlayerName
        };

        foreach (var p in room.Players)
        {
            var ws = Connections.GetSocket(p.PlayerId);
            if (ws != null && ws.State == WebSocketState.Open)
                await ws.SendJsonAsync(leftMsg);
        }

        // se la stanza è vuota → cleanup
        if (!room.Players.Any())
        {
            Games.RemoveGame(room.GameId);

            var removedMsg = new DeletedGameMessage
            {
                GameId = room.GameId
            };

            var snapshot = Connections.AllSockets.ToList();

            // avvisa tutti i client nella lobby
            foreach (var ws in snapshot)
            {
                if (ws != null && ws.State == WebSocketState.Open)
                    await ws.SendJsonAsync(removedMsg);
            }
        }

    }
}
