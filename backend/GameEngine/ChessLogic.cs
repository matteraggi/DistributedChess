using ChessDotNetCore;

namespace GameEngine
{
    public class ChessLogic
    {
        public string GetInitialFen()
        {
            return "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        }

        public bool TryMakeMove(string currentFen, string from, string to, out string newFen)
        {
            try
            {
                var game = new ChessGame(currentFen);

                var playerToMove = game.CurrentPlayer;

                var move = new Move(from, to, playerToMove);

                if (!game.IsValidMove(move))
                {
                    var promotionMove = new Move(from, to, playerToMove, 'Q');
                    if (game.IsValidMove(promotionMove))
                    {
                        move = promotionMove;
                    }
                    else
                    {
                        newFen = currentFen;
                        return false;
                    }
                }

                game.MakeMove(move, true);

                newFen = game.GetFen();
                return true;
            }
            catch
            {
                newFen = currentFen;
                return false;
            }
        }

        public char GetTurnColor(string fen)
        {
            var game = new ChessGame(fen);
            return game.CurrentPlayer == Player.White ? 'w' : 'b';
        }

        public bool IsCheckmate(string fen)
        {
            var game = new ChessGame(fen);
            return game.IsCheckmated(game.CurrentPlayer);
        }

        public bool IsStalemate(string fen)
        {
            var game = new ChessGame(fen);
            return game.IsStalemated(game.CurrentPlayer);
        }

        public char? GetPieceTypeAt(string fen, string square)
        {
            try
            {
                var game = new ChessGame(fen);
                var piece = game.GetPieceAt(new Position(square));
                if (piece == null) return null;

                return piece.GetFenCharacter();
            }
            catch
            {
                return null;
            }
        }

        public bool GetRandomMove(string currentFen, out string from, out string to, out string newFen)
        {
            from = ""; to = ""; newFen = currentFen;
            try
            {
                var game = new ChessGame(currentFen);
                var possibleMoves = game.GetValidMoves(game.CurrentPlayer);

                if (possibleMoves.Count == 0) return false;

                var rnd = new Random();
                var randomMove = possibleMoves[rnd.Next(possibleMoves.Count)];

                game.MakeMove(randomMove, true);

                from = randomMove.OriginalPosition.ToString();
                to = randomMove.NewPosition.ToString();
                newFen = game.GetFen();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}