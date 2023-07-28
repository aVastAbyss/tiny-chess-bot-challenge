// uses minimax with AB pruning and move ordering
// search depth = 4
// uses a naive evaluation function

using ChessChallenge.API;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

public class MyBot : IChessBot
{
    int[] pieceValues = {0, 1, 3, 3, 5, 9, 128};
    Random rand = new Random();
    int numOfNodesVisited;

    public Move Think(Board board, Timer timer)
    {
        Move[] legalMoves = board.GetLegalMoves();
        List<double> evalList = new List<double>();

        numOfNodesVisited = 0;
        foreach (Move move in legalMoves)
        {
            board.MakeMove(move);
            evalList.Add(Minimax(board, -128, 128, 3));
            board.UndoMove(move);
        }

        if (board.IsWhiteToMove)
        {
            return legalMoves[evalList.IndexOf(evalList.Max())];
        }

        return legalMoves[evalList.IndexOf(evalList.Min())];
    }

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
}
