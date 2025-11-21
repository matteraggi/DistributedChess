using System.Text.Json.Serialization;

namespace Shared.Messages;

public class ErrorMessage : BaseMessage
{
    [JsonPropertyName("error")]
    public string Error { get; set; } = string.Empty;

    public ErrorMessage()
    {
        Type = MessageType.Error;
    }
}
