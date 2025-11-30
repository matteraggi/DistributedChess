using Shared.Models;
using System.Text.Json;

namespace Shared.Redis
{
    public partial class RedisService
    {
        // Aggiunge un giocatore a una partita
        public async Task AddPlayerToGameAsync(string gameId, string playerId, string playerName)
        {
            var player = new Player { PlayerId = playerId, PlayerName = playerName };
            var json = JsonSerializer.Serialize(player);
            await Db.ListRightPushAsync($"game:{gameId}:players", json);
        }

        // Rimuove un giocatore da una partita
        public async Task RemovePlayerFromGameAsync(string gameId, string playerId)
        {
            var players = await GetPlayersInGameAsync(gameId);
            var toRemove = players.FirstOrDefault(p => p.PlayerId == playerId);
            if (toRemove != null)
            {
                await Db.ListRemoveAsync($"game:{gameId}:players", JsonSerializer.Serialize(toRemove));
            }
        }

        // Aggiorna lo stato "ready" di un giocatore
        public async Task SetPlayerReadyInGameAsync(string gameId, string playerId, bool ready)
        {
            var players = await GetPlayersInGameAsync(gameId);
            var p = players.FirstOrDefault(x => x.PlayerId == playerId);
            if (p != null)
            {
                p.IsReady = ready;
                await Db.ListSetByIndexAsync($"game:{gameId}:players", players.IndexOf(p), JsonSerializer.Serialize(p));
            }
        }

        // Recupera tutti i giocatori di una partita
        public async Task<List<Player>> GetPlayersInGameAsync(string gameId)
        {
            var jsonList = await Db.ListRangeAsync($"game:{gameId}:players");
            return jsonList.Select(j => JsonSerializer.Deserialize<Player>(j.ToString())!).ToList();
        }

        // Restituisce tutti i giocatori tranne uno specifico
        public async Task<IEnumerable<(string PlayerId, string PlayerName)>> GetPlayersExcludingAsync(string gameId, string excludedPlayerId)
        {
            var players = await GetPlayersInGameAsync(gameId);
            return players
                .Where(p => p.PlayerId != excludedPlayerId)
                .Select(p => (p.PlayerId, p.PlayerName));
        }
    }
}
