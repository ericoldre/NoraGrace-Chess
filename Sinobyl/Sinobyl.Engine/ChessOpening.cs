using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.ObjectModel;
using System.IO;

namespace Sinobyl.Engine
{
	public class ChessOpening
	{
		private class ECOEntry
		{
			public static int CountCreated;
			public readonly string Code;
			public readonly string Name;
			public ECOEntry(string a_code, string a_name)
			{
				Code = a_code;
				Name = a_name;
				CountCreated++;
			}
			
		}
		private class PositionEntry
		{
			public ECOEntry ECO{get;set;}
			public int OpeningCount;
		}

		private static List<ECOEntry> _names;
		private static Dictionary<Int64, PositionEntry> _positions;


		static ChessOpening()
		{
			_names = new List<ECOEntry>();
			_positions = new Dictionary<long, PositionEntry>();

			//find resource of openings if it exists
			string ResourceName = ResourceFileName;
			if (ResourceName.Length == 0) { return; }

			Assembly a = Assembly.GetExecutingAssembly();
			Stream ecostream = a.GetManifestResourceStream(ResourceName);

			int linecount = 0;
			using (StreamReader reader = new StreamReader(ecostream))
			{
				while (!reader.EndOfStream)
				{
					linecount++;
					string line = reader.ReadLine().Trim();
					string[] arr = line.Split('\t');
					ECOEntry info = new ECOEntry(arr[1], arr[2]);
					_names.Add(info);
					int openingIndex = _names.Count - 1;



					ChessBoard board = new ChessBoard();
					string[] smoves = arr[0].Split(' ');


					foreach (string smove in smoves)
					{
						ChessPosition from = ChessPositionInfo.Parse(smove.Substring(0, 2));
                        ChessPosition to = ChessPositionInfo.Parse(smove.Substring(2, 2));
						ChessMove move = ChessMoveInfo.Create(from, to);
						board.MoveApply(move);
						if (_positions.ContainsKey(board.Zobrist))
						{
							PositionEntry posinfo = _positions[board.Zobrist];
							posinfo.ECO = info;
							posinfo.OpeningCount++;
						}
						else
						{
							PositionEntry posinfo = new PositionEntry();
							posinfo.ECO = info;
							posinfo.OpeningCount = 1;
							_positions.Add(board.Zobrist, posinfo);
						}
					}
				}
			}

						

		}

		private static string ResourceFileName
		{
			get
			{
				// get a reference to the current assembly
				Assembly a = Assembly.GetExecutingAssembly();

				// get a list of resource names from the manifest
				string[] resNames = a.GetManifestResourceNames();
				string ecotxtName = "";
				foreach (string s in resNames)
				{
					if (s.ToLower().EndsWith("eco.txt"))
					{
						ecotxtName = s;
					}
				}
				return ecotxtName;
			}
		}

		public static bool GetInfoFromPosition(Int64 zob, ref int popularity, ref string code, ref string name)
		{
			if (_positions.ContainsKey(zob))
			{
				PositionEntry entry = _positions[zob];
				popularity = entry.OpeningCount;
				code = entry.ECO.Code;
				name = entry.ECO.Name;
				return true;
			}
			return false;
		}
		
	}
}
