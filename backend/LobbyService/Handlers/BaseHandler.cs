using DistributedChess.LobbyService.Game;
using Shared.Messages;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace LobbyService.Handlers;

public abstract class BaseHandler
{
    protected readonly ConnectionManager Connections;
    protected readonly LobbyManager Lobby;
    protected readonly GameManager Games;

    protected BaseHandler(ConnectionManager connections, LobbyManager lobby, GameManager games)
    {
        Connections = connections;
        Lobby = lobby;
        Games = games;
    }
}
