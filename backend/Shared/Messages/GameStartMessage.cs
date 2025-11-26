using Shared.Messages;

public class GameStartMessage : BaseMessage
{
    public string GameId { get; set; } = "";
    public GameStartMessage()
    {
        Type = MessageType.GameStart;
    }

}
