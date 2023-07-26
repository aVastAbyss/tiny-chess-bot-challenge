double Minimax(Board board, int depth)
{
    if (board.IsInCheckmate())
    {
        return (Convert.ToInt32(board.IsWhiteToMove) - 0.5) * -64 * (depth + 1);
    }

    if (board.IsDraw())
    {
        return 0;
    }

    if (depth == 0)
    {
        return EvaluateBoard(board);
    }

    Move[] legalMoves = board.GetLegalMoves();
    double eval;

    if (board.IsWhiteToMove)
    {
        double maxEval = -128;
        foreach (Move move in legalMoves)
        {
            board.MakeMove(move);

            eval = Minimax(board, depth - 1);
            maxEval = Math.Max(eval, maxEval);

            board.UndoMove(move);
        }

        return maxEval;
    }
    else
    {
        double minEval = 128;
        foreach (Move move in legalMoves)
        {
            board.MakeMove(move);

            eval = Minimax(board, depth - 1);
            minEval = Math.Min(eval, minEval);

            board.UndoMove(move);
        }

        return minEval;
    }
}
