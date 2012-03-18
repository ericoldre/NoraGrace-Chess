using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

namespace Sinobyl.Engine
{
	/// <summary>
	/// Summary description for Chess.
	/// </summary>
	/// 

	#region delegates

	

	public delegate void msgFEN(object sender, ChessFEN fen);

	public delegate void msgMove(object sender, object moveObj);
	public delegate void msgVoid(object sender);
	public delegate void msgString(object sender, string sString);
	public delegate void msgRequestTime(object sender, object player, ref TimeSpan timeleft);
	public delegate void msgRequestTimeControl(object sender, object player, ref int movesPerControl, ref TimeSpan initialTime, ref TimeSpan increment);
	public delegate void msgFromTo(object sender, ChessPosition from, ChessPosition to);
	//public delegate void msgResult(object sender, ChessResult result, string reason);
	public delegate void msgThinking(object sender, object thinking);


	#endregion

	#region enums



	public enum ChessPosition
	{
		A8 = 56, B8 = 57, C8 = 58, D8 = 59, E8 = 60, F8 = 61, G8 = 62, H8 = 63,
		A7 = 48, B7 = 49, C7 = 50, D7 = 51, E7 = 52, F7 = 53, G7 = 54, H7 = 55,
		A6 = 40, B6 = 41, C6 = 42, D6 = 43, E6 = 44, F6 = 45, G6 = 46, H6 = 47,
		A5 = 32, B5 = 33, C5 = 34, D5 = 35, E5 = 36, F5 = 37, G5 = 38, H5 = 39,
		A4 = 24, B4 = 25, C4 = 26, D4 = 27, E4 = 28, F4 = 29, G4 = 30, H4 = 31,
		A3 = 16, B3 = 17, C3 = 18, D3 = 19, E3 = 20, F3 = 21, G3 = 22, H3 = 23,
		A2 = 8, B2 = 9, C2 = 10, D2 = 11, E2 = 12, F2 = 13, G2 = 14, H2 = 15,
		A1 = 0, B1 = 1, C1 = 2, D1 = 3, E1 = 4, F1 = 5, G1 = 6, H1 = 7, 
        OUTOFBOUNDS = 64
	}
	public enum ChessPlayer
	{
		None = -1,
		White = 0, Black = 1
	}
	public enum ChessPiece
	{
		EMPTY = -1,
		WPawn = 0, WKnight = 1, WBishop = 2, WRook = 3, WQueen = 4, WKing = 5,
		BPawn = 6, BKnight = 7, BBishop = 8, BRook = 9, BQueen = 10, BKing = 11, 
		OOB = 12
	}
	public enum ChessFile
	{
		EMPTY = -1,
		FileA = 0, FileB = 1, FileC = 2, FileD = 3, FileE = 4, FileF = 5, FileG = 6, FileH = 7
	}
	public enum ChessRank
	{
		EMPTY = -1,
		Rank1 = 0, Rank2 = 1, Rank3 = 2, Rank4 = 3, Rank5 = 4, Rank6 = 5, Rank7 = 6, Rank8 = 7
	}
	

	public enum ChessDirection
	{
		DirN = 8, DirE = 1, DirS = -8, DirW = -1, 
        DirNE = DirN+DirE, DirSE = DirS+DirE, DirSW = DirS+DirW, DirNW = DirN+DirW,
		DirNNE = DirN+DirNE, DirEEN = DirE+DirNE, DirEES = DirE+DirSE, DirSSE = DirS+DirSE, DirSSW = DirS+DirSW, DirWWS = DirW+DirSW, DirWWN = DirW+DirNW, DirNNW = DirN+DirNW
	}

