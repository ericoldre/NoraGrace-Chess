using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

namespace Murderhole
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
		A8 = 91, B8 = 92, C8 = 93, D8 = 94, E8 = 95, F8 = 96, G8 = 97, H8 = 98,
		A7 = 81, B7 = 82, C7 = 83, D7 = 84, E7 = 85, F7 = 86, G7 = 87, H7 = 88,
		A6 = 71, B6 = 72, C6 = 73, D6 = 74, E6 = 75, F6 = 76, G6 = 77, H6 = 78,
		A5 = 61, B5 = 62, C5 = 63, D5 = 64, E5 = 65, F5 = 66, G5 = 67, H5 = 68,
		A4 = 51, B4 = 52, C4 = 53, D4 = 54, E4 = 55, F4 = 56, G4 = 57, H4 = 58,
		A3 = 41, B3 = 42, C3 = 43, D3 = 44, E3 = 45, F3 = 46, G3 = 47, H3 = 48,
		A2 = 31, B2 = 32, C2 = 33, D2 = 34, E2 = 35, F2 = 36, G2 = 37, H2 = 38,
		A1 = 21, B1 = 22, C1 = 23, D1 = 24, E1 = 25, F1 = 26, G1 = 27, H1 = 28
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
		FileA = 1, FileB = 2, FileC = 3, FileD = 4, FileE = 5, FileF = 6, FileG = 7, FileH = 8
	}
	public enum ChessRank
	{
		EMPTY = -1,
		Rank1 = 2, Rank2 = 3, Rank3 = 4, Rank4 = 5, Rank5 = 6, Rank6 = 7, Rank7 = 8, Rank8 = 9
	}
	

	public enum ChessDirection
	{
		DirN = 10, DirE = 1, DirS = -10, DirW = -1, DirNE = 11, DirSE = -9, DirSW = -11, DirNW = 9,
		DirNNE = 21, DirEEN = 12, DirEES = -8, DirSSE = -19, DirSSW = -21, DirWWS = -12, DirWWN = 8, DirNNW = 19
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

	public class Chess
	{

		public static readonly string FENStart = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

		//public arrays
		public static readonly ChessRank[] AllRanks = new ChessRank[] { ChessRank.Rank1, ChessRank.Rank2, ChessRank.Rank3, ChessRank.Rank4, ChessRank.Rank5, ChessRank.Rank6, ChessRank.Rank7, ChessRank.Rank8 };
		public static readonly ChessFile[] AllFiles = new ChessFile[] { ChessFile.FileA, ChessFile.FileB, ChessFile.FileC, ChessFile.FileD, ChessFile.FileE, ChessFile.FileF, ChessFile.FileG, ChessFile.FileH };
		public static readonly ChessPlayer[] AllPlayers = new ChessPlayer[] { ChessPlayer.White, ChessPlayer.Black };

		public static readonly ChessPiece[] AllPieces = new ChessPiece[]{
			ChessPiece.WPawn, ChessPiece.WKnight, ChessPiece.WBishop, ChessPiece.WRook, ChessPiece.WQueen, ChessPiece.WKing,
			ChessPiece.BPawn, ChessPiece.BKnight, ChessPiece.BBishop, ChessPiece.BRook, ChessPiece.BQueen, ChessPiece.BKing};

		//public static readonly ChessPosition[] AllPositions = new ChessPosition[]{
		//    ChessPosition.A1,ChessPosition.B1,ChessPosition.C1,ChessPosition.D1,ChessPosition.E1,ChessPosition.F1,ChessPosition.G1,ChessPosition.H1,
		//    ChessPosition.A2,ChessPosition.B2,ChessPosition.C2,ChessPosition.D2,ChessPosition.E2,ChessPosition.F2,ChessPosition.G2,ChessPosition.H2,
		//    ChessPosition.A3,ChessPosition.B3,ChessPosition.C3,ChessPosition.D3,ChessPosition.E3,ChessPosition.F3,ChessPosition.G3,ChessPosition.H3,
		//    ChessPosition.A4,ChessPosition.B4,ChessPosition.C4,ChessPosition.D4,ChessPosition.E4,ChessPosition.F4,ChessPosition.G4,ChessPosition.H4,
		//    ChessPosition.A5,ChessPosition.B5,ChessPosition.C5,ChessPosition.D5,ChessPosition.E5,ChessPosition.F5,ChessPosition.G5,ChessPosition.H5,
		//    ChessPosition.A6,ChessPosition.B6,ChessPosition.C6,ChessPosition.D6,ChessPosition.E6,ChessPosition.F6,ChessPosition.G6,ChessPosition.H6,
		//    ChessPosition.A7,ChessPosition.B7,ChessPosition.C7,ChessPosition.D7,ChessPosition.E7,ChessPosition.F7,ChessPosition.G7,ChessPosition.H7,
		//    ChessPosition.A8,ChessPosition.B8,ChessPosition.C8,ChessPosition.D8,ChessPosition.E8,ChessPosition.F8,ChessPosition.G8,ChessPosition.H8};

		public static readonly ChessPosition[] AllPositions = new ChessPosition[]{
		    ChessPosition.A8,ChessPosition.B8,ChessPosition.C8,ChessPosition.D8,ChessPosition.E8,ChessPosition.F8,ChessPosition.G8,ChessPosition.H8,
		    ChessPosition.A7,ChessPosition.B7,ChessPosition.C7,ChessPosition.D7,ChessPosition.E7,ChessPosition.F7,ChessPosition.G7,ChessPosition.H7,
		    ChessPosition.A6,ChessPosition.B6,ChessPosition.C6,ChessPosition.D6,ChessPosition.E6,ChessPosition.F6,ChessPosition.G6,ChessPosition.H6,
		    ChessPosition.A5,ChessPosition.B5,ChessPosition.C5,ChessPosition.D5,ChessPosition.E5,ChessPosition.F5,ChessPosition.G5,ChessPosition.H5,
		    ChessPosition.A4,ChessPosition.B4,ChessPosition.C4,ChessPosition.D4,ChessPosition.E4,ChessPosition.F4,ChessPosition.G4,ChessPosition.H4,
		    ChessPosition.A3,ChessPosition.B3,ChessPosition.C3,ChessPosition.D3,ChessPosition.E3,ChessPosition.F3,ChessPosition.G3,ChessPosition.H3,
		    ChessPosition.A2,ChessPosition.B2,ChessPosition.C2,ChessPosition.D2,ChessPosition.E2,ChessPosition.F2,ChessPosition.G2,ChessPosition.H2,
		    ChessPosition.A1,ChessPosition.B1,ChessPosition.C1,ChessPosition.D1,ChessPosition.E1,ChessPosition.F1,ChessPosition.G1,ChessPosition.H1
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

		private static readonly bool[] PositionInBounds = new bool[]{
			false, false, false, false, false, false, false, false, false, false,
			false, false, false, false, false, false, false, false, false, false,
			false, true,  true,  true,  true,  true,  true,  true,  true,  false, //rank 1
			false, true,  true,  true,  true,  true,  true,  true,  true,  false,
			false, true,  true,  true,  true,  true,  true,  true,  true,  false,
			false, true,  true,  true,  true,  true,  true,  true,  true,  false,
			false, true,  true,  true,  true,  true,  true,  true,  true,  false,
			false, true,  true,  true,  true,  true,  true,  true,  true,  false,
			false, true,  true,  true,  true,  true,  true,  true,  true,  false,
			false, true,  true,  true,  true,  true,  true,  true,  true,  false, //rank 8
			false, false, false, false, false, false, false, false, false, false,
			false, false, false, false, false, false, false, false, false, false};

		//private lookup arrays
		private static readonly string _filedesclookup = " abcdefgh";
		private static readonly string _rankdesclookup = "  12345678";
		private static readonly string _piecedesclookup = "PNBRQKpnbrqk";
		private static readonly int[] _directionrankinc = new int[] { 1, 0, -1, 0,/*diag*/1, -1, -1, 1,/*knight*/2, 1, -1, -2, -2, -1, 1, 2 };
		private static readonly int[] _directionfileinc = new int[] { 0, 1, 0, -1,/*diag*/1, 1, -1, -1,/*knight*/1, 2, 2, 1, -1, -2, -2, -1 };

		#region IsValidFunctions


		//public static bool IsValidPosition(ChessPosition pos)
		//{
		//    return ((int)pos >= -1 && (int)pos <= 63);
		//}
		//public static bool IsValidPlayer(ChessPlayer player)
		//{
		//    return ((int)player == -1 || (int)player == 0 || (int)player == 1);
		//}
		//public static bool IsValidPiece(ChessPiece piece)
		//{
		//    return ((int)piece >= -1 && (int)piece <= 11);
		//}
		//public static bool IsValidFile(ChessFile file)
		//{
		//    return ((int)file >= 0 && (int)file <= 7);
		//}
		//public static bool IsValidRank(ChessRank rank)
		//{
		//    return ((int)rank >= 0 && (int)rank <= 7);
		//}
		//public static bool IsValidDirection(ChessDirection dir)
		//{
		//    return ((int)dir >= 0 && (int)dir <= 15);
		//}
		#endregion

		#region AssertFunctions

		//public static void AssertPosition(ChessPosition pos)
		//{
		//    if (!IsValidPosition(pos)) { throw new Exception(pos.ToString() + " is not a valid position"); }
		//}
		//public static void AssertPlayer(ChessPlayer player)
		//{
		//    if (!IsValidPlayer(player)) { throw new Exception(player.ToString() + " is not a valid player"); }
		//}
		//public static void AssertPiece(ChessPiece piece)
		//{
		//    if (!IsValidPiece(piece)) { throw new Exception(piece.ToString() + " is not a valid piece"); }
		//}
		//public static void AssertFile(ChessFile file)
		//{
		//    if (!IsValidFile(file)) { throw new Exception(file.ToString() + " is not a valid file"); }
		//}
		//public static void AssertRank(ChessRank rank)
		//{
		//    if (!IsValidRank(rank)) { throw new Exception(rank.ToString() + " is not a valid rank"); }
		//}
		//public static void AssertDirection(ChessDirection dir)
		//{
		//    if (!IsValidDirection(dir)) { throw new Exception(dir.ToString() + " is not a valid direction"); }
		//}

		#endregion

		public static bool InBounds(ChessPosition pos)
		{
			return PositionInBounds[(int)pos];
			////AssertPosition(pos);
			//return (pos != ChessPosition.OUTOFBOUNDS);
		}
		public static int MateIn(int ply)
		{
			return 30000 - ply; //private static readonly int VALCHECKMATE = 30000;
		}
		public static ChessPosition FileRankToPos(ChessFile file, ChessRank rank)
		{
			//if (!IsValidFile(file)) { return ChessPosition.OUTOFBOUNDS; }
			//if (!IsValidRank(rank)) { return ChessPosition.OUTOFBOUNDS; }
			return (ChessPosition)((int)rank * 10) + (int)file;
		}
		public static ChessRank PositionToRank(ChessPosition pos)
		{
			//AssertPosition(pos);
			return (ChessRank)(((int)pos / 10));
		}
		public static ChessFile PositionToFile(ChessPosition pos)
		{
			//AssertPosition(pos);
			return (ChessFile)((int)pos % 10);
		}
		//public static bool PositionIsDarkColor(ChessPosition pos)
		//{
		//    //AssertPosition(pos);
		//    ChessRank rank = PositionToRank(pos);
		//    return ((int)pos % 2 + (int)rank % 2) % 2 == 0;
		//}

		public static ChessPiece CharToPiece(char c)
		{
			int idx = _piecedesclookup.IndexOf(c);
			if (idx < 0) { throw new ChessException(c.ToString() + " is not a valid piece"); }
			return (ChessPiece)idx;
		}
		public static ChessPiece CharToPiecePlayer(char c, ChessPlayer player)
		{
			ChessPiece tmppiece = CharToPiece(c.ToString().ToUpper()[0]);
			if (player == ChessPlayer.White)
			{
				return tmppiece;
			}
			else
			{
				return (ChessPiece)((int)tmppiece + 6);
			}
		}
		public static ChessRank CharToRank(char c)
		{
			int idx = _rankdesclookup.IndexOf(c);
			if (idx < 0) { throw new ChessException(c.ToString() + " is not a valid rank"); }
			return (ChessRank)idx;
		}
		public static ChessFile CharToFile(char c)
		{
			int idx = _filedesclookup.IndexOf(c.ToString().ToLower());
			if (idx < 0) { throw new ChessException(c.ToString() + " is not a valid file"); }
			return (ChessFile)idx;
		}
		public static ChessPosition StrToPosition(string s)
		{
			if (s.Length != 2) { throw new ChessException(s + " is not a valid position"); }
			ChessFile file = Chess.CharToFile(s.ToCharArray()[0]);
			ChessRank rank = Chess.CharToRank(s.ToCharArray()[1]);
			return FileRankToPos(file, rank);
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
		public static string PositionToString(ChessPosition pos)
		{
			//AssertPosition(pos);
			ChessFile file = PositionToFile(pos);
			ChessRank rank = PositionToRank(pos);
			return FileToString(file) + RankToString(rank);
		}
		public static string PieceToString(ChessPiece piece)
		{
			//AssertPiece(piece);
			return _piecedesclookup.Substring((int)piece, 1);
		}

		public static int PieceValBasic(ChessPiece piece)
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
		public static ChessPiece PieceReverse(ChessPiece piece)
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
			ChessDirection retval = (ChessDirection)((int)to - (int)from);
			if(Chess.IsDirectionKnight(retval))
			{
				return retval;
			}
			ChessRank rankfrom = Chess.PositionToRank(from);
			ChessFile filefrom = Chess.PositionToFile(from);
			ChessRank rankto = Chess.PositionToRank(to);
			ChessFile fileto = Chess.PositionToFile(to);

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
			if (rankchangeabs != filechangeabs)
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
		public static ChessPosition PositionInDirection(ChessPosition pos, ChessDirection dir)
		{
			return (ChessPosition)((int)pos + (int)dir);
		}
		public static ChessPosition PositionReverse(ChessPosition pos)
		{
			ChessRank r = PositionToRank(pos);
			ChessFile f = PositionToFile(pos);
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
			return Chess.FileRankToPos(f, newrank);
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