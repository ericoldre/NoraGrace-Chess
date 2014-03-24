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
        public readonly int[,] PcSqValuePosStage;
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

            //setup passed pawn array
            PcSqValuePosStage = new int[64, 2];
            //PawnPassedValuePosStage = new int[64, 2];
            foreach (ChessPosition pos in ChessPositionInfo.AllPositions)
            {
                foreach (ChessGameStage stage in ChessGameStageInfo.AllGameStages)
                {
                    PcSqValuePosStage[(int)pos, (int)stage] = settings.PcSqTables.Pawn[stage][pos];
                    //PawnPassedValuePosStage[(int)pos, (int)stage] = settings.PawnPassedValues[stage][pos];
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
            int wpcSqStart = 0;
            int wpcSqEnd = 0;

            EvalWhitePawns(whitePawns, blackPawns, ref WStartVal, ref WEndVal, ref wpcSqStart, ref wpcSqEnd ,out passed, out doubled, out isolated, out unconnected);

            //create inverse situation
            ChessBitboard bpassed = ChessBitboard.Empty;
            ChessBitboard bdoubled = ChessBitboard.Empty;
            ChessBitboard bisolated = ChessBitboard.Empty;
            ChessBitboard bunconnected = ChessBitboard.Empty;

            ChessBitboard blackRev = blackPawns.Reverse();
            ChessBitboard whiteRev = whitePawns.Reverse();

            int BStartVal = 0;
            int BEndVal = 0;
            int bPcSqStart = 0;
            int bPcSqEnd = 0;

            //actually passing in the black pawns from their own perspective
            EvalWhitePawns(blackRev, whiteRev, ref BStartVal, ref BEndVal, ref bPcSqStart, ref bPcSqEnd, out bpassed, out bdoubled, out bisolated, out bunconnected);

            doubled |= bdoubled.Reverse();
            passed |= bpassed.Reverse();
            isolated |= bisolated.Reverse();
            unconnected |= bunconnected.Reverse();

            //set return values;
            int StartVal = WStartVal - BStartVal;
            int EndVal = WEndVal - BEndVal;
            int pcSqStart = wpcSqStart - bPcSqStart;
            int pcSqEnd = wpcSqEnd - bPcSqEnd;

            return new PawnInfo(pawnZobrist, StartVal, EndVal, passed, pcSqStart, pcSqEnd, doubled, isolated, unconnected);

        }
        private void EvalWhitePawns(ChessBitboard whitePawns, ChessBitboard blackPawns, ref int StartVal, ref int EndVal, ref int pcSqStart, ref int pcSqEnd, out ChessBitboard passed, out ChessBitboard doubled, out ChessBitboard isolated, out ChessBitboard unconnected)
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

                //pcsq values
                pcSqStart += this.PcSqValuePosStage[(int)pos, (int)ChessGameStage.Opening];
                pcSqEnd += this.PcSqValuePosStage[(int)pos, (int)ChessGameStage.Endgame];

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

    }

    public class PawnInfo
    {

        public readonly Int64 PawnZobrist;
        public readonly int StartVal;
        public readonly int EndVal;
        public readonly ChessBitboard PassedPawns;
        public readonly int StartValPcSq;
        public readonly int EndValPcSq;
        public readonly ChessBitboard Doubled;
        public readonly ChessBitboard Isolated;
        public readonly ChessBitboard Unconnected;

        public PawnInfo(Int64 pawnZobrist, int startVal, int endVal, ChessBitboard passedPawns, int startValPcSq, int endValPcSq, ChessBitboard doubled, ChessBitboard isolated, ChessBitboard unconnected)
        {
            PawnZobrist = pawnZobrist;
            StartVal = startVal;
            EndVal = endVal;
            PassedPawns = passedPawns;
            StartValPcSq = startValPcSq;
            EndValPcSq = endValPcSq;
            Doubled = doubled;
            Isolated = isolated;
            Unconnected = unconnected;
        }

    }
}
