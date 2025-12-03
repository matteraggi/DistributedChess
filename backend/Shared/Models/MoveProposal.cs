using System;
using System.Collections.Generic;

namespace Shared.Models
{
    public class MoveProposal
    {
        public string ProposalId { get; set; } = Guid.NewGuid().ToString();

        // Chi ha fatto la proposta (es. Player A)
        public string ProposerId { get; set; } = string.Empty;

        // La mossa proposta (es. Cavallo in F3)
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public string Promotion { get; set; } = string.Empty; // Opzionale

        // Lista degli ID di chi ha votato per QUESTA proposta
        // Usiamo HashSet per unicità (un player non può votare 2 volte la stessa cosa)
        public HashSet<string> Votes { get; set; } = new();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}