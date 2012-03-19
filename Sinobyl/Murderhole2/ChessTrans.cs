using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Murderhole
{
	public class ChessTrans
	{
		//public static ChessTrans Global = new ChessTrans();


		private EntryPair[] hashtable = new EntryPair[5000];


		public enum EntryType
		{
			Worthless = 0, AtLeast = 1, AtMost = 2, Exactly = 3
		}

		public class EntryPair
		{
			public Entry Deepest = new Entry(0,new ChessMove(),-10,0,EntryType.Worthless);
			public Entry Recent = new Entry(0, new ChessMove(), -10, 0, EntryType.Worthless);
		}
		public class Entry
		{
			public readonly Int64 Zobrist;
			public readonly ChessMove BestMove;
			public readonly int depth;
			public readonly int value;
			public readonly EntryType Type;
			public int age;


			public Entry(Int64 a_zob, ChessMove move, int a_depth, int a_value, EntryType a_type)
			{
				Zobrist = a_zob;
				BestMove = move;
				depth = a_depth;
				value = a_value;
				Type = a_type;
				age = 0;
			}
			public void AgeBy(int by)
			{
				age = age + by;
			}
			
		}

		private int GetAddress(Int64 zob)
		{
			if (zob < 0) { zob = -zob; }
			return (int)(zob % this.hashtable.GetUpperBound(0));
		}
		private EntryPair FindPair(Int64 zob)
		{
			return hashtable[GetAddress(zob)];
		}
		private void PairStore(EntryPair pair)
		{
			if (pair.Deepest.Zobrist != 0 && pair.Recent.Zobrist != 0)
			{
				//both entries have values, make sure they are really suppose to go to the same slot
				if (GetAddress(pair.Deepest.Zobrist) != GetAddress(pair.Recent.Zobrist))
				{
					throw new Exception("hash table entries out of synch");
				}
			}
			hashtable[GetAddress(pair.Deepest.Zobrist)] = pair;
		}

		public bool QueryCutoff(ChessBoard board, int depth, int alpha, int beta, ref ChessMove bestmove, ref int value)
		{

			EntryPair epair = FindPair(board.Zobrist);
			Entry e = epair.Deepest;
			bool foundEntry = false;

			if (epair.Deepest.Zobrist == board.Zobrist)
			{
				e = epair.Deepest;
				foundEntry = true;
			}
			else if (epair.Recent.Zobrist == board.Zobrist)
			{
				e = epair.Recent;
				foundEntry = true;
			}
			if (!foundEntry) { return false; }

			//we found a valid entry for this position
			bestmove = e.BestMove;
			e.age = 0;

			//entry doesn't have a valid valud
			if (e.Type == EntryType.Worthless) { return false; }

			//entry isn't deep enough
			if (e.depth < depth){return false;}

			if (e.Type == EntryType.Exactly)
			{
				value = e.value;
				if (value > beta) { value = beta; }
				if (value < alpha) { value = alpha; }
				return true;
			}
			if ((e.Type == EntryType.AtLeast) && (e.value >= beta))
			{
				value = beta;
				return true;
			}
			if ((e.Type == EntryType.AtMost) && (e.value <= alpha))
			{
				value = alpha;
				return true;
			}

			return false;
		}

		public ChessTrans()
		{
			int max = hashtable.GetUpperBound(0);
			for (int i = 0; i <= max; i++)
			{
				hashtable[i] = new EntryPair();
			}
		}
		public void AgeEntries(int by)
		{
			
			foreach (EntryPair pair in this.hashtable)
			{
				pair.Deepest.AgeBy(by);
				pair.Recent.AgeBy(by);
			}
		}

		public void StoreVariation(ChessBoard board, ChessMoves pv)
		{
			Int64 zobInit = board.Zobrist;
			foreach (ChessMove move in pv)
			{
				this.Store(board, 0, EntryType.Worthless, 0, move);
				board.MoveApply(move);
			}
			while (board.Zobrist != zobInit)
			{
				board.MoveUndo();
			}
		}
		public void Store(ChessBoard board, int depth, EntryType type, int value, ChessMove move)
		{

			#region adjust near mate scores
			//don't store scores close to mate
			if (value >= Chess.MateIn(200))
			{
				if (type == EntryType.Exactly || type == EntryType.AtLeast)
				{
					type = EntryType.AtLeast;
					value = Chess.MateIn(200);
				}
				else
				{
					type = EntryType.Worthless;
				}
			}
			else if (value <= -Chess.MateIn(200))
			{
				if (type == EntryType.Exactly || type == EntryType.AtMost)
				{
					type = EntryType.AtMost;
					value = -Chess.MateIn(200);
				}
				else
				{
					type = EntryType.Worthless;
				}
			}
			#endregion

			EntryPair epair = this.FindPair(board.Zobrist);

			if (depth >= ((epair.Deepest.depth) - (epair.Deepest.age)))
			{
				//our data to store is better/deeper than entry in first slot

				//push a copy of this entry to the second slot, if the position is different from the current
				if (board.Zobrist != epair.Deepest.Zobrist)
				{
					epair.Recent = epair.Deepest; //we will want to test taking this out, because the recent entry we are replacing may well be newer than the one we are replacing it with
				}

				//store current information in 1st slot.
				epair.Deepest = new Entry(board.Zobrist, move, depth, value, type);
			}
			else
			{
				//if not better than first slot put in the 2nd
				epair.Recent = new Entry(board.Zobrist, move, depth, value, type);
			}

			this.PairStore(epair);

		}
	}
}
