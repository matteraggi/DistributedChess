using System.Text.Json.Serialization;

namespace Shared.Messages;

public class CreateGameMessage : BaseMessage
{
    [JsonPropertyName("gameName")]
    public string GameName { get; set; } = "";
    public string PlayerId { get; set; } = "";

    public CreateGameMessage()
    {
        Type = MessageType.CreateGame;
    }
}
