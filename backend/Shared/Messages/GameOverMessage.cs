namespace Shared.Messages
{
    public class GameOverMessage
    {
        public string GameId { get; set; } = "";
        public string WinnerPlayerId { get; set; } = "";
        public string Reason { get; set; } = "Checkmate"; // "Checkmate", "Resignation", "Draw"
    }
}