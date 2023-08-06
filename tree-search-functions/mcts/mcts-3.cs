using ChessChallenge.API;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

public class Node
{
    public double Value = 0;
    public int Visits = 0;
    public double UCT = 0;

    public Move? ParentMove;
    public Node? Parent;
    public Node[] Children;

    public Node(Node? parentNode = null, Move? parentMove = null)
    {
        Parent = parentNode;
        ParentMove = parentMove;
    }
}

public class MyBot : IChessBot
{
    int[] pieceValues = {0, 1, 3, 3, 5, 9, 128};
    Random rand = new Random();

    public Move Think(Board board, Timer timer)
    {
        Node rootNode = new Node();
        double eval;

        for (int iter = 500; iter > 0; iter--)
        {
            Node node = SelectLeaf(board, rootNode);

            if (board.IsInCheckmate() || board.IsDraw())
            {
                eval = EvaluateBoard(board);
                Backpropagate(board, node, eval);
                continue;
            }

            node = ExpandNode(board, node);
            eval = EvaluateBoard(board);
            Backpropagate(board, node, eval);
        }

        double maxUCT = -128;
        Move[] legalMoves = board.GetLegalMoves();
        Move bestMove = legalMoves[0];

        for (int i = 0; i < rootNode.Children.Length; i++)
            if (rootNode.Children[i].UCT > maxUCT)
            {
                maxUCT = rootNode.Children[i].UCT;
                bestMove = legalMoves[i];
            }

        return bestMove;
    }

    // returns the highest value leaf node
    Node SelectLeaf(Board board, Node node)
    {
        // returns the current node if it has no children
        if (node.Children == null)
           return node; 

        // selects the child node with the highest UCT value
        double maxUCT = -128;
        Move[] legalMoves = board.GetLegalMoves();
        Move bestMove = legalMoves[0];
        Node bestChild = node.Children[0];

        for (int i = 0; i < node.Children.Length; i++)
            if (node.Children[i].UCT > maxUCT)
            {
                maxUCT = node.Children[i].UCT;
                bestChild = node.Children[i];
                bestMove = legalMoves[i];
            }

        board.MakeMove(bestMove);
        return SelectLeaf(board, bestChild);
    }

    double CalculateUCT(Node node)
    {
        const double explorationConstant = 1.41;

        if (node.Visits == 0)
            return 128;

        double winRate = node.Value / node.Visits;
        return winRate + explorationConstant * Math.Sqrt(Math.Log(node.Parent.Visits) / node.Visits);
    }

    // returns the likelihood of winning as a probability from -1 to 1
    double EvaluateBoard(Board board)
    {
        if (board.IsInCheckmate())
            return board.IsWhiteToMove ? -1 : 1;

        if (board.IsDraw())
            return 0;

        double eval = 0;

        foreach (PieceList pieceList in board.GetAllPieceLists())
        {
            int colorValue = pieceList.IsWhitePieceList ? 1 : -1;
            eval += pieceValues[(int)pieceList[0].PieceType] * pieceList.Count * colorValue;
        }

        return 0.9 * Math.Tanh(eval * 0.25);
    }

    // returns a random child node given a leaf node
    Node ExpandNode(Board board, Node node)
    {
        Move[] legalMoves = board.GetLegalMoves();
        node.Children = new Node[legalMoves.Length];

        // create child nodes
        for (int i = 0; i < legalMoves.Length; i++)
        {
            node.Children[i] = new Node(node, legalMoves[i]);
            node.Children[i].UCT = CalculateUCT(node.Children[i]);
        }

        // select a random child node
        Node childNode = node.Children[rand.Next(0, legalMoves.Length)];
        board.MakeMove(childNode.ParentMove);
        return childNode;
    }

    // traverses back up the tree while updating nodes
    void Backpropagate(Board board, Node node, double eval)
    {
        while (node.Parent != null)
        {
            node.Value += eval;
            node.Visits++;

            board.UndoMove(node.ParentMove);
            node = node.Parent;
            eval = -eval;
        }

        node.Value += eval;
        node.Visits++;
    }
}
