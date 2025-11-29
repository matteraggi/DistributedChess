using System.Text.Json.Serialization;

namespace Shared.Messages;

public class JoinGameMessage
{
    [JsonPropertyName("gameId")]
    public string GameId { get; set; } = "";
    [JsonPropertyName("playerId")]
    public string PlayerId { get; set; } = "";
}
