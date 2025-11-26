using Shared.Models;
using System.Collections.Generic;

namespace Shared.Messages
{
    public class GameStateMessage : BaseMessage
    {
        public GameStateMessage()
        {
            Type = MessageType.GameState;
        }
        public string GameId { get; set; } = "";
        public List<Player> Players { get; set; } = new();
    }
}