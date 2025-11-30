using Shared.Messages;
using System.Text.Json.Serialization;

public class GameStartMessage
{
    [JsonPropertyName("gameId")]
    public string GameId { get; set; } = "";
}
