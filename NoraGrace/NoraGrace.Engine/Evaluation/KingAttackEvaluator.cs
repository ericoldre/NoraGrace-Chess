using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoraGrace.Engine.Evaluation
{

    public class KingAttackSettings
    {
        public int KingAttackCountValue = 0;
        public int KingAttackWeightValue = 0;
        public int KingAttackWeightCutoff = 0;
        public int KingRingAttack = 0;
        public int KingRingAttackControlBonus = 0;

        public double KingAttackFactor = 0;
        public double KingAttackFactorQueenTropismBonus = 0;
    }

    public class KingAttackEvaluator
    {
        public readonly int KingAttackCountValue = 0;
        public readonly int KingAttackWeightValue = 0;
        public readonly int KingAttackWeightCutoff = 0;
        public readonly int KingRingAttack = 0;
        public readonly int KingRingAttackControlBonus = 0;
        public readonly int[] KingQueenTropismFactor;

        private static Bitboard[] _kingSafetyRegion;
        private static int[] _kingAttackerWeight;

        static KingAttackEvaluator()
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

        public KingAttackEvaluator(KingAttackSettings settings)
        {
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

        public static int KingAttackerWeight(PieceType pieceType)
        {
            return _kingAttackerWeight[(int)pieceType];
        }

        public static Bitboard KingRegion(Position pos)
        {
            return _kingSafetyRegion[(int)pos];
        }

        public int EvaluateMyKingAttack(Board board, Player me, EvalResults info)
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



    }
}
