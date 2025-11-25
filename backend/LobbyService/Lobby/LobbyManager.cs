using Shared.Messages;
using System.Collections.Concurrent;
using System.Linq;

public class LobbyManager
{
    // Mappa playerId → playerName
    private readonly ConcurrentDictionary<string, string> _players = new();

    // Mappa playerId → socketId
    private readonly ConcurrentDictionary<string, string> _sockets = new();

    // Aggiunge o aggiorna un giocatore
    public void AddOrUpdatePlayer(string playerId, string playerName, string socketId)
    {
        _players[playerId] = playerName;
        _sockets[playerId] = socketId;
    }

    public string? GetPlayerName(string playerId)
    {
        return _players.TryGetValue(playerId, out var name)
            ? name
            : null;
    }

    public string? GetSocketId(string playerId)
    {
        return _sockets.TryGetValue(playerId, out var socketId)
            ? socketId
            : null;
    }

    public void RemovePlayer(string playerId)
    {
        _players.TryRemove(playerId, out _);
        _sockets.TryRemove(playerId, out _);
    }

    public IEnumerable<(string PlayerId, string PlayerName)> Players =>
        _players.Select(kv => (kv.Key, kv.Value));
}
