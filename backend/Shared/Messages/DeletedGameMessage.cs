using Shared.Messages;
using System.Text.Json.Serialization;

public class DeletedGameMessage
{
    [JsonPropertyName("gameId")]
    public string GameId { get; set; } = "";
}
