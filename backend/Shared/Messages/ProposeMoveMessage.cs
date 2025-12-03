using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Messages
{
    public class ProposeMoveMessage
    {
        public string GameId { get; set; } = "";
        public string PlayerId { get; set; } = "";
        public string From { get; set; } = "";
        public string To { get; set; } = "";
        public string Promotion { get; set; } = "";
    }
}
