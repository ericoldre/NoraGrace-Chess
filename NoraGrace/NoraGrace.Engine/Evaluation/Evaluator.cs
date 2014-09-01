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

        public readonly PhasedScore[][] _mobilityPieceTypeCount = new PhasedScore[PieceTypeUtil.LookupArrayLength][];

        public readonly int[] _endgameMateKingPcSq;


        protected readonly Settings _settings;

        protected readonly PhasedScore RookFileOpen;
        protected readonly PhasedScore RookFileHalfOpen;

        public readonly int KingAttackCountValue = 0;
        public readonly int KingAttackWeightValue = 0;
        public readonly int KingAttackWeightCutoff = 0;
        public readonly int KingRingAttack = 0;
        public readonly int KingRingAttackControlBonus = 0;
        public readonly int[] KingQueenTropismFactor;

        public static readonly Evaluator Default = new Evaluator();

        public static int TotalEvalCount = 0;

        public int DrawScore { get; set; }
        
        private static Bitboard[] _kingSafetyRegion;
        private static int[] _kingAttackerWeight;

        static Evaluator()
        {
            _kingSafetyRegion = new Bitboard[64];
            foreach (var kingPos in PositionUtil.AllPositions)
            {
                var rank = kingPos.ToRank();
                if (rank == Rank.Rank1) { rank = Rank.Rank2; }
                if (rank == Rank.Rank8) { rank = Rank.Rank7; }
                var file = kingPos.ToFile();
                if (file == File.FileA) { file = File.FileB; }
                if (file == File.FileH) { file = File.FileG; }
                var adjusted = rank.ToPosition(file);
                _kingSafetyRegion[(int)kingPos] = Attacks.KingAttacks(adjusted) | adjusted.ToBitboard();

                System.Diagnostics.Debug.Assert(_kingSafetyRegion[(int)kingPos].BitCount() == 9);

            }

            _kingAttackerWeight = new int[7];
            _kingAttackerWeight[(int)PieceType.Pawn] = 1;
            _kingAttackerWeight[(int)PieceType.Knight] = 2;
            _kingAttackerWeight[(int)PieceType.Bishop] = 2;
            _kingAttackerWeight[(int)PieceType.Rook] = 3;
            _kingAttackerWeight[(int)PieceType.Queen] = 4;
            _kingAttackerWeight[(int)PieceType.King] = 1;
            

        }

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

            //setup mobility arrays

            foreach (PieceType pieceType in new PieceType[]{PieceType.Knight, PieceType.Bishop, PieceType.Rook, PieceType.Queen})
            {
                var pieceSettings = settings.Mobility[pieceType];

                int[] openingVals = pieceSettings.Opening.GetValues(pieceType.MaximumMoves());
                int[] endgameVals = pieceSettings.Endgame.GetValues(pieceType.MaximumMoves());

                PhasedScore[] combined = PhasedScoreUtil.Combine(openingVals, endgameVals).ToArray();

                _mobilityPieceTypeCount[(int)pieceType] = combined;
                //for (int attacksCount = 0; attacksCount < 28; attacksCount++)
                //{
                //    var mobSettings = settings.Mobility;
                    


                //    int startVal = (attacksCount - opiece[GameStage.Opening].ExpectedAttacksAvailable) * opiece[GameStage.Opening].AmountPerAttackDefault;
                //    int endVal = (attacksCount - opiece[GameStage.Endgame].ExpectedAttacksAvailable) * opiece[GameStage.Endgame].AmountPerAttackDefault;

                //    _mobilityPieceTypeCount[(int)pieceType][attacksCount] = PhasedScoreUtil.Create(startVal, endVal);
                //}
            }

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

            RookFileOpen = PhasedScoreUtil.Create(settings.RookFileOpen, settings.RookFileOpen / 2);
            RookFileHalfOpen = PhasedScoreUtil.Create(settings.RookFileOpen / 2, settings.RookFileOpen / 4);

            KingAttackCountValue = settings.KingAttackCountValue;
            KingAttackWeightValue = settings.KingAttackWeightValue;
            KingAttackWeightCutoff = settings.KingAttackWeightCutoff;
            KingRingAttack = settings.KingRingAttack;
            KingRingAttackControlBonus = settings.KingRingAttackControlBonus;

            KingQueenTropismFactor = new int[25];
            for (int d = 0; d <= 24; d++)
            {
                int min = 4;
                int max = 12;
                double minFactor = settings.KingAttackFactor;
                double maxFactor = minFactor + settings.KingAttackFactorQueenTropismBonus;
                
                if (d >= max) { KingQueenTropismFactor[d] = (int)Math.Round(100 * minFactor); continue; }
                if (d <= min) { KingQueenTropismFactor[d] = (int)Math.Round(100 * maxFactor); continue; }

                var each = (1f / (max - min)) * (maxFactor - minFactor);
                double thisFactor = minFactor + (each * (max - d));

                KingQueenTropismFactor[d] = KingQueenTropismFactor[d] = (int)Math.Round(100 * thisFactor);


                //double pct = minFactor + (maxFactor * ((d - min) / (max - min)));

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

            EvaluateMyPieces(board, Player.White, evalInfo);
            EvaluateMyPieces(board, Player.Black, evalInfo);

            EvaluateMyKingAttack(board, Player.White, evalInfo);
            EvaluateMyKingAttack(board, Player.Black, evalInfo);


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

        protected int EvaluateMyKingAttack(Board board, Player me, EvalResults info)
        {
            //king attack info should have counts and weight of everything but pawns and kings.

            var him = me.PlayerOther();
            var myAttacks = info.Attacks[(int)me];
            var hisAttacks = info.Attacks[(int)him];

            var hisKingZone = _kingSafetyRegion[(int)board.KingPosition(him)];

            int retval = 0;
            int c;
            if (myAttacks.KingAttackerCount >= 2 && myAttacks.KingAttackerWeight >= KingAttackWeightCutoff)
            {
                //if we don't at least have a decent attack, just give credit for building count of attackers and move on.

                //add in pawns to king attack.
                Bitboard myInvolvedPawns = Bitboard.Empty;
                Bitboard myPawns = board[me] & board[PieceType.Pawn];

                if (me == Player.White)
                {
                    myInvolvedPawns |= hisKingZone.ShiftDirSE() & myPawns;
                    myInvolvedPawns |= hisKingZone.ShiftDirSW() & myPawns;
                }
                else
                {
                    myInvolvedPawns |= hisKingZone.ShiftDirNE() & myPawns;
                    myInvolvedPawns |= hisKingZone.ShiftDirNW() & myPawns;
                }

                if (myInvolvedPawns != Bitboard.Empty)
                {
                    c = myInvolvedPawns.BitCount();
                    myAttacks.KingAttackerCount += c;
                    myAttacks.KingAttackerWeight += c * _kingAttackerWeight[(int)PieceType.Pawn];
                }

                //add in my king to the attack.
                if ((Attacks.KingAttacks(board.KingPosition(me)) & hisKingZone) != Bitboard.Empty)
                {
                    myAttacks.KingAttackerCount++;
                    myAttacks.KingAttackerWeight += _kingAttackerWeight[(int)PieceType.King];
                }

                //add bonus for piece involvement over threshold;
                retval += (myAttacks.KingAttackerWeight - KingAttackWeightCutoff) * KingAttackWeightValue;

                //now calculate bonus for attacking squares directly surrounding king;
                Bitboard kingAdjecent = Attacks.KingAttacks(board.KingPosition(him));
                while (kingAdjecent != Bitboard.Empty)
                {
                    Position pos = BitboardUtil.PopFirst(ref kingAdjecent);
                    Bitboard posBB = pos.ToBitboard();
                    if ((posBB & myAttacks.All()) != Bitboard.Empty)
                    {
                        retval += KingRingAttack; //attacking surrounding square in some aspect.

                        int myCount = myAttacks.AttackCountTo(pos);
                        int hisCount = hisAttacks.AttackCountTo(pos);
                        if (myCount > hisCount)
                        {
                            retval += KingRingAttackControlBonus * (myCount - hisCount);
                        }
                    }
                }
            }

            retval += KingAttackCountValue * myAttacks.KingAttackerCount;

            retval = (retval * KingQueenTropismFactor[myAttacks.KingQueenTropism]) / 100;
            myAttacks.KingAttackerScore = retval;

            return retval;

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

        protected int EvaluateMyPieces(Board board, Player me, EvalResults info)
        {
            int retval = 0;
            PhasedScore mobility = 0;
            var him = me.PlayerOther();
            var myAttacks = info.Attacks[(int)me];
            var hisAttacks = info.Attacks[(int)him];

            var hisKing = board.KingPosition(him);
            var hisKingZone = _kingSafetyRegion[(int)hisKing];


            Bitboard myPieces = board[me];
            Bitboard pieceLocationsAll = board.PieceLocationsAll;
            Bitboard pawns = board[PieceType.Pawn];

            Bitboard slidersAndKnights = myPieces &
               (board[PieceType.Knight]
               | board[PieceType.Bishop]
               | board[PieceType.Rook]
               | board[PieceType.Queen]);

            Bitboard MobilityTargets = ~myPieces & ~(hisAttacks.PawnEast | hisAttacks.PawnWest);

            Bitboard myDiagSliders = myPieces & board.BishopSliders;
            Bitboard myHorizSliders = myPieces & board.RookSliders;
            Bitboard potentialOutputs = OUTPOST_AREA & (myAttacks.PawnEast | myAttacks.PawnWest);

            while (slidersAndKnights != Bitboard.Empty) //foreach(ChessPosition pos in slidersAndKnights.ToPositions())
            {
                Position pos = BitboardUtil.PopFirst(ref slidersAndKnights);

                PieceType pieceType = board.PieceAt(pos).ToPieceType();

                //generate attacks
                Bitboard slidingAttacks = Bitboard.Empty;

                switch (pieceType)
                {
                    case PieceType.Knight:
                        slidingAttacks = Attacks.KnightAttacks(pos);
                        if (myAttacks.Knight != Bitboard.Empty)
                        {
                            myAttacks.Knight2 |= slidingAttacks;
                        }
                        else
                        {
                            myAttacks.Knight |= slidingAttacks;
                        }
                        //if (potentialOutputs.Contains(pos))
                        //{
                        //    mobility = mobility.Add(EvaluateOutpost(board, me, PieceType.Knight, pos));
                        //}
                        break;
                    case PieceType.Bishop:
                        slidingAttacks = Attacks.BishopAttacks(pos, pieceLocationsAll & ~myHorizSliders);
                        myAttacks.Bishop |= slidingAttacks;
                        //if (potentialOutputs.Contains(pos))
                        //{
                        //    mobility = mobility.Add(EvaluateOutpost(board, me, PieceType.Bishop, pos));
                        //}
                        break;
                    case PieceType.Rook:
                        slidingAttacks = Attacks.RookAttacks(pos, pieceLocationsAll & ~myDiagSliders);
                        if (myAttacks.Rook != Bitboard.Empty)
                        {
                            myAttacks.Rook2 |= slidingAttacks;
                        }
                        else
                        {
                            myAttacks.Rook |= slidingAttacks;
                        }
                        if ((pos.ToFile().ToBitboard() & pawns & myPieces) == Bitboard.Empty)
                        {
                            if ((pos.ToFile().ToBitboard() & pawns) == Bitboard.Empty)
                            {
                                mobility = mobility.Add(RookFileOpen);
                            }
                            else
                            {
                                mobility = mobility.Add(RookFileHalfOpen);
                            }
                        }
                        break;
                    case PieceType.Queen:
                        slidingAttacks = Attacks.QueenAttacks(pos, pieceLocationsAll & ~(myDiagSliders | myHorizSliders));
                        myAttacks.Queen |= slidingAttacks;

                        myAttacks.KingQueenTropism = hisKing.DistanceTo(pos) + hisKing.DistanceToNoDiag(pos);
                        break;
                }

                // calc mobility score
                int mobilityCount = (slidingAttacks & MobilityTargets).BitCount();
                mobility = mobility.Add(_mobilityPieceTypeCount[(int)pieceType][mobilityCount]);

                //see if involved in a king attack
                if((hisKingZone & slidingAttacks) != Bitboard.Empty)
                {
                    myAttacks.KingAttackerCount++;
                    myAttacks.KingAttackerWeight += _kingAttackerWeight[(int)pieceType];
                }
                
            }
            


            myAttacks.Mobility = mobility;
            return retval;
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
