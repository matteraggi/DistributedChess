using System.Text.Json.Serialization;

namespace Shared.Game;

public class BoardSquareDto
{
    [JsonPropertyName("rank")]
    public int Rank { get; set; }   // 0–7

    [JsonPropertyName("file")]
    public int File { get; set; }   // 0–7

    [JsonPropertyName("pieceType")]
    public string? PieceType { get; set; }   // "pawn", "rook", etc.

    [JsonPropertyName("pieceColor")]
    public string? PieceColor { get; set; }  // "white", "black"
}
