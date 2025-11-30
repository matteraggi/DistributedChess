namespace Shared.Messages;
using System.Text.Json.Serialization;

public class PlayerJoinedLobbyMessage
{
    [JsonPropertyName("playerName")]
    public string PlayerName { get; set; } = "";
    [JsonPropertyName("playerId")]
    public string PlayerId { get; set; } = "";
}
