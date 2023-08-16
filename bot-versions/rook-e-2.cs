using ChessChallenge.API;
using System;
using System.Linq;

public class MyBot : IChessBot
{
    // private static TunerSettings tunerSettings;
    // private Board lastBoard;
    // private bool started = false;

    // ORDER: qsearch min depth, null move reduction, late move reduction min depth, late move reduction depth reduction
    // private static int[] hyperparams = new int[4]; // TODO get num of hyperparams

    // static MyBot()
    // {
    //     double[] valueMins =  { 0.0, 1.0, 4.0, 1.0 }; // TODO make mins
    //     double[] valueMaxes = { 2.0, 4.0, 8.0, 3.0 }; // TODO make maxes
    //     tunerSettings = new TunerSettings(new[] { valueMins, valueMaxes }, X =>
    //     {
    //         for (int i = 0; i < X.Length; i++)
    //         {
    //             hyperparams[i] = (int) Math.Round(X[i]);
    //         }
    //     });
    // }

    // public MyBot()
    // {
    //     Console.Write("Hyperparams ");
    //     for (int i = 0; i < hyperparams.Length; i++)
    //     {
    //         Console.Write(hyperparams[i] + " ");
    //     }
    //     Console.WriteLine();
    // }

    // ~MyBot()
    // {
    //     if (lastBoard != null)
    //         tunerSettings.endRound(lastBoard);
    // }
    
    Move bestmoveRoot = Move.NullMove;
    private int nodesSearched;

    // https://www.chessprogramming.org/PeSTO%27s_Evaluation_Function
    int[] pieceVal = {0, 100, 310, 330, 500, 1000, 10000 }; // TODO tune
    int[] piecePhase = {0, 0, 1, 1, 2, 4, 0};
    ulong[] psts = {657614902731556116, 420894446315227099, 384592972471695068, 312245244820264086, 364876803783607569, 366006824779723922, 366006826859316500, 786039115310605588, 421220596516513823, 366011295806342421, 366006826859316436, 366006896669578452, 162218943720801556, 440575073001255824, 657087419459913430, 402634039558223453, 347425219986941203, 365698755348489557, 311382605788951956, 147850316371514514, 329107007234708689, 402598430990222677, 402611905376114006, 329415149680141460, 257053881053295759, 291134268204721362, 492947507967247313, 367159395376767958, 384021229732455700, 384307098409076181, 402035762391246293, 328847661003244824, 365712019230110867, 366002427738801364, 384307168185238804, 347996828560606484, 329692156834174227, 365439338182165780, 386018218798040211, 456959123538409047, 347157285952386452, 365711880701965780, 365997890021704981, 221896035722130452, 384289231362147538, 384307167128540502, 366006826859320596, 366006826876093716, 366002360093332756, 366006824694793492, 347992428333053139, 457508666683233428, 329723156783776785, 329401687190893908, 366002356855326100, 366288301819245844, 329978030930875600, 420621693221156179, 422042614449657239, 384602117564867863, 419505151144195476, 366274972473194070, 329406075454444949, 275354286769374224, 366855645423297932, 329991151972070674, 311105941360174354, 256772197720318995, 365993560693875923, 258219435335676691, 383730812414424149, 384601907111998612, 401758895947998613, 420612834953622999, 402607438610388375, 329978099633296596, 67159620133902};

    // https://www.chessprogramming.org/Transposition_Table
    // struct TTEntry {
    //     public ulong key;
    //     public Move move;
    //     public int depth, score, bound;
    //     public TTEntry(ulong _key, Move _move, int _depth, int _score, int _bound) {
    //         key = _key; move = _move; depth = _depth; score = _score; bound = _bound;
    //     }
    // }
    record struct TTEntry(ulong key, Move move, int depth, int score, int bound);
    
    const int entries = 1 << 20;
    TTEntry[] tt = new TTEntry[entries];
    private Move[] killerMoves = new Move[200];

    public int getPstVal(int psq) {
        return (int)(((psts[psq / 10] >> (6 * (psq % 10))) & 63) - 20) * 8;
    }

    public int Evaluate(Board board)
    {
        nodesSearched++;
        int mg = 0, eg = 0, phase = 0;
    
        foreach(bool stm in new[] {true, false}) {
            for(var p = PieceType.Pawn; p <= PieceType.King; p++) {
                int piece = (int)p, ind;
                ulong mask = board.GetPieceBitboard(p, stm);
                while(mask != 0) {
                    phase += piecePhase[piece];
                    ind = 128 * (piece - 1) + BitboardHelper.ClearAndGetIndexOfLSB(ref mask) ^ (stm ? 56 : 0);
                    mg += getPstVal(ind) + pieceVal[piece];
                    eg += getPstVal(ind + 64) + pieceVal[piece];
                }
            }
    
            mg = -mg;
            eg = -eg;
        }
    
        return (mg * phase + eg * (24 - phase)) / 24 * (board.IsWhiteToMove ? 1 : -1);
    }

