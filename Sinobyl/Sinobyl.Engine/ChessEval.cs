using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{
	public class ChessEval
	{

		#region Piece Square Tables
		public static readonly int[] PawnStart64 = new int[]{
			0,  0,  0,  0,  0,  0,  0,  0,	
			-10, -5, 0, 5,  5,  0, -5, -10,
			-10, -5, 0, 5,  5,  0, -5, -10,	
			-10, -5, 0, 10, 10, 0, -5, -10,	
			-10, -5, 5, 15, 15, 0, -5, -10,	
			-10, -5, 5, 10, 10, 0, -5, -10,	
			-10, -5, 0, 5,  5,  5, -5, -10,	
			 0,  0,  0,  0,  0,  0,  0,  0,	
		};
		public static readonly int[] PawnEnd64 = new int[] { 
			0,  0,  0,  0,  0,  0,  0,  0,	
			20, 25, 34, 38, 38, 34, 25, 20,
			12, 20, 24, 32, 32, 24, 20, 12,	
			 6, 11, 18,	27, 27, 16,	11,  6,	
			 4,  7, 10, 12, 12, 10,  7,  4,	
			-3, -3, -3, -3, -3, -3, -3,  -3,	
			-10,-10,-10,-10,-10,-10,-10,-10,	
			 0,  0,  0,  0,  0,  0,  0,  0,			
		};

		public static readonly int[] KnightStart64 = new int[] { 
			-7,	-3,	 1,	 3,	 3,	 1,	-3,	-7,	
			 2,	 3,	 7,	10,	10,	 7,	 3,	 2,	
			 3,	 7,	14,	16,	16,	14,	 7,	 3,	
			 4,	 9,	15,	16,	16,	15,	 9,	 4,	
			 4,	 9,	13,	15,	15,	13,	 9,	 4,	
			 3,	 7,	11,	13,	13,	11,	 7,	 3,	
			 2,	 3,	 7,	10,	10,	 7,	 3,	 2,	
			-7,	-3,	 1,	 3,	 3,	 1,	-3,	-7,			
		};

		public static readonly int[] KnightEnd64 = new int[] { 
			-7,	-3,	 1,	 3,	 3,	 1,	-3,	-7,	
			 2,	 3,	 7,	10,	10,	 7,	 3,	 2,	
			 3,	 7,	14,	16,	16,	14,	 7,	 3,	
			 4,	 9,	15,	16,	16,	15,	 9,	 4,	
			 4,	 9,	13,	15,	15,	13,	 9,	 4,	
			 3,	 7,	11,	13,	13,	11,	 7,	 3,	
			 2,	 3,	 7,	10,	10,	 7,	 3,	 2,	
			-7,	-3,	 1,	 3,	 3,	 1,	-3,	-7,				
		};

		public static readonly int[] BishopStart64 = new int[] { 
			-3,	-3,	-3,	-3,	-3,	-3,	-3,	-3,
			-3,	 9,	11,	11,	11,	11,	 9,	-3,	
			-3,	 8,	12,	12,	12,	12,	 8,	-3,	
			-3,	 0,	12,	12,	12,	12,	 6,	-3,	
			-3,	 0,	12,	12,	12,	12,	 6,	-3,	
			 6,	 8,	12,	12,	12,	12,	 8,	 6,	
			 6,	 9,	11,	11,	11,	11,	 9,	 6,	
			-3,	-3,	-3,	-3,	-3,	-3,	-3,	-3,			
		};


		public static readonly int[] BishopEnd64 = new int[] { 
			-3,	-3,	-3,	-3,	-3,	-3,	-3,	-3,
			-3,	 9,	11,	11,	11,	11,	 9,	-3,	
			-3,	 8,	12,	12,	12,	12,	 8,	-3,	
			-3,	 0,	12,	12,	12,	12,	 6,	-3,	
			-3,	 0,	12,	12,	12,	12,	 6,	-3,	
			 6,	 8,	12,	12,	12,	12,	 8,	 6,	
			 6,	 9,	11,	11,	11,	11,	 9,	 6,	
			-3,	-3,	-3,	-3,	-3,	-3,	-3,	-3,					
		};

		public static readonly int[] RookStart64 = new int[] { 
			0,	0,	4,	6,	6,	4,	0,	0,	
			10,	10,	14,	16,	16,	14,	10,	10,
			0,	0,	4,	6,	6,	4,	0,	0,
			0,	0,	4,	6,	6,	4,	0,	0,
			0,	0,	4,	6,	6,	4,	0,	0,	
			0,	0,	4,	6,	6,	4,	0,	0,	
			0,	0,	4,	6,	6,	4,	0,	0,	
			0,	0,	4,	6,	6,	4,	0,	0			
		};

		public static readonly int[] RookEnd64 = new int[] { 
			0,	0,	0,	0,	0,	0,	0,	0,	
			0,	0,	0,	0,	0,	0,	0,	0,	
			0,	0,	0,	0,	0,	0,	0,	0,
			0,	0,	0,	0,	0,	0,	0,	0,	
			0,	0,	0,	0,	0,	0,	0,	0,	
			0,	0,	0,	0,	0,	0,	0,	0,	
			0,	0,	0,	0,	0,	0,	0,	0,	
			0,	0,	0,	0,	0,	0,	0,	0			
		};

		public static readonly int[] QueenStart64 = new int[] { 
			0,	0,	0,	0,	0,	0,	0,	0,	
			0,  0,	0,	0,	0,	0,	0,	0,	
			0,	0,	0,	0,	0,	0,	0,	0,	
			0,	0,	0,	2,	2,	0,	0,	0,	
			0,	0,	0,	2,	2,	0,	0,	0,	
			0,	1,	1,	1,	1,	0,	0,	0,	
			0,	0,	1,	1,	1,	0,	0,	0,	
			-2,-2, -2,  0,	0, -2, -2, -2
		};

		public static readonly int[] QueenEnd64 = new int[] { 
			0,	0,	0,	0,	0,	0,	0,	0,	
			0,  0,	0,	0,	0,	0,	0,	0,	
			0,	0,	0,	0,	0,	0,	0,	0,	
			0,	0,	0,	2,	2,	0,	0,	0,	
			0,	0,	0,	2,	2,	0,	0,	0,	
			0,	1,	1,	1,	1,	0,	0,	0,	
			0,	0,	1,	1,	1,	0,	0,	0,	
			-2,-2, -2,  0,	0, -2, -2, -2		
		};

		public static readonly int[] KingStart64 = new int[] { 
			-80,-80,-80,-80,-80,-80,-80,-80,
			-80,-80,-80,-80,-80,-80,-80,-80,
			-80,-80,-80,-80,-80,-80,-80,-80,
			-60,-60,-60,-60,-60,-60,-60,-60,
			-40,-40,-40,-40,-40,-40,-40,-40,
			-7,	-15,-15,-15,-15,-15,-15,-7,	
			-5,	-5,	-12,-12,-12,-12,-5,	-5,	
			 3,	 3,	 8,	-5,  -8,-5, 10,	 5		
		};

		public static readonly int[] KingEnd64 = new int[] { 
			 -8, -8, -8, -8, -8, -8, -8, -8,
			 -8, -0, -0, -0, -0, -0, -0, -8,
			 -8, -0, 05, 05, 05, 05, -0, -8,
			 -8, -0, 05, 30, 30, 05, -0,- -8,
			 -8, -0, 05, 30, 30, 05, -0, -8,
			 -8, -0, 05, 30, 30, 05, -0, -8,
			 -8, -0, -0, -0, -0, -0, -0, -8,
			 -8, -8, -8, -8, -8, -8, -8, -8,		
		};
		#endregion

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
		public static int MatPawnValue = 100;
		public static int MatKnightValue = 300;
		public static int MatBishopValue = 305;
		public static int MatRookValue = 500;
		public static int MatQueenValue = 900;
		public static int MatKingValue = 0;

		public static int TotalAIEvals = 0;

		public readonly int[,] PawnPassedValueStartEnd = new int[120,2];
		public readonly int[,,] PieceSquareStartEndVals = new int[12,120,2];
		public readonly int[] PieceValue = new int[12]; 


		public ChessEval()
		{
			//init material value
			PieceValue[(int)ChessPiece.WPawn] = MatPawnValue;
			PieceValue[(int)ChessPiece.WKnight] = MatKnightValue;
			PieceValue[(int)ChessPiece.WBishop] = MatBishopValue;
			PieceValue[(int)ChessPiece.WRook] = MatRookValue;
			PieceValue[(int)ChessPiece.WQueen] = MatQueenValue;
			PieceValue[(int)ChessPiece.WKing] = MatKingValue;
			PieceValue[(int)ChessPiece.BPawn] = -MatPawnValue;
			PieceValue[(int)ChessPiece.BKnight] = -MatKnightValue;
			PieceValue[(int)ChessPiece.BBishop] = -MatBishopValue;
			PieceValue[(int)ChessPiece.BRook] = -MatRookValue;
			PieceValue[(int)ChessPiece.BQueen] = -MatQueenValue;
			PieceValue[(int)ChessPiece.BKing] = -MatKingValue;

			//init piece sq tables
			for (int ipos = 0; ipos < 64; ipos++)
			{
				ChessPosition wpos = Chess.AllPositions[ipos];
				//passed pawn values;
				PawnPassedValueStartEnd[(int)wpos, 0] = ChessEval.PassedPawnStart[ipos];
				PawnPassedValueStartEnd[(int)wpos, 1] = ChessEval.PassedPawnEnd[ipos];

				//value of white at start
				PieceSquareStartEndVals[(int)ChessPiece.WPawn, (int)wpos, 0] = ChessEval.PawnStart64[ipos];
				PieceSquareStartEndVals[(int)ChessPiece.WKnight, (int)wpos, 0] = ChessEval.KnightStart64[ipos];
				PieceSquareStartEndVals[(int)ChessPiece.WBishop, (int)wpos, 0] = ChessEval.BishopStart64[ipos];
				PieceSquareStartEndVals[(int)ChessPiece.WRook, (int)wpos, 0] = ChessEval.RookStart64[ipos];
				PieceSquareStartEndVals[(int)ChessPiece.WQueen, (int)wpos, 0] = ChessEval.QueenStart64[ipos];
				PieceSquareStartEndVals[(int)ChessPiece.WKing, (int)wpos, 0] = ChessEval.KingStart64[ipos];

				//value of white in end
				PieceSquareStartEndVals[(int)ChessPiece.WPawn, (int)wpos, 1] = ChessEval.PawnEnd64[ipos];
				PieceSquareStartEndVals[(int)ChessPiece.WKnight, (int)wpos, 1] = ChessEval.KnightEnd64[ipos];
				PieceSquareStartEndVals[(int)ChessPiece.WBishop, (int)wpos, 1] = ChessEval.BishopEnd64[ipos];
				PieceSquareStartEndVals[(int)ChessPiece.WRook, (int)wpos, 1] = ChessEval.RookEnd64[ipos];
				PieceSquareStartEndVals[(int)ChessPiece.WQueen, (int)wpos, 1] = ChessEval.QueenEnd64[ipos];
				PieceSquareStartEndVals[(int)ChessPiece.WKing, (int)wpos, 1] = ChessEval.KingEnd64[ipos];

				ChessPosition bpos = Chess.PositionReverse(wpos);

				//value of black at start
				PieceSquareStartEndVals[(int)ChessPiece.BPawn, (int)bpos, 0] = -ChessEval.PawnStart64[ipos];
				PieceSquareStartEndVals[(int)ChessPiece.BKnight, (int)bpos, 0] = -ChessEval.KnightStart64[ipos];
				PieceSquareStartEndVals[(int)ChessPiece.BBishop, (int)bpos, 0] = -ChessEval.BishopStart64[ipos];
				PieceSquareStartEndVals[(int)ChessPiece.BRook, (int)bpos, 0] = -ChessEval.RookStart64[ipos];
				PieceSquareStartEndVals[(int)ChessPiece.BQueen, (int)bpos, 0] = -ChessEval.QueenStart64[ipos];
				PieceSquareStartEndVals[(int)ChessPiece.BKing, (int)bpos, 0] = -ChessEval.KingStart64[ipos];

				//value of black in end
				PieceSquareStartEndVals[(int)ChessPiece.BPawn, (int)bpos, 1] = -ChessEval.PawnEnd64[ipos];
				PieceSquareStartEndVals[(int)ChessPiece.BKnight, (int)bpos, 1] = -ChessEval.KnightEnd64[ipos];
				PieceSquareStartEndVals[(int)ChessPiece.BBishop, (int)bpos, 1] = -ChessEval.BishopEnd64[ipos];
				PieceSquareStartEndVals[(int)ChessPiece.BRook, (int)bpos, 1] = -ChessEval.RookEnd64[ipos];
				PieceSquareStartEndVals[(int)ChessPiece.BQueen, (int)bpos, 1] = -ChessEval.QueenEnd64[ipos];
				PieceSquareStartEndVals[(int)ChessPiece.BKing, (int)bpos, 1] = -ChessEval.KingEnd64[ipos];

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

				valStartMat += this.PieceValue[(int)piece];
				valEndMat += this.PieceValue[(int)piece];

				valStartPieceSq += this.PieceSquareStartEndVals[(int)piece, (int)pos, 0];
				valEndPieceSq += this.PieceSquareStartEndVals[(int)piece, (int)pos, 1];

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
