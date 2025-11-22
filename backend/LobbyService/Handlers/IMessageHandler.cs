using Shared.Messages;
using System.Net.WebSockets;

namespace LobbyService.Handlers;

public interface IMessageHandler
{
    MessageType Type { get; }
    Task HandleAsync(string socketId, WebSocket socket, BaseMessage baseMsg, string rawJson);
}