    [Flags]
    public enum ChessBitboard : ulong
    {
        A8 = (ulong)1 << 56, B8 = (ulong)1 << 57, C8 = (ulong)1 << 58, D8 = (ulong)1 << 59, E8 = (ulong)1 << 60, F8 = (ulong)1 << 61, G8 = (ulong)1 << 62, H8 = (ulong)1 << 63,
        A7 = (ulong)1 << 48, B7 = (ulong)1 << 49, C7 = (ulong)1 << 50, D7 = (ulong)1 << 51, E7 = (ulong)1 << 52, F7 = (ulong)1 << 53, G7 = (ulong)1 << 54, H7 = (ulong)1 << 55,
        A6 = (ulong)1 << 40, B6 = (ulong)1 << 41, C6 = (ulong)1 << 42, D6 = (ulong)1 << 43, E6 = (ulong)1 << 44, F6 = (ulong)1 << 45, G6 = (ulong)1 << 46, H6 = (ulong)1 << 47,
        A5 = (ulong)1 << 32, B5 = (ulong)1 << 33, C5 = (ulong)1 << 34, D5 = (ulong)1 << 35, E5 = (ulong)1 << 36, F5 = (ulong)1 << 37, G5 = (ulong)1 << 38, H5 = (ulong)1 << 39,
        A4 = (ulong)1 << 24, B4 = (ulong)1 << 25, C4 = (ulong)1 << 26, D4 = (ulong)1 << 27, E4 = (ulong)1 << 28, F4 = (ulong)1 << 29, G4 = (ulong)1 << 30, H4 = (ulong)1 << 31,
        A3 = (ulong)1 << 16, B3 = (ulong)1 << 17, C3 = (ulong)1 << 18, D3 = (ulong)1 << 19, E3 = (ulong)1 << 20, F3 = (ulong)1 << 21, G3 = (ulong)1 << 22, H3 = (ulong)1 << 23,
        A2 = (ulong)1 << 8, B2 = (ulong)1 << 9, C2 = (ulong)1 << 10, D2 = (ulong)1 << 11, E2 = (ulong)1 << 12, F2 = (ulong)1 << 13, G2 = (ulong)1 << 14, H2 = (ulong)1 << 15,
        A1 = (ulong)1 << 0, B1 = (ulong)1 << 1, C1 = (ulong)1 << 2, D1 = (ulong)1 << 3, E1 = (ulong)1 << 4, F1 = (ulong)1 << 5, G1 = (ulong)1 << 6, H1 = (ulong)1 << 7,
        Rank1 = A1 | B1 | C1 | D1 | E1 | F1 | G1 | H1,
        Rank2 = A2 | B2 | C2 | D2 | E2 | F2 | G2 | H2,
        Rank3 = A3 | B3 | C3 | D3 | E3 | F3 | G3 | H3,
        Rank4 = A4 | B4 | C4 | D4 | E4 | F4 | G4 | H4,
        Rank5 = A5 | B5 | C5 | D5 | E5 | F5 | G5 | H5,
        Rank6 = A6 | B6 | C6 | D6 | E6 | F6 | G6 | H6,
        Rank7 = A7 | B7 | C7 | D7 | E7 | F7 | G7 | H7,
        Rank8 = A8 | B8 | C8 | D8 | E8 | F8 | G8 | H8,
        FileA = A1 | A2 | A3 | A4 | A5 | A6 | A7 | A8,
        FileB = B1 | B2 | B3 | B4 | B5 | B6 | B7 | B8,
        FileC = C1 | C2 | C3 | C4 | C5 | C6 | C7 | C8,
        FileD = D1 | D2 | D3 | D4 | D5 | D6 | D7 | D8,
        FileE = E1 | E2 | E3 | E4 | E5 | E6 | E7 | E8,
        FileF = F1 | F2 | F3 | F4 | F5 | F6 | F7 | F8,
        FileG = G1 | G2 | G3 | G4 | G5 | G6 | G7 | G8,
        FileH = H1 | H2 | H3 | H4 | H5 | H6 | H7 | H8,
        Empty = 0,
        Full = Rank1 | Rank2 | Rank3 | Rank4 | Rank5 | Rank6 | Rank7 | Rank8
    }

	public enum ChessResult
	{
		Draw = 0, WhiteWins = 1, BlackWins = -1
	}

	public enum ChessResultReason
	{
		NotDecided = 0, 
		Checkmate = 1, Resign = 2, OutOfTime = 3, Adjudication = 4, //win reasons
		Stalemate = 5, FiftyMoveRule = 6, InsufficientMaterial = 7, MutualAgreement = 8, Repetition = 9, //draw reasons
		Unknown = 10
	}
	#endregion

	public class ChessException : Exception
	{
		public ChessException(string message)
			: base(message)
		{
		}
	}

	public static class Chess
	{

		public static readonly string FENStart = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

		//public arrays
		public static readonly ChessRank[] AllRanks = new ChessRank[] { ChessRank.Rank1, ChessRank.Rank2, ChessRank.Rank3, ChessRank.Rank4, ChessRank.Rank5, ChessRank.Rank6, ChessRank.Rank7, ChessRank.Rank8 };
		public static readonly ChessFile[] AllFiles = new ChessFile[] { ChessFile.FileA, ChessFile.FileB, ChessFile.FileC, ChessFile.FileD, ChessFile.FileE, ChessFile.FileF, ChessFile.FileG, ChessFile.FileH };
		public static readonly ChessPlayer[] AllPlayers = new ChessPlayer[] { ChessPlayer.White, ChessPlayer.Black };

