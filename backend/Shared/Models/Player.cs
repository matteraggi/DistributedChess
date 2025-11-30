namespace Shared.Models
{
    public class Player
    {
        public string PlayerId { get; set; } = "";     // socketId
        public string PlayerName { get; set; } = "";
        public string SocketId { get; set; } = "";
        public bool IsReady { get; set; } = false;

        // Indica se il giocatore è dentro una partita
        public string? CurrentGameId { get; set; } = null;

        public Player() {}
    }
}
