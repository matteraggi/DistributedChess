using Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Shared.Messages
{
    public class RequestGameStateMessage
    {
        [JsonPropertyName("gameId")]
        public string GameId { get; set; } = "";
        [JsonPropertyName("playerId")]
        public string PlayerId { get; set; } = "";
    }
}