		public static readonly ChessPiece[] AllPieces = new ChessPiece[]{
			ChessPiece.WPawn, ChessPiece.WKnight, ChessPiece.WBishop, ChessPiece.WRook, ChessPiece.WQueen, ChessPiece.WKing,
			ChessPiece.BPawn, ChessPiece.BKnight, ChessPiece.BBishop, ChessPiece.BRook, ChessPiece.BQueen, ChessPiece.BKing};


		public static readonly ChessPosition[] AllPositions = new ChessPosition[]{
		    ChessPosition.A1,ChessPosition.B1,ChessPosition.C1,ChessPosition.D1,ChessPosition.E1,ChessPosition.F1,ChessPosition.G1,ChessPosition.H1,
            ChessPosition.A2,ChessPosition.B2,ChessPosition.C2,ChessPosition.D2,ChessPosition.E2,ChessPosition.F2,ChessPosition.G2,ChessPosition.H2,
		    ChessPosition.A3,ChessPosition.B3,ChessPosition.C3,ChessPosition.D3,ChessPosition.E3,ChessPosition.F3,ChessPosition.G3,ChessPosition.H3,
		    ChessPosition.A4,ChessPosition.B4,ChessPosition.C4,ChessPosition.D4,ChessPosition.E4,ChessPosition.F4,ChessPosition.G4,ChessPosition.H4,
		    ChessPosition.A5,ChessPosition.B5,ChessPosition.C5,ChessPosition.D5,ChessPosition.E5,ChessPosition.F5,ChessPosition.G5,ChessPosition.H5,
		    ChessPosition.A6,ChessPosition.B6,ChessPosition.C6,ChessPosition.D6,ChessPosition.E6,ChessPosition.F6,ChessPosition.G6,ChessPosition.H6,
		    ChessPosition.A7,ChessPosition.B7,ChessPosition.C7,ChessPosition.D7,ChessPosition.E7,ChessPosition.F7,ChessPosition.G7,ChessPosition.H7,
		    ChessPosition.A8,ChessPosition.B8,ChessPosition.C8,ChessPosition.D8,ChessPosition.E8,ChessPosition.F8,ChessPosition.G8,ChessPosition.H8,
		};


		public static readonly ChessDirection[] AllDirections = new ChessDirection[]{
			ChessDirection.DirN, ChessDirection.DirE, ChessDirection.DirS, ChessDirection.DirW, ChessDirection.DirNE, ChessDirection.DirSE, ChessDirection.DirSW, ChessDirection.DirNW,
			ChessDirection.DirNNE, ChessDirection.DirEEN, ChessDirection.DirEES, ChessDirection.DirSSE, ChessDirection.DirSSW, ChessDirection.DirWWS, ChessDirection.DirWWN, ChessDirection.DirNNW};

		public static readonly ChessDirection[] AllDirectionsKnight = new ChessDirection[]{
			ChessDirection.DirNNE, ChessDirection.DirEEN, ChessDirection.DirEES, ChessDirection.DirSSE, ChessDirection.DirSSW, ChessDirection.DirWWS, ChessDirection.DirWWN, ChessDirection.DirNNW};

		public static readonly ChessDirection[] AllDirectionsRook = new ChessDirection[]{
			ChessDirection.DirN, ChessDirection.DirE, ChessDirection.DirS, ChessDirection.DirW};

		public static readonly ChessDirection[] AllDirectionsBishop = new ChessDirection[]{
			ChessDirection.DirNE, ChessDirection.DirSE, ChessDirection.DirSW, ChessDirection.DirNW};

		public static readonly ChessDirection[] AllDirectionsQueen = new ChessDirection[]{
			ChessDirection.DirN, ChessDirection.DirE, ChessDirection.DirS, ChessDirection.DirW, ChessDirection.DirNE, ChessDirection.DirSE, ChessDirection.DirSW, ChessDirection.DirNW};


		//private lookup arrays
		private static readonly string _filedesclookup = "abcdefgh";
		private static readonly string _rankdesclookup = "12345678";
		private static readonly string _piecedesclookup = "PNBRQKpnbrqk";
		private static readonly int[] _directionrankinc = new int[] { 1, 0, -1, 0,/*diag*/1, -1, -1, 1,/*knight*/2, 1, -1, -2, -2, -1, 1, 2 };
		private static readonly int[] _directionfileinc = new int[] { 0, 1, 0, -1,/*diag*/1, 1, -1, -1,/*knight*/1, 2, 2, 1, -1, -2, -2, -1 };


