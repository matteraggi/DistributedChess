using Shared.Messages;

public class LobbyStateMessage : BaseMessage
{
    public List<LobbyPlayerDto> Players { get; set; } = new();
    public List<LobbyGameDto> Games { get; set; } = new();

    public LobbyStateMessage()
    {
        Type = MessageType.LobbyState;
    }
}

public class LobbyPlayerDto
{
    public string PlayerId { get; set; } = "";
    public string PlayerName { get; set; } = "";
}

public class LobbyGameDto
{
    public string GameId { get; set; } = "";
    public string GameName { get; set; } = "";
}
