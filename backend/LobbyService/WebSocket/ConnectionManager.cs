using Shared.Redis;
using System.Collections.Concurrent;
using System.Net.WebSockets;

public class ConnectionManager
{
    private readonly ConcurrentDictionary<string, WebSocket> _sockets = new();
    private readonly ConcurrentDictionary<string, string> _socketToPlayer = new(); // socketId → playerId
    private readonly RedisService _redis;

    public ConnectionManager(RedisService redis)
    {
        _redis = redis;
    }

    public IEnumerable<WebSocket> AllSockets => _sockets.Values;

    public void AddSocket(string socketId, WebSocket socket)
    {
        _sockets[socketId] = socket;
    }

    public void AddPlayerMapping(string socketId, string playerId)
    {
        _socketToPlayer[socketId] = playerId;
    }

    public WebSocket? GetSocket(string socketId) => _sockets.TryGetValue(socketId, out var ws) ? ws : null;

    public string? GetPlayerIdBySocket(string socketId) => _socketToPlayer.TryGetValue(socketId, out var pid) ? pid : null;

    public async Task RemoveSocketAsync(string socketId)
    {
        _sockets.TryRemove(socketId, out var ws);
        if (_socketToPlayer.TryRemove(socketId, out var playerId))
        {
            // rimuovi lo stato locale su Redis (opzionale, dipende se vuoi "log out" completo)
            await _redis.RemovePlayerAsync(playerId);

            // notifica a tutti che il giocatore ha lasciato
            var leftMsg = new PlayerLeftLobbyMessage
            {
                PlayerId = playerId,
                Username = (await _redis.GetPlayerAsync(playerId))?.PlayerName ?? ""
            };

            await BroadcastAsync(leftMsg);
        }
    }

    public async Task BroadcastAsync(object message)
    {
        foreach (var ws in _sockets.Values)
        {
            if (ws.State == WebSocketState.Open)
                await ws.SendJsonAsync(message);
        }
    }
}
