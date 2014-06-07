using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{
	public class TranspositionTable
	{
		//public static ChessTrans Global = new ChessTrans();


		private readonly EntryPair[] hashtable;

		public enum EntryType
		{
			Worthless = 0, AtLeast = 1, AtMost = 2, Exactly = 3
		}

		public class EntryPair
		{
			public readonly Entry Deepest = new Entry();
			public readonly Entry Recent = new Entry();
		}
		public class Entry
		{
			public Int64 Zobrist;
			public ChessMove BestMove;
			public int depth;
			public int value;
			public EntryType Type;
			public int age;

            public Entry()
            {
                Reset(0, ChessMove.EMPTY, -10, 0, EntryType.Worthless);
            }
			public void Reset(Int64 a_zob, ChessMove move, int a_depth, int a_value, EntryType a_type)
			{
				Zobrist = a_zob;
				BestMove = move;
				depth = a_depth;
				value = a_value;
				Type = a_type;
				age = 0;
			}

            public void Reset(Entry other)
            {
                Zobrist = other.Zobrist;
                BestMove = other.BestMove;
                depth = other.depth;
                value = other.value;
                Type = other.Type;
                age = other.age;
            }

           
			public void AgeBy(int by)
			{
				age = age + by;
			}
			
		}

        public TranspositionTable(int hastTableSize = 250000)
        {
            if (hastTableSize <= 0) { throw new ArgumentOutOfRangeException("hashTableSize"); }

            hashtable = new EntryPair[hastTableSize];

            int max = hashtable.GetUpperBound(0);
            for (int i = 0; i <= max; i++)
            {
                hashtable[i] = new EntryPair();
            }

        }

		public int GetAddress(Int64 zob)
		{
			if (zob < 0) { zob = -zob; }
			return (int)(zob % this.hashtable.Length);
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

        public Entry GetEntry(long boardZob)
        {
            EntryPair epair = FindPair(boardZob);
            if (epair.Deepest.Zobrist == boardZob)
            {
                return epair.Deepest;
            }
            else if (epair.Recent.Zobrist == boardZob)
            {
                return epair.Recent;
            }
            else
            {
                return null;
            }
        }

		public bool QueryCutoff(long boardZob, int depth, int alpha, int beta, out ChessMove bestmove, out int value)
		{
            bestmove = ChessMove.EMPTY;
            value = 0;

            //EntryPair epair = FindPair(boardZob);
            Entry e = GetEntry(boardZob);
            if (e == null) { return false; }

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


		public void AgeEntries(int by)
		{
			
			foreach (EntryPair pair in this.hashtable)
			{
				pair.Deepest.AgeBy(by);
				pair.Recent.AgeBy(by);
			}
		}

        public void StoreVariation(Board board, List<ChessMove> pv)
		{
			Int64 zobInit = board.ZobristBoard;
			foreach (ChessMove move in pv)
			{
				this.Store(board.ZobristBoard, 0, EntryType.Worthless, 0, move);
				board.MoveApply(move);
			}
			while (board.ZobristBoard != zobInit)
			{
				board.MoveUndo();
			}
		}
		public void Store(long boardZob, int depth, EntryType type, int value, ChessMove move)
		{

			#region adjust near mate scores
			//don't store scores close to mate
			if (value >= ChessSearch.MateIn(200))
			{
				if (type == EntryType.Exactly || type == EntryType.AtLeast)
				{
					type = EntryType.AtLeast;
                    value = ChessSearch.MateIn(200);
				}
				else
				{
					type = EntryType.Worthless;
				}
			}
            else if (value <= -ChessSearch.MateIn(200))
			{
				if (type == EntryType.Exactly || type == EntryType.AtMost)
				{
					type = EntryType.AtMost;
                    value = -ChessSearch.MateIn(200);
				}
				else
				{
					type = EntryType.Worthless;
				}
			}
			#endregion

            EntryPair epair = this.FindPair(boardZob);
            
            //if we are storing without a move, see if one of existing entries at least has a move to store.
            if(move == ChessMove.EMPTY)
            {
                if (epair.Deepest.Zobrist == boardZob) { move = epair.Deepest.BestMove; }
                else if (epair.Recent.Zobrist == boardZob) { move = epair.Recent.BestMove; }
            }

			if (depth >= ((epair.Deepest.depth) - (epair.Deepest.age)))
			{
				//our data to store is better/deeper than entry in first slot

                ////push a copy of this entry to the second slot, if the position is different from the current
                if (boardZob != epair.Deepest.Zobrist)
                {
                    epair.Recent.Reset(epair.Deepest); //we may want to test taking this out, because the recent entry we are replacing may well be newer than the one we are replacing it with
                }

				//store current information in 1st slot.
                epair.Deepest.Reset(boardZob, move, depth, value, type);
			}
			else
			{
				//if not better than first slot put in the 2nd
                epair.Recent.Reset(boardZob, move, depth, value, type);
			}

			this.PairStore(epair);

		}
	}
}
