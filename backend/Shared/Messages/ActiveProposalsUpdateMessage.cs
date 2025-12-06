using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shared.Models; // Per MoveProposal

namespace Shared.Messages
{
    public class ActiveProposalsUpdateMessage
    {
        public string GameId { get; set; } = "";
        public List<MoveProposal> Proposals { get; set; } = new();
    }
}