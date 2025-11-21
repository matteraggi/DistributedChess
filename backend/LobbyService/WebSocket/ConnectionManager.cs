using System.Net.WebSockets;

public class ConnectionManager
{
    private readonly Dictionary<string, WebSocket> _sockets = new();

    public void AddSocket(string id, WebSocket socket) => _sockets[id] = socket;

    public WebSocket? GetSocket(string id) => _sockets.TryGetValue(id, out var s) ? s : null;

    public IEnumerable<WebSocket> AllSockets => _sockets.Values;
}
