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
        public readonly double CandidatePct;
        private readonly double[] PassedDistancePct;

        //public readonly int[,] PcSqValuePosStage;
        //public readonly int[,] PawnPassedValuePosStage;
        public readonly PawnInfo[] pawnHash = new PawnInfo[1000];
        public static int TotalEvalPawnCount = 0;

        private static ChessBitboard[][] _attackMask = new ChessBitboard[2][];
        private static ChessBitboard[] _telestop = new ChessBitboard[64];
        static ChessEvalPawns()
        {
            _attackMask[0] = new ChessBitboard[64];
            _attackMask[1] = new ChessBitboard[64];
            foreach (var position in ChessPositionInfo.AllPositions)
            {
                ChessBitboard north = position.Bitboard().Flood(ChessDirection.DirN) & ~position.Bitboard();
                ChessBitboard south = position.Bitboard().Flood(ChessDirection.DirS) & ~position.Bitboard();
                
                _attackMask[(int)ChessPlayer.White][(int)position] = north.ShiftDirW() | north.ShiftDirE();
                _attackMask[(int)ChessPlayer.Black][(int)position] = south.ShiftDirW() | south.ShiftDirE();
                _telestop[(int)position] = north;
            }
        }

        public ChessEvalPawns(ChessEvalSettings settings, uint hashSize = 1000)
        {
            pawnHash = new PawnInfo[hashSize];

            this.DoubledPawnValueStart = settings.PawnDoubled.Opening;
            this.DoubledPawnValueEnd = settings.PawnDoubled.Endgame;
            this.IsolatedPawnValueStart = settings.PawnIsolated.Opening;
            this.IsolatedPawnValueEnd = settings.PawnIsolated.Endgame;
            this.UnconnectedPawnStart = settings.PawnUnconnected.Opening;
            this.UnconnectedPawnEnd = settings.PawnUnconnected.Endgame;
            this.CandidatePct = settings.PawnCandidatePct;
            this.PassedInitialization(settings);

            this.PassedDistancePct = new double[8];
            for (int i = 0; i < 8; i++)
            {
                int min = 3;
                int max = 7;

                if (i <= min) { PassedDistancePct[i] = settings.PawnPassedClosePct; continue; }
                double p = (double)(i - min) / (double)(max - min);
                double d = settings.PawnPassedFarPct - settings.PawnPassedClosePct;

                PassedDistancePct[i] = settings.PawnPassedClosePct + (p * d);

                //this.PassedDistancePct[i] = 
                    
            }
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

            retval = EvalAllPawns(board[ChessPiece.WPawn], board[ChessPiece.BPawn], board.ZobristPawn);
            pawnHash[idx] = retval;
            return retval;
        }

        public PawnInfo EvalAllPawns(ChessBitboard whitePawns, ChessBitboard blackPawns, long pawnZobrist)
        {
            TotalEvalPawnCount++;

            //eval for white
            ChessBitboard doubled = ChessBitboard.Empty;
            ChessBitboard passed = ChessBitboard.Empty;
            ChessBitboard isolated = ChessBitboard.Empty;
            ChessBitboard unconnected = ChessBitboard.Empty;
            ChessBitboard candidates = ChessBitboard.Empty;
            
            int WStartVal = 0;
            int WEndVal = 0;

            EvalWhitePawns(whitePawns, blackPawns, ref WStartVal, ref WEndVal,out passed, out doubled, out isolated, out unconnected, out candidates);

            //create inverse situation
            ChessBitboard bpassed = ChessBitboard.Empty;
            ChessBitboard bdoubled = ChessBitboard.Empty;
            ChessBitboard bisolated = ChessBitboard.Empty;
            ChessBitboard bunconnected = ChessBitboard.Empty;
            ChessBitboard bcandidates = ChessBitboard.Empty;

            ChessBitboard blackRev = blackPawns.Reverse();
            ChessBitboard whiteRev = whitePawns.Reverse();

            int BStartVal = 0;
            int BEndVal = 0;

            //actually passing in the black pawns from their own perspective
            EvalWhitePawns(blackRev, whiteRev, ref BStartVal, ref BEndVal, out bpassed, out bdoubled, out bisolated, out bunconnected, out bcandidates);

            doubled |= bdoubled.Reverse();
            passed |= bpassed.Reverse();
            isolated |= bisolated.Reverse();
            unconnected |= bunconnected.Reverse();
            candidates |= bcandidates.Reverse();

            //set return values;
            int StartVal = WStartVal - BStartVal;
            int EndVal = WEndVal - BEndVal;

            return new PawnInfo(pawnZobrist, whitePawns, blackPawns, StartVal, EndVal, passed, doubled, isolated, unconnected, candidates);

        }
        private void EvalWhitePawns(ChessBitboard whitePawns, ChessBitboard blackPawns, ref int StartVal, ref int EndVal, out ChessBitboard passed, out ChessBitboard doubled, out ChessBitboard isolated, out ChessBitboard unconnected, out ChessBitboard candidates)
        {
            passed = ChessBitboard.Empty;
            doubled = ChessBitboard.Empty;
            isolated = ChessBitboard.Empty;
            unconnected = ChessBitboard.Empty;
            candidates = ChessBitboard.Empty;

            ChessBitboard positions = whitePawns;

            while(positions != ChessBitboard.Empty)
            {
                ChessPosition pos = ChessBitboardInfo.PopFirst(ref positions);
                ChessFile f = pos.GetFile();
                ChessRank r = pos.GetRank();
                ChessBitboard bbFile = pos.GetFile().Bitboard();
                ChessBitboard bbFile2E = bbFile.ShiftDirE();
                ChessBitboard bbFile2W = bbFile.ShiftDirW();
                ChessBitboard telestop = _telestop[(int)pos];

                //substract doubled score
                if (!(telestop & whitePawns).Empty())
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

                if ((telestop & whitePawns).Empty())
                {
                    ChessBitboard blockPositions = _attackMask[0][(int)pos] | _telestop[(int)pos];
                    if ((blockPositions & blackPawns).Empty())
                    {
                        //StartVal += this.PawnPassedValuePosStage[(int)pos, (int)ChessGameStage.Opening];
                        //EndVal += this.PawnPassedValuePosStage[(int)pos, (int)ChessGameStage.Endgame];
                        passed |= pos.Bitboard();
                    }
                    else if (IsCandidate(pos, whitePawns, blackPawns))
                    {
                        candidates |= pos.Bitboard();
                    }
                }
                
            }

        }

        private static bool IsCandidate(ChessPosition pos, ChessBitboard white, ChessBitboard black)
        {
            System.Diagnostics.Debug.Assert(white.Contains(pos));

            if ((_telestop[(int)pos] & black) != ChessBitboard.Empty)
            {
                return false;
            }

            if(!(ChessBitboard.Rank5 | ChessBitboard.Rank6 | ChessBitboard.Rank7).Contains(pos))
            {
                return false;
            }

            ChessBitboard blockers = _attackMask[0][(int)pos] & black;
            ChessBitboard helpers = _attackMask[1][(int)pos.PositionInDirectionUnsafe(ChessDirection.DirN)] & white;

            System.Diagnostics.Debug.Assert(blockers != ChessBitboard.Empty); //otherwise it's a passed pawn.

            if (helpers == ChessBitboard.Empty) { return false; }

            return helpers.BitCount() >= blockers.BitCount();
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



        public PhasedScore EvalPassedPawns(ChessBoard board, ChessEvalAttackInfo[] attackInfo, ChessBitboard passedPawns, ChessBitboard candidatePawns, int[] workspace)
        {
            
            PhasedScore retval = PhasedScoreInfo.Create(0, 0);

            var positions = (passedPawns | candidatePawns) & board[ChessPlayer.White];
            if (positions != ChessBitboard.Empty)
            {
                var white = EvalPassedPawnsSide(ChessPlayer.White, board, attackInfo[0], attackInfo[1], passedPawns & board[ChessPlayer.White], candidatePawns & board[ChessPlayer.White], workspace);
                retval = retval.Add(white);

            }

            positions = (passedPawns | candidatePawns) & board[ChessPlayer.Black];
            if (positions != ChessBitboard.Empty)
            {
                var black = EvalPassedPawnsSide(ChessPlayer.Black, board, attackInfo[1], attackInfo[0], passedPawns & board[ChessPlayer.Black], candidatePawns & board[ChessPlayer.Black], workspace);
                retval = retval.Subtract(black);
            }


            return retval;

        }

        public PhasedScore EvalPassedPawnsSide(ChessPlayer me, ChessBoard board, ChessEvalAttackInfo myAttackInfo, ChessEvalAttackInfo hisAttackInfo, ChessBitboard passedPawns, ChessBitboard candidatePawns, int[] workspace)
        {
            
            int bestEndScore = -1;
            int bestEndFile = -1;
            int totalStartScore = 0;
            ChessPlayer him = me.PlayerOther();

            ChessPosition myKing = board.KingPosition(me);
            ChessPosition hisKing = board.KingPosition(him);
            ChessBitboard allPieces = board.PieceLocationsAll;
            ChessBitboard myPawnAttacks = myAttackInfo.PawnEast | myAttackInfo.PawnWest;
            ChessBitboard myAttacks = myAttackInfo.All();
            ChessBitboard hisAttacks = hisAttackInfo.All();
            ChessBitboard positions = passedPawns | candidatePawns;

            ChessDirection mySouth = me == ChessPlayer.White ? ChessDirection.DirS : ChessDirection.DirN;

            Array.Clear(workspace, 0, 8);

            while (positions != ChessBitboard.Empty)// (ChessPosition passedPos in white.ToPositions())
            {
                ChessPosition passedPos = ChessBitboardInfo.PopFirst(ref positions);
                ChessPosition trailerPos = ChessPosition.OUTOFBOUNDS;

                ChessPiece trailerPiece = board.PieceInDirection(passedPos, mySouth, ref trailerPos);

                bool isCandidate = candidatePawns.Contains(passedPos);

                bool attackingTrailer = trailerPiece.PieceIsSliderRook() && trailerPiece.PieceToPlayer() == him;
                bool supportingTrailer = trailerPiece.PieceIsSliderRook() && trailerPiece.PieceToPlayer() == me;

                int startScore;
                int endScore;

                EvalPassedPawn(
                    me: me,
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
                 
                int iFile = (int)passedPos.GetFile();

                //adjust scores down for candidates.
                if (isCandidate)
                {
                    startScore = 0;
                    endScore = (int)(endScore * CandidatePct);
                }

                totalStartScore += startScore;

                

                //scores other than best are halved
                endScore = endScore & ~1; //make even number for div /2
                workspace[iFile] = endScore;
                if (endScore > bestEndScore)
                {
                    bestEndScore = endScore;
                    bestEndFile = iFile;
                }

            }

            System.Diagnostics.Debug.Assert(bestEndScore > 0);
            System.Diagnostics.Debug.Assert(bestEndFile >= 0 && bestEndFile <= 7);

            int totalEndScore = 0;

            for (int i = 0; i < 8; i++)
            {
                if (i != bestEndFile && workspace[i] != 0)
                {
                    int fileDiff = Math.Abs(i - bestEndFile);
                    workspace[i] = (int)((double)workspace[i] * PassedDistancePct[fileDiff]);
                }
                totalEndScore += workspace[i];
            }

            return PhasedScoreInfo.Create(totalStartScore, totalEndScore);
        }

        private void EvalPassedPawn(ChessPlayer me, ChessPosition p, ChessPosition myKing, ChessPosition hisKing,
            ChessBitboard allPieces, ChessBitboard myPawnAttacks, ChessBitboard myAttacks, ChessBitboard hisAttacks,
            bool attackingTrailer, bool supportingTrailer, out int mbonus, out int ebonus)
        {
            ChessRank rank = p.GetRank();

            if (me == ChessPlayer.Black) { rank = 7 - rank; }

            //int r = Math.Abs(rank - ChessRank.Rank2);
            //int rr = r * (r - 1);

            // Base bonus based on rank
            mbonus = startScore[(int)rank];
            ebonus = endScore[(int)rank];
            int dangerFactor = factors[(int)rank];

            ChessPosition blockSq = p.PositionInDirection(me == ChessPlayer.White ? ChessDirection.DirN: ChessDirection.DirS);

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


        public void EvalPassedPawnsOld(ChessBoard board, ChessEvalInfo evalInfo, ChessBitboard passedPawns)
        {

            ChessPosition myKing, hisKing;
            ChessBitboard allPieces, myPawnAttacks, myAttacks, hisAttacks;
            bool attackingTrailer, supportingTrailer;

            int startScore, endScore, bestEndScore;

            var positions = passedPawns & board[ChessPlayer.White];
            if (positions != ChessBitboard.Empty)
            {
                bestEndScore = 0;

                myKing = board.KingPosition(ChessPlayer.White);
                hisKing = board.KingPosition(ChessPlayer.Black);
                allPieces = board.PieceLocationsAll;
                myPawnAttacks = evalInfo.Attacks[(int)ChessPlayer.White].PawnEast | evalInfo.Attacks[(int)ChessPlayer.White].PawnWest;
                myAttacks = evalInfo.Attacks[(int)ChessPlayer.White].All();
                hisAttacks = evalInfo.Attacks[(int)ChessPlayer.Black].All();

                while (positions != ChessBitboard.Empty)// (ChessPosition passedPos in white.ToPositions())
                {
                    ChessPosition passedPos = ChessBitboardInfo.PopFirst(ref positions);
                    ChessPosition trailerPos = ChessPosition.OUTOFBOUNDS;
                    ChessPiece trailerPiece = board.PieceInDirection(passedPos, ChessDirection.DirS, ref trailerPos);

                    attackingTrailer = trailerPiece.PieceIsSliderRook() && trailerPiece.PieceToPlayer() == ChessPlayer.Black;
                    supportingTrailer = trailerPiece.PieceIsSliderRook() && trailerPiece.PieceToPlayer() == ChessPlayer.White;

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

                    evalInfo.PawnsPassedStart = evalInfo.PawnsPassedStart.Add(PhasedScoreInfo.Create(startScore, endScore));

                    ///evalInfo.PawnsPassedStart += mbonus;
                    //evalInfo.PawnsPassedEnd += ebonus;
                }
            }

            positions = passedPawns & board[ChessPlayer.Black];
            if (positions != ChessBitboard.Empty)
            {
                bestEndScore = 0;
                myKing = board.KingPosition(ChessPlayer.Black).Reverse();
                hisKing = board.KingPosition(ChessPlayer.White).Reverse();
                allPieces = board.PieceLocationsAll.Reverse();
                myPawnAttacks = (evalInfo.Attacks[(int)ChessPlayer.Black].PawnEast | evalInfo.Attacks[(int)ChessPlayer.Black].PawnWest).Reverse();
                myAttacks = evalInfo.Attacks[(int)ChessPlayer.Black].All().Reverse();
                hisAttacks = evalInfo.Attacks[(int)ChessPlayer.White].All().Reverse();

                while (positions != ChessBitboard.Empty) // (ChessPosition passedPos in black.ToPositions())
                {
                    ChessPosition passedPos = ChessBitboardInfo.PopFirst(ref positions);
                    ChessPosition passesPos2 = passedPos.Reverse();
                    ChessPosition trailerPos = ChessPosition.OUTOFBOUNDS;
                    ChessPiece trailerPiece = board.PieceInDirection(passedPos, ChessDirection.DirN, ref trailerPos);

                    attackingTrailer = trailerPiece.PieceIsSliderRook() && trailerPiece.PieceToPlayer() == ChessPlayer.White;
                    supportingTrailer = trailerPiece.PieceIsSliderRook() && trailerPiece.PieceToPlayer() == ChessPlayer.Black;

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

                    evalInfo.PawnsPassedStart = evalInfo.PawnsPassedStart.Subtract(PhasedScoreInfo.Create(startScore, endScore));
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
        public readonly PhasedScore Value;
        public readonly ChessBitboard PassedPawns;
        public readonly ChessBitboard Doubled;
        public readonly ChessBitboard Isolated;
        public readonly ChessBitboard Unconnected;
        public readonly ChessBitboard Candidates;



        public PawnInfo(Int64 pawnZobrist, ChessBitboard whitePawns, ChessBitboard blackPawns, int startVal, int endVal, ChessBitboard passedPawns, ChessBitboard doubled, ChessBitboard isolated, ChessBitboard unconnected, ChessBitboard candidates)
        {
            PawnZobrist = pawnZobrist;
            WhitePawns = whitePawns;
            BlackPawns = blackPawns;
            Value = PhasedScoreInfo.Create(startVal, endVal);
            PassedPawns = passedPawns;
            Doubled = doubled;
            Isolated = isolated;
            Unconnected = unconnected;
            Candidates = candidates;
        }




        int _shelterCacheKey = 0x3a184743; //random number, important not zero, or anything that key could compute to.
        int _shelterCacheValue = 0;

        private static readonly int[] _shelterFactor = new int[] { 7, 0, 2, 5, 6, 7, 7, 7 };

        public int EvalShelter(ChessFile whiteKingFile, ChessFile blackKingFile, CastleFlags castleFlags)
        {
            int key = ((int)whiteKingFile
                | ((int)blackKingFile << 8)
                | ((int)castleFlags << 20));

            if (key == _shelterCacheKey) 
            {
                System.Diagnostics.Debug.Assert(_shelterCacheValue == EvalShelterCalc(whiteKingFile, blackKingFile, castleFlags));
                return _shelterCacheValue; 
            }
            else
            {
                int retval = EvalShelterCalc(whiteKingFile, blackKingFile, castleFlags);
                _shelterCacheKey = key;
                _shelterCacheValue = retval;
                return retval;
            }


        }
        private int EvalShelterCalc(ChessFile whiteKingFile, ChessFile blackKingFile, CastleFlags castleFlags)
        {
            
            int retval = 0;

            int castlePenalty;

            ChessBitboard wpRev = WhitePawns.Reverse();
            int lowestWhitePenalty = CalcKingShelterPenaltyFactorBlackPerspective(whiteKingFile, wpRev);
            if ((castleFlags & CastleFlags.WhiteShort) != 0)
            {
                castlePenalty = CalcKingShelterPenaltyFactorBlackPerspective(ChessFile.FileG, wpRev) + 2;
                if (castlePenalty < lowestWhitePenalty) { lowestWhitePenalty = castlePenalty; }
            }
            if ((castleFlags & CastleFlags.WhiteLong) != 0)
            {
                castlePenalty = CalcKingShelterPenaltyFactorBlackPerspective(ChessFile.FileC, wpRev) + 2;
                if (castlePenalty < lowestWhitePenalty) { lowestWhitePenalty = castlePenalty; }
            }

            int lowestBlackPenalty = CalcKingShelterPenaltyFactorBlackPerspective(blackKingFile, BlackPawns);
            if ((castleFlags & CastleFlags.BlackShort) != 0)
            {
                castlePenalty = CalcKingShelterPenaltyFactorBlackPerspective(ChessFile.FileG, BlackPawns) + 2;
                if (castlePenalty < lowestBlackPenalty) { lowestBlackPenalty = castlePenalty; }
            }
            if ((castleFlags & CastleFlags.BlackLong) != 0)
            {
                castlePenalty = CalcKingShelterPenaltyFactorBlackPerspective(ChessFile.FileC, BlackPawns) + 2;
                if (castlePenalty < lowestBlackPenalty) { lowestBlackPenalty = castlePenalty; }
            }

            retval -= lowestWhitePenalty;
            retval += lowestBlackPenalty;

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

