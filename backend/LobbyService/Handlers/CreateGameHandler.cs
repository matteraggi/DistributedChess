using DistributedChess.LobbyService.Game;
using GameEngine.Board;
using Shared.Game;
using Shared.Messages;
using System.Net.WebSockets;
using System.Text.Json;

namespace LobbyService.Handlers;

public class CreateGameHandler : BaseHandler, IMessageHandler
{
    public MessageType Type => MessageType.CreateGame;

    public CreateGameHandler(ConnectionManager c, LobbyManager l, GameManager g)
        : base(c, l, g) { }

    public async Task HandleAsync(string socketId, WebSocket socket, BaseMessage baseMsg, string rawJson)
    {
        var msg = JsonSerializer.Deserialize<CreateGameMessage>(rawJson, WebSocketExtensions.JsonOptions);
        if (msg == null)
        {
            await socket.SendErrorAsync("Invalid CreateGame message");
            return;
        }

        await HandleCreateGame(socketId, socket, msg);
    }

    private async Task HandleCreateGame(string socketId, WebSocket socket, CreateGameMessage msg)
    {
        var room = Games.CreateGame(msg.GameName);

        var playerName = Lobby.GetPlayerName(socketId);

        if (playerName == null)
        {
            return;
        }

        room.AddPlayer(socketId, playerName);

        var gameCreated = new GameCreatedMessage
        {
            GameId = room.GameId,
            GameName = room.GameName,
            InitialBoard = MapBoard(room.Board)
        };

        foreach (var ws in Connections.AllSockets)
            await socket.SendJsonAsync(gameCreated);


        var joinedMsg = new PlayerJoinedGameMessage
        {
            GameId = room.GameId,
            PlayerId = socketId,
            PlayerName = playerName
        };

        await socket.SendJsonAsync(joinedMsg);
    }


    private static List<BoardSquareDto> MapBoard(Board board)
    {
        var result = new List<BoardSquareDto>();

        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                var piece = board.GetPiece(rank, file);
                if (piece == null) continue;

                result.Add(new BoardSquareDto
                {
                    Rank = rank,
                    File = file,
                    PieceType = piece.Type.ToString().ToLowerInvariant(),
                    PieceColor = piece.Color.ToString().ToLowerInvariant()
                });
            }
        }

        return result;
    }
}
