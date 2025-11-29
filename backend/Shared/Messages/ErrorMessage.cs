using System.Text.Json.Serialization;

namespace Shared.Messages;

public class ErrorMessage
{
    [JsonPropertyName("error")]
    public string Error { get; set; } = string.Empty;
}
