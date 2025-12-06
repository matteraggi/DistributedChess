namespace Shared.Models
{
    public class GameRoom
    {
        public string GameId { get; set; } = "";
        public string GameName { get; set; } = "";
        public List<Player> Players { get; set; } = new();
        public GameMode Mode { get; set; } = GameMode.Classic1v1;
        public int Capacity { get; set; } = 2;
        public Dictionary<string, string> Teams { get; set; } = new();
        public string Fen { get; set; } = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        public Dictionary<string, List<char>> PiecePermissions { get; set; } = new();
        public List<MoveProposal> ActiveProposals { get; set; } = new();
        public DateTime LastMoveAt { get; set; } = DateTime.UtcNow;
        public GameRoom(string gameId, string gameName)
        {
            GameId = gameId;
            GameName = gameName;
        }

        public GameRoom() { }

    }
}
