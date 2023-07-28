// numOfNodesVisited is a globally defined integer that stores the number of nodes visited
// alpha and beta are initialized to -127 and 127 respectively
// pieceValues is a globally defined array that contains the values of each type of piece

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

    Move[] orderedMoves = GetOrderedMoves(board);
    double eval;

    if (board.IsWhiteToMove)
    {
        double maxEval = -128;
        foreach (Move move in orderedMoves)
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
        foreach (Move move in orderedMoves)
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

Move[] GetOrderedMoves(Board board)
{
    Move[] legalMoves = board.GetLegalMoves();

    int[] moveValues = new int[legalMoves.Length];
    Move[] movesToSort = new Move[legalMoves.Length];

    for (int i = 0; i < legalMoves.Length; i++)
    {
        Move move = legalMoves[i];

        moveValues[i] = 0;
        movesToSort[i] = move;

        Piece attackingPiece = board.GetPiece(move.StartSquare);
        Piece capturedPiece = board.GetPiece(move.TargetSquare);

        moveValues[i] += pieceValues[(int)capturedPiece.PieceType];

        if (board.SquareIsAttackedByOpponent(move.TargetSquare))
        {
            moveValues[i] -= pieceValues[(int)attackingPiece.PieceType];
        }
    }

    Array.Sort(moveValues, movesToSort);
    Array.Reverse(movesToSort);
    return movesToSort;
}
