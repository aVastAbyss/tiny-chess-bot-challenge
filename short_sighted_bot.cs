using ChessChallenge.API;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

public class MyBot : IChessBot
{
    int[] pieceValues = {-3, 1, 3, 3, 5, 9, 12};
    Random rand = new Random();

    public Move Think(Board board, Timer timer)
    {
        Move[] legalMoves = board.GetLegalMoves();
        Move[] captureMoves = board.GetLegalMoves(true);

        foreach (Move move in legalMoves)
        {
            if (MoveIsCheckmate(board, move))   // Always play checkmate in one
            {
                return move;
            }
        }

        if (captureMoves.Length > 0)
        {
            return GetBestCapture(board, legalMoves);   // Assume most captures are trades and choose captures accordingly
        }

        return TryPushPawn(board, legalMoves);  // Sometimes push a pawn when available, else choose a random move
    }

    bool MoveIsCheckmate(Board board, Move move)
    {
        board.MakeMove(move);
        bool isMate = board.IsInCheckmate();
        board.UndoMove(move);

        return isMate;
    }

    Move GetBestCapture(Board board, Move[] possibleMoves)
    {
        List<int> netCaptureValues = new List<int>();

        foreach (Move move in possibleMoves)
        {

            Piece attackingPiece = board.GetPiece(move.StartSquare);
            Piece capturedPiece = board.GetPiece(move.TargetSquare);

            int attackingPieceValue = pieceValues[(int)attackingPiece.PieceType];
            int capturedPieceValue = pieceValues[(int)capturedPiece.PieceType];

            netCaptureValues.Add(capturedPieceValue - attackingPieceValue + 3);
        }

        return possibleMoves[netCaptureValues.IndexOf(netCaptureValues.Max())];
    }
    
    Move TryPushPawn(Board board, Move[] legalMoves)
    {
        foreach (Move move in legalMoves)
        {
            Piece movingPiece = board.GetPiece(move.StartSquare);
            PieceList[] piecesOnBoard = board.GetAllPieceLists();
            int pawnCount = piecesOnBoard[6 - Convert.ToInt32(board.IsWhiteToMove) * 6].Count;

            if ((int)movingPiece.PieceType == 1 && rand.Next(0, pawnCount * 2) == 0)
            {
                return move;
            }
        }

        return legalMoves[rand.Next(0, legalMoves.Length - 1)];
    }
}
