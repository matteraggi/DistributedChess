using DistributedChess.LobbyService.Game;
using Shared.Models;
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
        var msg = JsonSerializer.Deserialize<JoinLobbyMessage>(rawJson, WebSocketExtensions.JsonOptions);
        if (msg == null)
        {
            await socket.SendErrorAsync("Invalid JoinLobby message");
            return;
        }

        var playerId = string.IsNullOrEmpty(msg.PlayerId) ? socketId : msg.PlayerId;

        // Salva o aggiorna il giocatore in Redis
        await Lobby.AddOrUpdatePlayerAsync(playerId, msg.PlayerName, socketId);

        Connections.AddPlayerMapping(socketId, playerId);

        // Notifica a tutti i giocatori in lobby
        var joined = new PlayerJoinedLobbyMessage
        {
            PlayerId = playerId,
            PlayerName = msg.PlayerName
        };

        foreach (var ws in Connections.AllSockets.ToList())
        {
            try
            {
                if (ws?.State == WebSocketState.Open)
                    await ws.SendJsonAsync(joined);
            }
            catch { /* ignore */ }
        }

        // Recupera lo stato completo della lobby da Redis
        var players = await Lobby.GetAllPlayersAsync();
        var games = await Games.GetAllGamesAsync();

        var lobbyState = new LobbyStateMessage
        {
            Players = players.Select(p => new Player
            {
                PlayerId = p.PlayerId,
                PlayerName = p.PlayerName
            }).ToList(),

            Games = games.Select(g => new GameRoom(g.GameId, g.GameName)
            {
                Players = g.Players,
                Capacity = g.Capacity
            }).ToList()

        };

        try
        {
            if (socket?.State == WebSocketState.Open)
                await socket.SendJsonAsync(lobbyState);
        }
        catch { /* ignore */ }
    }
}
