using ChessChallenge.API;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

public class MyBot : IChessBot
{
    int[] pieceValues = {0, 1, 3, 3, 5, 9, 64};
    Random rand = new Random();

    public Move Think(Board board, Timer timer)
    {
        Move[] legalMoves = board.GetLegalMoves();
        List<double> values = new List<double>();

        foreach (Move move in legalMoves)
        {
            board.MakeMove(move);
            values.Add(Minimax(board, 3));
            board.UndoMove(move);
        }

        if (board.IsWhiteToMove)
        {
            Console.WriteLine(values.Max());
            return legalMoves[values.IndexOf(values.Max())];
        }

        Console.WriteLine(values.Min());
        return legalMoves[values.IndexOf(values.Min())];
    }

    double Minimax(Board board, int depth)
    {
        if (board.IsInCheckmate())
        {
            return (Convert.ToInt32(board.IsWhiteToMove) - 0.5) * -128;
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
        List<double> values = new List<double>();

        foreach (Move move in legalMoves)
        {
            board.MakeMove(move);
            values.Add(Minimax(board, depth - 1));
            board.UndoMove(move);
        }

        if (board.IsWhiteToMove)
        {
            return values[values.IndexOf(values.Max())];
        }

        return values[values.IndexOf(values.Min())];
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
