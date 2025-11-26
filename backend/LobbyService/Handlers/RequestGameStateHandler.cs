using DistributedChess.LobbyService.Game;
using LobbyService.Handlers;
using Shared.Messages;
using System.Net.WebSockets;
using System.Text.Json;
using Shared.Models;

public class RequestGameStateHandler : BaseHandler, IMessageHandler
{
    public MessageType Type => MessageType.RequestGameState;

    public RequestGameStateHandler(ConnectionManager c, LobbyManager l, GameManager g)
        : base(c, l, g) { }

    public async Task HandleAsync(string socketId, WebSocket socket, BaseMessage baseMsg, string rawJson)
    {
        var msg = JsonSerializer.Deserialize<RequestGameStateMessage>(
            rawJson, WebSocketExtensions.JsonOptions);

        if (msg == null)
        {
            await socket.SendErrorAsync("Invalid RequestGameState message");
            return;
        }

        var room = await Games.GetGameAsync(msg.GameId);
        if (room == null)
        {
            await socket.SendErrorAsync("Game not found");
            return;
        }

        // Prova a recuperare il player dalla lobby
        var player = await Lobby.GetPlayerAsync(msg.PlayerId);

        // Se non c'è, crealo ora (caso tipico: refresh pagina o riapertura)
        if (player == null)
        {
            player = new Player
            {
                PlayerId = msg.PlayerId,
                PlayerName = "Player " + msg.PlayerId[..4] // oppure recuperalo dal client
            };

            await Lobby.AddOrUpdatePlayerAsync(player.PlayerId, player.PlayerName, socketId);
        }

        // Controlla se è nella partita
        var currentPlayers = await Games.GetPlayersAsync(msg.GameId);

        if (!currentPlayers.Any(p => p.PlayerId == msg.PlayerId))
        {
            // NON fare errore: aggiungilo
            await Games.AddPlayerAsync(msg.GameId, msg.PlayerId, player.PlayerName);

            currentPlayers = await Games.GetPlayersAsync(msg.GameId);
        }

        // Invia lo stato aggiornato
        var stateMsg = new GameStateMessage
        {
            GameId = room.GameId,
            Players = currentPlayers.ToList()
        };

        if (socket.State == WebSocketState.Open)
            await socket.SendJsonAsync(stateMsg);
    }
}
