// pieceValues is a globally defined array that contains the values of each type of piece

double EvaluateBoard(Board board)
{
    PieceList[] piecesOnBoard = board.GetAllPieceLists();
    double noise = rand.Next(0, 100);
    double eval = noise / 100 - 0.5;

    foreach (PieceList pieceList in piecesOnBoard)
    {
        if (pieceList.IsWhitePieceList)
        {
            eval += pieceValues[(int)pieceList[0].PieceType] * pieceList.Count;
        }
        else
        {
            eval -= pieceValues[(int)pieceList[0].PieceType] * pieceList.Count;   
        }
    }

    string[] pawnSquares = {"d5", "e5", "d4", "e4"};
    string[] centerSquares = {"c6", "d6", "e6", "f6",
                                  "c5", "d5", "e5", "f5",
                                  "c4", "d4", "e4", "f4",
                                  "c3", "d3", "e3", "f3"};

    foreach (string squareName in centerSquares)
    {
        Square square = new Square(squareName);

        if (pawnSquares.Contains(squareName) && board.GetPiece(square).IsPawn && board.PlyCount < 10)
        {
            eval += Convert.ToInt32(board.IsWhiteToMove) - 0.5;
        }

        if (board.GetPiece(square).IsKnight && board.PlyCount < 10)
        {
            eval += Convert.ToInt32(board.IsWhiteToMove) - 0.5;
        }
    }

    return Math.Round(eval, 2);
}
