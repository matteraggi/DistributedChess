namespace Shared.Messages
{
    public class MoveMadeMessage
    {
        public string GameId { get; set; } = string.Empty;
        public string PlayerId { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public string? Promotion { get; set; }
        public string Fen { get; set; } = string.Empty;
    }
}