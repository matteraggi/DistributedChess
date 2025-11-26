using Shared.Messages;
using Shared.Models;

public class PlayerReadyStatusMessage : BaseMessage
{
    public string GameId { get; set; } = "";
    public List<Player> PlayersReady { get; set; } = new();

    public PlayerReadyStatusMessage()
    {
        Type = MessageType.PlayerReadyStatus;
    }
}
