using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoraGrace.Engine.Evaluation
{
    public class PawnEvaluator
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
        public readonly PawnResults[] pawnHash = new PawnResults[1000];
        public static int TotalEvalPawnCount = 0;

        private static Bitboard[][] _attackMask = new Bitboard[2][];
        private static Bitboard[] _telestop = new Bitboard[64];
        static PawnEvaluator()
        {
            _attackMask[0] = new Bitboard[64];
            _attackMask[1] = new Bitboard[64];
            foreach (var position in PositionUtil.AllPositions)
            {
                Bitboard north = position.ToBitboard().Flood(Direction.DirN) & ~position.ToBitboard();
                Bitboard south = position.ToBitboard().Flood(Direction.DirS) & ~position.ToBitboard();
                
                _attackMask[(int)Player.White][(int)position] = north.ShiftDirW() | north.ShiftDirE();
                _attackMask[(int)Player.Black][(int)position] = south.ShiftDirW() | south.ShiftDirE();
                _telestop[(int)position] = north;
            }
        }

        public PawnEvaluator(Settings settings, uint hashSize = 1000)
        {
            pawnHash = new PawnResults[hashSize];

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



        public PawnResults PawnEval(Board board)
        {
            long idx = board.ZobristPawn % pawnHash.GetUpperBound(0);
            if (idx < 0) { idx = -idx; }
            PawnResults retval = pawnHash[idx];
            if (retval != null && retval.PawnZobrist == board.ZobristPawn)
            {
                return retval;
            }

            retval = EvalAllPawns(board[PieceType.Pawn] & board[Player.White], board[PieceType.Pawn] & board[Player.Black], board.ZobristPawn);
            pawnHash[idx] = retval;
            return retval;
        }

        public PawnResults EvalAllPawns(Bitboard whitePawns, Bitboard blackPawns, long pawnZobrist)
        {
            TotalEvalPawnCount++;

            //eval for white
            Bitboard doubled = Bitboard.Empty;
            Bitboard passed = Bitboard.Empty;
            Bitboard isolated = Bitboard.Empty;
            Bitboard unconnected = Bitboard.Empty;
            Bitboard candidates = Bitboard.Empty;
            
            int WStartVal = 0;
            int WEndVal = 0;

            EvalWhitePawns(whitePawns, blackPawns, ref WStartVal, ref WEndVal,out passed, out doubled, out isolated, out unconnected, out candidates);

            //create inverse situation
            Bitboard bpassed = Bitboard.Empty;
            Bitboard bdoubled = Bitboard.Empty;
            Bitboard bisolated = Bitboard.Empty;
            Bitboard bunconnected = Bitboard.Empty;
            Bitboard bcandidates = Bitboard.Empty;

            Bitboard blackRev = blackPawns.Reverse();
            Bitboard whiteRev = whitePawns.Reverse();

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

            return new PawnResults(pawnZobrist, whitePawns, blackPawns, StartVal, EndVal, passed, doubled, isolated, unconnected, candidates);

        }
        private void EvalWhitePawns(Bitboard whitePawns, Bitboard blackPawns, ref int StartVal, ref int EndVal, out Bitboard passed, out Bitboard doubled, out Bitboard isolated, out Bitboard unconnected, out Bitboard candidates)
        {
            passed = Bitboard.Empty;
            doubled = Bitboard.Empty;
            isolated = Bitboard.Empty;
            unconnected = Bitboard.Empty;
            candidates = Bitboard.Empty;

            Bitboard positions = whitePawns;

            while(positions != Bitboard.Empty)
            {
                Position pos = BitboardUtil.PopFirst(ref positions);
                File f = pos.ToFile();
                Rank r = pos.ToRank();
                Bitboard bbFile = pos.ToFile().ToBitboard();
                Bitboard bbFile2E = bbFile.ShiftDirE();
                Bitboard bbFile2W = bbFile.ShiftDirW();
                Bitboard telestop = _telestop[(int)pos];

                //substract doubled score
                if (!(telestop & whitePawns).Empty())
                {
                    StartVal -= this.DoubledPawnValueStart;
                    EndVal -= this.DoubledPawnValueEnd;
                    doubled |= pos.ToBitboard();
                }

                

                //substract isolated score
                if ((bbFile2E & whitePawns).Empty() && (bbFile2W & whitePawns).Empty())
                {
                    StartVal -= this.IsolatedPawnValueStart;
                    EndVal -= this.IsolatedPawnValueEnd;
                    isolated |= pos.ToBitboard();
                }
                else
                {
                    //substract unconnected penalty, isolated is not also unconnected
                    Bitboard bbRank = r.ToBitboard();
                    Bitboard connectedRanks = bbRank | bbRank.ShiftDirN() | bbRank.ShiftDirS();
                    Bitboard connectedPos = connectedRanks & (bbFile2E | bbFile2W);
                    if ((whitePawns & connectedPos).Empty())
                    {
                        StartVal -= this.UnconnectedPawnStart;
                        EndVal -= this.UnconnectedPawnEnd;
                        unconnected |= pos.ToBitboard();
                    }
                }

                if ((telestop & whitePawns).Empty())
                {
                    Bitboard blockPositions = _attackMask[0][(int)pos] | _telestop[(int)pos];
                    if ((blockPositions & blackPawns).Empty())
                    {
                        //StartVal += this.PawnPassedValuePosStage[(int)pos, (int)ChessGameStage.Opening];
                        //EndVal += this.PawnPassedValuePosStage[(int)pos, (int)ChessGameStage.Endgame];
                        passed |= pos.ToBitboard();
                    }
                    else if (IsCandidate(pos, whitePawns, blackPawns))
                    {
                        candidates |= pos.ToBitboard();
                    }
                }
                
            }

        }

        private static bool IsCandidate(Position pos, Bitboard white, Bitboard black)
        {
            System.Diagnostics.Debug.Assert(white.Contains(pos));

            if ((_telestop[(int)pos] & black) != Bitboard.Empty)
            {
                return false;
            }

            if (!(Bitboard.Rank4 | Bitboard.Rank5 | Bitboard.Rank6 | Bitboard.Rank7).Contains(pos))
            {
                return false;
            }

            Bitboard blockers = _attackMask[0][(int)pos] & black;
            Bitboard helpers = _attackMask[1][(int)pos.PositionInDirectionUnsafe(Direction.DirN)] & white;

            System.Diagnostics.Debug.Assert(blockers != Bitboard.Empty); //otherwise it's a passed pawn.

            if (helpers == Bitboard.Empty) { return false; }

            return helpers.BitCount() >= blockers.BitCount();
        }
        

        public static GameResult? EndgameKPK(Position whiteKing, Position blackKing, Position whitePawn, bool whiteToMove)
        {
            if (!whiteToMove
                && ((Attacks.KingAttacks(blackKing) & whitePawn.ToBitboard()) != Bitboard.Empty)
                && (Attacks.KingAttacks(whiteKing) & whitePawn.ToBitboard()) == Bitboard.Empty)
            {
                //black can just take white.
                return GameResult.Draw;
            }

            Position prom = Rank.Rank8.ToPosition(whitePawn.ToFile());
            int pdist = prom.DistanceTo(whitePawn);
            if (whitePawn.ToRank() == Rank.Rank2) { pdist--; }
            if (!whiteToMove) { pdist++; }
            if (prom.DistanceTo(blackKing) > pdist)
            {
                //pawn can run for end.
                return GameResult.WhiteWins;
            }

            return null;


        }

        #region passed pawns

        int PASSED_PAWN_MIN_SCORE = 10;
        int[] endScore = new int[8];
        int[] factors = new int[8];
        int[] startScore = new int[8];

        private void PassedInitialization(Settings settings)
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



        public PhasedScore EvalPassedPawns(Board board, ChessEvalAttackInfo[] attackInfo, Bitboard passedPawns, Bitboard candidatePawns, int[] workspace)
        {
            
            PhasedScore retval = PhasedScoreUtil.Create(0, 0);

            var positions = (passedPawns | candidatePawns) & board[Player.White];
            if (positions != Bitboard.Empty)
            {
                var white = EvalPassedPawnsSide(Player.White, board, attackInfo[0], attackInfo[1], passedPawns & board[Player.White], candidatePawns & board[Player.White], workspace);
                retval = retval.Add(white);

            }

            positions = (passedPawns | candidatePawns) & board[Player.Black];
            if (positions != Bitboard.Empty)
            {
                var black = EvalPassedPawnsSide(Player.Black, board, attackInfo[1], attackInfo[0], passedPawns & board[Player.Black], candidatePawns & board[Player.Black], workspace);
                retval = retval.Subtract(black);
            }


            return retval;

        }

        public PhasedScore EvalPassedPawnsSide(Player me, Board board, ChessEvalAttackInfo myAttackInfo, ChessEvalAttackInfo hisAttackInfo, Bitboard passedPawns, Bitboard candidatePawns, int[] workspace)
        {
            
            int bestEndScore = -1;
            int bestEndFile = -1;
            int totalStartScore = 0;
            Player him = me.PlayerOther();

            Position myKing = board.KingPosition(me);
            Position hisKing = board.KingPosition(him);
            Bitboard allPieces = board.PieceLocationsAll;
            Bitboard myPawnAttacks = myAttackInfo.PawnEast | myAttackInfo.PawnWest;
            Bitboard myAttacks = myAttackInfo.All();
            Bitboard hisAttacks = hisAttackInfo.All();
            Bitboard positions = passedPawns | candidatePawns;

            Direction mySouth = me == Player.White ? Direction.DirS : Direction.DirN;

            Array.Clear(workspace, 0, 8);

            while (positions != Bitboard.Empty)// (ChessPosition passedPos in white.ToPositions())
            {
                Position passedPos = BitboardUtil.PopFirst(ref positions);
                Position trailerPos = Position.OUTOFBOUNDS;

                Piece trailerPiece = board.PieceInDirection(passedPos, mySouth, ref trailerPos);

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
                 
                int iFile = (int)passedPos.ToFile();

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

            return PhasedScoreUtil.Create(totalStartScore, totalEndScore);
        }

        private void EvalPassedPawn(Player me, Position p, Position myKing, Position hisKing,
            Bitboard allPieces, Bitboard myPawnAttacks, Bitboard myAttacks, Bitboard hisAttacks,
            bool attackingTrailer, bool supportingTrailer, out int mbonus, out int ebonus)
        {
            Rank rank = p.ToRank();

            if (me == Player.Black) { rank = 7 - rank; }

            //int r = Math.Abs(rank - ChessRank.Rank2);
            //int rr = r * (r - 1);

            // Base bonus based on rank
            mbonus = startScore[(int)rank];
            ebonus = endScore[(int)rank];
            int dangerFactor = factors[(int)rank];

            Position blockSq = p.PositionInDirection(me == Player.White ? Direction.DirN: Direction.DirS);

            int k = 0;

            if (rank <= Rank.Rank5)
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


        public void EvalPassedPawnsOld(Board board, EvalResults evalInfo, Bitboard passedPawns)
        {

            Position myKing, hisKing;
            Bitboard allPieces, myPawnAttacks, myAttacks, hisAttacks;
            bool attackingTrailer, supportingTrailer;

            int startScore, endScore, bestEndScore;

            var positions = passedPawns & board[Player.White];
            if (positions != Bitboard.Empty)
            {
                bestEndScore = 0;

                myKing = board.KingPosition(Player.White);
                hisKing = board.KingPosition(Player.Black);
                allPieces = board.PieceLocationsAll;
                myPawnAttacks = evalInfo.Attacks[(int)Player.White].PawnEast | evalInfo.Attacks[(int)Player.White].PawnWest;
                myAttacks = evalInfo.Attacks[(int)Player.White].All();
                hisAttacks = evalInfo.Attacks[(int)Player.Black].All();

                while (positions != Bitboard.Empty)// (ChessPosition passedPos in white.ToPositions())
                {
                    Position passedPos = BitboardUtil.PopFirst(ref positions);
                    Position trailerPos = Position.OUTOFBOUNDS;
                    Piece trailerPiece = board.PieceInDirection(passedPos, Direction.DirS, ref trailerPos);

                    attackingTrailer = trailerPiece.PieceIsSliderRook() && trailerPiece.PieceToPlayer() == Player.Black;
                    supportingTrailer = trailerPiece.PieceIsSliderRook() && trailerPiece.PieceToPlayer() == Player.White;

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

                    evalInfo.PawnsPassed = evalInfo.PawnsPassed.Add(PhasedScoreUtil.Create(startScore, endScore));

                    ///evalInfo.PawnsPassedStart += mbonus;
                    //evalInfo.PawnsPassedEnd += ebonus;
                }
            }

            positions = passedPawns & board[Player.Black];
            if (positions != Bitboard.Empty)
            {
                bestEndScore = 0;
                myKing = board.KingPosition(Player.Black).Reverse();
                hisKing = board.KingPosition(Player.White).Reverse();
                allPieces = board.PieceLocationsAll.Reverse();
                myPawnAttacks = (evalInfo.Attacks[(int)Player.Black].PawnEast | evalInfo.Attacks[(int)Player.Black].PawnWest).Reverse();
                myAttacks = evalInfo.Attacks[(int)Player.Black].All().Reverse();
                hisAttacks = evalInfo.Attacks[(int)Player.White].All().Reverse();

                while (positions != Bitboard.Empty) // (ChessPosition passedPos in black.ToPositions())
                {
                    Position passedPos = BitboardUtil.PopFirst(ref positions);
                    Position passesPos2 = passedPos.Reverse();
                    Position trailerPos = Position.OUTOFBOUNDS;
                    Piece trailerPiece = board.PieceInDirection(passedPos, Direction.DirN, ref trailerPos);

                    attackingTrailer = trailerPiece.PieceIsSliderRook() && trailerPiece.PieceToPlayer() == Player.White;
                    supportingTrailer = trailerPiece.PieceIsSliderRook() && trailerPiece.PieceToPlayer() == Player.Black;

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

                    evalInfo.PawnsPassed = evalInfo.PawnsPassed.Subtract(PhasedScoreUtil.Create(startScore, endScore));
                }
            }




        }


        private void EvalPassedPawnBoth(Position p, Position myKing, Position hisKing,
            Bitboard allPieces, Bitboard myPawnAttacks, Bitboard myAttacks, Bitboard hisAttacks,
            bool attackingTrailer, bool supportingTrailer, out int mbonus, out int ebonus)
        {
            Rank rank = p.ToRank();

            //int r = Math.Abs(rank - ChessRank.Rank2);
            //int rr = r * (r - 1);

            // Base bonus based on rank
            mbonus = startScore[(int)rank];
            ebonus = endScore[(int)rank];
            int dangerFactor = factors[(int)rank];

            Position blockSq = p.PositionInDirection(Direction.DirN);

            int k = 0;

            if (rank <= Rank.Rank5)
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

        #region unstoppable pawns

        public static PhasedScore EvalUnstoppablePawns(Board board, Bitboard passed, Bitboard candidates)
        {
            int plysToPromoteWhite = 99;
            if (board[Player.Black] == (board[Player.Black, PieceType.King] | board[Player.Black, PieceType.Pawn]))
            {
                //black has only king/pawns, check to see if white has clear path to promote.
                Bitboard playerPassed = passed & board[Player.White];
                while (playerPassed != 0)
                {
                    Position pawnPos = BitboardUtil.PopFirst(ref playerPassed);
                    int plys;
                    if (IsPassedUnstoppable(board, Player.White, pawnPos, out plys))
                    {
                        plysToPromoteWhite = Math.Min(plysToPromoteWhite, plys);
                    }
                }
            }

            int plysToPromoteBlack = 99;
            if (board[Player.White] == (board[Player.White, PieceType.King] | board[Player.White, PieceType.Pawn]))
            {
                //white has only king/pawns
                //black has only king/pawns, check to see if white has clear path to promote.
                Bitboard playerPassed = passed & board[Player.Black];
                while (playerPassed != 0)
                {
                    Position pawnPos = BitboardUtil.PopFirst(ref playerPassed);
                    int plys;
                    if (IsPassedUnstoppable(board, Player.Black, pawnPos, out plys))
                    {
                        plysToPromoteBlack = Math.Min(plysToPromoteBlack, plys);
                    }
                }
            }

            if (plysToPromoteWhite != plysToPromoteBlack && Math.Abs(plysToPromoteWhite - plysToPromoteBlack) > 2)
            {
                if (plysToPromoteWhite < plysToPromoteBlack)
                {
                    return PhasedScoreUtil.Create(0, 750 - (plysToPromoteWhite * 20));
                }
                else
                {
                    return PhasedScoreUtil.Create(0, -750 + (plysToPromoteBlack * 20));
                }
            }
            return PhasedScoreUtil.Create(0, 0);
        }

        public static bool IsPassedUnstoppable(Board board, Player pawnPlayer, Position pawnPos, out int plysToPromote)
        {

            Player kingPlayer = pawnPlayer.PlayerOther();


            //should not call unless other player does not have pawns.
            System.Diagnostics.Debug.Assert(board[kingPlayer] == (board[kingPlayer, PieceType.King] | board[kingPlayer, PieceType.Pawn]));

            //resolve basic locations
            File pawnFile = pawnPos.ToFile();
            Rank pawnRank = pawnPos.ToRank();
            Rank pawnRank8 = pawnPlayer.MyRank(Rank.Rank8);
            Position queenSq = pawnRank8.ToPosition(pawnFile);
            Position kingPosition = board.KingPosition(kingPlayer);



            //calculate dist
            int pawnDist = Math.Abs(pawnRank8 - pawnRank);
            int kingDist = kingPosition.DistanceTo(queenSq);


            //calc plys to capture or promote
            plysToPromote = (pawnDist * 2) - (board.WhosTurn == pawnPlayer ? 1 : 0);
            int plysToCapture = (kingDist * 2) - (board.WhosTurn == kingPlayer ? 1 : 0);

            //find path to promotion
            Bitboard path = pawnPos.Between(queenSq) | queenSq.ToBitboard();

            

            if (path == (path & Attacks.KingAttacks(board.KingPosition(pawnPlayer))))
            {
                //entire path guarded by king.
                return true;
            }

            //something in the way. add plys to clear out of way.
            if ((path & board.PieceLocationsAll) != 0)
            {
                plysToPromote += (path & board.PieceLocationsAll).BitCount() * 2;
            }


            if (plysToCapture <= plysToPromote + 1)
            {
                return false;
            }

            return plysToCapture > (plysToPromote + 1);

        }

        #endregion
    }

    public class PawnResults
    {

        public readonly Int64 PawnZobrist;
        public readonly Bitboard WhitePawns;
        public readonly Bitboard BlackPawns;
        public readonly PhasedScore Value;
        public readonly Bitboard PassedPawns;
        public readonly Bitboard Doubled;
        public readonly Bitboard Isolated;
        public readonly Bitboard Unconnected;
        public readonly Bitboard Candidates;



        public PawnResults(Int64 pawnZobrist, Bitboard whitePawns, Bitboard blackPawns, int startVal, int endVal, Bitboard passedPawns, Bitboard doubled, Bitboard isolated, Bitboard unconnected, Bitboard candidates)
        {
            PawnZobrist = pawnZobrist;
            WhitePawns = whitePawns;
            BlackPawns = blackPawns;
            Value = PhasedScoreUtil.Create(startVal, endVal);
            PassedPawns = passedPawns;
            Doubled = doubled;
            Isolated = isolated;
            Unconnected = unconnected;
            Candidates = candidates;
        }




        int _shelterCacheKey = 0x3a184743; //random number, important not zero, or anything that key could compute to.
        int _shelterCacheValue = 0;

        private static readonly int[] _shelterFactor = new int[] { 7, 0, 2, 5, 6, 7, 7, 7 };

        public int EvalShelter(File whiteKingFile, File blackKingFile, CastleFlags castleFlags)
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
        private int EvalShelterCalc(File whiteKingFile, File blackKingFile, CastleFlags castleFlags)
        {
            
            int retval = 0;

            int castlePenalty;

            Bitboard wpRev = WhitePawns.Reverse();
            int lowestWhitePenalty = CalcKingShelterPenaltyFactorBlackPerspective(whiteKingFile, wpRev);
            if ((castleFlags & CastleFlags.WhiteShort) != 0)
            {
                castlePenalty = CalcKingShelterPenaltyFactorBlackPerspective(File.FileG, wpRev) + 2;
                if (castlePenalty < lowestWhitePenalty) { lowestWhitePenalty = castlePenalty; }
            }
            if ((castleFlags & CastleFlags.WhiteLong) != 0)
            {
                castlePenalty = CalcKingShelterPenaltyFactorBlackPerspective(File.FileC, wpRev) + 2;
                if (castlePenalty < lowestWhitePenalty) { lowestWhitePenalty = castlePenalty; }
            }

            int lowestBlackPenalty = CalcKingShelterPenaltyFactorBlackPerspective(blackKingFile, BlackPawns);
            if ((castleFlags & CastleFlags.BlackShort) != 0)
            {
                castlePenalty = CalcKingShelterPenaltyFactorBlackPerspective(File.FileG, BlackPawns) + 2;
                if (castlePenalty < lowestBlackPenalty) { lowestBlackPenalty = castlePenalty; }
            }
            if ((castleFlags & CastleFlags.BlackLong) != 0)
            {
                castlePenalty = CalcKingShelterPenaltyFactorBlackPerspective(File.FileC, BlackPawns) + 2;
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
        private static int CalcKingShelterPenaltyFactorBlackPerspective(File kingFile, Bitboard myPawns)
        {
            if (kingFile == File.FileA) { kingFile = File.FileB; }
            if (kingFile == File.FileH) { kingFile = File.FileG; }

            int retval = 0;
            int count7th = 0;
            for (File f = kingFile - 1; f <= kingFile + 1; f++)
            {
                Bitboard bbFile = f.ToBitboard();
                Bitboard bbPawnFile = bbFile & myPawns;
                Rank pawnRank;
                if (bbPawnFile == Bitboard.Empty)
                {
                    pawnRank = Rank.Rank1;
                }
                else
                {
                    pawnRank = bbPawnFile.NorthMostPosition().ToRank();
                }
                retval += _shelterFactor[(int)pawnRank];
                if (pawnRank == Rank.Rank7)
                {
                    count7th++;
                }
            }
            if (count7th >= 2) { retval--; }
            return retval;
        }


    }
}

