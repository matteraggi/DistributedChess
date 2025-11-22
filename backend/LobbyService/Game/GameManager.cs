using System.Collections.Concurrent;

namespace DistributedChess.LobbyService.Game;

public class GameManager
{
    private readonly ConcurrentDictionary<string, GameRoom> _games = new();

    public GameRoom CreateGame(string name)
    {
        var id = Guid.NewGuid().ToString();
        var room = new GameRoom(id, name);

        _games[id] = room;

        return room;
    }

    public GameRoom? GetGame(string id)
    {
        return _games.TryGetValue(id, out var room) ? room : null;
    }

    public IEnumerable<GameRoom> GetAllGames() => _games.Values;
}
