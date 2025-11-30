using System.Text.Json.Serialization;

namespace Shared.Messages;

public class JoinLobbyMessage
{
    [JsonPropertyName("playerName")]
    public string PlayerName { get; set; } = "";
    [JsonPropertyName("playerId")]
    public string PlayerId { get; set; } = "";
}
