using ChessBackend.Hubs;
using ChessBackend.Manager;
using GameEngine;
using Microsoft.AspNetCore.SignalR;
using Shared.Interfaces;
using Shared.Messages;
using Shared.Models;

namespace ChessBackend.Services
{
    public class ProposalTimeoutService : BackgroundService
    {
        private readonly GameManager _gameManager;
        private readonly IHubContext<GameHub, IChessClient> _hubContext;
        private readonly ChessLogic _chessLogic;
        private readonly ILogger<ProposalTimeoutService> _logger;
        private readonly TimeSpan _turnDuration = TimeSpan.FromSeconds(120);

        public ProposalTimeoutService(
            GameManager gameManager,
            IHubContext<GameHub, IChessClient> hubContext,
            ChessLogic chessLogic,
            ILogger<ProposalTimeoutService> logger)
        {
            _gameManager = gameManager;
            _hubContext = hubContext;
            _chessLogic = chessLogic;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
                await CheckTurnTimeouts();
            }
        }

        private async Task CheckTurnTimeouts()
        {
            var games = await _gameManager.GetAllGamesAsync();

            foreach (var room in games)
            {
                // (Dovresti aggiungere un flag IsGameActive in GameRoom per ottimizzare, ma per ora va bene)
                if (DateTime.UtcNow - room.LastMoveAt > _turnDuration)
                {
                    await ForceMoveExecution(room);
                }
            }
        }

        private async Task ForceMoveExecution(GameRoom room)
        {
            _logger.LogInformation($"Timeout scaduto per partita {room.GameId}. Forzatura mossa.");

            char turnColor = _chessLogic.GetTurnColor(room.Fen);
            string turnString = turnColor.ToString();

            var validProposals = room.ActiveProposals
                .Where(p => room.Teams.ContainsKey(p.ProposerId) && room.Teams[p.ProposerId] == turnString)
                .ToList();

            MoveProposal? selectedProposal = null;

            if (validProposals.Count > 0)
            {
                int maxVotes = validProposals.Max(p => p.Votes.Count);
                var topProposals = validProposals.Where(p => p.Votes.Count == maxVotes).ToList();
                var rnd = new Random();
                selectedProposal = topProposals[rnd.Next(topProposals.Count)];
            }
            else
            {
                _logger.LogWarning($"Nessuna proposta per partita {room.GameId}. Generazione mossa random.");

                if (_chessLogic.GetRandomMove(room.Fen, out string rFrom, out string rTo, out string rNewFen))
                {
                    selectedProposal = new MoveProposal
                    {
                        ProposerId = "Server_Auto",
                        From = rFrom,
                        To = rTo
                    };
                }
                else
                {
                    room.LastMoveAt = DateTime.UtcNow;
                    await _gameManager.UpdateGameAsync(room);
                    return;
                }
            }

            if (selectedProposal != null)
            {
                if (_chessLogic.TryMakeMove(room.Fen, selectedProposal.From, selectedProposal.To, out string newFen))
                {
                    room.Fen = newFen;
                    room.LastMoveAt = DateTime.UtcNow;
                    room.ActiveProposals.Clear();

                    await _gameManager.UpdateGameAsync(room);

                    var moveMadeMsg = new MoveMadeMessage
                    {
                        GameId = room.GameId,
                        PlayerId = selectedProposal.ProposerId,
                        From = selectedProposal.From,
                        To = selectedProposal.To,
                        Fen = newFen
                    };
                    await _hubContext.Clients.Group(room.GameId).MoveMade(moveMadeMsg);

                    var emptyUpdate = new ActiveProposalsUpdateMessage
                    {
                        GameId = room.GameId,
                        Proposals = new List<MoveProposal>()
                    };
                    await _hubContext.Clients.Group($"{room.GameId}_w").ActiveProposalsUpdate(emptyUpdate);
                    await _hubContext.Clients.Group($"{room.GameId}_b").ActiveProposalsUpdate(emptyUpdate);

                    if (_chessLogic.IsCheckmate(newFen))
                    {
                        var gameOverMsg = new GameOverMessage
                        {
                            GameId = room.GameId,
                            WinnerPlayerId = selectedProposal.ProposerId,
                            Reason = "Checkmate (Timeout Decision)"
                        };
                        await _hubContext.Clients.Group(room.GameId).GameOver(gameOverMsg);

                        // Opzionale: Rimuovi partita
                        // await _gameManager.RemoveGameAsync(room.GameId);
                    }
                }
            }
        }
    }
}