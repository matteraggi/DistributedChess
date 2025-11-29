using System.Text.Json.Serialization;

namespace Shared.Messages;

public class HelloMessage
{
    [JsonPropertyName("clientId")]
    public string ClientId { get; set; } = string.Empty;
}
