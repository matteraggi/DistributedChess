using Shared.Messages;

public class ReadyGameMessage : BaseMessage
{
    public string GameId { get; set; } = "";
    public string PlayerId { get; set; } = "";
    public bool IsReady { get; set; } = false;

    public ReadyGameMessage()
    {
        Type = MessageType.ReadyGame;
    }
}
