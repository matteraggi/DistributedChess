using Shared.Messages;
using Shared.Models;
using System.Text.Json.Serialization;

public class PlayerReadyStatusMessage
{
    [JsonPropertyName("gameId")]
    public string GameId { get; set; } = "";
    [JsonPropertyName("playersReady")]
    public List<Player> PlayersReady { get; set; } = new();
}