		public static bool IsInBounds(this ChessPosition pos)
		{
            return (int)pos >= 0 && (int)pos <= 63;
			
			////AssertPosition(pos);
			//return (pos != ChessPosition.OUTOFBOUNDS);
		}
		public static int MateIn(int ply)
		{
			return 30000 - ply; //private static readonly int VALCHECKMATE = 30000;
		}
		public static ChessPosition ToPosition(this ChessFile file, ChessRank rank)
		{
			//if (!IsValidFile(file)) { return ChessPosition.OUTOFBOUNDS; }
			//if (!IsValidRank(rank)) { return ChessPosition.OUTOFBOUNDS; }
			return (ChessPosition)((int)rank * 8) + (int)file;
		}
        public static ChessPosition ToPosition(this ChessRank rank, ChessFile file)
        {
            //if (!IsValidFile(file)) { return ChessPosition.OUTOFBOUNDS; }
            //if (!IsValidRank(rank)) { return ChessPosition.OUTOFBOUNDS; }
            return (ChessPosition)((int)rank * 8) + (int)file;
        }
        public static ChessBitboard Bitboard(this ChessRank rank)
        {
            switch (rank)
            {
                case ChessRank.Rank1:
                    return ChessBitboard.Rank1;
                case ChessRank.Rank2:
                    return ChessBitboard.Rank2;
                case ChessRank.Rank3:
                    return ChessBitboard.Rank3;
                case ChessRank.Rank4:
                    return ChessBitboard.Rank4;
                case ChessRank.Rank5:
                    return ChessBitboard.Rank5;
                case ChessRank.Rank6:
                    return ChessBitboard.Rank6;
                case ChessRank.Rank7:
                    return ChessBitboard.Rank7;
                case ChessRank.Rank8:
                    return ChessBitboard.Rank8;
                default:
                    return 0;
            }
        }
        public static ChessBitboard Reverse(this ChessBitboard bits)
        {
            return (ChessBitboard)(((ulong)(bits & ChessBitboard.Rank8) >> 56)
                | ((ulong)(bits & ChessBitboard.Rank7) >> 40)
                | ((ulong)(bits & ChessBitboard.Rank6) >> 24)
                | ((ulong)(bits & ChessBitboard.Rank5) >> 8)
                | ((ulong)(bits & ChessBitboard.Rank4) << 8)
                | ((ulong)(bits & ChessBitboard.Rank3) << 24)
                | ((ulong)(bits & ChessBitboard.Rank2) << 40)
                | ((ulong)(bits & ChessBitboard.Rank1) << 56));
        }
        public static ChessBitboard BitboardAllNorth(this ChessRank rank)
        {
            switch (rank)
            {
                case ChessRank.Rank1:
                    return ChessBitboard.Full;
                case ChessRank.Rank2:
                    return ChessBitboard.Rank2 | ChessBitboard.Rank3 | ChessBitboard.Rank4 | ChessBitboard.Rank5 | ChessBitboard.Rank6 | ChessBitboard.Rank7 | ChessBitboard.Rank8;
                case ChessRank.Rank3:
                    return ChessBitboard.Rank3 | ChessBitboard.Rank4 | ChessBitboard.Rank5 | ChessBitboard.Rank6 | ChessBitboard.Rank7 | ChessBitboard.Rank8;
                case ChessRank.Rank4:
                    return  ChessBitboard.Rank4 | ChessBitboard.Rank5 | ChessBitboard.Rank6 | ChessBitboard.Rank7 | ChessBitboard.Rank8;
                case ChessRank.Rank5:
                    return  ChessBitboard.Rank5 | ChessBitboard.Rank6 | ChessBitboard.Rank7 | ChessBitboard.Rank8;
                case ChessRank.Rank6:
                    return  ChessBitboard.Rank6 | ChessBitboard.Rank7 | ChessBitboard.Rank8;
                case ChessRank.Rank7:
                    return ChessBitboard.Rank7 | ChessBitboard.Rank8;
                case ChessRank.Rank8:
                    return ChessBitboard.Rank8;
                default:
                    return 0;
            }
        }
        public static ChessBitboard Bitboard(this ChessFile file)
        {
            switch (file)
            {
                case ChessFile.FileA:
                    return ChessBitboard.FileA;
                case ChessFile.FileB:
                    return ChessBitboard.FileB;
                case ChessFile.FileC:
                    return ChessBitboard.FileC;
                case ChessFile.FileD:
                    return ChessBitboard.FileD;
                case ChessFile.FileE:
                    return ChessBitboard.FileE;
                case ChessFile.FileF:
                    return ChessBitboard.FileF;
                case ChessFile.FileG:
                    return ChessBitboard.FileG;
                case ChessFile.FileH:
                    return ChessBitboard.FileH;
                default:
                    return 0;
            }
        }
		public static ChessRank GetRank(this ChessPosition pos)
		{
			//AssertPosition(pos);
			return (ChessRank)(((int)pos / 8));
		}
		public static ChessFile GetFile(this ChessPosition pos)
		{
			//AssertPosition(pos);
			return (ChessFile)((int)pos % 8);
		}

