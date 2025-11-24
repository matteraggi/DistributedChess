using System.Net.WebSockets;

namespace LobbyService.Models
{
    public class Player
    {
        public string PlayerId { get; set; } = "";     // socketId
        public string PlayerName { get; set; } = "";
        public WebSocket Socket { get; set; } = null!;

        // Indica se il giocatore è dentro una partita
        public string? CurrentGameId { get; set; } = null;
    }
}
