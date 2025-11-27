using DistributedChess.LobbyService.Game;
using Shared.Models;
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

        await HandleLeaveGame(socket, msg);
    }

    private async Task HandleLeaveGame(WebSocket socket, LeaveGameMessage msg)
    {
        var room = await Games.GetGameAsync(msg.GameId);
        if (room == null)
        {
            await socket.SendErrorAsync("Game not found");
            return;
        }

        var redisPlayers = await Games.GetPlayersAsync(msg.GameId);
        var player = redisPlayers.FirstOrDefault(p => p.PlayerId == msg.PlayerId);

        if (player == null)
        {
            await socket.SendErrorAsync("Player not in this game");
            return;
        }

        // sync room model
        if (!room.Players.Any(p => p.PlayerId == player.PlayerId))
            room.Players.Add(player);


        // remove
        await Games.RemovePlayerAsync(msg.GameId, msg.PlayerId);

        // reload updated room (might be null!)
        room = await Games.GetGameAsync(msg.GameId);

        if (room == null || room.Players.Count == 0)
        {
            // cleanup
            await Games.RemoveGameAsync(msg.GameId);

            var removedMsg = new DeletedGameMessage
            {
                GameId = msg.GameId
            };

            foreach (var ws in Connections.AllSockets.ToList())
            {
                if (ws != null && ws.State == WebSocketState.Open)
                    await ws.SendJsonAsync(removedMsg);
            }

            return;
        }

        // broadcast PlayerLeftGame
        var leftMsg = new PlayerLeftGameMessage
        {
            GameId = room.GameId,
            PlayerId = msg.PlayerId,
            PlayerName = player.PlayerName
        };

        foreach (var p in room.Players)
        {
            var ws = Connections.GetSocket(p.PlayerId);
            if (ws != null && ws.State == WebSocketState.Open)
                await ws.SendJsonAsync(leftMsg);
        }
    }
}
