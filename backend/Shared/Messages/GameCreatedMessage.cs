using Shared.Models;
using System.Text.Json.Serialization;

namespace Shared.Messages;

public class GameCreatedMessage : BaseMessage
{
    [JsonPropertyName("gameId")]
    public string GameId { get; set; } = "";

    [JsonPropertyName("gameName")]
    public string GameName { get; set; } = "";
    public GameCreatedMessage()
    {
        Type = MessageType.GameCreated;
    }
}
