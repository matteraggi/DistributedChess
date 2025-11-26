using Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messages
{
    public class RequestGameStateMessage : BaseMessage
    {
        public RequestGameStateMessage()
        {
            Type = MessageType.RequestGameState;
        }
        public string GameId { get; set; } = "";
        public string PlayerId { get; set; } = "";
    }
}
