using Shared.Messages;
using System.Text.Json.Serialization;

public class LeaveGameMessage
{
    [JsonPropertyName("playerId")]
    public string PlayerId { get; set; } = "";
    [JsonPropertyName("gameName")]
    public string PlayerName { get; set; } = "";
    [JsonPropertyName("gameId")]
    public string GameId { get; set; } = "";
}
