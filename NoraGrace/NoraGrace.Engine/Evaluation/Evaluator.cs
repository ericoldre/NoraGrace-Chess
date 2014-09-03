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


        public readonly int[] _endgameMateKingPcSq;


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



            //initialize pcsq for trying to mate king in endgame, try to push it to edge of board.
            _endgameMateKingPcSq = new int[64];
            foreach (var pos in PositionUtil.AllPositions)
            {
                List<int> distToMid = new List<int>();
                distToMid.Add(pos.DistanceToNoDiag(Position.D4));
                distToMid.Add(pos.DistanceToNoDiag(Position.D5));
                distToMid.Add(pos.DistanceToNoDiag(Position.E4));
                distToMid.Add(pos.DistanceToNoDiag(Position.E5));
                var minDist = distToMid.Min();
                _endgameMateKingPcSq[(int)pos] = minDist * 50;
            }


            

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

            EvalAdvanced(board, plyData, material, pawns);
            return plyData.EvalResults.Score;
        }


        public void EvalAdvanced(Board board, PlyData plyData, MaterialResults material, PawnResults pawns)
        {

            EvalResults evalInfo = plyData.EvalResults;

            TotalEvalCount++;

            //mark that advanced eval terms are from this ply
            evalInfo.LazyAge = 0;

            //set up 
            var attacksWhite = evalInfo.Attacks[(int)Player.White];
            var attacksBlack = evalInfo.Attacks[(int)Player.Black];

            attacksWhite.PawnEast = board[Player.White, PieceType.Pawn].ShiftDirNE();
            attacksWhite.PawnWest = board[Player.White, PieceType.Pawn].ShiftDirNW();
            attacksBlack.PawnEast = board[Player.Black, PieceType.Pawn].ShiftDirSE();
            attacksBlack.PawnWest = board[Player.Black, PieceType.Pawn].ShiftDirSW();

            attacksWhite.King = Attacks.KingAttacks(board.KingPosition(Player.White));
            attacksBlack.King = Attacks.KingAttacks(board.KingPosition(Player.Black));

            _evalMobility.EvaluateMyPieces(board, Player.White, evalInfo);
            _evalMobility.EvaluateMyPieces(board, Player.Black, evalInfo);

            _evalKing.EvaluateMyKingAttack(board, Player.White, evalInfo);
            _evalKing.EvaluateMyKingAttack(board, Player.Black, evalInfo);


            //first check for unstoppable pawns, if none, then eval normal passed pawns.
            evalInfo.PawnsPassed = PawnEvaluator.EvalUnstoppablePawns(board, pawns.PassedPawns, pawns.Candidates);
            if (evalInfo.PawnsPassed == 0)
            {
                evalInfo.PawnsPassed = this._evalPawns.EvalPassedPawns(board, evalInfo.Attacks, pawns.PassedPawns, pawns.Candidates, evalInfo.Workspace); ;
            }

            //evalInfo.PawnsPassed = this._evalPawns.EvalPassedPawns(board, evalInfo.Attacks, pawns.PassedPawns, pawns.Candidates, evalInfo.Workspace); ;
            


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
            if (UseEndGamePcSq(board, Player.White, out endGamePcSq))
            {
                evalInfo.PcSq = endGamePcSq;
                evalInfo.Attacks[0].Mobility = 0;
                evalInfo.Attacks[1].Mobility = 0;
            }
            else if (UseEndGamePcSq(board, Player.Black, out endGamePcSq))
            {
                evalInfo.PcSq = endGamePcSq.Negate();
                evalInfo.Attacks[0].Mobility = 0;
                evalInfo.Attacks[1].Mobility = 0;
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

        protected bool UseEndGamePcSq(Board board, Player winPlayer, out PhasedScore newPcSq)
        {
            Player losePlayer = winPlayer.PlayerOther();
            if (
                board.PieceCount(losePlayer, PieceType.Pawn) == 0
                && board.PieceCount(losePlayer, PieceType.Queen) == 0
                && board.PieceCount(losePlayer, PieceType.Rook) == 0
                && (board.PieceCount(losePlayer, PieceType.Bishop) + board.PieceCount(losePlayer, PieceType.Knight) <= 1))
            {
                if(board.PieceCount(winPlayer, PieceType.Queen) > 0
                    || board.PieceCount(winPlayer, PieceType.Rook) > 0
                    || board.PieceCount(winPlayer, PieceType.Bishop) + board.PieceCount(winPlayer, PieceType.Bishop) >= 2)
                {
                    Position loseKing = board.KingPosition(losePlayer);
                    Position winKing = board.KingPosition(winPlayer);
                    newPcSq = PhasedScoreUtil.Create(0, _endgameMateKingPcSq[(int)loseKing] - (winKing.DistanceTo(loseKing) * 25));
                    return true;
                }
            }
            newPcSq = 0;
            return false;
        }

        
    }

    
    public class ChessEvalAttackInfo
    {
        public Bitboard PawnEast;
        public Bitboard PawnWest;

        public Bitboard Knight;
        public Bitboard Knight2;
        public Bitboard Bishop;

        public Bitboard Rook;
        public Bitboard Rook2;
        public Bitboard Queen;
        public Bitboard King;

        public PhasedScore Mobility;

        public int KingQueenTropism;
        public int KingAttackerWeight;
        public int KingAttackerCount;
        public int KingAttackerScore;

        public void Reset()
        {
            PawnEast = Bitboard.Empty;
            PawnWest = Bitboard.Empty;
            Knight = Bitboard.Empty;
            Knight2 = Bitboard.Empty;
            Bishop = Bitboard.Empty;
            Rook = Bitboard.Empty;
            Rook2 = Bitboard.Empty;
            Queen = Bitboard.Empty;
            King = Bitboard.Empty;
            Mobility = 0;
            KingQueenTropism = 24; //init to far away.
            KingAttackerWeight = 0;
            KingAttackerCount = 0;
            KingAttackerScore = 0;
        }

        public Bitboard All()
        {
            return PawnEast | PawnWest | Knight | Knight2 | Bishop | Rook | Rook2 | Queen | King;
        }

        public int AttackCountTo(Position pos)
        {
            return 0
                + (int)(((ulong)PawnEast >> (int)pos) & 1)
                + (int)(((ulong)PawnWest >> (int)pos) & 1)
                + (int)(((ulong)Knight >> (int)pos) & 1)
                + (int)(((ulong)Knight2 >> (int)pos) & 1)
                + (int)(((ulong)Bishop >> (int)pos) & 1)
                + (int)(((ulong)Rook >> (int)pos) & 1)
                + (int)(((ulong)Rook2 >> (int)pos) & 1)
                + (int)(((ulong)Queen >> (int)pos) & 1)
                + (int)(((ulong)King >> (int)pos) & 1);
        }

        public ChessEvalAttackInfo Reverse()
        {
            return new ChessEvalAttackInfo()
            {
                PawnEast = PawnEast.Reverse(),
                PawnWest = PawnWest.Reverse(),
                Knight = Knight.Reverse(),
                Knight2 = Knight2.Reverse(),
                Bishop = Bishop.Reverse(),
                Rook = Rook.Reverse(),
                Rook2 = Rook2.Reverse(),
                Queen = Queen.Reverse(),
                King = King.Reverse()
            };
        }
    }


}
