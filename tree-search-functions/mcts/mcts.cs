using ChessChallenge.API;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

public class MyBot : IChessBot
{
    Random rand = new Random();

    struct Node
    {
        public Board board;
        public int netValue;
        public int numOfVisits;
    }

    public Move Think(Board board, Timer timer)
    {
        return MCTS(board, 1000);
    }

    Move MCTS(Board board, int iterations)
    {
    
    }

    Move SelectNode(Board board)
    {
        Move[] legalMoves = board.GetLegalMoves();
        Node[] nodes = 

        for (int i = 0; i < legalMoves.Length; i++)
        {
            
        }
    }
}
