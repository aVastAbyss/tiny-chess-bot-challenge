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

        public Board State;
        public Move? NodeAction;

        public int NetValue = 0;
        public int Visits = 0;

        public Node? Parent;
        public List<Node> Children = new List<Node>();

        public Node(Board board, Move? move = null, Node? parentNode = null)
        {
            State = board;
            NodeAction = move;
            Parent = parentNode;
        }

        // Selects a leaf node
        public Node SelectNode()
        {
            // Returns the current node if it's a leaf node
            if (Children.Count == 0)
                return this;

            // Explores a new node
            if (rand.Next(1, 100) <= 25)
                return Children[rand.Next(0, Children.Count)].SelectNode();

            // Exploits current knowledge by selecting best node
            return GetBestChild();
        }

        // Creates child nodes for a given leaf node
        public void Expand()
        {
            Move[] actions = State.GetLegalMoves();

            foreach (Move move in actions)
            {
                State.MakeMove(move);
                Children.Add(new Node(State, move, this));
                State.UndoMove(move);
            }
        }

        // Simulates a game using random moves
        public int Rollout(Node node)
        {
            if (node.State.IsInCheckmate())
                return node.State.IsWhiteToMove ? -1 : 1;

            if (node.State.IsDraw())
                return 0;

            Move[] actions = node.State.GetLegalMoves();
            Move move = actions[rand.Next(0, actions.Length)];

            node.State.MakeMove(move);
            int rolloutValue = node.Rollout(node);
            node.State.UndoMove(move);

            return rolloutValue;
        }

        // Updates the attributes of each node up the tree
        public void Backprop(int rolloutValue)
        {
            NetValue += rolloutValue;
            Visits++;

            if (Parent != null)
                Parent.Backprop(rolloutValue);
        }

        // Returns the best child node
        public Node GetBestChild()
        {
            double bestValue = -1;
            int bestNodeIndex = 0;

            for (int i = 0; i < Children.Count; i++)
            {
                // Nodes that haven't been explored yet are given priority
                if (Children[i].Visits == 0)
                    return Children[i];

                if ((double)Children[i].NetValue / Children[i].Visits >= bestValue)
                {
                    bestNodeIndex = i;
                    bestValue = (double)Children[i].NetValue / Children[i].Visits;
                }
            }

            return Children[bestNodeIndex];
        }
    }

    public Move Think(Board board, Timer timer)
    {
        int iterations = 10000;

        Node rootNode = new Node(board);
        Node node = rootNode.SelectNode();

        for (int i = 0; i < iterations; i++)
        {
            node = rootNode.SelectNode();
            node.Expand();
            int rolloutValue = node.Rollout(node.Children[rand.Next(0, node.Children.Count)]);
            node.Backprop(rolloutValue);
        }

        Node bestNode = rootNode.GetBestChild();
        Console.WriteLine(Math.Round(0.5 + (double)bestNode.NetValue / bestNode.Visits, 2));

        return bestNode.NodeAction.Value;
    }
}
