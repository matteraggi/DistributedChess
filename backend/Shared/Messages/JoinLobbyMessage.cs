namespace Shared.Messages;

public class JoinLobbyMessage : BaseMessage
{
    public string PlayerName { get; set; } = "";

    public string PlayerId { get; set; } = "";

    public JoinLobbyMessage()
    {
        Type = MessageType.JoinLobby;
    }
}
