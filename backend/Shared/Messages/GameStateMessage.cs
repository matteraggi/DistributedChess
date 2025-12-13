using Shared.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Shared.Messages
{
    public class GameStateMessage
    {
        [JsonPropertyName("gameId")]
        public string GameId { get; set; } = "";
        [JsonPropertyName("players")]
        public List<Player> Players { get; set; } = new();
        [JsonPropertyName("fen")]
        public string Fen { get; set; } = "";
        [JsonPropertyName("teams")]
        public Dictionary<string, string> Teams { get; set; } = new();
        [JsonPropertyName("mode")]
        public GameMode Mode { get; set; } = GameMode.Classic1v1;
        [JsonPropertyName("piecePermission")]
        public Dictionary<string, List<char>> PiecePermissions { get; set; } = new();

        [JsonPropertyName("activeProposals")]
        public List<MoveProposal> ActiveProposals { get; set; } = new();
        [JsonPropertyName("lastMoveAt")]
        public DateTime LastMoveAt { get; set; }
        [JsonPropertyName("capacity")]
        public int Capacity { get; set; } = 2;

    }
}