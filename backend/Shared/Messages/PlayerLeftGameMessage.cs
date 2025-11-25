using Shared.Messages;

public class PlayerLeftGameMessage : BaseMessage
{
    public PlayerLeftGameMessage()
    {
        Type = MessageType.PlayerLeftGame;
    }
    public string PlayerId { get; set; } = "";
    public string PlayerName { get; set; } = "";
    public string GameId { get; set; } = "";
}
