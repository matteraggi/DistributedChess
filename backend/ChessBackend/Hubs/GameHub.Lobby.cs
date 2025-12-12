using Microsoft.AspNetCore.SignalR;
using Shared.Interfaces;
using Shared.Messages;
using Shared.Models;

namespace ChessBackend.Hubs
{
    public partial class GameHub
    {
        public async Task JoinLobby(JoinLobbyMessage msg)
        {
            var connectionId = Context.ConnectionId;
            var playerId = string.IsNullOrEmpty(msg.PlayerId) ? connectionId : msg.PlayerId;

            Context.Items["PlayerId"] = playerId;

            var player = await _lobbyManager.GetPlayerAsync(playerId);

            if (player == null)
            {
                player = new Player
                {
                    PlayerId = playerId,
                    PlayerName = msg.PlayerName,
                    CurrentGameId = null
                };
            }
            else
            {
                player.PlayerName = msg.PlayerName;
            }

            player.ConnectionId = connectionId;

            await _lobbyManager.AddOrUpdatePlayerAsync(player);

            var joinedMsg = new PlayerJoinedLobbyMessage
            {
                PlayerId = playerId,
                PlayerName = msg.PlayerName
            };

            await Clients.All.PlayerJoined(joinedMsg);

            var players = await _lobbyManager.GetAllPlayersAsync();
            var games = await _gameManager.GetAllGamesAsync();

            var lobbyState = new LobbyStateMessage
            {
                Players = players.Select(p => new Player
                {
                    PlayerId = p.PlayerId,
                    PlayerName = p.PlayerName
                }).ToList(),

                Games = games.Select(g => new GameRoom(g.GameId, g.GameName)
                {
                    Players = g.Players,
                    Capacity = g.Capacity
                }).ToList()
            };

            await Clients.Caller.ReceiveLobbyState(lobbyState);
        }
    }
}