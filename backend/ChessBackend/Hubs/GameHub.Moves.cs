using Microsoft.AspNetCore.SignalR;
using Shared.Messages;
using Shared.Models;
using GameEngine;

namespace ChessBackend.Hubs
{
    public partial class GameHub
    {
        public async Task MakeMove(MakeMoveMessage msg)
        {
            string playerId;
            if (Context.Items.TryGetValue("PlayerId", out var pidObj) && pidObj is string pid)
                playerId = pid;
            else
                playerId = msg.PlayerId;

            var room = await _gameManager.GetGameAsync(msg.GameId);
            if (room == null) throw new HubException("Game not found");

            // Chiediamo all'Engine di chi è il turno guardando la FEN
            char turnColor = _chessLogic.GetTurnColor(room.Fen); // 'w'

            if (room.Teams.TryGetValue(playerId, out string? playerColor) && playerColor != null)
            {
                if (playerColor[0] != turnColor)
                {
                    throw new HubException("Not your turn!");
                }
            }
            else
            {
                throw new HubException("You are not playing in this match!");
            }

            // L'Engine ci dirà se è valida e ci darà la nuova FEN.
            if (!_chessLogic.TryMakeMove(room.Fen, msg.From, msg.To, out string newFen))
            {
                throw new HubException("Illigal move!");
            }

            room.Fen = newFen; // La nuova scacchiera

            await _gameManager.UpdateGameAsync(room);

            var moveMadeMsg = new MoveMadeMessage
            {
                GameId = room.GameId,
                PlayerId = playerId,
                From = msg.From,
                To = msg.To,
                Fen = newFen
            };

            await Clients.Group(msg.GameId).MoveMade(moveMadeMsg);

            if (_chessLogic.IsCheckmate(newFen))
            {
                var gameOverMsg = new GameOverMessage
                {
                    GameId = room.GameId,
                    WinnerPlayerId = playerId,
                    Reason = "Checkmate"
                };

                await Clients.Group(msg.GameId).GameOver(gameOverMsg);

                //await _gameManager.RemoveGameAsync(room.GameId); 
            }
            else if (_chessLogic.IsStalemate(newFen))
            {
                // Gestione pareggio...
            }
        }
    }
}