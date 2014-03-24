using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{
    public class ChessEvalPassed
    {
        public const int PASSED_PAWN_MIN_SCORE = 15;
        public const double PASSED_PAWN_8TH = 420;
        public const double RANK_REDUCTION = .7f;
        public const double FACTOR = 14;

        public static readonly int[] endScore = new int[8];
        public static readonly int[] factors = new int[8];
        public static readonly int[] startScore = new int[8];
        static ChessEvalPassed()
        {
            for(int i = 0; i < 8; i++)
            {
                double pct = Math.Pow((double)RANK_REDUCTION, (double)i);
                endScore[i] = (int)(PASSED_PAWN_8TH * pct);
                factors[i] = (int)(endScore[i] / FACTOR);
                endScore[i] = endScore[i] + PASSED_PAWN_MIN_SCORE;
                startScore[i] = endScore[i] / 3;
            }
        }
        

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

        //public static int[] _passedValRankStart = new int[] { 15, 15, 15, 20, 30, 50 };
        //public static int[] _passedValRankEnd = new int[] { 15, 25, 45, 80, 120, 175 };

        public static void EvalPassedPawnBoth(ChessPosition p, ChessPosition myKing, ChessPosition hisKing, 
            ChessBitboard allPieces, ChessBitboard myPawnAttacks, ChessBitboard myAttacks, ChessBitboard hisAttacks, 
            bool attackingTrailer, bool supportingTrailer, out int mbonus, out int ebonus)
        {
            ChessRank rank = p.GetRank();

            //int r = Math.Abs(rank - ChessRank.Rank2);
            //int rr = r * (r - 1);

            // Base bonus based on rank
            mbonus = startScore[(int)rank];
            ebonus = endScore[(int)rank];
            int dangerFactor = factors[(int)rank];

            ChessPosition blockSq = p.PositionInDirection(ChessDirection.DirN);

            int k = 0;

            if (rank <= ChessRank.Rank5)
            {

                k += hisKing.DistanceTo(blockSq);
                k -= myKing.DistanceTo(blockSq);

                if (!allPieces.Contains(blockSq))
                {
                    k += 1;

                    if (!hisAttacks.Contains(blockSq))
                    {
                        k += 1;
                    }

                }
            }

            if (attackingTrailer)
            {
                k -= 2;
            }

            if (supportingTrailer)
            {
                k += 2;
            }

            if (myPawnAttacks.Contains(blockSq))
            {
                k += 3;
            }
            else if (myPawnAttacks.Contains(p))
            {
                k += 2;
            }

            ebonus += k * dangerFactor;

        }




    }
}

