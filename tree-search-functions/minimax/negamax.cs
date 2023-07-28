// numOfNodesVisited is a globally defined integer that stores the number of nodes visited

// this uses a variation of minimax called negamax for token optimization
double Negamax(Board board, int depth)
{
    numOfNodesVisited++;
    int colorValue = (int)((Convert.ToInt32(board.IsWhiteToMove) - 0.5) * 2);

    if (board.IsInCheckmate())
    {
        return -colorValue * 32 * (depth + 1);
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
    double maxEval = -128;

    foreach (Move move in legalMoves)
    {
        board.MakeMove(move);

        eval = colorValue * Negamax(board, depth - 1);
        maxEval = Math.Max(eval, maxEval);

        board.UndoMove(move);
    }

        return colorValue * maxEval;
}
