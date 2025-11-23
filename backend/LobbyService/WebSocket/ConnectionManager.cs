using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

public class ConnectionManager
{
    private readonly ConcurrentDictionary<string, WebSocket> _sockets = new();

    private readonly LobbyManager _lobby;

    public ConnectionManager(LobbyManager lobby)
    {
        _lobby = lobby;
    }

    public async Task BroadcastAsync(object message)
    {
        foreach (var (_, socket) in _sockets)
        {
            if (socket.State == WebSocketState.Open)
            {
                await socket.SendJsonAsync(message);
            }
        }
    }

    public IEnumerable<WebSocket> AllSockets => _sockets.Values;

    public void AddSocket(string id, WebSocket socket)
    {
        _sockets[id] = socket;
    }

    public async Task RemoveSocketAsync(string socketId)
    {
        _sockets.TryRemove(socketId, out var socket);

        var username = _lobby.GetPlayerName(socketId);
        _lobby.RemovePlayer(socketId);

        var leftMsg = new PlayerLeftLobbyMessage
        {
            PlayerId = socketId,
            Username = username ?? ""
        };

        await BroadcastAsync(leftMsg);
    }


    public WebSocket? GetSocket(string id)
    {
        _sockets.TryGetValue(id, out var socket);
        return socket;
    }

}
