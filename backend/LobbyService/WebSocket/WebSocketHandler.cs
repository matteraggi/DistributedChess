using DistributedChess.LobbyService.Game;
using Shared.Messages;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;



public class WebSocketHandler
{
    private readonly ConnectionManager _manager;
    private readonly LobbyManager _lobby;
    private readonly GameManager _games;


    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public WebSocketHandler(ConnectionManager manager, LobbyManager lobby, GameManager games)
    {
        _manager = manager;
        _lobby = lobby;
        _games = games;

    }

    public async Task HandleAsync(HttpContext context)
    {
        var socket = await context.WebSockets.AcceptWebSocketAsync();

        var id = Guid.NewGuid().ToString();
        _manager.AddSocket(id, socket);

        await Listen(id, socket);
    }

    private async Task Listen(string id, WebSocket socket)
    {
        var buffer = new byte[2048];

        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(buffer, CancellationToken.None);
            if (result.CloseStatus.HasValue)
                break;

            var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
            Console.WriteLine($"Received raw: {json}");

            BaseMessage? baseMsg;
            try
            {
                baseMsg = JsonSerializer.Deserialize<BaseMessage>(json, JsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Deserialization error: {ex.Message}");
                await SendError(socket, "Invalid JSON");
                continue;
            }

            if (baseMsg is null)
            {
                await SendError(socket, "Invalid message format");
                continue;
            }

            switch (baseMsg.Type)
            {
                case MessageType.Hello:
                    {
                        var hello = JsonSerializer.Deserialize<HelloMessage>(json, JsonOptions);
                        if (hello is null)
                        {
                            await SendError(socket, "Invalid Hello message");
                            break;
                        }

                        await HandleHello(socket, hello);
                        break;
                    }
                case MessageType.JoinLobby:
                    {
                        var join = JsonSerializer.Deserialize<JoinLobbyMessage>(json, JsonOptions);
                        if (join is null)
                        {
                            await SendError(socket, "Invalid JoinLobby message");
                            break;
                        }

                        await HandleJoinLobby(id, socket, join);
                        break;
                    }
                case MessageType.CreateGame:
                    {
                        var msg = JsonSerializer.Deserialize<CreateGameMessage>(json, JsonOptions);
                        if (msg == null)
                        {
                            await SendError(socket, "Invalid CreateGame message");
                            break;
                        }

                        await HandleCreateGame(socketId: id, socket: socket, msg: msg);
                        break;
                    }
                case MessageType.JoinGame:
                    {
                        var jg = JsonSerializer.Deserialize<JoinGameMessage>(json, JsonOptions);
                        if (jg == null)
                        {
                            await SendError(socket, "Invalid JoinGame message");
                            break;
                        }

                        await HandleJoinGame(socketId: id, socket: socket, jg);
                        break;
                    }

                default:
                    await SendError(socket, $"Unknown message type: {baseMsg.Type}");
                    break;
            }
        }
    }

    private Task SendJson(WebSocket socket, object obj)
    {
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(obj));
        return socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private Task SendError(WebSocket socket, string error)
    {
        var msg = new ErrorMessage
        {
            Error = error
        };

        return SendJson(socket, msg);
    }

    private Task HandleHello(WebSocket socket, HelloMessage hello)
    {
        var pong = new PongMessage
        {
            Message = $"Hello {hello.ClientId}"
        };

        return SendJson(socket, pong);
    }

    private async Task HandleJoinLobby(string socketId, WebSocket socket, JoinLobbyMessage join)
    {
        _lobby.AddPlayer(socketId, join.PlayerName);

        var broadcast = new PlayerJoinedLobbyMessage
        {
            PlayerId = socketId,
            PlayerName = join.PlayerName
        };

        foreach (var kvp in _manager.AllSockets)
        {
            await SendJson(kvp, broadcast);
        }
        
    }

private async Task HandleCreateGame(string socketId, WebSocket socket, CreateGameMessage msg)
{
    // 1. crea stanza
    var room = _games.CreateGame(msg.GameName);

    // 2. recupera nome del creatore
    var playerName = _lobby.GetPlayerName(socketId);

    // 3. aggiunge il creatore alla stanza
    room.AddPlayer(socketId, playerName);

    // 4. manda GameCreated a tutta la lobby (facoltativo ma molto utile)
    var gameCreated = new GameCreatedMessage
    {
        GameId = room.GameId,
        GameName = room.GameName
    };

    foreach (var ws in _manager.AllSockets)
        await SendJson(ws, gameCreated);

    // 5. manda PlayerJoinedGame SOLO ai giocatori della stanza
    var joinedMsg = new PlayerJoinedGameMessage
    {
        GameId = room.GameId,
        PlayerId = socketId,
        PlayerName = playerName
    };

    await SendJson(socket, joinedMsg);  // unico giocatore della room
}


    private async Task HandleJoinGame(string socketId, WebSocket socket, JoinGameMessage msg)
    {
        var room = _games.GetGame(msg.GameId);
        if (room == null)
        {
            await SendError(socket, "Game not found");
            return;
        }

        var playerName = _lobby.GetPlayerName(socketId);
        if (playerName == null)
        {
            await SendError(socket, "Player not in lobby");
            return;
        }

        // aggiungi al game
        room.AddPlayer(socketId, playerName);

        var joined = new PlayerJoinedGameMessage
        {
            GameId = room.GameId,
            PlayerId = socketId,
            PlayerName = playerName
        };

        // broadcast solo ai player della room
        foreach (var p in room.Players)
        {
            var ws = _manager.GetSocket(p.PlayerId);
            if (ws != null)
                await SendJson(ws, joined);
        }
    }


}
