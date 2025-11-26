using System.Text.Json;
using Shared.Models;
namespace Shared.Redis
{
    public partial class RedisService
    {
        public async Task<GameRoom> CreateGameAsync(string name)
        {
            var id = Guid.NewGuid().ToString();
            var room = new GameRoom(id, name);

            var json = JsonSerializer.Serialize(room);
            await Db.StringSetAsync($"game:{id}", json);

            // Mantieni un insieme globale di tutti i giochi
            await Db.SetAddAsync("games:all", id);

            return room;
        }

        public async Task<bool> RemoveGameAsync(string id)
        {
            var removed = await Db.KeyDeleteAsync($"game:{id}");
            if (removed)
            {
                await Db.SetRemoveAsync("games:all", id);
            }
            return removed;
        }

        public async Task UpdateGameAsync(GameRoom room)
        {
            var json = JsonSerializer.Serialize(room);
            await Db.StringSetAsync($"game:{room.GameId}", json);
        }

        public async Task<GameRoom?> GetGameAsync(string id)
        {
            var value = await Db.StringGetAsync($"game:{id}");
            if (value.IsNullOrEmpty) return null;
            return JsonSerializer.Deserialize<GameRoom>(value.ToString());
        }

        public async Task<IEnumerable<GameRoom>> GetAllGamesAsync()
        {
            var ids = await Db.SetMembersAsync("games:all");
            var tasks = ids.Select(async id =>
            {
                var value = await Db.StringGetAsync($"game:{id}");
                return value.IsNullOrEmpty ? null : JsonSerializer.Deserialize<GameRoom>(value.ToString());
            });

            var rooms = await Task.WhenAll(tasks);
            return rooms.Where(r => r != null)!;
        }
    }
}