		public static ChessPiece ParseAsPiece(this char c)
		{
			int idx = _piecedesclookup.IndexOf(c);
			if (idx < 0) { throw new ChessException(c.ToString() + " is not a valid piece"); }
			return (ChessPiece)idx;
		}
		public static ChessPiece ParseAsPiece(this char c, ChessPlayer player)
		{
			ChessPiece tmppiece = c.ToString().ToUpper()[0].ParseAsPiece();
			if (player == ChessPlayer.White)
			{
				return tmppiece;
			}
			else
			{
				return (ChessPiece)((int)tmppiece + 6);
			}
		}
		public static ChessRank ParseAsRank(this char c)
		{
			int idx = _rankdesclookup.IndexOf(c);
			if (idx < 0) { throw new ChessException(c.ToString() + " is not a valid rank"); }
			return (ChessRank)idx;
		}
		public static ChessFile ParseAsFile(this char c)
		{
			int idx = _filedesclookup.IndexOf(c.ToString().ToLower());
			if (idx < 0) { throw new ChessException(c.ToString() + " is not a valid file"); }
			return (ChessFile)idx;
		}
		public static ChessPosition ParseAsPosition(this string s)
		{
			if (s.Length != 2) { throw new ChessException(s + " is not a valid position"); }
            ChessFile file = s.ToCharArray()[0].ParseAsFile();
            ChessRank rank = s.ToCharArray()[1].ParseAsRank();
			return file.ToPosition(rank);
		}
		public static string RankToString(ChessRank rank)
		{
			//AssertRank(rank);
			return _rankdesclookup.Substring((int)rank, 1);
		}
		public static string FileToString(ChessFile file)
		{
			//AssertFile(file);
			return _filedesclookup.Substring((int)file, 1);
		}
		public static string PositionToString(this ChessPosition pos)
		{
			return FileToString(pos.GetFile()) + RankToString(pos.GetRank());
		}
		public static string PieceToString(this ChessPiece piece)
		{
			//AssertPiece(piece);
			return _piecedesclookup.Substring((int)piece, 1);
		}

        private static int[] debrujinPositions =
	    {
	        0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8,
	        31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9
	    };

        private static int debrujinLSB(int number)
        {
            uint res = unchecked((uint)(number & -number) * 0x077CB531U) >> 27;
            return debrujinPositions[res];
        }
        public static ChessPosition FirstPosition(this ChessBitboard bitboard)
        {
            int lsb;
            if (((ulong)bitboard & 0xFFFFFFFF) != 0)
            {
                ulong x = (ulong)bitboard & 0xFFFFFFFF;
                lsb = debrujinLSB((int)x);
            }
            else
            {
                ulong x = (ulong)bitboard >> 32;
                lsb = (debrujinLSB((int)x) + 32);
            }
            return AllPositions[lsb];
        }

        public static ChessBitboard Shift(this ChessBitboard bits, ChessDirection dir)
        {
            switch (dir)
            {
                case ChessDirection.DirN:
                    return bits.ShiftDirN();
                case ChessDirection.DirE:
                    return bits.ShiftDirE();
                case ChessDirection.DirS:
                    return bits.ShiftDirS();
                case ChessDirection.DirW:
                    return bits.ShiftDirW();
                case ChessDirection.DirNE:
                    return bits.ShiftDirNE();
                case ChessDirection.DirSE:
                    return bits.ShiftDirSE();
                case ChessDirection.DirSW:
                    return bits.ShiftDirSW();
                case ChessDirection.DirNW:
                    return bits.ShiftDirNW();
                case ChessDirection.DirNNE:
                    return bits.ShiftDirNE().ShiftDirN();
                case ChessDirection.DirEEN:
                    return bits.ShiftDirNE().ShiftDirE();
                case ChessDirection.DirEES:
                    return bits.ShiftDirSE().ShiftDirE();
                case ChessDirection.DirSSE:
                    return bits.ShiftDirSE().ShiftDirS();
                case ChessDirection.DirSSW:
                    return bits.ShiftDirSW().ShiftDirS();
                case ChessDirection.DirWWS:
                    return bits.ShiftDirSW().ShiftDirW();
                case ChessDirection.DirWWN:
                    return bits.ShiftDirNW().ShiftDirW();
                case ChessDirection.DirNNW:
                    return bits.ShiftDirNW().ShiftDirN();
                default:
                    return ChessBitboard.Empty;
            }

        }
        public static ChessBitboard ShiftDirN(this ChessBitboard bits)
        {
            return (ChessBitboard)((ulong)(bits & ~ChessBitboard.Rank8) << 8);
        }
        public static ChessBitboard ShiftDirE(this ChessBitboard bits)
        {
            return (ChessBitboard)((ulong)(bits & ~ChessBitboard.FileH) << 1);
        }
        public static ChessBitboard ShiftDirS(this ChessBitboard bits)
        {
            return (ChessBitboard)((ulong)(bits & ~ChessBitboard.Rank1) >> 8);
        }
        public static ChessBitboard ShiftDirW(this ChessBitboard bits)
        {
            return (ChessBitboard)((ulong)(bits & ~ChessBitboard.FileA) >> 1);
        }
        public static ChessBitboard ShiftDirNE(this ChessBitboard bits)
        {
            return (ChessBitboard)((ulong)(bits & ~ChessBitboard.Rank8 & ~ChessBitboard.FileH ) << 9);
        }
        public static ChessBitboard ShiftDirSE(this ChessBitboard bits)
        {
            return (ChessBitboard)((ulong)(bits & ~ChessBitboard.Rank1 & ~ChessBitboard.FileH) >> 7);
        }
        public static ChessBitboard ShiftDirSW(this ChessBitboard bits)
        {
            return (ChessBitboard)((ulong)(bits & ~ChessBitboard.Rank1 & ~ChessBitboard.FileA) >> 9);
        }
        public static ChessBitboard ShiftDirNW(this ChessBitboard bits)
        {
            return (ChessBitboard)((ulong)(bits & ~ChessBitboard.Rank8 & ~ChessBitboard.FileA) << 7);
        }
        


