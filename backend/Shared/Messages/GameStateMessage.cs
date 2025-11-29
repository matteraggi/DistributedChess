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
    }
}