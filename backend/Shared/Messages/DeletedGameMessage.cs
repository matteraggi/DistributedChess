using Shared.Messages;

public class DeletedGameMessage : BaseMessage
{
    public string GameId { get; set; } = "";
    public DeletedGameMessage()
    {
        Type = MessageType.DeletedGame;
    }
}
