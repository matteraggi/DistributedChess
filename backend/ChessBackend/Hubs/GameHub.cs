using ChessBackend.Manager;
using GameEngine;
using Microsoft.AspNetCore.SignalR;
using Shared.Interfaces;
using Shared.Messages;
using Shared.Models; // Assumo che qui ci sia PlayerLeftLobbyMessage

namespace ChessBackend.Hubs
{
    // Partial: così il file resta pulito e la logica specifica va negli altri file
    public partial class GameHub : Hub<IChessClient>
    {
        private readonly LobbyManager _lobbyManager;
        private readonly GameManager _gameManager;
        private readonly ChessLogic _chessLogic;

        public GameHub(LobbyManager lobbyManager, GameManager gameManager, ChessLogic chessLogic)
        {
            _lobbyManager = lobbyManager;
            _gameManager = gameManager;
            _chessLogic = chessLogic;
        }

        // Opzionale: Utile per loggare quando qualcuno apre il sito
        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"Client connesso a SignalR: {Context.ConnectionId}");
            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (Context.Items.TryGetValue("PlayerId", out var pidObj) && pidObj is string playerId)
            {
                var player = await _lobbyManager.GetPlayerAsync(playerId);

                if (player != null && !string.IsNullOrEmpty(player.CurrentGameId))
                {
                    var gameId = player.CurrentGameId;
                    var room = await _gameManager.GetGameAsync(gameId);

                    if (room != null && room.Mode == GameMode.TeamConsensus)
                    {
                        var currentPlayers = await _gameManager.GetPlayersAsync(gameId);
                        room.Players = currentPlayers.ToList();

                        await HandleFailover(room, playerId);

                        await _gameManager.UpdateGameAsync(room);

                        await BroadcastGameState(room);
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        private async Task BroadcastGameState(GameRoom room)
        {
            var stateMsg = new GameStateMessage
            {
                GameId = room.GameId,
                GameName = room.GameName,
                Players = room.Players,
                Fen = room.Fen,
                Teams = room.Teams,
                Mode = room.Mode,
                Capacity = room.Capacity,
                PiecePermissions = room.PiecePermissions,
                ActiveProposals = room.ActiveProposals,
                LastMoveAt = room.LastMoveAt
            };

            await Clients.Group(room.GameId).ReceiveGameState(stateMsg);
        }
        private async Task HandleFailover(GameRoom room, string leaverId)
        {
            // Se non aveva permessi specifici, nulla da fare
            if (!room.PiecePermissions.TryGetValue(leaverId, out var lostPermissions)) return;

            // Trova il colore del leaver (se non c'è, esci)
            if (!room.Teams.TryGetValue(leaverId, out string? myTeamColor)) return;

            // Trova un compagno di squadra sopravvissuto
            var teammate = room.Players.FirstOrDefault(p =>
                p.PlayerId != leaverId &&
                room.Teams.ContainsKey(p.PlayerId) &&
                room.Teams[p.PlayerId] == myTeamColor
            );

            if (teammate != null)
            {
                // Trasferisci i permessi al compagno
                if (!room.PiecePermissions.ContainsKey(teammate.PlayerId))
                    room.PiecePermissions[teammate.PlayerId] = new List<char>();

                room.PiecePermissions[teammate.PlayerId].AddRange(lostPermissions);

                // Rimuoviamo i duplicati per pulizia
                room.PiecePermissions[teammate.PlayerId] = room.PiecePermissions[teammate.PlayerId].Distinct().ToList();
            }
        }
    }
}