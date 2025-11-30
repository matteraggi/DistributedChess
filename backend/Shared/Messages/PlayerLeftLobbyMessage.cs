using Shared.Messages;
using System.Text.Json.Serialization;

public class PlayerLeftLobbyMessage
{
    [JsonPropertyName("playerId")]
    public string PlayerId { get; set; } = "";
    [JsonPropertyName("playerName")]
    public string PlayerName { get; set; } = "";
    [JsonPropertyName("username")]
    public string Username { get; set; } = "";
}
