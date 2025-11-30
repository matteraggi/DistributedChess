using Shared.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Interfaces
{
    public interface IChessClient
    {
        // nome del metodo -> nome del messaggio che arriva al frontend
        Task PlayerJoined(PlayerJoinedLobbyMessage msg);
        Task PlayerLeft(PlayerLeftLobbyMessage msg);
        Task ReceiveLobbyState(LobbyStateMessage msg);
        Task DeletedGame(DeletedGameMessage msg);
        Task PlayerJoinedGame(PlayerJoinedGameMessage msg);
        Task GameCreated(GameCreatedMessage msg);
        Task PlayerLeftGame(PlayerLeftGameMessage msg);
        Task PlayerReadyStatus(PlayerReadyStatusMessage msg);
        Task GameStart(GameStartMessage msg);
        Task ReceiveGameState(GameStateMessage msg);
        Task MakeMove(MakeMoveMessage msg);
        Task MoveMade(MoveMadeMessage msg);
        Task GameOver(GameOverMessage msg);
    }
}
