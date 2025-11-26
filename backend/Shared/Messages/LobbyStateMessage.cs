using Shared.Models;
using Shared.Messages;

public class LobbyStateMessage : BaseMessage
{
    public List<Player> Players { get; set; } = new();
    public List<GameRoom> Games { get; set; } = new();

    public LobbyStateMessage()
    {
        Type = MessageType.LobbyState;
    }
}
