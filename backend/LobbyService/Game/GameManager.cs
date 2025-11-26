using Shared.Redis;
using Shared.Models;

namespace DistributedChess.LobbyService.Game
{
    public class GameManager
    {
        private readonly RedisService _redis;

        public GameManager(RedisService redis)
        {
            _redis = redis;
        }

        // Gestione delle partite
        public Task<GameRoom> CreateGameAsync(string name) => _redis.CreateGameAsync(name);
        public Task<bool> RemoveGameAsync(string id) => _redis.RemoveGameAsync(id);
        public Task UpdateGameAsync(GameRoom room) => _redis.UpdateGameAsync(room);
        public Task<GameRoom?> GetGameAsync(string id) => _redis.GetGameAsync(id);
        public Task<IEnumerable<GameRoom>> GetAllGamesAsync() => _redis.GetAllGamesAsync();

        // Gestione giocatori nelle partite
        public Task AddPlayerAsync(string gameId, string playerId, string playerName) =>
            _redis.AddPlayerToGameAsync(gameId, playerId, playerName);

        public Task RemovePlayerAsync(string gameId, string playerId) =>
            _redis.RemovePlayerFromGameAsync(gameId, playerId);

        public Task SetPlayerReadyAsync(string gameId, string playerId, bool ready) =>
            _redis.SetPlayerReadyInGameAsync(gameId, playerId, ready);

        public Task<List<Player>> GetPlayersAsync(string gameId) =>
            _redis.GetPlayersInGameAsync(gameId);

        public Task<IEnumerable<(string PlayerId, string PlayerName)>> GetPlayersExcludingAsync(string gameId, string excludedPlayerId) =>
            _redis.GetPlayersExcludingAsync(gameId, excludedPlayerId);
    }
}
