// numOfNodesVisited is a globally defined integer that stores the number of nodes visited
// alpha and beta are initialized to -128 and 128 respectively

// this uses a variation of minimax called negamax for token optimization
double Negamax(Board board, double alpha, double beta, int depth)
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

        eval = colorValue * Negamax(board, -beta, -alpha, depth - 1);
        maxEval = Math.Max(eval, maxEval);
        alpha = Math.Max(alpha, eval);

        board.UndoMove(move);

        if (beta <= alpha)
        {
            break;
        }
    }

    return colorValue * maxEval;
}
