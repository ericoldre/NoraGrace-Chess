using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.ObjectModel;


namespace NoraGrace.Engine
{
	public class Opening
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


		static Opening()
		{
			_names = new List<ECOEntry>();
			_positions = new Dictionary<long, PositionEntry>();

			//find resource of openings if it exists
			string ResourceName = ResourceFileName;
			if (ResourceName.Length == 0) { return; }

			Assembly a = Assembly.GetExecutingAssembly();
            System.IO.Stream ecostream = a.GetManifestResourceStream(ResourceName);

			int linecount = 0;
            using (System.IO.StreamReader reader = new System.IO.StreamReader(ecostream))
			{
				while (!reader.EndOfStream)
				{
					linecount++;
					string line = reader.ReadLine().Trim();
					string[] arr = line.Split('\t');
					ECOEntry info = new ECOEntry(arr[1], arr[2]);
					_names.Add(info);
					int openingIndex = _names.Count - 1;



					Board board = new Board();
					string[] smoves = arr[0].Split(' ');


					foreach (string smove in smoves)
					{
						Position from = PositionInfo.Parse(smove.Substring(0, 2));
                        Position to = PositionInfo.Parse(smove.Substring(2, 2));
                        Move move = MoveInfo.Parse(board, smove);
						board.MoveApply(move);
						if (_positions.ContainsKey(board.ZobristBoard))
						{
							PositionEntry posinfo = _positions[board.ZobristBoard];
							posinfo.ECO = info;
							posinfo.OpeningCount++;
						}
						else
						{
							PositionEntry posinfo = new PositionEntry();
							posinfo.ECO = info;
							posinfo.OpeningCount = 1;
							_positions.Add(board.ZobristBoard, posinfo);
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
