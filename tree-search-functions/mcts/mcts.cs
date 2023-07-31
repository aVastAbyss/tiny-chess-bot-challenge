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
        Board GameState;

        int NetValue = 0;
        int TimesVisited = 0;

        List<Node> Children = new List<Node>();

        public Node(Board board)
        {
            GameState = board;
        }

        public Node SelectNode()    // Selects a random leaf node
        {
            if (Children.Count == 0)
            {
                return this;
            }

            // Explore a new node
            if (rand.Next(1, 100) <= 10)
            {
                return Children[rand.Next(0, Children.Count - 1)].SelectNode();
            }

            // Exploit current knowledge
            Node bestNode;
            double bestValue = -128;

            foreach (Node childNode in Children)
            {
                if (childNode.TimesVisited == 0)
                {
                    return childNode;
                }

                if (childNode.NetValue / childNode.TimesVisited > bestValue)
                {
                    bestNode = childNode;
                    bestValue = childNode.NetValue / childNode.TimesVisited;
                }
            }

            return bestNode;
        }

        public void Expand()    // Generates the children of that leaf node
        {
            Move[] actions = GameState.GetLegalMoves();

            foreach (Move move in actions)
            {
                GameState.MakeMove(move);
                Children.Add(new Node(GameState));
                GameState.UndoMove(move);
            }
        }

        public int Rollout()   // Simulates a random game
        {
            if (GameState.IsInCheckmate())
            {
                return (int)((Convert.ToInt32(GameState.IsWhiteToMove) - 0.5) * -2);
            }

            if (GameState.IsDraw())
            {
                return 0;
            }

            int result;

            Move[] actions = GameState.GetLegalMoves();
            Move move = actions[rand.Next(0, actions.Length - 1)];

            GameState.MakeMove(move);
            result = node.Rollout();
            GameState.UndoMove(move);

            return result;
        }
    }

    public Move Think(Board board, Timer timer)
    {
        return MCTS(board, 1000);
    }

    Move MCTS(Board board, int iterations)
    {
        Node rootNode = new Node(board);
        Node node;
        int rolloutResult;

        for (int i = 0; i < iterations; i++)
        {
            node = rootNode.SelectNode();
            node.Expand();
            rolloutResult = node.RollOut();
        }
    }
}
