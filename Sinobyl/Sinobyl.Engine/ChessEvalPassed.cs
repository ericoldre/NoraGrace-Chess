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
            
            ChessPosition myKing, hisKing;
            ChessBitboard allPieces, myPawnAttacks ,myAttacks, hisAttacks;
            bool attackingTrailer, supportingTrailer;
            int mbonus, ebonus;

            
            var white = passedPawns & board.PlayerLocations(ChessPlayer.White);
            if (!white.Empty())
            {
                myKing = board.KingPosition(ChessPlayer.White);
                hisKing = board.KingPosition(ChessPlayer.Black);
                allPieces = board.PieceLocationsAll;
                myPawnAttacks = evalInfo.Attacks[(int)ChessPlayer.White].PawnEast | evalInfo.Attacks[(int)ChessPlayer.White].PawnWest;
                myAttacks = evalInfo.Attacks[(int)ChessPlayer.White].All();
                hisAttacks = evalInfo.Attacks[(int)ChessPlayer.Black].All();

                foreach (ChessPosition passedPos in white.ToPositions())
                {
                    ChessPosition trailerPos = ChessPosition.OUTOFBOUNDS;
                    ChessPiece trailerPiece = board.PieceInDirection(passedPos, ChessDirection.DirS, ref trailerPos);

                    attackingTrailer = trailerPiece.PieceIsSliderRook() & trailerPiece.PieceToPlayer() == ChessPlayer.Black;
                    supportingTrailer = trailerPiece.PieceIsSliderRook() & trailerPiece.PieceToPlayer() == ChessPlayer.White;

                    EvalPassedPawnBoth(
                        p: passedPos,
                        allPieces: allPieces,
                        myAttacks: myAttacks,
                        hisAttacks: hisAttacks,
                        myKing: myKing,
                        hisKing: hisKing,
                        myPawnAttacks: myPawnAttacks,
                        attackingTrailer: attackingTrailer,
                        supportingTrailer: supportingTrailer,
                        mbonus: out mbonus,
                        ebonus: out ebonus);

                    evalInfo.PawnsPassedStart += mbonus;
                    evalInfo.PawnsPassedEnd += ebonus;
                }
            }

            var black = passedPawns & board.PlayerLocations(ChessPlayer.Black);
            if (!black.Empty())
            {
                myKing = board.KingPosition(ChessPlayer.Black).Reverse();
                hisKing = board.KingPosition(ChessPlayer.White).Reverse();
                allPieces = board.PieceLocationsAll.Reverse();
                myPawnAttacks = (evalInfo.Attacks[(int)ChessPlayer.Black].PawnEast | evalInfo.Attacks[(int)ChessPlayer.Black].PawnWest).Reverse();
                myAttacks = evalInfo.Attacks[(int)ChessPlayer.Black].All().Reverse();
                hisAttacks = evalInfo.Attacks[(int)ChessPlayer.White].All().Reverse();

                foreach (ChessPosition passedPos in black.ToPositions())
                {
                    ChessPosition passesPos2 = passedPos.Reverse();
                    ChessPosition trailerPos = ChessPosition.OUTOFBOUNDS;
                    ChessPiece trailerPiece = board.PieceInDirection(passedPos, ChessDirection.DirN, ref trailerPos);

                    attackingTrailer = trailerPiece.PieceIsSliderRook() & trailerPiece.PieceToPlayer() == ChessPlayer.White;
                    supportingTrailer = trailerPiece.PieceIsSliderRook() & trailerPiece.PieceToPlayer() == ChessPlayer.Black;

                    EvalPassedPawnBoth(
                        p: passesPos2,
                        allPieces: allPieces,
                        myAttacks: myAttacks,
                        hisAttacks: hisAttacks,
                        myKing: myKing,
                        hisKing: hisKing,
                        myPawnAttacks: myPawnAttacks,
                        attackingTrailer: attackingTrailer,
                        supportingTrailer: supportingTrailer,
                        mbonus: out mbonus,
                        ebonus: out ebonus);

                    evalInfo.PawnsPassedStart -= mbonus;
                    evalInfo.PawnsPassedEnd -= ebonus;
                }
            }
            

            

        }

        public static int[] _passedValRankStart = new int[] { 15, 15, 15, 20, 30, 50 };
        public static int[] _passedValRankEnd = new int[] { 15, 25, 45, 80, 120, 175 };

        public static void EvalPassedPawnBoth(ChessPosition p, ChessPosition myKing, ChessPosition hisKing, 
            ChessBitboard allPieces, ChessBitboard myPawnAttacks, ChessBitboard myAttacks, ChessBitboard hisAttacks, 
            bool attackingTrailer, bool supportingTrailer, out int mbonus, out int ebonus)
        {
            ChessRank rank = p.GetRank();

            int r = Math.Abs(rank - ChessRank.Rank2);
            int rr = r * (r - 1);

            // Base bonus based on rank
            mbonus = _passedValRankStart[r];
            ebonus = _passedValRankEnd[r];

            ChessPosition blockSq = p.PositionInDirection(ChessDirection.DirN);

            if (rr > 0)
            {
                ebonus += myKing.DistanceTo(blockSq) * 3 * rr;
                ebonus -= hisKing.DistanceTo(blockSq) * 2 * rr;

                if (!allPieces.Contains(blockSq))
                {
                    ChessBitboard squaresAhead = blockSq.GetRank().BitboardAllNorth() & p.GetFile().Bitboard();

                    ChessBitboard unsafeAhead = attackingTrailer ? squaresAhead : squaresAhead & hisAttacks;
                    ChessBitboard defendedAhead = supportingTrailer ? squaresAhead : squaresAhead & myAttacks;

                    int k = unsafeAhead.Empty() ? 6 : !unsafeAhead.Contains(blockSq) ? 3 : 0;

                    if (defendedAhead == squaresAhead)
                    {
                        k += 4;
                    }
                    else if (defendedAhead.Contains(blockSq))
                    {
                        k += (unsafeAhead & defendedAhead) == squaresAhead ? 2 : 0;
                    }

                    mbonus += k * rr;
                    ebonus += k * rr;

                }

            }

            if (myPawnAttacks.Contains(blockSq))
            {
                ebonus += r * 15;
            }
            else if (myPawnAttacks.Contains(p))
            {
                ebonus += r * 8;
            }
        }




    }
}

