using System.Text.Json.Serialization;

public class GameStartMessage
{
    [JsonPropertyName("gameId")]
    public string GameId { get; set; } = "";

    [JsonPropertyName("fen")]
    public string Fen { get; set; } = "";
    [JsonPropertyName("teams")]
    public Dictionary<string, string> Teams { get; set; } = new();
}