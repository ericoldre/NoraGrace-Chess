using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoraGrace.Engine.Evaluation
{
    public interface IChessEval
    {
        int EvalFor(Board board, Player who);
        int DrawScore { get; set; }
    }

    public class Evaluator: IChessEval
    {

        
        public const int MaxValue = int.MaxValue - 100;
        public const int MinValue = -MaxValue;

        public readonly PawnEvaluator _evalPawns;
        public readonly MaterialEvaluator _evalMaterial;
        private readonly PcSqEvaluator _evalPcSq;
        private readonly KingAttackEvaluator _evalKing;
        private readonly MobilityEvaluator _evalMobility;





        protected readonly Settings _settings;





        public static readonly Evaluator Default = new Evaluator();

        public static int TotalEvalCount = 0;

        public int DrawScore { get; set; }
        


        

        public Evaluator()
            : this(Settings.Default())
        {

        }

        public Evaluator(Settings settings)
        {
            _settings = settings.CloneDeep();

            //setup pawn evaluation
            _evalPawns = new PawnEvaluator(_settings, 10000);
            _evalMaterial = new MaterialEvaluator(_settings.MaterialValues);
            _evalPcSq = new PcSqEvaluator(settings);
            _evalKing = new KingAttackEvaluator(settings.KingAttack);
            _evalMobility = new MobilityEvaluator(settings.Mobility);
            //setup mobility arrays






            

        }

        public PcSqEvaluator PcSq
        {
            get { return _evalPcSq; }
        }

        public int EvalFor(Board board, Player who)
        {
            int retval = Eval(board);
            if (who == Player.Black) { retval = -retval; }
            return retval;

        }

        public PlyData _plyData = new PlyData();

        public virtual int Eval(Board board, PlyData plyData = null)
        {
            var material = _evalMaterial.EvalMaterialHash(board);
            var pawns = _evalPawns.PawnEval(board);
            if (plyData == null) { plyData = _plyData; }

            plyData.EvalResults.MaterialPawnsApply(board, material, pawns, DrawScore);
            EvalAdvanced(board, plyData, material, pawns);
            return plyData.EvalResults.Score;
        }


        public void EvalAdvanced(Board board, PlyData plyData, MaterialResults material, PawnResults pawns)
        {

            EvalResults evalInfo = plyData.EvalResults;

            TotalEvalCount++;

            //mark that advanced eval terms are from this ply
            evalInfo.LazyAge = 0;

            plyData.AttacksWhite.Initialize(board);
            plyData.AttacksBlack.Initialize(board);
            var whiteAttackInfo = plyData.AttacksWhite;
            var blackAttackInfo = plyData.AttacksBlack;

            

            Bitboard whiteAttackers;
            Bitboard blackAttackers;

            var whiteMobility = _evalMobility.EvaluateMyPieces(board, Player.White, evalInfo, plyData, out whiteAttackers);
            var blackMobility = _evalMobility.EvaluateMyPieces(board, Player.Black, evalInfo, plyData, out blackAttackers);
            evalInfo.Mobility = whiteMobility.Subtract(blackMobility);

            var whiteKingAttack = _evalKing.EvaluateMyKingAttack(board, Player.White, evalInfo, plyData, whiteAttackers);
            var blackKingAttack = _evalKing.EvaluateMyKingAttack(board, Player.Black, evalInfo, plyData, blackAttackers);
            evalInfo.KingAttack = whiteKingAttack - blackKingAttack;

            //first check for unstoppable pawns, if none, then eval normal passed pawns.
            evalInfo.PawnsPassed = PawnEvaluator.EvalUnstoppablePawns(board, pawns.PassedPawns, pawns.Candidates);
            if (evalInfo.PawnsPassed == 0)
            {
                evalInfo.PawnsPassed = this._evalPawns.EvalPassedPawns(board, plyData, pawns.PassedPawns, pawns.Candidates, evalInfo.Workspace); ;
            }

            //shelter storm;
            if (material.DoShelter)
            {
                evalInfo.ShelterStorm = PhasedScoreUtil.Create(_settings.PawnShelterFactor * pawns.EvalShelter(
                    whiteKingFile: board.KingPosition(Player.White).ToFile(),
                    blackKingFile: board.KingPosition(Player.Black).ToFile(),
                    castleFlags: board.CastleRights), 0);
            }
            else
            {
                evalInfo.ShelterStorm = 0;
            }
            
            //test to see if we are just trying to force the king to the corner for mate.
            PhasedScore endGamePcSq = 0;
            if (PcSqEvaluator.UseEndGamePcSq(board, Player.White, out endGamePcSq))
            {
                evalInfo.PcSq = endGamePcSq;
                evalInfo.Mobility = 0;
            }
            else if (PcSqEvaluator.UseEndGamePcSq(board, Player.Black, out endGamePcSq))
            {
                evalInfo.PcSq = endGamePcSq.Negate();
                evalInfo.Mobility = 0;
            }


        }


        public const Bitboard OUTPOST_AREA = (Bitboard.FileC | Bitboard.FileD | Bitboard.FileE | Bitboard.FileF)
            & (Bitboard.Rank3 | Bitboard.Rank4 | Bitboard.Rank5 | Bitboard.Rank6);


        public static PhasedScore EvaluateOutpost(Board board, Player me, PieceType pieceType, Position pos)
        {
            System.Diagnostics.Debug.Assert((Attacks.PawnAttacks(pos, me.PlayerOther()) & board[me, PieceType.Pawn]) != 0); //assert is guarded by own pawn;
            System.Diagnostics.Debug.Assert(OUTPOST_AREA.Contains(pos)); //is in the designated outpost area.

            if (!Attacks.PawnAttacksFlood(pos, me).Contains(board[me.PlayerOther(), PieceType.Pawn]))
            {
                int dist = Math.Max(pos.DistanceToNoDiag(board.KingPosition(me.PlayerOther())) - 4, 0);
                int score = Math.Max(0, 15 - dist * 2);
                if (pieceType == PieceType.Bishop)
                {
                    score = score / 2;
                }
                return PhasedScoreUtil.Create(score, 0);
            }
            else
            {
                return 0;
            }

        }


        protected virtual float CalcStartWeight(int basicMaterialCount)
        {
            //full material would be 62
            if (basicMaterialCount >= 56)
            {
                return 1;
            }
            else if (basicMaterialCount <= 10)
            {
                return 0;
            }
            else
            {
                int rem = basicMaterialCount - 10;
                float retval = (float)rem / 46f;
                return retval;
            }
        }



        
    }

   

}
