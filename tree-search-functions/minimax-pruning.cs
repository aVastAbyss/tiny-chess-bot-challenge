// numOfNodesVisited is a globally defined integer that stores the number of nodes visited

double Minimax(Board board, double alpha, double beta, int depth)
{
    numOfNodesVisited++;

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

            eval = Minimax(board, alpha, beta, depth - 1);
            maxEval = Math.Max(eval, maxEval);
            alpha = Math.Max(alpha, eval);

            board.UndoMove(move);

            if (beta <= alpha)
            {
                break;
            }
        }

        return maxEval;
    }
    else
    {
        double minEval = 128;
        foreach (Move move in legalMoves)
        {
            board.MakeMove(move);

            eval = Minimax(board, alpha, beta, depth - 1);
            minEval = Math.Min(eval, minEval);
            beta = Math.Min(beta, eval);

            board.UndoMove(move);

            if (beta <= alpha)
            {
                break;
            }
        }

        return minEval;
    }
}
