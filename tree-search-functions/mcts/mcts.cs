using ChessChallenge.API;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

public class Node
{
    Random rand = new Random();
    int[] pieceValues = {0, 1, 3, 3, 5, 9, 128};

    public Board State;
    public Move? ParentMove;

    public int Depth;
    public int Visits = 0;

    public double Value;

    public Node? Parent;
    public List<Node> Children = new List<Node>();

    public Node(Board board, int depth, Move? move = null, Node? parentNode = null)
    {
        State = board;
        Depth = depth;
        ParentMove = move;
        Parent = parentNode;

        Value = (board.IsWhiteToMove ? -1 : 1) * 128;
    }

    // Selects a leaf node
    public Node SelectLeaf()
    {
        // Returns the current node if it's a leaf node
        if (Children.Count == 0)
            return this;

        // Explores a new node
        if (rand.Next(1, 100) <= 50)
            return Children[rand.Next(0, Children.Count)].SelectLeaf();

        // Exploits current knowledge by selecting best node
        Node bestChildNode = GetBestChild();
        return bestChildNode.SelectLeaf();
    }

    // Returns the best child node
    public Node GetBestChild()
    {
        double bestValue = -128;
        int bestNodeIndex = 0;

        double colorValue = State.IsWhiteToMove ? 1 : -1;

        for (int i = 0; i < Children.Count; i++)
        {
            if (colorValue * Children[i].Value >= bestValue)
            {
                bestNodeIndex = i;
                bestValue = colorValue * Children[i].Value;
            }
        }

        return Children[bestNodeIndex];
    }

    // Creates child nodes for a given leaf node and selects a random one
    public Node Expand()
    {
        Move[] actions = State.GetLegalMoves();

        foreach (Move move in actions)
        {
            Board newState = Board.CreateBoardFromFEN(State.GetFenString());
            newState.MakeMove(move);
            Children.Add(new Node(newState, Depth + 1, move, this));
        }

        return Children[rand.Next(0, Children.Count)];
    }

    // Uses an evaluation function instead of random rollouts
    public double Evaluate()
    {
        if (State.IsInCheckmate())
            return State.IsWhiteToMove ? -128 : 128;

        if (State.IsDraw())
            return 0;

        PieceList[] piecesOnBoard = State.GetAllPieceLists();
        double eval = 0;

        foreach (PieceList pieceList in piecesOnBoard)
        {
            int colorValue = pieceList.IsWhitePieceList ? 1 : -1;
            eval += pieceValues[(int)pieceList[0].PieceType] * pieceList.Count * colorValue;
        }

        /*
        string[] pawnSquares = {"d5", "e5", "d4", "e4"};
        string[] centerSquares = {"c6", "d6", "e6", "f6",
                                  "c5", "d5", "e5", "f5",
                                  "c4", "d4", "e4", "f4",
                                  "c3", "d3", "e3", "f3"};

        foreach (string squareName in centerSquares)
        {
            Square square = new Square(squareName);

            if (pawnSquares.Contains(squareName) && State.GetPiece(square).IsPawn && State.PlyCount < 20)
                eval += State.GetPiece(square).IsWhite ? 1 : -1;

            if (State.GetPiece(square).IsKnight && State.PlyCount < 20)
                eval += State.GetPiece(square).IsWhite ? 1 : -1;
        }
        */

        return eval;
    }

    // Updates the attributes of each node up the tree
    public void Backprop(double eval)
    {
        int colorValue = State.IsWhiteToMove ? 1 : -1;

        Value = colorValue * Math.Max(colorValue * Value, colorValue * eval);
        Visits++;

        if (Parent != null)
            Parent.Backprop(eval);
    }
}

public class MyBot : IChessBot
{
    Random rand = new Random();

    public Move Think(Board board, Timer timer)
    {
        int iterations = 1000;

        Node rootNode = new Node(board, 0);
        Node leafNode = rootNode.SelectLeaf();

        int maxDepth = 0;

        for (int i = 0; i < iterations; i++)
        {
            leafNode = rootNode.SelectLeaf();
            leafNode = leafNode.Expand();
            double eval = leafNode.Evaluate();
            leafNode.Backprop(eval);

            maxDepth = Math.Max(maxDepth, leafNode.Depth);
        }

        Node bestNode = rootNode.GetBestChild();

        Console.WriteLine(bestNode.Visits);
        Console.WriteLine(bestNode.Value);

        return bestNode.ParentMove.Value;
    }
}
