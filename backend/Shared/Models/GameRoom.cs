namespace Shared.Models
{
    public class GameRoom
    {
        public string GameId { get; set; } = "";
        public string GameName { get; set; } = "";
        public List<Player> Players { get; set; } = new();
        public int Capacity { get; set; } = 2;
        public string? WhitePlayerId { get; set; }
        public string? BlackPlayerId { get; set; }
        public string Fen { get; set; } = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        public GameRoom(string gameId, string gameName)
        {
            GameId = gameId;
            GameName = gameName;
        }

        public GameRoom() { }

    }
}