        public static bool Empty(this ChessBitboard bitboard)
        {
            return bitboard == 0;
        }

        public static IEnumerable<ChessPosition> ToPositions(this ChessBitboard bitboard)
        {
            while (bitboard != 0)
            {
                ChessPosition first = bitboard.FirstPosition();
                yield return first;
                bitboard = bitboard & ~first.Bitboard();
            }
        }
        public static ChessBitboard Bitboard(this ChessPosition position)
        {
            if (position.IsInBounds())
            {
                return (ChessBitboard)((ulong)1 << position.GetIndex64());
            }
            else
            {
                return (ChessBitboard.Empty);
            }
        }

        public static int GetIndex64(this ChessPosition position)
        {
            return (int)position;
        }

        public static ChessBitboard ToBitboard(this IEnumerable<ChessPosition> positions)
        {
            ChessBitboard bitboard = 0;
            foreach (var position in positions)
            {
                bitboard |= position.Bitboard();
            }
            return bitboard;
        }

		public static int PieceValBasic(this ChessPiece piece)
		{
			switch (piece)
			{
				case ChessPiece.WPawn:
				case ChessPiece.BPawn:
					return 100;
				case ChessPiece.WKnight:
				case ChessPiece.BKnight:
					return 300;
				case ChessPiece.WBishop:
				case ChessPiece.BBishop:
					return 300;
				case ChessPiece.WRook:
				case ChessPiece.BRook:
					return 500;
				case ChessPiece.WQueen:
				case ChessPiece.BQueen:
					return 900;
				case ChessPiece.WKing:
				case ChessPiece.BKing:
					return 10000;
				default:
					return 0;
			}
		}
		public static ChessPiece ToOppositePlayer(this ChessPiece piece)
		{
			switch (piece)
			{
				case ChessPiece.WPawn:
					return ChessPiece.BPawn;
				case ChessPiece.WKnight:
					return ChessPiece.BKnight;
				case ChessPiece.WBishop:
					return ChessPiece.BBishop;
				case ChessPiece.WRook:
					return ChessPiece.BRook;
				case ChessPiece.WQueen:
					return ChessPiece.BQueen;
				case ChessPiece.WKing:
					return ChessPiece.BKing;

				case ChessPiece.BPawn:
					return ChessPiece.WPawn;
				case ChessPiece.BKnight:
					return ChessPiece.WKnight;
				case ChessPiece.BBishop:
					return ChessPiece.WBishop;
				case ChessPiece.BRook:
					return ChessPiece.WRook;
				case ChessPiece.BQueen:
					return ChessPiece.WQueen;
				case ChessPiece.BKing:
					return ChessPiece.WKing;
				default:
					return ChessPiece.EMPTY;
			}
		}
		public static ChessPlayer PieceToPlayer(ChessPiece piece)
		{
			if (piece == ChessPiece.EMPTY) { return ChessPlayer.None; }
			//AssertPiece(piece);
			if ((int)piece >= 0 && (int)piece <= 5)
			{
				return ChessPlayer.White;
			}
			else
			{
				return ChessPlayer.Black;
			}
		}
		public static bool PieceIsSliderRook(ChessPiece piece)
		{
			switch (piece)
			{
				case ChessPiece.WRook:
				case ChessPiece.WQueen:
				case ChessPiece.BRook:
				case ChessPiece.BQueen:
					return true;
				default:
					return false;
			}
		}
		public static bool PieceIsSliderBishop(ChessPiece piece)
		{
			switch (piece)
			{
				case ChessPiece.WBishop:
				case ChessPiece.WQueen:
				case ChessPiece.BBishop:
				case ChessPiece.BQueen:
					return true;
				default:
					return false;
			}
		}
        public static ChessDirection DirectionFromTo(ChessPosition from, ChessPosition to)
        {
            
            ChessRank rankfrom = from.GetRank();
            ChessFile filefrom = from.GetFile();
            ChessRank rankto = to.GetRank();
            ChessFile fileto = to.GetFile();

            if (fileto == filefrom)
            {
                if (rankfrom > rankto) { return ChessDirection.DirS; }
                return ChessDirection.DirN;
            }
            else if (rankfrom == rankto)
            {
                if (filefrom > fileto) { return ChessDirection.DirW; }
                return ChessDirection.DirE;
            }
            int rankchange = rankto - rankfrom;
            int filechange = fileto - filefrom;
            int rankchangeabs = rankchange > 0 ? rankchange : -rankchange;
            int filechangeabs = filechange > 0 ? filechange : -filechange;
            if ((rankchangeabs == 1 && filechangeabs == 2) || (rankchangeabs == 2 && filechangeabs == 1))
            {
                //knight direction
                return (ChessDirection)((int)rankchange * 8) + (int)filechange;
            }
            else if (rankchangeabs != filechangeabs)
            {
                return 0;
            }
            if (rankchange > 0)
            {
                if (filechange > 0) { return ChessDirection.DirNE; }
                return ChessDirection.DirNW;
            }
            else
            {
                if (filechange > 0) { return ChessDirection.DirSE; }
                return ChessDirection.DirSW;
            }

        }

