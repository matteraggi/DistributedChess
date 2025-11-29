using Shared.Messages;
using System.Text.Json.Serialization;


public class PlayerLeftGameMessage
{
    [JsonPropertyName("playerId")]
    public string PlayerId { get; set; } = "";
    [JsonPropertyName("playerName")]
    public string PlayerName { get; set; } = "";
    [JsonPropertyName("gameId")]
    public string GameId { get; set; } = "";
}
