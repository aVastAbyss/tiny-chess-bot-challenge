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

    return Math.Round(eval, 2);
}
