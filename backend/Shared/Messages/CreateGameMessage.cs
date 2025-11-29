using System.Text.Json.Serialization;

namespace Shared.Messages;

public class CreateGameMessage
{
    [JsonPropertyName("gameName")]
    public string GameName { get; set; } = "";
    [JsonPropertyName("playerId")]
    public string PlayerId { get; set; } = "";
}
