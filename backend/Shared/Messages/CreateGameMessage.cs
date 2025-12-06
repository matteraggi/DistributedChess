using Shared.Models;
using System.Text.Json.Serialization;

namespace Shared.Messages;

public class CreateGameMessage
{
    [JsonPropertyName("gameName")]
    public string GameName { get; set; } = "";
    [JsonPropertyName("playerId")]
    public string PlayerId { get; set; } = "";
    [JsonPropertyName("mode")]
    public GameMode Mode { get; set; } = GameMode.Classic1v1;
    [JsonPropertyName("teamSize")]
    public int TeamSize { get; set; } = 1;
}
