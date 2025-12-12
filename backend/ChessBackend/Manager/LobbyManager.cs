using Shared.Models;
using Shared.Redis;

public class LobbyManager
{
    private readonly RedisService _redis;

    public LobbyManager(RedisService redis)
    {
        _redis = redis;
    }

    // Salva/aggiorna player su Redis
    public async Task AddOrUpdatePlayerAsync(Player player)
    {
        await _redis.SetPlayerAsync(player);
    }

    public async Task<Player?> GetPlayerAsync(string playerId)
    {
        return await _redis.GetPlayerAsync(playerId);
    }

    public async Task<IEnumerable<Player>> GetAllPlayersAsync()
    {
        return await _redis.GetAllPlayersAsync(); // puoi implementare un metodo in RedisService che ritorna tutti i players
    }

    public async Task RemovePlayerAsync(string playerId)
    {
        await _redis.RemovePlayerAsync(playerId);
    }
}
