using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Messages
{
    public class ProposalResultMessage
    {
        public string GameId { get; set; } = "";
        public string ProposalId { get; set; } = "";
        public bool IsAccepted { get; set; }
        public string Reason { get; set; } = "";
    }
}