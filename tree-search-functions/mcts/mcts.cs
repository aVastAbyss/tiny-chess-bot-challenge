using ChessChallenge.API;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

public class MyBot : IChessBot
{
    Random rand = new Random();

    class Node
    {
        Random rand = new Random();
        int[] pieceValues = {0, 1, 3, 3, 5, 9, 128};

        public Board State;
        public Move? NodeAction;

        public int Depth;

        public double NodeValue;
        public int Visits = 0;

        public Node? Parent;
        public List<Node> Children = new List<Node>();

        public Node(Board board, int depth, Move? move = null, Node? parentNode = null)
        {
            State = board;
            Depth = depth;
            NodeAction = move;
            Parent = parentNode;

            NodeValue = (board.IsWhiteToMove ? -1 : 1) * 128;
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

            int colorValue = State.IsWhiteToMove ? 1 : -1;

            for (int i = 0; i < Children.Count; i++)
            {
                // Nodes that haven't been explored yet are given priority
                if (Children[i].Visits == 0)
                    return Children[i];

                if (colorValue * Children[i].NodeValue >= bestValue)
                {
                    bestNodeIndex = i;
                    bestValue = colorValue * Children[i].NodeValue;
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
                State.MakeMove(move);
                Children.Add(new Node(State, Depth + 1, move, this));
                State.UndoMove(move);
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

            return eval;
        }

        // Updates the attributes of each node up the tree
        public void Backprop(double eval)
        {
            int colorValue = State.IsWhiteToMove ? 1 : -1;

            NodeValue = colorValue * Math.Max(colorValue * NodeValue, colorValue * eval);
            Visits++;

            if (Parent != null)
                Parent.Backprop(eval);
        }
    }

    public Move Think(Board board, Timer timer)
    {
        int iterations = 500;

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

        Console.WriteLine(bestNode.NodeValue);

        return bestNode.NodeAction.Value;
    }
}
