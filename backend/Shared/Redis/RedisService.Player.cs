using Shared.Models;
using System.Text.Json;
using StackExchange.Redis;

namespace Shared.Redis
{
    public partial class RedisService
    {
        // Salva o aggiorna un giocatore

        public async Task SetPlayerAsync(Player player)
        {
            var value = JsonSerializer.Serialize(player);
            await Db.StringSetAsync($"player:{player.PlayerId}", value);
        }


        // Ottieni un giocatore
        public async Task<Player?> GetPlayerAsync(string playerId)
        {
            var value = await Db.StringGetAsync($"player:{playerId}");
            if (value.IsNullOrEmpty) return null;
            return JsonSerializer.Deserialize<Player>(value.ToString());
        }

        // Rimuovi un giocatore
        public async Task RemovePlayerAsync(string playerId)
        {
            await Db.KeyDeleteAsync($"player:{playerId}");
        }

        // Ottieni tutti i giocatori (approccio semplice, usa pattern)
        public async Task<List<Player>> GetAllPlayersAsync()
        {
            var server = _redis.GetServer(_redis.GetEndPoints()[0]);
            var keys = server.Keys(pattern: "player:*");

            var players = new List<Player>();
            foreach (var key in keys)
            {
                var value = await Db.StringGetAsync(key);
                if (!value.IsNullOrEmpty)
                {
                    var player = JsonSerializer.Deserialize<Player>(value.ToString());
                    if (player != null)
                        players.Add(player);
                }
            }

            return players;
        }
    }
}
