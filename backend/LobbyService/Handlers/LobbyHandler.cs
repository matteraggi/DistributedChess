using DistributedChess.LobbyService.Game;
using Shared.Messages;
using System.Net.WebSockets;
using System.Text.Json;
using System.Linq;

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

        // Se il frontend ha mandato playerId, usalo; altrimenti fallback su socketId
        var playerId = string.IsNullOrEmpty(msg.PlayerId) ? socketId : msg.PlayerId;

        // Salva o aggiorna il giocatore in lobby
        Lobby.AddOrUpdatePlayer(playerId, msg.PlayerName, socketId);

        // Notifica a tutti i giocatori in lobby che qualcuno è entrato
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
            catch { /* ignore write errors */ }
        }

        // Invia stato completo solo al nuovo entrato
        var lobbyState = new LobbyStateMessage
        {
            Players = Lobby.Players
                .Select(p => new LobbyPlayerDto
                {
                    PlayerId = p.PlayerId,
                    PlayerName = p.PlayerName
                })
                .ToList(),

            Games = Games.GetAllGames()
                .Select(g => new LobbyGameDto
                {
                    GameId = g.GameId,
                    GameName = g.GameName
                })
                .ToList()
        };

        try
        {
            if (socket?.State == WebSocketState.Open)
                await socket.SendJsonAsync(lobbyState);
        }
        catch { /* ignore */ }
    }
}
