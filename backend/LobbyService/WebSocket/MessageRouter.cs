using Shared.Messages;
using LobbyService.Handlers;
using System.Net.WebSockets;

public class MessageRouter
{
    private readonly Dictionary<MessageType, IMessageHandler> _handlers;

    public MessageRouter(IEnumerable<IMessageHandler> handlers)
    {
        _handlers = handlers.ToDictionary(h => h.Type, h => h);
    }

    public Task RouteAsync(string socketId, WebSocket socket, BaseMessage msg, string raw)
    {
        if (_handlers.TryGetValue(msg.Type, out var handler))
            return handler.HandleAsync(socketId, socket, msg, raw);

        Console.WriteLine($"NO HANDLER for type {msg.Type}");
        return Task.CompletedTask;
    }
}
