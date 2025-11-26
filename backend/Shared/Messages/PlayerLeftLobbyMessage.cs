using Shared.Messages;

public class PlayerLeftLobbyMessage : BaseMessage
{
    public string PlayerId { get; set; } = "";
    public string PlayerName { get; set; } = "";
    public string Username { get; set; } = "";

    public PlayerLeftLobbyMessage()
    {
        Type = MessageType.PlayerLeftLobby;
    }
}