        public static bool IsInBounds(this ChessFile file)
        {
            return (int)file >= 0 && (int)file <= 7;
        }
        public static bool IsInBounds(this ChessRank rank)
        {
            return (int)rank >= 0 && (int)rank <= 7;
        }
        public static ChessPosition PositionInDirectionUnsafe(this ChessPosition pos, ChessDirection dir)
        {
            return (ChessPosition)((int)pos + (int)dir);
        }
		public static ChessPosition PositionInDirection(this ChessPosition pos, ChessDirection dir)
		{
            ChessFile file = pos.GetFile();
            ChessRank rank = pos.GetRank();
            switch (dir)
            {
                case ChessDirection.DirN:
                    rank += 1;
                    break;
                case ChessDirection.DirE:
                    file += 1;
                    break;
                case ChessDirection.DirS:
                    rank -= 1;
                    break;
                case ChessDirection.DirW:
                    file -= 1;
                    break;
                case ChessDirection.DirNE:
                    rank += 1; file+=1;
                    break;
                case ChessDirection.DirSE:
                    rank -= 1; file += 1;
                    break;
                case ChessDirection.DirSW:
                    rank -= 1; file -= 1;
                    break;
                case ChessDirection.DirNW:
                    rank += 1; file -= 1;
                    break;

                case ChessDirection.DirNNE:
                    rank += 2; file += 1;
                    break;
                case ChessDirection.DirEEN:
                    rank += 1; file += 2;
                    break;
                case ChessDirection.DirEES:
                    rank -= 1; file += 2;
                    break;
                case ChessDirection.DirSSE:
                    rank -= 2; file += 1;
                    break;

                case ChessDirection.DirSSW:
                    rank -= 2; file -= 1;
                    break;
                case ChessDirection.DirWWS:
                    rank -= 1; file -= 2;
                    break;
                case ChessDirection.DirWWN:
                    rank += 1; file -= 2;
                    break;
                case ChessDirection.DirNNW:
                    rank += 2; file -= 1;
                    break;
                default:
                    return (ChessPosition.OUTOFBOUNDS);
            }
            if (rank.IsInBounds() && file.IsInBounds())
            {
                return rank.ToPosition(file);
            }
            else
            {
                return (ChessPosition.OUTOFBOUNDS);
            }

		}
		public static ChessPosition PositionReverse(ChessPosition pos)
		{
			ChessRank r = pos.GetRank();
			ChessFile f = pos.GetFile();
			ChessRank newrank = ChessRank.EMPTY;
			switch(r)
			{
				case ChessRank.Rank1:
					newrank = ChessRank.Rank8;
					break;
				case ChessRank.Rank2:
					newrank = ChessRank.Rank7;
					break;
				case ChessRank.Rank3:
					newrank = ChessRank.Rank6;
					break;
				case ChessRank.Rank4:
					newrank = ChessRank.Rank5;
					break;
				case ChessRank.Rank5:
					newrank = ChessRank.Rank4;
					break;
				case ChessRank.Rank6:
					newrank = ChessRank.Rank3;
					break;
				case ChessRank.Rank7:
					newrank = ChessRank.Rank2;
					break;
				case ChessRank.Rank8:
					newrank = ChessRank.Rank1;
					break;
			}
			return f.ToPosition(newrank);
		}
		public static ChessPlayer PlayerOther(ChessPlayer player)
		{
			//AssertPlayer(player);
			if (player == ChessPlayer.White) { return ChessPlayer.Black; }
			else { return ChessPlayer.White; }
		}
		public static bool IsDirectionRook(ChessDirection dir)
		{
			//AssertDirection(dir);
			switch (dir)
			{
				case ChessDirection.DirN:
				case ChessDirection.DirE:
				case ChessDirection.DirS:
				case ChessDirection.DirW:
					return true;
				default:
					return false;
			}
		}
		public static bool IsDirectionBishop(ChessDirection dir)
		{
			//AssertDirection(dir);
			switch (dir)
			{
				case ChessDirection.DirNW:
				case ChessDirection.DirNE:
				case ChessDirection.DirSW:
				case ChessDirection.DirSE:
					return true;
				default:
					return false;
			}
		}
		public static bool IsDirectionKnight(ChessDirection dir)
		{
			//AssertDirection(dir);
			switch (dir)
			{
				case ChessDirection.DirNNE:
				case ChessDirection.DirEEN:
				case ChessDirection.DirEES:
				case ChessDirection.DirSSE:
				case ChessDirection.DirSSW:
				case ChessDirection.DirWWS:
				case ChessDirection.DirWWN:
				case ChessDirection.DirNNW:
					return true;
				default:
					return false;
			}
		}

