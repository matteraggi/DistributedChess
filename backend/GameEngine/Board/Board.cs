namespace GameEngine.Board;

public class Board
{
    // [rank][file] → rank 0 = lato bianco, file 0 = colonna 'a'
    private readonly Piece?[,] _squares = new Piece?[8, 8];

    public Piece? GetPiece(int rank, int file) => _squares[rank, file];
    public void SetPiece(int rank, int file, Piece? piece) => _squares[rank, file] = piece;

    public Piece?[,] Snapshot()
    {
        var copy = new Piece?[8, 8];
        Array.Copy(_squares, copy, _squares.Length);
        return copy;
    }
}
