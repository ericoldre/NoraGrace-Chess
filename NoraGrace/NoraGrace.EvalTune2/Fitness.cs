using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NoraGrace.Engine;
using NoraGrace.Engine.Evaluation;

namespace NoraGrace.EvalTune2
{
    public class Fitness
    {

        public static Evaluator DefaultEvaluator()
        {
            return new Evaluator(Settings.Default());
        }

        public static double FindFitness(Func<Engine.Evaluation.Evaluator> fnEvaluator, Action<int> progCallback = null)
        {
            return PgnListEParallel(BinaryPGN.Read("noise.bin", progCallback), fnEvaluator);
        }

        public static double PgnListEParallel(IEnumerable<BinaryPGN> pgnList, Func<Engine.Evaluation.Evaluator> fnCreateEval)
        {
            double sum = 0;
            int count = 0;
            object _resultLock = new object();
            Dictionary<System.Threading.Thread, Tuple<Evaluator, MovePicker.Stack>> dic = new Dictionary<System.Threading.Thread, Tuple<Evaluator, MovePicker.Stack>>();

            ParallelOptions parallelOptions = new ParallelOptions();
            parallelOptions.MaxDegreeOfParallelism = 5;
#if DEBUG
            parallelOptions.MaxDegreeOfParallelism = 1;
#endif

            Parallel.ForEach(pgnList, parallelOptions, pgn =>
            {
                Evaluator myThreadEvaluator = null;
                MovePicker.Stack myThreadStack = null;
                lock (_resultLock)
                {
                    System.Threading.Thread currentThread = System.Threading.Thread.CurrentThread;
                    if (!dic.ContainsKey(currentThread))
                    {
                        dic.Add(currentThread, new Tuple<Evaluator, MovePicker.Stack>(fnCreateEval(), new MovePicker.Stack()));
                    }
                    myThreadEvaluator = dic[currentThread].Item1;
                    myThreadStack = dic[currentThread].Item2;
                }


                var pgnE = PgnE(pgn, myThreadEvaluator, myThreadStack);

                lock (_resultLock)
                {
                    if (!double.IsNaN(pgnE))
                    {
                        sum += pgnE;
                        count++;
                    }
                }
            });
            double retval = sum / count;

            return retval;
        }



        public static double PgnE(BinaryPGN pgn, Evaluator evaluator, MovePicker.Stack moveStack)
        {
            Board board = new Board();
            GameResult result = pgn.Result;

            int c = 0;
            double sum = 0;
            for (int i = 0; i < pgn.MoveCount; i++)
            {
                Move move = pgn.Moves[i];
                bool exclude = pgn.Exclude[i];

                board.MoveApply(move);
                if (exclude) { continue; }

                var e = PosE(board, result, evaluator, moveStack);
                sum += e;
                c++;
            }

            var retval = sum / c;

            //System.Diagnostics.Debug.Assert(!double.IsNaN(retval));
            return retval;
        }

        public static double PosE(Board board, GameResult gameResult, Evaluator evaluator, MovePicker.Stack moveStack)
        {
            double qscore = qScore(board, evaluator, moveStack);
            double retval = TanDiff(qscore, gameResult);
            return retval;

        }

        public static double TanPow = 2;
        public static double TanDiv = 340;

        public static double TanDiff(double qscore, GameResult gameResult)
        {
            double tanScore = TanScore(qscore);
            double diff = Math.Abs(tanScore - (double)gameResult);

            double retval = Math.Pow(diff, 2);
            //System.Diagnostics.Debug.Assert(retval >= 0 && retval <= 1);
            return retval;
        }

        public static double TanScore(double qscore)
        {
            double divided = qscore / TanDiv;
            double tan = Math.Tanh(divided);
            return tan;
        }

        public static int qScore(Board board, Evaluator evaluator, MovePicker.Stack moveStack)
        {
            //var playerScore = qSearch(board, evaluator, moveStack);
            var playerScore = evaluator.EvalFor(board, board.WhosTurn);
            return board.WhosTurn == Player.White ? playerScore : -playerScore;
        }

        public static int qSearch(Board board, Evaluator evaluator, MovePicker.Stack moveStack, int ply = 0, int alpha = Evaluator.MinValue, int beta = Evaluator.MaxValue)
        {
            var me = board.WhosTurn;
            var ischeck = board.IsCheck();
            if (!ischeck)
            {
                int staticEval = evaluator.EvalFor(board, me);
                if (staticEval >= beta) { return beta; }
                alpha = Math.Max(alpha, staticEval);
            }

            var plyMoves = moveStack[ply];
            plyMoves.Initialize(board, Move.EMPTY, !ischeck);
            ChessMoveData moveData;
            //Bitboard taken = Bitboard.Empty;

            while ((moveData = plyMoves.NextMoveData()).Move != Move.EMPTY)
            {
                Move move = moveData.Move;

                if (!ischeck)
                {
                    if (moveData.SEE < 0) { continue; }
                    //if ((move.To().ToBitboard() & taken) != Bitboard.Empty) { continue; }
                }
                //taken |= move.To().ToBitboard();

                board.MoveApply(move);

                if (board.IsCheck(board.WhosTurn.PlayerOther()))
                {
                    board.MoveUndo();
                    continue;
                }

                var score = -qSearch(board, evaluator, moveStack, ply + 1, -beta, -alpha);


                board.MoveUndo();

                if (score >= beta) { return beta; }
                alpha = Math.Max(alpha, score);

            }
            return alpha;

        }
    }
}
