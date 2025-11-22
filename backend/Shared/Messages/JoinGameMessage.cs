using System.Text.Json.Serialization;

namespace Shared.Messages;

public class JoinGameMessage : BaseMessage
{
    [JsonPropertyName("gameId")]
    public string GameId { get; set; } = "";

    public JoinGameMessage()
    {
        Type = MessageType.JoinGame;
    }
}
