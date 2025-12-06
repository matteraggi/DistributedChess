using Microsoft.AspNetCore.SignalR;
using Shared.Messages;
using Shared.Models;

namespace ChessBackend.Hubs
{
    public partial class GameHub
    {
        // 1. PROPONI MOSSA
        public async Task ProposeMove(ProposeMoveMessage msg)
        {
            var playerId = GetPlayerId();
            var room = await _gameManager.GetGameAsync(msg.GameId);
            if (room == null) throw new HubException("Game not found");

            // A. CONTROLLO TURNO (È il turno della mia squadra?)
            char turnColor = _chessLogic.GetTurnColor(room.Fen);
            if (room.Teams.TryGetValue(playerId, out string? pColor) && pColor[0] != turnColor)
            {
                throw new HubException("Not your turn!");
            }

            // B. CONTROLLO SHARDING (Possiedo questo pezzo?)
            // Se siamo in modalità TeamConsensus e ci sono permessi definiti
            if (room.Mode == GameMode.TeamConsensus &&
                room.PiecePermissions.TryGetValue(playerId, out var allowedPieces) &&
                allowedPieces != null && allowedPieces.Count > 0)
            {
                // Chiediamo all'engine che pezzo c'è nella casella di partenza
                char? pieceChar = _chessLogic.GetPieceTypeAt(room.Fen, msg.From);

                if (pieceChar == null) throw new HubException("No piece at source!");

                char pieceType = char.ToUpper(pieceChar.Value);

                if (!allowedPieces.Contains(pieceType))
                {
                    throw new HubException($"You don't control {pieceType} pieces! You can only move: {string.Join(",", allowedPieces)}");
                }
            }

            room.ActiveProposals.RemoveAll(p => p.ProposerId == playerId);

            // C. CREA O AGGIORNA PROPOSTA
            // Nel modello competitivo, aggiungiamo una nuova proposta alla lista
            var proposal = new MoveProposal
            {
                ProposerId = playerId,
                From = msg.From,
                To = msg.To,
                Promotion = msg.Promotion
            };

            // Il proponente vota automaticamente per se stesso
            // mettere in frontend: proponi o vota
            proposal.Votes.Add(playerId);

            // Aggiungi alla lista
            room.ActiveProposals.Add(proposal);

            // Salva su Redis
            await _gameManager.UpdateGameAsync(room);

            await BroadcastProposals(room);

            await CheckVoteResult(room);
        }

        // 2. VOTA MOSSA
        public async Task VoteMove(VoteMessage msg)
        {
            var playerId = GetPlayerId();
            var room = await _gameManager.GetGameAsync(msg.GameId);
            if (room == null) return;

            var targetProposal = room.ActiveProposals.FirstOrDefault(p => p.ProposalId == msg.ProposalId);
            if (targetProposal == null) return;

            if (!room.Teams.TryGetValue(playerId, out string? voterColor)) return;

            if (!room.Teams.TryGetValue(targetProposal.ProposerId, out string? proposerColor)) return;

            if (voterColor != proposerColor)
            {
                return;
            }

            foreach (var prop in room.ActiveProposals)
            {
                if (room.Teams[prop.ProposerId] == voterColor)
                    prop.Votes.Remove(playerId);
            }

            targetProposal.Votes.Add(playerId);

            await _gameManager.UpdateGameAsync(room);
            await BroadcastProposals(room);
            await CheckVoteResult(room);
        }

        // 3. CONTEGGIO E ESECUZIONE
        private async Task CheckVoteResult(GameRoom room)
        {
            char turnColor = _chessLogic.GetTurnColor(room.Fen);

            // Solo i membri del team attivo
            var teamPlayerIds = room.Teams.Where(t => t.Value[0] == turnColor).Select(t => t.Key).ToList();
            int teamSize = teamPlayerIds.Count;
            int majorityNeeded = (teamSize / 2) + 1;

            // Quanti voti totali sono stati espressi
            var teamProposals = room.ActiveProposals
                .Where(p => room.Teams.ContainsKey(p.ProposerId) && room.Teams[p.ProposerId][0] == turnColor)
                .ToList();

            int totalVotesCast = teamProposals.Sum(p => p.Votes.Count);


            MoveProposal? winningProposal = null;

            // CASO A: Qualcuno ha già la maggioranza assoluta? (Vittoria immediata)
            winningProposal = teamProposals.FirstOrDefault(p => p.Votes.Count >= majorityNeeded);

            // CASO B: Tutti hanno votato ma c'è stallo? (es. 1 vs 1 in un team da 2)
            if (winningProposal == null && totalVotesCast >= teamSize)
            {
                // Troviamo le proposte con il massimo dei voti
                int maxVotes = teamProposals.Max(p => p.Votes.Count);
                var topProposals = teamProposals.Where(p => p.Votes.Count == maxVotes).ToList();

                if (topProposals.Count == 1)
                {
                    winningProposal = topProposals[0];
                }
                else
                {
                    winningProposal = topProposals.OrderBy(p => p.CreatedAt).First();
                }
            }


            if (winningProposal != null)
            {
                await ExecuteMoveInternal(room, winningProposal);

                room.ActiveProposals.Clear();
                await _gameManager.UpdateGameAsync(room);

                await BroadcastProposals(room);
            }
        }

        private async Task BroadcastProposals(GameRoom room)
        {
            // Filtra le proposte per i Bianchi
            var whiteProposals = room.ActiveProposals
                .Where(p => room.Teams.ContainsKey(p.ProposerId) && room.Teams[p.ProposerId] == "w")
                .ToList();

            // Filtra le proposte per i Neri
            var blackProposals = room.ActiveProposals
                .Where(p => room.Teams.ContainsKey(p.ProposerId) && room.Teams[p.ProposerId] == "b")
                .ToList();

            // Invia al gruppo dei Bianchi
            await Clients.Group($"{room.GameId}_w").ActiveProposalsUpdate(new ActiveProposalsUpdateMessage
            {
                GameId = room.GameId,
                Proposals = whiteProposals
            });

            // Invia al gruppo dei Neri
            await Clients.Group($"{room.GameId}_b").ActiveProposalsUpdate(new ActiveProposalsUpdateMessage
            {
                GameId = room.GameId,
                Proposals = blackProposals
            });
        }

        private string GetPlayerId()
        {
            if (Context.Items.TryGetValue("PlayerId", out var pidObj) && pidObj is string pid)
            {
                return pid;
            }

            return Context.ConnectionId;
        }
    }
}