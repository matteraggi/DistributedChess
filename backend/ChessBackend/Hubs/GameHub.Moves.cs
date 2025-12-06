using Microsoft.AspNetCore.SignalR;
using Shared.Messages;
using Shared.Models;
using GameEngine;

namespace ChessBackend.Hubs
{
    public partial class GameHub
    {
        private async Task ExecuteMoveInternal(GameRoom room, MoveProposal prop)
        {
            if (!_chessLogic.TryMakeMove(room.Fen, prop.From, prop.To, out string newFen))
            {
                // Se siamo qui, i client hanno raggiunto il consenso su una mossa illegale.
                // Blocchiamo tutto per sicurezza.
                throw new HubException("Security Alert: Consensus reached on an illegal move!");
            }

            room.Fen = newFen;
            room.LastMoveAt = DateTime.UtcNow;

            var moveMadeMsg = new MoveMadeMessage
            {
                GameId = room.GameId,
                PlayerId = prop.ProposerId,
                From = prop.From,
                To = prop.To,
                Fen = newFen
            };

            await Clients.Group(room.GameId).MoveMade(moveMadeMsg);

            if (_chessLogic.IsCheckmate(newFen))
            {
                var gameOverMsg = new GameOverMessage
                {
                    GameId = room.GameId,
                    WinnerPlayerId = prop.ProposerId,
                    Reason = "Checkmate"
                };

                await Clients.Group(room.GameId).GameOver(gameOverMsg);
            }
            else if (_chessLogic.IsStalemate(newFen))
            {
                // Gestione pareggio...
            }
        }
    }
}