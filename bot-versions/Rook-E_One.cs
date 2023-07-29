// uses a variation of minimax called negamax with AB pruning and move ordering
// search depth = 4
// uses a center favoring evaluation function

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
            evalList.Add(Negamax(board, -128, 128, 3));
            board.UndoMove(move);
        }

        if (board.IsWhiteToMove)
        {
            Console.WriteLine("Rook-E One, Eval: " + evalList.Max().ToString() + ", Nodes: " + numOfNodesVisited.ToString());
            return legalMoves[evalList.IndexOf(evalList.Max())];
        }

        Console.WriteLine("Rook-E One, Eval: " + evalList.Min().ToString() + ", Nodes: " + numOfNodesVisited.ToString());
        return legalMoves[evalList.IndexOf(evalList.Min())];
    }

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

        Move[] orderedMoves = GetOrderedMoves(board);
        double eval;
        double maxEval = -128;

        foreach (Move move in orderedMoves)
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

    Move[] GetOrderedMoves(Board board)
    {
        Move[] legalMoves = board.GetLegalMoves();

        int[] moveValues = new int[legalMoves.Length];
        Move[] movesArray = new Move[legalMoves.Length];

        for (int i = 0; i < legalMoves.Length; i++)
        {
            Move move = legalMoves[i];

            moveValues[i] = 0;
            movesArray[i] = move;

            Piece attackingPiece = board.GetPiece(move.StartSquare);
            Piece capturedPiece = board.GetPiece(move.TargetSquare);

            moveValues[i] += pieceValues[(int)capturedPiece.PieceType];

            if (board.SquareIsAttackedByOpponent(move.TargetSquare))
            {
                moveValues[i] -= pieceValues[(int)attackingPiece.PieceType];
            }
        }

        Array.Sort(moveValues, movesArray);
        Array.Reverse(movesArray);
        return movesArray;
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
}
