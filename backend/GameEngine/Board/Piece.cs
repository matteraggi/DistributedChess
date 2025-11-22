namespace GameEngine.Board;

public class Piece
{
    public PieceColor Color { get; }
    public PieceType Type { get; }

    public Piece(PieceColor color, PieceType type)
    {
        Color = color;
        Type = type;
    }
}
