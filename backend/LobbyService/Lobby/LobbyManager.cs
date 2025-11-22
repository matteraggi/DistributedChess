using Shared.Messages;
using System.Linq;


public class LobbyManager
{
    private readonly Dictionary<string, string> _players = new();

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
        _players.Remove(socketId);
    }

    public IEnumerable<(string id, string name)> Players =>
        _players.Select(kv => (kv.Key, kv.Value));
}
