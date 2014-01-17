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

		


		
		public static int MateIn(int ply)
		{
			return 30000 - ply; //private static readonly int VALCHECKMATE = 30000;
		}


		



		public static ChessPosition ParseAsPosition(this string s)
		{
			if (s.Length != 2) { throw new ChessException(s + " is not a valid position"); }
            ChessFile file = ChessFileInfo.Parse(s[0]);
            ChessRank rank = ChessRankInfo.Parse(s[1]);
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