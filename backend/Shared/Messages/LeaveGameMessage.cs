using Shared.Messages;

public class LeaveGameMessage : BaseMessage
{
    public LeaveGameMessage()
    {
        Type = MessageType.LeaveGame;
    }
    public string PlayerId { get; set; } = "";
    public string PlayerName { get; set; } = "";
    public string GameId { get; set; } = "";
}
