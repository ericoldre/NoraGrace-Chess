using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{
    public class ChessEval
    {
        public static int TotalAIEvals = 0;

        private readonly int PawnDoubledStart = 10;
        private readonly int PawnDoubledEnd = 25;
        private readonly int PawnIsoStart = 15;
        private readonly int PawnIsoEnd = 20;

        public readonly int[,] PawnPassedValuePosStage = new int[64,2];
        public readonly int[,,] _pcsqPiecePosStage = new int[12,64,2];
        public readonly int[,] _matPieceStage = new int[12,2];
        public readonly int[, ,] _mobilityPiecesStage = new int[28, 12, 2];

        public ChessEval()
            : this(ChessEvalSettings.Default())
        {

        }

        public ChessEval(ChessEvalSettings settings)
        {
            //setup material arrays
            foreach (ChessPiece piece in Chess.AllPieces)
            {
                foreach (ChessGameStage stage in Chess.AllGameStages)
                {
                    if (piece.PieceToPlayer() == ChessPlayer.White)
                    {
                        _matPieceStage[(int)piece, (int)stage] = settings.MaterialValues[piece.ToPieceType()][stage];
                    }
                    else
                    {
                        _matPieceStage[(int)piece, (int)stage] = -settings.MaterialValues[piece.ToPieceType()][stage];
                    }
                }
            }

            //setup piecesq tables
            foreach (ChessPosition pos in Chess.AllPositions)
            {
                foreach (ChessPiece piece in Chess.AllPieces)
                {
                    foreach (ChessGameStage stage in Chess.AllGameStages)
                    {
                        if (piece.PieceToPlayer() == ChessPlayer.White)
                        {
                            _pcsqPiecePosStage[(int)piece, (int)pos, (int)stage] = settings.PcSqTables[piece.ToPieceType()][ChessGameStage.Opening][pos];
                        }
                        else
                        {
                            _pcsqPiecePosStage[(int)piece, (int)pos, (int)stage] = -settings.PcSqTables[piece.ToPieceType()][ChessGameStage.Opening][pos.Reverse()];
                        }
                    }
                    
                }
            }
            
            //setup passed pawn array
            foreach (ChessPosition pos in Chess.AllPositions)
            {
                foreach (ChessGameStage stage in Chess.AllGameStages)
                {
                    PawnPassedValuePosStage[(int)pos, (int)stage] = settings.PawnPassedValues[stage][pos];
                }
            }

            //setup pawn info
            this.PawnDoubledStart = settings.PawnDoubled.Opening;
            this.PawnDoubledEnd = settings.PawnDoubled.Endgame;
            this.PawnIsoStart = settings.PawnIsolated.Opening;
            this.PawnIsoEnd = settings.PawnIsolated.Endgame;

            //setup mobility arrays
            
            foreach (ChessPiece piece in Chess.AllPieces)
            {
                foreach (ChessGameStage stage in Chess.AllGameStages)
                {
                    for (int attacksCount = 0; attacksCount < 28; attacksCount++)
                    {
                        var mob = settings.Mobility;
                        var opiece = mob[piece.ToPieceType()];
                        var ostage = opiece[stage];

                        int val = (attacksCount - ostage.ExpectedAttacksAvailable) * ostage.AmountPerAttackDefault;
                        _mobilityPiecesStage[attacksCount, (int)piece, (int)stage] = piece.PieceToPlayer() == ChessPlayer.White ? val : -val;
                    }
                    
                }
            }
            
        }

        public int EvalFor(ChessBoard board, ChessPlayer who)
        {
            int retval = Eval(board);
            if (who == ChessPlayer.Black) { retval = -retval; }
            return retval;

        }
        public int Eval(ChessBoard board)
        {
            TotalAIEvals++; //for debugging

            int[] PieceCount = new int[12];

            int valStartMat = 0;
            int valEndMat = 0;
            int valStartPieceSq = 0;
            int valEndPieceSq = 0;
            int valStartMobility = 0;
            int valEndMobility = 0;
            for (int ipos = 0; ipos < 64; ipos++)
            {
                ChessPosition pos = Chess.AllPositions[ipos];
                ChessPiece piece = board.PieceAt(pos);
                if (piece == ChessPiece.EMPTY) { continue; }

                PieceCount[(int)piece]++;

                //add material score
                valStartMat += this._matPieceStage[(int)piece, (int)ChessGameStage.Opening];
                valEndMat += this._matPieceStage[(int)piece, (int)ChessGameStage.Endgame];

                //add pcsq score;
                valStartPieceSq += this._pcsqPiecePosStage[(int)piece, (int)pos, (int)ChessGameStage.Opening];
                valEndPieceSq += this._pcsqPiecePosStage[(int)piece, (int)pos, (int)ChessGameStage.Endgame];

                //generate attacks
                ChessBitboard slidingAttacks = ChessBitboard.Empty;
                switch (piece)
                {
                    case ChessPiece.WPawn:
                        break;
                    case ChessPiece.WKnight:
                        slidingAttacks = Attacks.KnightAttacks(pos);
                        break;
                    case ChessPiece.WBishop:
                        slidingAttacks = Attacks.BishopAttacks(pos, board.PieceLocationsAllA1H8, board.PieceLocationsAllH1A8);
                        break;
                    case ChessPiece.WRook:
                        slidingAttacks = Attacks.RookAttacks(pos, board.PieceLocationsAll, board.PieceLocationsAllVert);
                        break;
                    case ChessPiece.WQueen:
                        slidingAttacks = Attacks.QueenAttacks(pos, board.PieceLocationsAll, board.PieceLocationsAllVert, board.PieceLocationsAllA1H8, board.PieceLocationsAllH1A8);
                        break;
                    case ChessPiece.WKing:
                        break;
                    case ChessPiece.BPawn:
                        break;
                    case ChessPiece.BKnight:
                        slidingAttacks = Attacks.KnightAttacks(pos);
                        break;
                    case ChessPiece.BBishop:
                        slidingAttacks = Attacks.BishopAttacks(pos, board.PieceLocationsAllA1H8, board.PieceLocationsAllH1A8);
                        break;
                    case ChessPiece.BRook:
                        slidingAttacks = Attacks.RookAttacks(pos, board.PieceLocationsAll, board.PieceLocationsAllVert);
                        break;
                    case ChessPiece.BQueen:
                        slidingAttacks = Attacks.QueenAttacks(pos, board.PieceLocationsAll, board.PieceLocationsAllVert, board.PieceLocationsAllA1H8, board.PieceLocationsAllH1A8);
                        break;
                    case ChessPiece.BKing:
                        break;
                }

                //
                ChessBitboard slidingMoves = slidingAttacks & ~board.PlayerLocations(piece.PieceToPlayer());
                int moveCount = slidingMoves.BitCount();
                valStartMobility += _mobilityPiecesStage[moveCount, (int)piece, (int)ChessGameStage.Opening];
                valEndMobility += _mobilityPiecesStage[moveCount, (int)piece, (int)ChessGameStage.Endgame];
            }
            


            //pawns
            PawnInfo pawns = PawnEval(board);


            
            //calculate total start and end values
            int valStart = valStartMat + valStartPieceSq + valStartMobility + pawns.StartVal;
            int valEnd = valEndMat + valEndPieceSq + valEndMobility + pawns.EndVal;

            //calculate current stage of the game
            int WhiteCount = PieceCount[(int)ChessPiece.WKnight] + PieceCount[(int)ChessPiece.WBishop] + PieceCount[(int)ChessPiece.WRook] + PieceCount[(int)ChessPiece.WQueen];
            int BlackCount = PieceCount[(int)ChessPiece.BKnight] + PieceCount[(int)ChessPiece.BBishop] + PieceCount[(int)ChessPiece.BRook] + PieceCount[(int)ChessPiece.BQueen];
            
            float StartFactor = (float)(WhiteCount + BlackCount) / (float)14;
            float EndFactor = 1 - StartFactor;


            int retval = (int)(valStart * StartFactor) + (int)(valEnd * EndFactor);

            return retval;
        }


        #region pawn eval

        public PawnInfo[] pawnHash = new PawnInfo[1000];

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
            retval = new PawnInfo(board.ZobristPawn, board.PieceLocations(ChessPiece.WPawn), board.PieceLocations(ChessPiece.BPawn), this);
            pawnHash[idx] = retval;
            return retval;
        }

        public class PawnInfo
        {


            public readonly Int64 PawnZobrist;
            public readonly int StartVal;
            public readonly int EndVal;

            public PawnInfo(Int64 zob, ChessBitboard whitePawns, ChessBitboard blackPawns, ChessEval eval)
            {
                PawnZobrist = zob;
                EvalAllPawns(whitePawns, blackPawns, eval, ref StartVal, ref EndVal);

            }
            public static void EvalAllPawns(ChessBitboard whitePawns, ChessBitboard blackPawns, ChessEval eval, ref int StartVal, ref int EndVal)
            {
                ChessBitboard doubled = ChessBitboard.Empty;
                ChessBitboard passed = ChessBitboard.Empty;
                ChessBitboard isolated = ChessBitboard.Empty;
                EvalAllPawns(whitePawns, blackPawns, eval, ref StartVal, ref EndVal, out passed, out doubled, out isolated);
            }
            public static void EvalAllPawns(ChessBitboard whitePawns, ChessBitboard blackPawns, ChessEval eval, ref int StartVal, ref int EndVal, out ChessBitboard passed, out ChessBitboard doubled, out ChessBitboard isolated)
            {
                //eval for white
                doubled = ChessBitboard.Empty;
                passed = ChessBitboard.Empty;
                isolated = ChessBitboard.Empty;

                int WStartVal = 0;
                int WEndVal = 0;

                EvalWhitePawns(whitePawns, blackPawns, eval, ref WStartVal, ref WEndVal, out passed, out doubled, out isolated);

                //create inverse situation
                ChessBitboard bpassed = ChessBitboard.Empty;
                ChessBitboard bdoubled = ChessBitboard.Empty;
                ChessBitboard bisolated = ChessBitboard.Empty;

                ChessBitboard blackRev = blackPawns.Reverse();
                ChessBitboard whiteRev = whitePawns.Reverse();

                int BStartVal = 0;
                int BEndVal = 0;

                //actually passing in the black pawns from their own perspective
                EvalWhitePawns(blackRev, whiteRev, eval, ref BStartVal, ref BEndVal, out bpassed, out bdoubled, out bisolated);

                doubled |= bdoubled.Reverse();
                passed |= bpassed.Reverse();
                isolated |= bisolated.Reverse();

                //set return values;
                StartVal = WStartVal - BStartVal;
                EndVal = WEndVal - BEndVal;

            }
            private static void EvalWhitePawns(ChessBitboard whitePawns, ChessBitboard blackPawns, ChessEval eval, ref int StartVal, ref int EndVal, out ChessBitboard passed, out ChessBitboard doubled, out ChessBitboard isolated)
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
                        StartVal -= eval.PawnDoubledStart;
                        EndVal -= eval.PawnDoubledEnd;
                        doubled |= pos.Bitboard();
                    }

                    if ((bbFile2E & whitePawns).Empty() && (bbFile2W & whitePawns).Empty())
                    {
                        StartVal -= eval.PawnIsoStart;
                        EndVal -= eval.PawnIsoEnd;
                        isolated |= pos.Bitboard();
                    }

                    ChessBitboard blockPositions = (bbFile | bbFile2E | bbFile2W) & pos.GetRank().BitboardAllNorth().ShiftDirN();
                    if ((blockPositions & blackPawns).Empty())
                    {
                        StartVal += eval.PawnPassedValuePosStage[(int)pos, (int)ChessGameStage.Opening];
                        EndVal += eval.PawnPassedValuePosStage[(int)pos, (int)ChessGameStage.Endgame];
                        passed |= pos.Bitboard();
                    }
                }

            }

        }

        #endregion
    }
}
