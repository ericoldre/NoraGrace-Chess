using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{
    public class ChessEvalPassed
    {
        public static void EvalPassedPawns(ChessBoard board, ChessEvalInfo evalInfo, ChessBitboard passedPawns)
        {
            
            
            var white = passedPawns & board.PlayerLocations(ChessPlayer.White);
            if (!white.Empty())
            {
                foreach (ChessPosition passedPos in white.ToPositions())
                {
                    int mbonus = 0;
                    int ebonus = 0;

                    EvalPassedPawnWhite(board, passedPos, evalInfo.Attacks[(int)ChessPlayer.White], evalInfo.Attacks[(int)ChessPlayer.Black], out mbonus, out ebonus);

                    evalInfo.PawnsPassedStart += mbonus;
                    evalInfo.PawnsPassedEnd += ebonus;
                }
            }

            var black = passedPawns & board.PlayerLocations(ChessPlayer.Black);
            if (!black.Empty())
            {
                foreach (ChessPosition passedPos in black.ToPositions())
                {
                    int mbonus = 0;
                    int ebonus = 0;

                    EvalPassedPawnBlack(board, passedPos, evalInfo.Attacks[(int)ChessPlayer.Black], evalInfo.Attacks[(int)ChessPlayer.White], out mbonus, out ebonus);

                    evalInfo.PawnsPassedStart -= mbonus;
                    evalInfo.PawnsPassedEnd -= ebonus;
                }
            }
            

            

        }

        public static void EvalPassedPawnWhite(ChessBoard board, ChessPosition p, ChessEvalAttackInfo myAttacks, ChessEvalAttackInfo hisAttacks, out int mbonus, out int ebonus)
        {
            ChessRank rank = p.GetRank();

            int r = Math.Abs(rank - ChessRank.Rank2);
            int rr = r * (r - 1);

            // Base bonus based on rank
            mbonus = 7 * rr;
            ebonus = 7 * (rr + r + 1);

            ChessPosition blockSq = p.PositionInDirection(ChessDirection.DirN);

            if (rr > 0)
            {
                ebonus += board.KingPosition(ChessPlayer.Black).DistanceTo(blockSq) * 5 * rr;
                ebonus -= board.KingPosition(ChessPlayer.White).DistanceTo(blockSq) * 2 * rr;

                if (board.PieceAt(blockSq) == ChessPiece.EMPTY)
                {
                    ChessBitboard squaresAhead = blockSq.GetRank().BitboardAllNorth() & p.GetFile().Bitboard();

                    ChessPosition trailerPos = ChessPosition.OUTOFBOUNDS;
                    var trailer = board.PieceInDirection(p, ChessDirection.DirS, ref trailerPos);
                    bool attackingTrailer = trailer.PieceIsSliderRook() && trailer.PieceToPlayer() == ChessPlayer.Black;
                    bool supportingTrailer = trailer.PieceIsSliderRook() && trailer.PieceToPlayer() == ChessPlayer.White;

                    

                    ChessBitboard unsafeAhead = attackingTrailer ? squaresAhead : squaresAhead & hisAttacks.All();
                    ChessBitboard defendedAhead = supportingTrailer ? squaresAhead : squaresAhead & myAttacks.All();

                    int k = unsafeAhead.Empty() ? 15 : unsafeAhead.Contains(blockSq) ? 3 : 9;

                    if (defendedAhead == squaresAhead)
                    {
                        k += 6;
                    }
                    else if (defendedAhead.Contains(blockSq))
                    {
                        k += (unsafeAhead & defendedAhead) == squaresAhead ? 4 : 2;
                    }

                    mbonus += k * rr;
                    ebonus += k * rr;

                }

            }

            ChessBitboard whitePawnAttacks = myAttacks.PawnEast | myAttacks.PawnWest;
            if(whitePawnAttacks.Contains(blockSq))
            {
                ebonus += r * 20;
            }
            else if (whitePawnAttacks.Contains(p))
            {
                ebonus += r * 12;
            }
        }

        public static void EvalPassedPawnBlack(ChessBoard board, ChessPosition p, ChessEvalAttackInfo myAttacks, ChessEvalAttackInfo hisAttacks, out int mbonus, out int ebonus)
        {
            ChessRank rank = p.GetRank();

            int r = Math.Abs(rank - ChessRank.Rank7);
            int rr = r * (r - 1);

            // Base bonus based on rank
            mbonus = 7 * rr;
            ebonus = 7 * (rr + r + 1);

            ChessPosition blockSq = p.PositionInDirection(ChessDirection.DirS);

            if (rr > 0)
            {
                ebonus += board.KingPosition(ChessPlayer.White).DistanceTo(blockSq) * 5 * rr;
                ebonus -= board.KingPosition(ChessPlayer.Black).DistanceTo(blockSq) * 2 * rr;

                if (board.PieceAt(blockSq) == ChessPiece.EMPTY)
                {
                    ChessBitboard squaresAhead = blockSq.GetRank().BitboardAllSouth() & p.GetFile().Bitboard();

                    ChessPosition trailerPos = ChessPosition.OUTOFBOUNDS;
                    var trailer = board.PieceInDirection(p, ChessDirection.DirN, ref trailerPos);
                    bool attackingTrailer = trailer.PieceIsSliderRook() && trailer.PieceToPlayer() == ChessPlayer.White;
                    bool supportingTrailer = trailer.PieceIsSliderRook() && trailer.PieceToPlayer() == ChessPlayer.Black;



                    ChessBitboard unsafeAhead = attackingTrailer ? squaresAhead : squaresAhead & hisAttacks.All();
                    ChessBitboard defendedAhead = supportingTrailer ? squaresAhead : squaresAhead & myAttacks.All();

                    int k = unsafeAhead.Empty() ? 15 : unsafeAhead.Contains(blockSq) ? 3 : 9;

                    if (defendedAhead == squaresAhead)
                    {
                        k += 6;
                    }
                    else if (defendedAhead.Contains(blockSq))
                    {
                        k += (unsafeAhead & defendedAhead) == squaresAhead ? 4 : 2;
                    }

                    mbonus += k * rr;
                    ebonus += k * rr;

                }

            }

            ChessBitboard myPawnAttacks = myAttacks.PawnEast | myAttacks.PawnWest;
            if (myPawnAttacks.Contains(blockSq))
            {
                ebonus += r * 20;
            }
            else if (myPawnAttacks.Contains(p))
            {
                ebonus += r * 12;
            }
        }

    }
}
