namespace GameEngine.Board;

public static class BoardFactory
{
    public static Board CreateInitialBoard()
    {
        var board = new Board();

        // Pedoni bianchi
        for (int file = 0; file < 8; file++)
            board.SetPiece(1, file, new Piece(PieceColor.White, PieceType.Pawn));

        // Pedoni neri
        for (int file = 0; file < 8; file++)
            board.SetPiece(6, file, new Piece(PieceColor.Black, PieceType.Pawn));

        // Prima fila bianca
        board.SetPiece(0, 0, new Piece(PieceColor.White, PieceType.Rook));
        board.SetPiece(0, 7, new Piece(PieceColor.White, PieceType.Rook));
        board.SetPiece(0, 1, new Piece(PieceColor.White, PieceType.Knight));
        board.SetPiece(0, 6, new Piece(PieceColor.White, PieceType.Knight));
        board.SetPiece(0, 2, new Piece(PieceColor.White, PieceType.Bishop));
        board.SetPiece(0, 5, new Piece(PieceColor.White, PieceType.Bishop));
        board.SetPiece(0, 3, new Piece(PieceColor.White, PieceType.Queen));
        board.SetPiece(0, 4, new Piece(PieceColor.White, PieceType.King));

        // Prima fila nera
        board.SetPiece(7, 0, new Piece(PieceColor.Black, PieceType.Rook));
        board.SetPiece(7, 7, new Piece(PieceColor.Black, PieceType.Rook));
        board.SetPiece(7, 1, new Piece(PieceColor.Black, PieceType.Knight));
        board.SetPiece(7, 6, new Piece(PieceColor.Black, PieceType.Knight));
        board.SetPiece(7, 2, new Piece(PieceColor.Black, PieceType.Bishop));
        board.SetPiece(7, 5, new Piece(PieceColor.Black, PieceType.Bishop));
        board.SetPiece(7, 3, new Piece(PieceColor.Black, PieceType.Queen));
        board.SetPiece(7, 4, new Piece(PieceColor.Black, PieceType.King));

        return board;
    }
}
