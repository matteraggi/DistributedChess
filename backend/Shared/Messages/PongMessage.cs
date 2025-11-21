using System.Text.Json.Serialization;

namespace Shared.Messages;

public class PongMessage : BaseMessage
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    public PongMessage()
    {
        Type = MessageType.Pong;
    }
}
