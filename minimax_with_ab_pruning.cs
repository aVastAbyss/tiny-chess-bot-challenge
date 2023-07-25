using ChessChallenge.API;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

public class MyBot : IChessBot
{
    int[] pieceValues = {0, 1, 3, 3, 5, 9, 128};
    Random rand = new Random();

    public Move Think(Board board, Timer timer)
    {
        Move[] legalMoves = board.GetLegalMoves();
        List<double> evalList = new List<double>();

        foreach (Move move in legalMoves)
        {
            board.MakeMove(move);
            evalList.Add(Minimax(board, -128, 128, 3));
            board.UndoMove(move);
        }

        if (board.IsWhiteToMove)
        {
            Console.WriteLine(evalList.Max());
            return legalMoves[evalList.IndexOf(evalList.Max())];
        }

        Console.WriteLine(evalList.Min());
        return legalMoves[evalList.IndexOf(evalList.Min())];
    }
    
    double Minimax(Board board, double alpha, double beta, int depth)
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
            return EvaluatePosition(board);
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
                    return maxEval;
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
                    return minEval;
                }
            }

            return minEval;
        }
    }

    double EvaluatePosition(Board board)
    {
        PieceList[] piecesOnBoard = board.GetAllPieceLists();
        int noise = rand.Next(0, 100);
        double eval = (double)noise / 100 - 0.5;

        if (board.IsInCheck())
        {
            eval -= (Convert.ToInt32(board.IsWhiteToMove) - 0.5) * 6;
        }

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
