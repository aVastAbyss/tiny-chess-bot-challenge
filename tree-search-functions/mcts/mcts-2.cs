using ChessChallenge.API;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

public class MyBot : IChessBot
{
    class Node
    {
        public double Value;
        public Move NodeMove;
        public Node[] Children;
    }

    Random rand = new Random();
    int[] pieceValues = {0, 1, 3, 3, 5, 9, 128};

    public Move Think(Board board, Timer timer)
    {
        Node rootNode = new Node();
        rootNode.Value = board.IsWhiteToMove ? -128 : 128;
        double eval = 0;

        while (timer.MillisecondsElapsedThisTurn < 500)
        {
            eval = SelectLeaf(board, rootNode);
        }

        Node bestNode = rootNode.Children[0];

        foreach (Node childNode in rootNode.Children)
        {
            if (childNode.Value >= bestNode.Value)      // Only plays well as white (FIX THIS)
                bestNode = childNode;
        }

        Console.WriteLine(bestNode.Value);
        return bestNode.NodeMove;
    }

    // Selects a random leaf node and performs the other steps
    double SelectLeaf(Board board, Node node)
    {
        double eval = 0;

        // Expand if its a leaf node
        if (node.Children == null)
        {
            eval = ExpandNode(board, node);
        }

        // Explore a random node
        else if (rand.Next(1, 100) <= 100)
        {
            eval = SelectLeaf(board, node.Children[rand.Next(0, node.Children.Length)]);
        }

        // Otherwise select the best node (TO DO)

        double colorValue = board.IsWhiteToMove ? -1 : 1;
        node.Value = colorValue * Math.Max(colorValue * node.Value, colorValue * eval);
        return node.Value;
    }

    // Creates a child nodes for a given leaf node and evaluates a random one
    double ExpandNode(Board board, Node node)
    {
        Move[] legalMoves = board.GetLegalMoves();
        node.Children = new Node[legalMoves.Length];

        for (int i = 0; i < legalMoves.Length; i++)
        {
            node.Children[i] = new Node();

            // board.IsWhiteToMove is the color of the parent node
            node.Children[i].Value = board.IsWhiteToMove ? 128 : -128;
            node.Children[i].NodeMove = legalMoves[i];
        }

        Node randNode = node.Children[rand.Next(0, node.Children.Length)];
        board.MakeMove(randNode.NodeMove);

        double eval = EvaluatePosition(board);
        randNode.Value = eval;

        board.UndoMove(randNode.NodeMove);

        return eval;
    }

    double EvaluatePosition(Board board)
    {
        PieceList[] piecesOnBoard = board.GetAllPieceLists();
        double noise = rand.Next(0, 100);
        double eval = noise / 100 - 0.5;

        foreach (PieceList pieceList in piecesOnBoard)
        {
            int pieceValue = pieceList.IsWhitePieceList ? 1 : -1;
            eval += pieceValues[(int)pieceList[0].PieceType] * pieceList.Count * pieceValue;
        }

        return Math.Round(eval, 2);
    }
}
