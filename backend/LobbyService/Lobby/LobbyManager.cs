using Shared.Messages;
using System.Collections.Concurrent;
using System.Linq;


public class LobbyManager
{
    private readonly ConcurrentDictionary<string, string> _players = new();

    public void AddPlayer(string socketId, string playerName)
    {
        _players[socketId] = playerName;
    }

    public string? GetPlayerName(string socketId)
    {
        return _players.TryGetValue(socketId, out var name)
            ? name
            : null;
    }

    public void RemovePlayer(string socketId)
    {
        _players.TryRemove(socketId, out _);
    }

    public IEnumerable<(string id, string name)> Players =>
        _players.Select(kv => (kv.Key, kv.Value));
}
