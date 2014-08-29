using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoraGrace.Engine.Evaluation
{
    public class Lazy
    {
        public static int EvalFor(SearchData sdata, int ply, Board board, Player player, out EvalResults info, int alpha, int beta)
        {
            System.Diagnostics.Debug.Assert(alpha >= Evaluator.MinValue);
            System.Diagnostics.Debug.Assert(beta <= Evaluator.MaxValue);

            if (player == Player.White)
            {
                return Eval(sdata, ply, board, out info, alpha, beta);
            }
            else
            {
                return -Eval(sdata, ply, board, out info, -beta, -alpha);
            }
        }

        public static int Eval(SearchData sdata, int ply, Board board, out EvalResults info, int alpha, int beta)
        {
            System.Diagnostics.Debug.Assert(alpha >= Evaluator.MinValue);
            System.Diagnostics.Debug.Assert(beta <= Evaluator.MaxValue);

            info = sdata[ply].EvalResults;


            //check to see if we already have evaluated.
            if (board.ZobristBoard == info.Zobrist)
            {
                if (info.LazyAge == 0) { return info.Score; }
                if (info.LazyHigh < alpha)
                {
                    return info.LazyHigh;
                }
                else if (info.LazyLow > beta)
                {
                    return info.LazyLow;
                }
            }


            EvalResults prev = null;
            if (ply > 0)
            {
                prev = sdata[ply - 1].EvalResults;
                if (prev.Zobrist != board.ZobristPrevious)
                {
                    prev = null;
                }
            }

            return EvalLazy(sdata.Evaluator, board, info, prev, alpha, beta);

        }

        private static int EvalLazy(Evaluator evaluator, Board board, EvalResults evalInfo, EvalResults prevEvalInfo, int alpha, int beta)
        {
            System.Diagnostics.Debug.Assert(alpha >= Evaluator.MinValue);
            System.Diagnostics.Debug.Assert(beta <= Evaluator.MaxValue);

            evalInfo.Reset();


            //material
            MaterialResults material = evaluator._evalMaterial.EvalMaterialHash(board);

            //pawns
            PawnResults pawns = evaluator._evalPawns.PawnEval(board);
            System.Diagnostics.Debug.Assert(pawns.WhitePawns == (board[PieceType.Pawn] & board[Player.White]));
            System.Diagnostics.Debug.Assert(pawns.BlackPawns == (board[PieceType.Pawn] & board[Player.Black]));


            evalInfo.MaterialPawnsApply(board, material, pawns, evaluator.DrawScore);

            if (prevEvalInfo != null && evalInfo.PassedPawns == prevEvalInfo.PassedPawns)
            {
                evalInfo.ApplyPreviousEval(board, prevEvalInfo);
                System.Diagnostics.Debug.Assert(evalInfo.LazyAge > 0);

                int fuzzyLazyScore = evalInfo.Score;
                int margin = evalInfo.LazyAge * 50;
                if (fuzzyLazyScore + margin < alpha)
                {
                    return alpha;
                }
                if (fuzzyLazyScore - margin > beta)
                {
                    return beta;
                }
            }

            evaluator.EvalAdvanced(board, evalInfo, material, pawns);

            return evalInfo.Score;
        }


    }
}
