using LobbyService.Manager;
using Microsoft.AspNetCore.SignalR;
using Shared.Interfaces;
using Shared.Messages; // Assumo che qui ci sia PlayerLeftLobbyMessage

namespace LobbyService.Hubs
{
    // Partial: così il file resta pulito e la logica specifica va negli altri file
    public partial class GameHub : Hub<IChessClient>
    {
        private readonly LobbyManager _lobbyManager;
        private readonly GameManager _gameManager;

        public GameHub(LobbyManager lobbyManager, GameManager gameManager)
        {
            _lobbyManager = lobbyManager;
            _gameManager = gameManager;
        }

        // Opzionale: Utile per loggare quando qualcuno apre il sito
        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"Client connesso a SignalR: {Context.ConnectionId}");
            return base.OnConnectedAsync();
        }

        // --- QUI SPOSTIAMO LA LOGICA DI ConnectionManager.RemoveSocketAsync ---
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Recuperiamo il PlayerId dallo "zaino" della connessione
            // (Lo avremo salvato lì dentro quando chiamano JoinLobby)
            if (Context.Items.TryGetValue("PlayerId", out var playerIdObj) && playerIdObj is string playerId)
            {
                // 1. Rimuovi lo stato su Redis (usando il tuo Manager esistente)
                // Nota: Assumo che LobbyManager abbia un metodo RemovePlayerAsync che chiama Redis
                await _lobbyManager.RemovePlayerAsync(playerId);

                // 2. Recupera il nome (opzionale, se serve per il messaggio)
                // Se il metodo RemovePlayerAsync lo cancella, potresti doverlo recuperare PRIMA di rimuoverlo
                // Oppure mandi solo l'ID. Qui simulo la logica vecchia:
                var playerName = "Unknown"; // O recuperalo da Redis se ancora esiste

                // 3. Notifica a TUTTI (Distribuito grazie a Redis Backplane)
                var leftMsg = new PlayerLeftLobbyMessage
                {
                    PlayerId = playerId,
                    Username = playerName
                };

                // "PlayerLeft" è il nome dell'evento che Angular ascolterà
                await Clients.All.PlayerLeft(leftMsg);

                Console.WriteLine($"Player disconnesso e rimosso: {playerId}");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}