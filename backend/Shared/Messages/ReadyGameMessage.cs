using Shared.Messages;
using System.Text.Json.Serialization;

public class ReadyGameMessage
{
    [JsonPropertyName("gameId")]
    public string GameId { get; set; } = "";
    [JsonPropertyName("playerId")]
    public string PlayerId { get; set; } = "";
    [JsonPropertyName("isReady")]
    public bool IsReady { get; set; } = false;
}
