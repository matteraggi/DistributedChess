using Microsoft.AspNetCore.SignalR;
using Shared.Messages;
using Shared.Models;
using ChessBackend.Helper;

namespace ChessBackend.Hubs
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
                player = new Player
                {
                    PlayerId = playerId,
                    PlayerName = "Player " + (playerId.Length > 4 ? playerId[..4] : playerId)
                };
            }

            player.ConnectionId = Context.ConnectionId;
            player.CurrentGameId = room.GameId;
            player.IsReady = false;

            await _lobbyManager.AddOrUpdatePlayerAsync(player);

            bool isRejoining = room.Players.Any(p => p.PlayerId == playerId);

            if (!isRejoining)
            {
                if (room.Players.Count >= room.Capacity)
                {
                    throw new HubException("Game is full");
                }

                await _gameManager.AddPlayerAsync(room.GameId, playerId, player.PlayerName);
                room.Players.Add(new Player { PlayerId = playerId, PlayerName = player.PlayerName });
            }
            else
            {
                var pLocal = room.Players.First(p => p.PlayerId == playerId);
                pLocal.PlayerName = player.PlayerName;
            }

            if (room.Teams.Count > 0)
            {
                string myColor;

                if (isRejoining && room.Teams.ContainsKey(playerId))
                {
                    myColor = room.Teams[playerId];
                }
                else
                {
                    int whiteCount = room.Teams.Values.Count(c => c == "w");
                    int blackCount = room.Teams.Values.Count(c => c == "b");
                    myColor = (whiteCount <= blackCount) ? "w" : "b";
                    room.Teams[playerId] = myColor;
                }

                if (room.Mode == GameMode.TeamConsensus)
                {
                    var teamMembers = room.Players
                        .Where(p => room.Teams.ContainsKey(p.PlayerId) && room.Teams[p.PlayerId] == myColor)
                        .ToList();

                    GameHelper.AssignShards(teamMembers, room.PiecePermissions);
                }

                await _gameManager.UpdateGameAsync(room);
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, msg.GameId);

            if (room.Teams.TryGetValue(playerId, out string? assignedColor))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"{msg.GameId}_{assignedColor}");
            }

            var joinedMsg = new PlayerJoinedGameMessage
            {
                GameId = room.GameId,
                PlayerId = playerId,
                PlayerName = player.PlayerName,
                Capacity = room.Capacity
            };

            await Clients.All.PlayerJoinedGame(joinedMsg);
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

            var player = await _lobbyManager.GetPlayerAsync(playerId);
            if (player == null) 
            {
                player = new Player
                {
                    PlayerId = playerId,
                    PlayerName = "Player " + (playerId.Length > 4 ? playerId[..4] : playerId)
                };
            }

            player.ConnectionId = Context.ConnectionId;

            var room = await _gameManager.CreateGameAsync(msg.GameName);

            room.Mode = msg.Mode;

            room.Capacity = (msg.Mode == GameMode.TeamConsensus) ? (msg.TeamSize * 2) : 2;

            await _gameManager.UpdateGameAsync(room);

            player.CurrentGameId = room.GameId;
            await _lobbyManager.AddOrUpdatePlayerAsync(player);


            await _gameManager.AddPlayerAsync(room.GameId, playerId, player.PlayerName);

            await Groups.AddToGroupAsync(Context.ConnectionId, room.GameId);

            var joinedMsg = new PlayerJoinedGameMessage
            {
                GameId = room.GameId,
                PlayerId = playerId,
                PlayerName = player.PlayerName,
                Capacity = room.Capacity
            };
            await Clients.Caller.PlayerJoinedGame(joinedMsg);

            var gameCreatedMsg = new GameCreatedMessage
            {
                GameId = room.GameId,
                GameName = room.GameName,
                Capacity = room.Capacity,
                CreatorId = playerId,
                CreatorName = player.PlayerName
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

            var lobbyPlayer = await _lobbyManager.GetPlayerAsync(playerId);
            if (lobbyPlayer != null)
            {
                lobbyPlayer.IsReady = false;
                lobbyPlayer.CurrentGameId = null;

                await _lobbyManager.AddOrUpdatePlayerAsync(lobbyPlayer);
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

                await Clients.All.PlayerLeftGame(leftMsg);
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

            await _gameManager.SetPlayerReadyAsync(msg.GameId, playerId, msg.IsReady);

            var room = await _gameManager.GetGameAsync(msg.GameId);
            if (room == null) return;

            var currentPlayers = await _gameManager.GetPlayersAsync(msg.GameId);

            room.Players = currentPlayers.ToList();

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
                room.Teams = new Dictionary<string, string>();

                for (int i = 0; i < room.Players.Count; i++)
                {
                    string color = (i % 2 == 0) ? "w" : "b";
                    room.Teams[room.Players[i].PlayerId] = color;
                }

                foreach (var p in room.Players)
                {
                    // Rileggiamo da Redis per avere il ConnectionId più fresco
                    var fullPlayer = await _lobbyManager.GetPlayerAsync(p.PlayerId);

                    if (fullPlayer != null && !string.IsNullOrEmpty(fullPlayer.ConnectionId))
                    {
                        string color = room.Teams[p.PlayerId]; // "w" o "b"
                        await Groups.AddToGroupAsync(fullPlayer.ConnectionId, $"{room.GameId}_{color}");
                    }
                }

                room.PiecePermissions = new Dictionary<string, List<char>>();

                if (room.Mode == GameMode.TeamConsensus)
                {
                    var whiteTeam = room.Players.Where(p => room.Teams[p.PlayerId] == "w").ToList();
                    var blackTeam = room.Players.Where(p => room.Teams[p.PlayerId] == "b").ToList();

                    GameHelper.AssignShards(whiteTeam, room.PiecePermissions);
                    GameHelper.AssignShards(blackTeam, room.PiecePermissions);
                }


                room.Fen = _chessLogic.GetInitialFen();
                room.LastMoveAt = DateTime.UtcNow;
                await _gameManager.UpdateGameAsync(room);

                var startMsg = new GameStartMessage
                {
                    GameId = room.GameId,
                    Fen = room.Fen,
                    Teams = room.Teams
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

            player.ConnectionId = Context.ConnectionId;
            player.CurrentGameId = msg.GameId;

            await _lobbyManager.AddOrUpdatePlayerAsync(player);

            var currentPlayers = await _gameManager.GetPlayersAsync(msg.GameId);

            if (!currentPlayers.Any(p => p.PlayerId == playerId))
            {
                await _gameManager.AddPlayerAsync(msg.GameId, playerId, player.PlayerName);

                currentPlayers = await _gameManager.GetPlayersAsync(msg.GameId);
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, msg.GameId);

            string? myColor;
            if (room.Teams.TryGetValue(playerId, out myColor))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"{msg.GameId}_{myColor}");
            }

            bool permissionsChanged = false;

            if (room.Mode == GameMode.TeamConsensus && room.Teams.TryGetValue(playerId, out myColor))
            {
                var teamMembers = room.Players
                    .Where(p => room.Teams.ContainsKey(p.PlayerId) && room.Teams[p.PlayerId] == myColor)
                    .ToList();

                GameHelper.AssignShards(teamMembers, room.PiecePermissions);

                await _gameManager.UpdateGameAsync(room);
                permissionsChanged = true;
            }

            var stateMsg = new GameStateMessage
            {
                GameId = room.GameId,
                GameName = room.GameName,
                Players = currentPlayers.ToList(),
                Fen = room.Fen,
                Teams = room.Teams ?? new Dictionary<string, string>(),
                LastMoveAt = room.LastMoveAt,

                Mode = room.Mode,
                PiecePermissions = room.PiecePermissions ?? new Dictionary<string, List<char>>(),
                ActiveProposals = room.ActiveProposals ?? new List<MoveProposal>(),
                Capacity = room.Capacity
            };

            if (permissionsChanged)
            {
                await Clients.Group(msg.GameId).ReceiveGameState(stateMsg);
            }
            else
            {
                await Clients.Caller.ReceiveGameState(stateMsg);
            }
        }
    }
}