using Microsoft.AspNetCore.SignalR;
using Shared.Messages;
using Shared.Models;

namespace LobbyService.Hubs
{
    public partial class GameHub
    {
        public async Task JoinGame(JoinGameMessage msg)
        {
            string playerId;
            if (Context.Items.TryGetValue("PlayerId", out var pidObj) && pidObj is string pid)
            {
                playerId = pid;
            }
            else
            {
                playerId = string.IsNullOrEmpty(msg.PlayerId) ? Context.ConnectionId : msg.PlayerId;
                Context.Items["PlayerId"] = playerId;
            }

            var room = await _gameManager.GetGameAsync(msg.GameId);
            if (room == null)
            {
                throw new HubException("Game not found");
            }

            var player = await _lobbyManager.GetPlayerAsync(playerId);
            if (player == null)
            {
                throw new HubException("Player not in lobby");
            }

            if (!room.Players.Any(p => p.PlayerId == playerId))
            {
                await _gameManager.AddPlayerAsync(room.GameId, playerId, player.PlayerName);
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, msg.GameId);

            var joinedMsg = new PlayerJoinedGameMessage
            {
                GameId = room.GameId,
                PlayerId = playerId,
                PlayerName = player.PlayerName
            };

            await Clients.Group(msg.GameId).PlayerJoinedGame(joinedMsg);
        }

        public async Task CreateGame(CreateGameMessage msg)
        {
            string playerId;
            if (Context.Items.TryGetValue("PlayerId", out var pidObj) && pidObj is string pid)
            {
                playerId = pid;
            }
            else
            {
                playerId = string.IsNullOrEmpty(msg.PlayerId) ? Context.ConnectionId : msg.PlayerId;
                Context.Items["PlayerId"] = playerId;
            }

            var room = await _gameManager.CreateGameAsync(msg.GameName);

            var player = await _lobbyManager.GetPlayerAsync(playerId);
            if (player == null)
            {
                throw new HubException("Player not found in lobby");
            }

            await _gameManager.AddPlayerAsync(room.GameId, playerId, player.PlayerName);

            // Se UpdateGameAsync serve per salvare modifiche extra, chiamalo, 
            // ma solitamente AddPlayerAsync dovrebbe già persistere.
            // await _gameManager.UpdateGameAsync(room); 

            await Groups.AddToGroupAsync(Context.ConnectionId, room.GameId);

            var joinedMsg = new PlayerJoinedGameMessage
            {
                GameId = room.GameId,
                PlayerId = playerId,
                PlayerName = player.PlayerName
            };
            await Clients.Caller.PlayerJoinedGame(joinedMsg);

            var gameCreatedMsg = new GameCreatedMessage
            {
                GameId = room.GameId,
                GameName = room.GameName
            };

            await Clients.All.GameCreated(gameCreatedMsg);
        }

        public async Task LeaveGame(LeaveGameMessage msg)
        {
            string playerId;
            if (Context.Items.TryGetValue("PlayerId", out var pidObj) && pidObj is string pid)
            {
                playerId = pid;
            }
            else
            {
                playerId = string.IsNullOrEmpty(msg.PlayerId) ? Context.ConnectionId : msg.PlayerId;
            }

            var room = await _gameManager.GetGameAsync(msg.GameId);
            if (room == null)
            {
                throw new HubException("Game not found");
            }

            var redisPlayers = await _gameManager.GetPlayersAsync(msg.GameId);
            var player = redisPlayers.FirstOrDefault(p => p.PlayerId == playerId);

            if (player == null)
            {
                throw new HubException("Player not in this game");
            }

            await _gameManager.RemovePlayerAsync(msg.GameId, playerId);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, msg.GameId);

            room = await _gameManager.GetGameAsync(msg.GameId);

            if (room == null || room.Players.Count == 0)
            {
                await _gameManager.RemoveGameAsync(msg.GameId);

                var removedMsg = new DeletedGameMessage
                {
                    GameId = msg.GameId
                };

                await Clients.All.DeletedGame(removedMsg);
            }
            else
            {
                var leftMsg = new PlayerLeftGameMessage
                {
                    GameId = msg.GameId,
                    PlayerId = playerId,
                    PlayerName = player.PlayerName
                };

                await Clients.Group(msg.GameId).PlayerLeftGame(leftMsg);
            }
        }

        public async Task ReadyGame(ReadyGameMessage msg)
        {
            string playerId;
            if (Context.Items.TryGetValue("PlayerId", out var pidObj) && pidObj is string pid)
            {
                playerId = pid;
            }
            else
            {
                playerId = string.IsNullOrEmpty(msg.PlayerId) ? Context.ConnectionId : msg.PlayerId;
            }

            var room = await _gameManager.GetGameAsync(msg.GameId);
            if (room == null)
            {
                throw new HubException("Game not found");
            }

            await _gameManager.SetPlayerReadyAsync(msg.GameId, playerId, msg.IsReady);

            room = await _gameManager.GetGameAsync(msg.GameId);
            if (room == null) return;

            var readyStatus = room.Players
                .Select(p => new Player
                {
                    PlayerId = p.PlayerId,
                    IsReady = p.IsReady,
                    PlayerName = p.PlayerName
                })
                .ToList();

            var statusMsg = new PlayerReadyStatusMessage
            {
                GameId = room.GameId,
                PlayersReady = readyStatus
            };

            await Clients.Group(msg.GameId).PlayerReadyStatus(statusMsg);

            if (room.Players.All(p => p.IsReady) && room.Players.Count == room.Capacity)
            {
                var startMsg = new GameStartMessage
                {
                    GameId = room.GameId
                };

                await Clients.Group(msg.GameId).GameStart(startMsg);
            }
        }

        public async Task RequestGameState(RequestGameStateMessage msg)
        {
            // Se l'utente ha fatto F5, Context.Items è vuoto. Dobbiamo fidarci del messaggio.
            var playerId = msg.PlayerId;

            Context.Items["PlayerId"] = playerId;

            var room = await _gameManager.GetGameAsync(msg.GameId);
            if (room == null)
            {
                throw new HubException("Game not found");
                throw new HubException("Game not found");
            }

            var player = await _lobbyManager.GetPlayerAsync(playerId);

            if (player == null)
            {
                player = new Player
                {
                    PlayerId = playerId,
                    PlayerName = "Player " + (playerId.Length > 4 ? playerId[..4] : playerId)
                };
            }

            await _lobbyManager.AddOrUpdatePlayerAsync(player.PlayerId, player.PlayerName, Context.ConnectionId);


            var currentPlayers = await _gameManager.GetPlayersAsync(msg.GameId);

            if (!currentPlayers.Any(p => p.PlayerId == playerId))
            {
                await _gameManager.AddPlayerAsync(msg.GameId, playerId, player.PlayerName);

                currentPlayers = await _gameManager.GetPlayersAsync(msg.GameId);
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, msg.GameId);

            var stateMsg = new GameStateMessage
            {
                GameId = room.GameId,
                Players = currentPlayers.ToList()
            };

            await Clients.Caller.ReceiveGameState(stateMsg);
        }
    }
}