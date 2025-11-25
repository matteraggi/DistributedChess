using GameEngine.Board;
using LobbyService.Models;

namespace DistributedChess.LobbyService.Game;

public class GameRoom
{
    public string GameId { get; }
    public string GameName { get; }
    public Board Board { get; }

    private readonly List<Player> _players = new();

    public GameRoom(string id, string name)
    {
        GameId = id;
        GameName = name;
        Board = BoardFactory.CreateInitialBoard();
    }

    public IReadOnlyList<Player> Players => _players;

    public void AddPlayer(string playerId, string playerName)
    {
        _players.Add(new Player
        {
            PlayerId = playerId,
            PlayerName = playerName
        });
    }
    public IEnumerable<(string PlayerId, string PlayerName)> PlayersExcluding(string playerId)
    {
        return _players.Where(kv => kv.PlayerId != playerId)
                       .Select(kv => (kv.PlayerId, kv.PlayerName));
    }

    public void RemovePlayer(string playerId)
    {
        var player = _players.FirstOrDefault(p => p.PlayerId == playerId);
        if (player != null)
        {
            _players.Remove(player);
        }
    }
}
