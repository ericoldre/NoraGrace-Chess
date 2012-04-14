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








	




	public enum ChessResult
	{
		Draw = 0, WhiteWins = 1, BlackWins = -1
	}

	public enum ChessResultReason
	{
		NotDecided = 0, 
		Checkmate = 1, Resign = 2, OutOfTime = 3, Adjudication = 4, //win reasons
		Stalemate = 5, FiftyMoveRule = 6, InsufficientMaterial = 7, MutualAgreement = 8, Repetition = 9, //draw reasons
		Unknown = 10, IllegalMove = 11
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

		

		//public arrays
		public static readonly ChessRank[] AllRanks = new ChessRank[] { ChessRank.Rank8, ChessRank.Rank7, ChessRank.Rank6, ChessRank.Rank5, ChessRank.Rank4, ChessRank.Rank3, ChessRank.Rank2, ChessRank.Rank1 };
		public static readonly ChessFile[] AllFiles = new ChessFile[] { ChessFile.FileA, ChessFile.FileB, ChessFile.FileC, ChessFile.FileD, ChessFile.FileE, ChessFile.FileF, ChessFile.FileG, ChessFile.FileH };
		public static readonly ChessPlayer[] AllPlayers = new ChessPlayer[] { ChessPlayer.White, ChessPlayer.Black };

		public static readonly ChessPiece[] AllPieces = new ChessPiece[]{
			ChessPiece.WPawn, ChessPiece.WKnight, ChessPiece.WBishop, ChessPiece.WRook, ChessPiece.WQueen, ChessPiece.WKing,
			ChessPiece.BPawn, ChessPiece.BKnight, ChessPiece.BBishop, ChessPiece.BRook, ChessPiece.BQueen, ChessPiece.BKing};


		public static readonly ChessPosition[] AllPositions = new ChessPosition[]{
		    ChessPosition.A8,ChessPosition.B8,ChessPosition.C8,ChessPosition.D8,ChessPosition.E8,ChessPosition.F8,ChessPosition.G8,ChessPosition.H8,
            ChessPosition.A7,ChessPosition.B7,ChessPosition.C7,ChessPosition.D7,ChessPosition.E7,ChessPosition.F7,ChessPosition.G7,ChessPosition.H7,
		    ChessPosition.A6,ChessPosition.B6,ChessPosition.C6,ChessPosition.D6,ChessPosition.E6,ChessPosition.F6,ChessPosition.G6,ChessPosition.H6,
		    ChessPosition.A5,ChessPosition.B5,ChessPosition.C5,ChessPosition.D5,ChessPosition.E5,ChessPosition.F5,ChessPosition.G5,ChessPosition.H5,
		    ChessPosition.A4,ChessPosition.B4,ChessPosition.C4,ChessPosition.D4,ChessPosition.E4,ChessPosition.F4,ChessPosition.G4,ChessPosition.H4,
            ChessPosition.A3,ChessPosition.B3,ChessPosition.C3,ChessPosition.D3,ChessPosition.E3,ChessPosition.F3,ChessPosition.G3,ChessPosition.H3,
		    ChessPosition.A2,ChessPosition.B2,ChessPosition.C2,ChessPosition.D2,ChessPosition.E2,ChessPosition.F2,ChessPosition.G2,ChessPosition.H2,
            ChessPosition.A1,ChessPosition.B1,ChessPosition.C1,ChessPosition.D1,ChessPosition.E1,ChessPosition.F1,ChessPosition.G1,ChessPosition.H1,
		};

        public static readonly ChessGameStage[] AllGameStages = new ChessGameStage[] { ChessGameStage.Opening, ChessGameStage.Endgame };

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
		
		
		
		//private static readonly int[] _directionrankinc = new int[] { 1, 0, -1, 0,/*diag*/1, -1, -1, 1,/*knight*/2, 1, -1, -2, -2, -1, 1, 2 };
		//private static readonly int[] _directionfileinc = new int[] { 0, 1, 0, -1,/*diag*/1, 1, -1, -1,/*knight*/1, 2, 2, 1, -1, -2, -2, -1 };


		
		public static int MateIn(int ply)
		{
			return 30000 - ply; //private static readonly int VALCHECKMATE = 30000;
		}


		



		public static ChessPosition ParseAsPosition(this string s)
		{
			if (s.Length != 2) { throw new ChessException(s + " is not a valid position"); }
            ChessFile file = s.ToCharArray()[0].ParseAsFile();
            ChessRank rank = s.ToCharArray()[1].ParseAsRank();
			return file.ToPosition(rank);
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