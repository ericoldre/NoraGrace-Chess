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

        public PhasedScore EvaluateMyPieces(Board board, Player me, EvalResults info, PlyData plyData, out Bitboard kingInvolved)
        {
            kingInvolved = Bitboard.Empty;

            PhasedScore mobility = 0;
            var him = me.PlayerOther();

            var myAttackInfo = plyData.AttacksFor(me);
            var hisAttackInfo = plyData.AttacksFor(him);

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

            Bitboard MobilityTargets = ~myPieces & ~(hisAttackInfo.ByPieceType(PieceType.Pawn));

            Bitboard myDiagSliders = myPieces & board.BishopSliders;
            Bitboard myHorizSliders = myPieces & board.RookSliders;
            //Bitboard potentialOutputs = Evaluator.OUTPOST_AREA & (myAttacks.PawnEast | myAttacks.PawnWest);

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
                        break;
                    case PieceType.Bishop:
                        slidingAttacks = Attacks.BishopAttacks(pos, pieceLocationsAll ^ myDiagSliders);
                        break;
                    case PieceType.Rook:
                        slidingAttacks = Attacks.RookAttacks(pos, pieceLocationsAll ^ myHorizSliders);

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
                        slidingAttacks = Attacks.RookAttacks(pos, pieceLocationsAll ^ myHorizSliders);
                        slidingAttacks |= Attacks.BishopAttacks(pos, pieceLocationsAll ^ myDiagSliders);
                        break;
                }

                // calc mobility score
                int mobilityCount = (slidingAttacks & MobilityTargets).BitCount();
                mobility = mobility.Add(_mobilityPieceTypeCount[(int)pieceType][mobilityCount]);

                //see if involved in a king attack
                if ((hisKingZone & slidingAttacks) != Bitboard.Empty)
                {
                    //myAttacks.KingAttackerCount++;
                    //myAttacks.KingAttackerWeight += KingAttackEvaluator.KingAttackerWeight(pieceType);
                    kingInvolved |= pos.ToBitboard();
                }

            }

            return mobility;
        }

    }
}
