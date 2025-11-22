namespace Shared.Messages;

public class PlayerJoinedLobbyMessage : BaseMessage
{
    public string PlayerName { get; set; } = "";
    public string PlayerId { get; set; } = "";

    public PlayerJoinedLobbyMessage()
    {
        Type = MessageType.PlayerJoinedLobby;
    }
}
