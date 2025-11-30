using Shared.Messages;
using Shared.Models;
using System.Text.Json.Serialization;

public class LobbyStateMessage
{
    [JsonPropertyName("players")]
    public List<Player> Players { get; set; } = new();
    [JsonPropertyName("games")]
    public List<GameRoom> Games { get; set; } = new();
}
