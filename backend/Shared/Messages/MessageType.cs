namespace Shared.Messages;

public enum MessageType
{
    Hello = 0,
    Pong = 1,
    Error = 2,

    JoinLobby = 10,
    PlayerJoinedLobby = 11,

    CreateGame = 20,
    GameCreated = 21,

    JoinGame = 22,
    PlayerJoinedGame = 23
}