        public static ChessDirection Opposite(this ChessDirection dir)
        {
            return (ChessDirection)(-(int)dir);
        }


		/// <summary>
		/// To convert a Byte Array of Unicode values (UTF-8 encoded) to a complete String.
		/// </summary>
		/// <param name="characters">Unicode Byte Array to be converted to String</param>
		/// <returns>String converted from Unicode Byte Array</returns>
		//private static string UTF8ByteArrayToString(byte[] characters)
		//{
		//    UTF8Encoding encoding = new UTF8Encoding();
			
		//    string constructedString = encoding.GetString(characters);
		//    return (constructedString);
		//}

		/// <summary>
		/// Converts the String to UTF8 Byte array and is used in De serialization
		/// </summary>
		/// <param name="pXmlString"></param>
		/// <returns></returns>
		private static Byte[] StringToUTF8ByteArray(string pXmlString)
		{
			UTF8Encoding encoding = new UTF8Encoding();
			byte[] byteArray = encoding.GetBytes(pXmlString);
			return byteArray;
		}

		/// <summary>
		/// Serialize an object into an XML string
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static string SerializeObject<T>(T obj)
		{
			try
			{
				XmlSerializer xs = new XmlSerializer(typeof(T));
				MemoryStream memoryStream = new MemoryStream();
				
				StreamWriter writer = new StreamWriter(memoryStream,Encoding.UTF8);

				xs.Serialize(writer, obj);
				memoryStream.Position = 0;
				StreamReader reader = new StreamReader(memoryStream,Encoding.UTF8);
				string retval = reader.ReadToEnd();
				return retval;
				//return reader.ReadToEnd();
				//xmlString = UTF8ByteArrayToString(memoryStream.ToArray()); 
				//return xmlString;
			}
			catch
			{
				return string.Empty;
			}
		}

		/// <summary>
		/// Reconstruct an object from an XML string
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		public static T DeserializeObject<T>(string xml)
		{
			XmlSerializer xs = new XmlSerializer(typeof(T));
			MemoryStream memoryStream = new MemoryStream();
			StreamWriter writer = new StreamWriter(memoryStream, Encoding.UTF8);
			writer.Write(xml);
			writer.Flush();
			
			
			
			
			memoryStream.Position = 0;
			
			
			StreamReader reader = new StreamReader(memoryStream, Encoding.UTF8);
			return (T)xs.Deserialize(reader);
		}

		private static Random rand = new Random();

		public static Int64 Rand64()
		{
			byte[] bytes = new byte[8];
			rand.NextBytes(bytes);
			Int64 retval = 0;
			for (int i = 0; i <= 7; i++)
			{
				//Int64 ibyte = (Int64)bytes[i]&256;
				Int64 ibyte = (Int64)bytes[i];
				retval |= ibyte << (i * 8);
			}
			return retval;
		}

	}
	

}