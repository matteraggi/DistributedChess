using System.Text.Json.Serialization;

namespace Shared.Messages;

public class HelloMessage : BaseMessage
{
    [JsonPropertyName("clientId")]
    public string ClientId { get; set; } = string.Empty;

    public HelloMessage()
    {
        Type = MessageType.Hello;
    }
}
