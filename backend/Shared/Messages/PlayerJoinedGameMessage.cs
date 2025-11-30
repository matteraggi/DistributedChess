using System.Text.Json.Serialization;

namespace Shared.Messages;

public class PlayerJoinedGameMessage
{
    [JsonPropertyName("gameId")]
    public string GameId { get; set; } = "";

    [JsonPropertyName("playerId")]
    public string PlayerId { get; set; } = "";

    [JsonPropertyName("playerName")]
    public string PlayerName { get; set; } = "";
}
