using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoraGrace.Engine
{
	public abstract class Book
	{
		public abstract Move FindMove(FEN fen);
	}
	public class BookOpening: Book
	{
		private class moveinfo
		{
			public Move move { get; set; }
			public int pop { get; set; }
		}
		public override Move FindMove(FEN fen)
		{
			Board board = new Board(fen);
			var moves = MoveUtil.GenMoves(board);

			List<moveinfo> infos = new List<moveinfo>();
			int totalPop = 0;
			foreach (Move move in moves)
			{
				board.MoveApply(move);
				if (board.IsCheck())
				{
					board.MoveUndo();
					continue;
				}

				//add information about the position
				int pop = 0;
				string eco = "";
				string name = "";
				if(Opening.GetInfoFromPosition(board.ZobristBoard, ref pop, ref eco, ref name))
				{
					moveinfo info = new moveinfo();
					info.pop = pop;
					info.move = move;
					infos.Add(info);
					totalPop += pop;
				}
				
				//undo the move
				board.MoveUndo();
			}
			if (infos.Count == 0) { return Move.EMPTY; }
			Random rand = new Random();
			int i = rand.Next(1, totalPop);
			while (infos.Count>0)
			{
				moveinfo info = infos[infos.Count - 1];

				//check if the tot item meets this
				if (i <= info.pop)
				{
					return info.move;
				}
				else
				{
					i = i - info.pop;
					infos.Remove(info);
				}
			}
			return Move.EMPTY;
			
			
		}

	}
}
