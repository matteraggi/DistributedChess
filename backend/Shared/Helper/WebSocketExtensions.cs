using Shared.Messages;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

public static class WebSocketExtensions
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public static Task SendJsonAsync(this WebSocket socket, object obj)
    {
        var json = JsonSerializer.Serialize(obj, JsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json);

        return socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    public static Task SendErrorAsync(this WebSocket socket, string error)
    {
        var msg = new ErrorMessage { Error = error };
        return socket.SendJsonAsync(msg);
    }
}
