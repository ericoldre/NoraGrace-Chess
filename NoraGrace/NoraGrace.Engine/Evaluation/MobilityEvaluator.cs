using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoraGrace.Engine.Evaluation
{

    public class MobilitySettings : ChessPieceTypeDictionary<ChessGameStageDictionary<Helpers.Mobility>>
    {
        public int RookFileOpen = 0;
    }
    public class MobilityEvaluator
    {
        protected readonly PhasedScore RookFileOpen;
        protected readonly PhasedScore RookFileHalfOpen;
        public readonly PhasedScore[][] _mobilityPieceTypeCount = new PhasedScore[PieceTypeUtil.LookupArrayLength][];

        public MobilityEvaluator(MobilitySettings settings)
        {
            RookFileOpen = PhasedScoreUtil.Create(settings.RookFileOpen, settings.RookFileOpen / 2);
            RookFileHalfOpen = PhasedScoreUtil.Create(settings.RookFileOpen / 2, settings.RookFileOpen / 4);

            foreach (PieceType pieceType in new PieceType[] { PieceType.Knight, PieceType.Bishop, PieceType.Rook, PieceType.Queen })
            {
                var pieceSettings = settings[pieceType];

                int[] openingVals = pieceSettings.Opening.GetValues(pieceType.MaximumMoves());
                int[] endgameVals = pieceSettings.Endgame.GetValues(pieceType.MaximumMoves());

                PhasedScore[] combined = PhasedScoreUtil.Combine(openingVals, endgameVals).ToArray();

                _mobilityPieceTypeCount[(int)pieceType] = combined;

            }

        }

        public void EvaluateMyPieces(Board board, Player me, EvalResults info)
        {
            PhasedScore mobility = 0;
            var him = me.PlayerOther();
            var myAttacks = info.Attacks[(int)me];
            var hisAttacks = info.Attacks[(int)him];

            var hisKing = board.KingPosition(him);
            var hisKingZone = KingAttackEvaluator.KingRegion(hisKing);


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
            Bitboard potentialOutputs = Evaluator.OUTPOST_AREA & (myAttacks.PawnEast | myAttacks.PawnWest);

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
                if ((hisKingZone & slidingAttacks) != Bitboard.Empty)
                {
                    myAttacks.KingAttackerCount++;
                    myAttacks.KingAttackerWeight += KingAttackEvaluator.KingAttackerWeight(pieceType);
                }

            }

            myAttacks.Mobility = mobility;
        }

    }
}
