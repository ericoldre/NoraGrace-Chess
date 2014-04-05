using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{
    public class ChessEvalPawns
    {

        public readonly int DoubledPawnValueStart;
        public readonly int DoubledPawnValueEnd;
        public readonly int IsolatedPawnValueStart;
        public readonly int IsolatedPawnValueEnd;
        public readonly int UnconnectedPawnStart;
        public readonly int UnconnectedPawnEnd;
        //public readonly int[,] PcSqValuePosStage;
        //public readonly int[,] PawnPassedValuePosStage;
        public readonly PawnInfo[] pawnHash = new PawnInfo[1000];
        
        public ChessEvalPawns(ChessEvalSettings settings, uint hashSize = 1000)
        {
            pawnHash = new PawnInfo[hashSize];

            this.DoubledPawnValueStart = settings.PawnDoubled.Opening;
            this.DoubledPawnValueEnd = settings.PawnDoubled.Endgame;
            this.IsolatedPawnValueStart = settings.PawnIsolated.Opening;
            this.IsolatedPawnValueEnd = settings.PawnIsolated.Endgame;
            this.UnconnectedPawnStart = settings.PawnUnconnected.Opening;
            this.UnconnectedPawnEnd = settings.PawnUnconnected.Endgame;

            this.PassedInitialization(settings);
        }



        public PawnInfo PawnEval(ChessBoard board)
        {
            long idx = board.ZobristPawn % pawnHash.GetUpperBound(0);
            if (idx < 0) { idx = -idx; }
            PawnInfo retval = pawnHash[idx];
            if (retval != null && retval.PawnZobrist == board.ZobristPawn)
            {
                return retval;
            }

            retval = EvalAllPawns(board.PieceLocations(ChessPiece.WPawn), board.PieceLocations(ChessPiece.BPawn), board.ZobristPawn);
            pawnHash[idx] = retval;
            return retval;
        }

        public PawnInfo EvalAllPawns(ChessBitboard whitePawns, ChessBitboard blackPawns, long pawnZobrist)
        {
            //eval for white
            ChessBitboard doubled = ChessBitboard.Empty;
            ChessBitboard passed = ChessBitboard.Empty;
            ChessBitboard isolated = ChessBitboard.Empty;
            ChessBitboard unconnected = ChessBitboard.Empty;

            
            int WStartVal = 0;
            int WEndVal = 0;

            EvalWhitePawns(whitePawns, blackPawns, ref WStartVal, ref WEndVal,out passed, out doubled, out isolated, out unconnected);

            //create inverse situation
            ChessBitboard bpassed = ChessBitboard.Empty;
            ChessBitboard bdoubled = ChessBitboard.Empty;
            ChessBitboard bisolated = ChessBitboard.Empty;
            ChessBitboard bunconnected = ChessBitboard.Empty;

            ChessBitboard blackRev = blackPawns.Reverse();
            ChessBitboard whiteRev = whitePawns.Reverse();

            int BStartVal = 0;
            int BEndVal = 0;

            //actually passing in the black pawns from their own perspective
            EvalWhitePawns(blackRev, whiteRev, ref BStartVal, ref BEndVal, out bpassed, out bdoubled, out bisolated, out bunconnected);

            doubled |= bdoubled.Reverse();
            passed |= bpassed.Reverse();
            isolated |= bisolated.Reverse();
            unconnected |= bunconnected.Reverse();

            //set return values;
            int StartVal = WStartVal - BStartVal;
            int EndVal = WEndVal - BEndVal;

            return new PawnInfo(pawnZobrist, whitePawns, blackPawns, StartVal, EndVal, passed, doubled, isolated, unconnected);

        }
        private void EvalWhitePawns(ChessBitboard whitePawns, ChessBitboard blackPawns, ref int StartVal, ref int EndVal, out ChessBitboard passed, out ChessBitboard doubled, out ChessBitboard isolated, out ChessBitboard unconnected)
        {
            passed = ChessBitboard.Empty;
            doubled = ChessBitboard.Empty;
            isolated = ChessBitboard.Empty;
            unconnected = ChessBitboard.Empty;

            foreach (ChessPosition pos in whitePawns.ToPositions())
            {
                ChessFile f = pos.GetFile();
                ChessRank r = pos.GetRank();
                ChessBitboard bbFile = pos.GetFile().Bitboard();
                ChessBitboard bbFile2E = bbFile.ShiftDirE();
                ChessBitboard bbFile2W = bbFile.ShiftDirW();
                ChessBitboard bballNorth = r.BitboardAllNorth() & bbFile;


                //substract doubled score
                if (!(bballNorth & ~pos.Bitboard() & whitePawns).Empty())
                {
                    StartVal -= this.DoubledPawnValueStart;
                    EndVal -= this.DoubledPawnValueEnd;
                    doubled |= pos.Bitboard();
                }

                

                //substract isolated score
                if ((bbFile2E & whitePawns).Empty() && (bbFile2W & whitePawns).Empty())
                {
                    StartVal -= this.IsolatedPawnValueStart;
                    EndVal -= this.IsolatedPawnValueEnd;
                    isolated |= pos.Bitboard();
                }
                else
                {
                    //substract unconnected penalty, isolated is not also unconnected
                    ChessBitboard bbRank = r.Bitboard();
                    ChessBitboard connectedRanks = bbRank | bbRank.ShiftDirN() | bbRank.ShiftDirS();
                    ChessBitboard connectedPos = connectedRanks & (bbFile2E | bbFile2W);
                    if ((whitePawns & connectedPos).Empty())
                    {
                        StartVal -= this.UnconnectedPawnStart;
                        EndVal -= this.UnconnectedPawnEnd;
                        unconnected |= pos.Bitboard();
                    }
                }

                ChessBitboard blockPositions = (bbFile | bbFile2E | bbFile2W) & pos.GetRank().BitboardAllNorth().ShiftDirN();
                if ((blockPositions & blackPawns).Empty())
                {
                    //StartVal += this.PawnPassedValuePosStage[(int)pos, (int)ChessGameStage.Opening];
                    //EndVal += this.PawnPassedValuePosStage[(int)pos, (int)ChessGameStage.Endgame];
                    passed |= pos.Bitboard();
                }
            }

        }


        

        public static ChessResult? EndgameKPK(ChessPosition whiteKing, ChessPosition blackKing, ChessPosition whitePawn, bool whiteToMove)
        {
            if (!whiteToMove
                && ((Attacks.KingAttacks(blackKing) & whitePawn.Bitboard()) != ChessBitboard.Empty)
                && (Attacks.KingAttacks(whiteKing) & whitePawn.Bitboard()) == ChessBitboard.Empty)
            {
                //black can just take white.
                return ChessResult.Draw;
            }

            ChessPosition prom = ChessRank.Rank8.ToPosition(whitePawn.GetFile());
            int pdist = prom.DistanceTo(whitePawn);
            if (whitePawn.GetRank() == ChessRank.Rank2) { pdist--; }
            if (!whiteToMove) { pdist++; }
            if (prom.DistanceTo(blackKing) > pdist)
            {
                //pawn can run for end.
                return ChessResult.WhiteWins;
            }

            return null;


        }

        #region passed pawns

        int PASSED_PAWN_MIN_SCORE = 10;
        int[] endScore = new int[8];
        int[] factors = new int[8];
        int[] startScore = new int[8];

        private void PassedInitialization(ChessEvalSettings settings)
        {
            PASSED_PAWN_MIN_SCORE = settings.PawnPassedMinScore;
            for(int i = 0; i < 8; i++)
            {
                double pct = Math.Pow(settings.PawnPassedRankReduction, (double)i);
                endScore[i] = (int)(settings.PawnPassed8thRankScore * pct);
                factors[i] = (int)(endScore[i] * settings.PawnPassedDangerPct);
                endScore[i] = endScore[i] + settings.PawnPassedMinScore;
                startScore[i] = (int)((double)endScore[i] * settings.PawnPassedOpeningPct);
            }
        }

        public void EvalPassedPawns(ChessBoard board, ChessEvalInfo evalInfo, ChessBitboard passedPawns)
        {

            ChessPosition myKing, hisKing;
            ChessBitboard allPieces, myPawnAttacks, myAttacks, hisAttacks;
            bool attackingTrailer, supportingTrailer;

            int startScore, endScore, bestEndScore;

            var white = passedPawns & board.PlayerLocations(ChessPlayer.White);
            if (!white.Empty())
            {
                bestEndScore = 0;

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
                        mbonus: out startScore,
                        ebonus: out endScore);


                    //scores other than best are halved
                    endScore = endScore & ~1; //make even number for div /2
                    if (endScore >= bestEndScore)
                    {
                        int reduce = bestEndScore / 2;
                        bestEndScore = endScore;
                        endScore -= reduce;
                    }
                    else
                    {
                        endScore = endScore / 2;
                    }

                    evalInfo.PawnsPassedStart += startScore;
                    evalInfo.PawnsPassedEnd += endScore;
                    ///evalInfo.PawnsPassedStart += mbonus;
                    //evalInfo.PawnsPassedEnd += ebonus;
                }
            }

            var black = passedPawns & board.PlayerLocations(ChessPlayer.Black);
            if (!black.Empty())
            {
                bestEndScore = 0;
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
                        mbonus: out startScore,
                        ebonus: out endScore);

                    //scores other than best are halved
                    endScore = endScore & ~1; //make even number for div /2
                    if (endScore >= bestEndScore)
                    {
                        int reduce = bestEndScore / 2;
                        bestEndScore = endScore;
                        endScore -= reduce;
                    }
                    else
                    {
                        endScore = endScore / 2;
                    }

                    evalInfo.PawnsPassedStart -= startScore;
                    evalInfo.PawnsPassedEnd -= endScore;
                }
            }




        }


        private void EvalPassedPawnBoth(ChessPosition p, ChessPosition myKing, ChessPosition hisKing,
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
                    k += 2;

                    if (hisAttacks.Contains(blockSq))
                    {
                        k -= 1;
                    }
                    if (!myAttacks.Contains(blockSq))
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
                ebonus += PASSED_PAWN_MIN_SCORE;
            }

            if (myPawnAttacks.Contains(blockSq))
            {
                k += 2;
                ebonus += PASSED_PAWN_MIN_SCORE;
            }
            else if (myPawnAttacks.Contains(p))
            {
                k += 2;
                ebonus += PASSED_PAWN_MIN_SCORE;
            }

            ebonus += k * dangerFactor;

        }

        #endregion

    }

    public class PawnInfo
    {

        public readonly Int64 PawnZobrist;
        public readonly ChessBitboard WhitePawns;
        public readonly ChessBitboard BlackPawns;
        public readonly int StartVal;
        public readonly int EndVal;
        public readonly ChessBitboard PassedPawns;
        public readonly ChessBitboard Doubled;
        public readonly ChessBitboard Isolated;
        public readonly ChessBitboard Unconnected;
        


        public PawnInfo(Int64 pawnZobrist, ChessBitboard whitePawns, ChessBitboard blackPawns, int startVal, int endVal, ChessBitboard passedPawns, ChessBitboard doubled, ChessBitboard isolated, ChessBitboard unconnected)
        {
            PawnZobrist = pawnZobrist;
            WhitePawns = whitePawns;
            BlackPawns = blackPawns;
            StartVal = startVal;
            EndVal = endVal;
            PassedPawns = passedPawns;
            Doubled = doubled;
            Isolated = isolated;
            Unconnected = unconnected;
        }




        int _shelterCacheKey = 0;
        int _shelterCacheValue = 0;

        private static readonly int[] _shelterFactor = new int[] { 7, 0, 2, 5, 6, 7, 7, 7 };
        public int EvalShelter(ChessFile whiteKingFile, ChessFile blackKingFile, bool wsCastle, bool wlCastle, bool bsCastle, bool blCastle)
        {
            int key = (int)whiteKingFile
                | ((int)blackKingFile << 8)
                | (wsCastle ? (1 << 20) : 0)
                | (wlCastle ? (1 << 21) : 0)
                | (bsCastle ? (1 << 22) : 0)
                | (blCastle ? (1 << 23) : 0);

            if (key == _shelterCacheKey) { return _shelterCacheValue; }

            int retval = 0;

            int castlePenalty;

            ChessBitboard wpRev = WhitePawns.Reverse();
            int lowestWhitePenalty = CalcKingShelterPenaltyFactorBlackPerspective(whiteKingFile, wpRev);
            if (wsCastle)
            {
                castlePenalty = CalcKingShelterPenaltyFactorBlackPerspective(ChessFile.FileG, wpRev) + 2;
                if (castlePenalty < lowestWhitePenalty) { lowestWhitePenalty = castlePenalty; }
            }
            if (wlCastle)
            {
                castlePenalty = CalcKingShelterPenaltyFactorBlackPerspective(ChessFile.FileC, wpRev) + 2;
                if (castlePenalty < lowestWhitePenalty) { lowestWhitePenalty = castlePenalty; }
            }

            int lowestBlackPenalty = CalcKingShelterPenaltyFactorBlackPerspective(blackKingFile, BlackPawns);
            if (bsCastle)
            {
                castlePenalty = CalcKingShelterPenaltyFactorBlackPerspective(ChessFile.FileG, BlackPawns) + 2;
                if (castlePenalty < lowestBlackPenalty) { lowestBlackPenalty = castlePenalty; }
            }
            if (blCastle)
            {
                castlePenalty = CalcKingShelterPenaltyFactorBlackPerspective(ChessFile.FileC, BlackPawns) + 2;
                if (castlePenalty < lowestBlackPenalty) { lowestBlackPenalty = castlePenalty; }
            }

            retval -= lowestWhitePenalty;
            retval += lowestBlackPenalty;

            _shelterCacheKey = key;
            _shelterCacheValue = retval;

            return retval;
        }

        /// <summary>
        /// eval from perpective of black because of frequent calls to bb northmost, which is much faster than southmost.
        /// </summary>
        /// <param name="kingFile"></param>
        /// <param name="myPawns"></param>
        /// <param name="blackPawns"></param>
        /// <returns></returns>
        private static int CalcKingShelterPenaltyFactorBlackPerspective(ChessFile kingFile, ChessBitboard myPawns)
        {
            if (kingFile == ChessFile.FileA) { kingFile = ChessFile.FileB; }
            if (kingFile == ChessFile.FileH) { kingFile = ChessFile.FileG; }

            int retval = 0;
            int count7th = 0;
            for (ChessFile f = kingFile - 1; f <= kingFile + 1; f++)
            {
                ChessBitboard bbFile = f.Bitboard();
                ChessBitboard bbPawnFile = bbFile & myPawns;
                ChessRank pawnRank;
                if (bbPawnFile == ChessBitboard.Empty)
                {
                    pawnRank = ChessRank.Rank1;
                }
                else
                {
                    pawnRank = bbPawnFile.NorthMostPosition().GetRank();
                }
                retval += _shelterFactor[(int)pawnRank];
                if (pawnRank == ChessRank.Rank7)
                {
                    count7th++;
                }
            }
            if (count7th >= 2) { retval--; }
            return retval;
        }


    }
}

