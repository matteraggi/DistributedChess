namespace DistributedChess.LobbyService.Game;

public class GamePlayer
{
    public string PlayerId { get; set; } = "";
    public string PlayerName { get; set; } = "";
}

public class GameRoom
{
    public string GameId { get; }
    public string GameName { get; }

    private readonly List<GamePlayer> _players = new();

    public GameRoom(string id, string name)
    {
        GameId = id;
        GameName = name;
    }

    public IReadOnlyList<GamePlayer> Players => _players;

    public void AddPlayer(string playerId, string playerName)
    {
        _players.Add(new GamePlayer
        {
            PlayerId = playerId,
            PlayerName = playerName
        });
    }
}
