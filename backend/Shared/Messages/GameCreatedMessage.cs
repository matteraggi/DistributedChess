using Shared.Models;
using System.Text.Json.Serialization;

namespace Shared.Messages;

public class GameCreatedMessage
{
    [JsonPropertyName("gameId")]
    public string GameId { get; set; } = "";

    [JsonPropertyName("gameName")]
    public string GameName { get; set; } = "";

    [JsonPropertyName("capacity")]
    public int Capacity { get; set; }

    [JsonPropertyName("creatorId")]
    public string CreatorId { get; set; } = "";

    [JsonPropertyName("creatorName")]
    public string CreatorName { get; set; } = "";
}
