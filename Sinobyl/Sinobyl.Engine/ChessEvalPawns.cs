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
        public readonly int[,] PawnPassedValuePosStage;
        public readonly PawnInfo[] pawnHash = new PawnInfo[1000];

        public ChessEvalPawns(ChessEvalSettings settings, uint hashSize = 1000)
        {
            pawnHash = new PawnInfo[hashSize];

            this.DoubledPawnValueStart = settings.PawnDoubled.Opening;
            this.DoubledPawnValueEnd = settings.PawnDoubled.Endgame;
            this.IsolatedPawnValueStart = settings.PawnIsolated.Opening;
            this.IsolatedPawnValueEnd = settings.PawnIsolated.Endgame;

            //setup passed pawn array
            PawnPassedValuePosStage = new int[64, 2];
            foreach (ChessPosition pos in ChessPositionInfo.AllPositions)
            {
                foreach (ChessGameStage stage in ChessGameStageInfo.AllGameStages)
                {
                    PawnPassedValuePosStage[(int)pos, (int)stage] = settings.PawnPassedValues[stage][pos];
                }
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
            //not in the hash
            ChessBitboard passedPawns;
            int startVal;
            int endVal;
            EvalAllPawns(board.PieceLocations(ChessPiece.WPawn), board.PieceLocations(ChessPiece.BPawn), out startVal, out endVal, out passedPawns);
            retval = new PawnInfo(board.ZobristPawn, startVal, endVal, passedPawns);
            pawnHash[idx] = retval;
            return retval;
        }

        public void EvalAllPawns(ChessBitboard whitePawns, ChessBitboard blackPawns, out int StartVal, out int EndVal, out ChessBitboard passedPawns)
        {
            ChessBitboard doubled = ChessBitboard.Empty;
            ChessBitboard isolated = ChessBitboard.Empty;
            EvalAllPawns(whitePawns, blackPawns, out StartVal, out EndVal, out passedPawns, out doubled, out isolated);
        }
        public void EvalAllPawns(ChessBitboard whitePawns, ChessBitboard blackPawns, out int StartVal, out int EndVal, out ChessBitboard passed, out ChessBitboard doubled, out ChessBitboard isolated)
        {
            //eval for white
            doubled = ChessBitboard.Empty;
            passed = ChessBitboard.Empty;
            isolated = ChessBitboard.Empty;

            int WStartVal = 0;
            int WEndVal = 0;

            EvalWhitePawns(whitePawns, blackPawns, ref WStartVal, ref WEndVal, out passed, out doubled, out isolated);

            //create inverse situation
            ChessBitboard bpassed = ChessBitboard.Empty;
            ChessBitboard bdoubled = ChessBitboard.Empty;
            ChessBitboard bisolated = ChessBitboard.Empty;

            ChessBitboard blackRev = blackPawns.Reverse();
            ChessBitboard whiteRev = whitePawns.Reverse();

            int BStartVal = 0;
            int BEndVal = 0;

            //actually passing in the black pawns from their own perspective
            EvalWhitePawns(blackRev, whiteRev, ref BStartVal, ref BEndVal, out bpassed, out bdoubled, out bisolated);

            doubled |= bdoubled.Reverse();
            passed |= bpassed.Reverse();
            isolated |= bisolated.Reverse();

            //set return values;
            StartVal = WStartVal - BStartVal;
            EndVal = WEndVal - BEndVal;

        }
        private void EvalWhitePawns(ChessBitboard whitePawns, ChessBitboard blackPawns, ref int StartVal, ref int EndVal, out ChessBitboard passed, out ChessBitboard doubled, out ChessBitboard isolated)
        {
            passed = ChessBitboard.Empty;
            doubled = ChessBitboard.Empty;
            isolated = ChessBitboard.Empty;

            foreach (ChessPosition pos in whitePawns.ToPositions())
            {
                ChessFile f = pos.GetFile();
                ChessRank r = pos.GetRank();
                ChessBitboard bbFile = pos.GetFile().Bitboard();
                ChessBitboard bbFile2E = bbFile.ShiftDirE();
                ChessBitboard bbFile2W = bbFile.ShiftDirW();

                //substract doubled score
                if (!(bbFile & ~pos.Bitboard() & whitePawns).Empty())
                {
                    StartVal -= this.DoubledPawnValueStart;
                    EndVal -= this.DoubledPawnValueEnd;
                    doubled |= pos.Bitboard();
                }

                if ((bbFile2E & whitePawns).Empty() && (bbFile2W & whitePawns).Empty())
                {
                    StartVal -= this.IsolatedPawnValueStart;
                    EndVal -= this.IsolatedPawnValueEnd;
                    isolated |= pos.Bitboard();
                }

                ChessBitboard blockPositions = (bbFile | bbFile2E | bbFile2W) & pos.GetRank().BitboardAllNorth().ShiftDirN();
                if ((blockPositions & blackPawns).Empty())
                {
                    StartVal += this.PawnPassedValuePosStage[(int)pos, (int)ChessGameStage.Opening];
                    EndVal += this.PawnPassedValuePosStage[(int)pos, (int)ChessGameStage.Endgame];
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

    }

    public class PawnInfo
    {

        public readonly Int64 PawnZobrist;
        public readonly int StartVal;
        public readonly int EndVal;
        public readonly ChessBitboard PassedPawns;

        public PawnInfo(Int64 pawnZobrist, int startVal, int endVal, ChessBitboard passedPawns)
        {
            PawnZobrist = pawnZobrist;
            StartVal = startVal;
            EndVal = endVal;
            PassedPawns = passedPawns;
        }

    }
}
