using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{
	public class ChessEval
	{
        public class PcsqTables
        {
            //public readonly int[,,] _values = new int[12, 64, 2];
            private readonly int[, ,] _values;
            public PcsqTables(ChessPieceTypeDictionary<ChessGameStageDictionary<ChessPositionDictionary<int>>> settings)
            {
                _values = new int[12, 64, 2]; 
                foreach (ChessPosition pos in Chess.AllPositions)
                {
                    foreach(ChessPiece piece in Chess.AllPieces)
                    {
                        if (piece.PieceToPlayer() == ChessPlayer.White)
                        {
                            _values[(int)piece, (int)pos, (int)ChessGameStage.Opening] = settings[piece.ToPieceType()][ChessGameStage.Opening][pos];
                            _values[(int)piece, (int)pos, (int)ChessGameStage.Endgame] = settings[piece.ToPieceType()][ChessGameStage.Endgame][pos];
                        }
                        else
                        {
                            _values[(int)piece, (int)pos, (int)ChessGameStage.Opening] = -settings[piece.ToPieceType()][ChessGameStage.Opening][pos.Reverse()];
                            _values[(int)piece, (int)pos, (int)ChessGameStage.Endgame] = -settings[piece.ToPieceType()][ChessGameStage.Endgame][pos.Reverse()];
                        }
                    }
                }
            }
            public int this[ChessPiece piece, ChessPosition pos, ChessGameStage stage]
            {
                get
                {
                    return _values[(int)piece, (int)pos, (int)stage];
                }
            }
        }



		#region pawnsetup
		private static readonly int PawnDoubledStart = 10;
		private static readonly int PawnDoubledEnd = 25;
		private static readonly int PawnIsoStart = 15;
		private static readonly int PawnIsoEnd = 20;

		public static readonly int[] PassedPawnStart = new int[] { 
			  0,  0,  0,  0,  0,  0,  0,  0,
			 50, 50, 50, 50, 50, 50, 50, 50,
			 30, 30, 30, 30, 30, 30, 30, 30,
			 20, 20, 20, 20, 20, 20, 20, 20,
			 15, 15, 15, 15, 15, 15, 15, 15,
			 15, 15, 15, 15, 15, 15, 15, 15,
			 15, 15, 15, 15, 15, 15, 15, 15,
			  0,  0,  0,  0,  0,  0,  0,  0
		};
		public static readonly int[] PassedPawnEnd = new int[] { 
			  0,  0,  0,  0,  0,  0,  0,  0,
			140,140,140,140,140,140,140,140,
			 90, 90, 90, 90, 90, 90, 90, 90,
			 60, 60, 60, 60, 60, 60, 60, 60,
			 40, 40, 40, 40, 40, 40, 40, 40,
			 25, 25, 25, 25, 25, 25, 25, 25,
			 15, 15, 15, 15, 15, 15, 15, 15,
			  0,  0,  0,  0,  0,  0,  0,  0	
		};
		#endregion

		public static int TotalAIEvals = 0;

		public readonly int[,] PawnPassedValueStartEnd = new int[120,2];
		public readonly PcsqTables PieceSquareStartEndVals;
        public readonly ChessPieceArray<int> PieceValueOpening;
        public readonly ChessPieceArray<int> PieceValueEndGame;

        public ChessEval()
            : this(ChessEvalSettings.Default())
        {

        }

		public ChessEval(ChessEvalSettings settings)
		{
            PieceSquareStartEndVals = new PcsqTables(settings.PcSqTables);
            PieceValueOpening = new ChessPieceArray<int>(settings.MaterialValues.PieceValues().Select(o => new KeyValuePair<ChessPiece, int>(o.Key, o.Key.PieceToPlayer() == ChessPlayer.White ? o.Value.Opening : -o.Value.Opening)));
            PieceValueEndGame = new ChessPieceArray<int>(settings.MaterialValues.PieceValues().Select(o => new KeyValuePair<ChessPiece, int>(o.Key, o.Key.PieceToPlayer() == ChessPlayer.White ? o.Value.Endgame : -o.Value.Endgame)));


			//init piece sq tables
			for (int ipos = 0; ipos < 64; ipos++)
			{
				ChessPosition wpos = Chess.AllPositions[ipos];
				//passed pawn values;
				PawnPassedValueStartEnd[(int)wpos, 0] = ChessEval.PassedPawnStart[ipos];
				PawnPassedValueStartEnd[(int)wpos, 1] = ChessEval.PassedPawnEnd[ipos];

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
			for (int ipos = 0; ipos < 64; ipos++)
			{
				ChessPosition pos = Chess.AllPositions[ipos];
				ChessPiece piece = board.PieceAt(pos);
				if (piece == ChessPiece.EMPTY) { continue; }

				PieceCount[(int)piece]++;

				valStartMat += this.PieceValueOpening[piece];
				valEndMat += this.PieceValueEndGame[piece];

				valStartPieceSq += this.PieceSquareStartEndVals[piece, pos, ChessGameStage.Opening];
				valEndPieceSq += this.PieceSquareStartEndVals[piece, pos, ChessGameStage.Endgame];

			}
			int valStart = valStartMat + valStartPieceSq; ;
			int valEnd = valEndMat + valEndPieceSq;


			//pawns
			PawnInfo pawns = PawnEval(board);
			valStart += pawns.StartVal;
			valEnd += pawns.EndVal;

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
			if (retval!=null && retval.PawnZobrist == board.ZobristPawn)
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
                        StartVal -= ChessEval.PawnDoubledStart;
                        EndVal -= ChessEval.PawnDoubledEnd;
                        doubled |= pos.Bitboard();
                    }

                    if ((bbFile2E & whitePawns).Empty() && (bbFile2W & whitePawns).Empty())
                    {
                        StartVal -= ChessEval.PawnIsoStart;
                        EndVal -= ChessEval.PawnIsoEnd;
                        isolated |= pos.Bitboard();
                    }

                    ChessBitboard blockPositions = (bbFile | bbFile2E | bbFile2W) & pos.GetRank().BitboardAllNorth().ShiftDirN();
                    if ((blockPositions & blackPawns).Empty())
                    {
                        StartVal += eval.PawnPassedValueStartEnd[(int)pos, 0];
                        EndVal += eval.PawnPassedValueStartEnd[(int)pos, 1];
                        passed |= pos.Bitboard();
                    }
				}

			}
			
		}

		#endregion
	}
}