    // https://www.chessprogramming.org/Negamax
    // https://www.chessprogramming.org/Quiescence_Search
    public int Search(Board board, Timer timer, int alpha, int beta, int depth, int ply) {
        // if (board.IsInCheck())
        //     depth++;
        
        ulong key = board.ZobristKey;
        bool qsearch = depth <= 0, // TODO tune
            notRoot = ply > 0;
        // bool qsearch = depth <= hyperparams[0];
        int best = -99999999;

        // Check for repetition (this is much more important than material and 50 move rule draws)
        if(notRoot && board.IsRepeatedPosition())
            return 0;

        TTEntry entry = tt[key % entries];

        // TT cutoffs
        if(notRoot && entry.key == key && entry.depth >= depth && (
            entry.bound == 3 // exact score
                || entry.bound == 2 && entry.score >= beta // lower bound, fail high
                || entry.bound == 1 && entry.score <= alpha // upper bound, fail low
        )) return entry.score;

        int eval = Evaluate(board);
        
        bool doNullMove = depth >= 2 && !board.IsInCheck() && eval >= beta;
        if (doNullMove && board.TrySkipTurn()) {
            int R = 2; // Reduction amount for Null Move // TODO tune
            // int R = hyperparams[1];
            int nullScore = -Search(board, timer, -beta, -beta + 1, depth - 1 - R, ply + 1);
            board.UndoSkipTurn();
        
            if (nullScore >= beta)
                // Null Move Pruning
                return nullScore;
        }

        // Quiescence search is in the same function as negamax to save tokens
        if(qsearch) {
            best = eval;
            if(best >= beta) return best;
            alpha = Math.Max(alpha, best);
        }

        // Generate moves, only captures in qsearch
        Move[] moves = board.GetLegalMoves(qsearch);

        Move bestMove = Move.NullMove;
        int origAlpha = alpha;

        // Search moves
        Array.Sort(moves.Select(move =>
        {
            if(move == entry.move) return 1000000;
            int score = move.IsCapture
                ? 10000 * (int)move.CapturePieceType - (int)move.MovePieceType // MVV-LVA
                : killerMoves[ply] == move ? 1000 : 0; // Killer moves
            
            // check if its the same side's move for tempo bonus
            if (!board.IsWhiteToMove && ply % 2 == 0)
                score += 10; // tempo bonus
            
            return score;
        }).ToArray(), moves);
        for(int i = 0; i < moves.Length; i++) {
            if(timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / 30) return 99999999;

            Move move = moves[moves.Length - 1 - i];
            
            board.MakeMove(move);
            int depthReduction = i > 1 && depth >= 6 &&
                                 !(move.IsCapture || move.IsPromotion || board.IsInCheck()) ? 1 : 0; // apply LMR on moves unlikely to be good TODO tune
            // PVS
            bool doZW = i > 1 && !qsearch;
            int score = -Search(board, timer, doZW ? -alpha - 1 : -beta, -alpha, depth - 1 - depthReduction, ply + 1);
            // If the move failed high, turns out the move was good; search it at full depth
            score = doZW && score > alpha && (score < beta || depthReduction != 0)
                ? -Search(board, timer, -beta, -alpha, depth - 1, ply + 1)
                : score;
            
            board.UndoMove(move);

            // New best move
            if(score > best) {
                best = score;
                bestMove = move;
                if(ply == 0) bestmoveRoot = move;

                // Improve alpha
                alpha = Math.Max(alpha, score);

                // Fail-high
                if(alpha >= beta) break;

            }
        }

        // (Check/Stale)mate
        if(!qsearch && moves.Length == 0) return board.IsInCheck() ? -99999999 + ply : 0;

        // Did we fail high/low or get an exact score?
        int bound = best >= beta ? 2 : best > origAlpha ? 3 : 1;

        // Push to TT
        tt[key % entries] = new TTEntry(key, bestMove, depth, best, bound);
        
        // // Update killer moves
        if (alpha >= beta)
            // Insert new best move at the front of the list
            killerMoves[ply] = bestMove;

        return best;
    }
    
    public Move Think(Board board, Timer timer)
    {
        // if (!started)
        // {
        //     tunerSettings.startRound(board.IsWhiteToMove);
        //     started = true;
        // }
        nodesSearched = 0;
        // lastBoard = board;
        
        bestmoveRoot = Move.NullMove;
        // https://www.chessprogramming.org/Iterative_Deepening
        int depth;
        for(depth = 1; depth <= 1000; depth++) {
            Search(board, timer, -99999999, 99999999, depth, 0);

            // Out of time
            if (timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / 30)
                break;
        }
        
        Console.WriteLine("info string MyBot visited " + nodesSearched + " nodes at depth " + depth);
        return bestmoveRoot.IsNull ? board.GetLegalMoves()[0] : bestmoveRoot;
    }
}
