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
            char turnColor = _chessLogic.GetTurnColor(room.Fen);

            string? expectedPlayerId = (turnColor == 'w') ? room.WhitePlayerId : room.BlackPlayerId;

            if (playerId != expectedPlayerId)
            {
                throw new HubException("Non è il tuo turno! (O non sei il colore giusto)");
            }

            // L'Engine ci dirà se è valida e ci darà la nuova FEN.
            if (!_chessLogic.TryMakeMove(room.Fen, msg.From, msg.To, out string newFen))
            {
                throw new HubException("Mossa illegale secondo le regole degli scacchi.");
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
                // Notifica vittoria...
            }
        }
    }
}