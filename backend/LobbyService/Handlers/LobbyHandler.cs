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

        Lobby.AddPlayer(socketId, msg.PlayerName);

        var joined = new PlayerJoinedLobbyMessage
        {
            PlayerId = socketId,
            PlayerName = msg.PlayerName
        };

        // iterate over a snapshot and handle per-socket failures
        var snapshot = Connections.AllSockets.ToList();
        foreach (var ws in snapshot)
        {
            try
            {
                if (ws?.State == WebSocketState.Open)
                    await ws.SendJsonAsync(joined);
            }
            catch (WebSocketException)
            {
                // remote closed connection or write failed - ignore and continue
            }
            catch (ObjectDisposedException)
            {
                // underlying response stream was disposed - ignore and continue
            }
        }


        var lobbyState = new LobbyStateMessage
        {
            Players = Lobby.Players
                .Select(p => new LobbyPlayerDto
                {
                    PlayerId = p.id,
                    PlayerName = p.name
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
        catch (WebSocketException)
        {
            return;
        }
        catch (ObjectDisposedException)
        {
            return;
        }
    }
}
