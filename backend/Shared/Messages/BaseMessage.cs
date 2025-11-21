using System.Text.Json.Serialization;

namespace Shared.Messages;

public class BaseMessage
{
    [JsonPropertyName("type")]
    public MessageType Type { get; set; } = MessageType.Hello;
}
