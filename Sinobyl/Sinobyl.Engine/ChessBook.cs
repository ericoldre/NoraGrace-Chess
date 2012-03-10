using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{
	public abstract class ChessBook
	{
		public abstract ChessMove FindMove(ChessFEN fen);
	}
	public class ChessBookOpening: ChessBook
	{
		private class moveinfo
		{
			public ChessMove move { get; set; }
			public int pop { get; set; }
		}
		public override ChessMove FindMove(ChessFEN fen)
		{
			ChessBoard board = new ChessBoard(fen);
			ChessMoves moves = ChessMove.GenMoves(board);

			List<moveinfo> infos = new List<moveinfo>();
			int totalPop = 0;
			foreach (ChessMove move in moves)
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
				if(ChessOpening.GetInfoFromPosition(board.Zobrist, ref pop, ref eco, ref name))
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
			if (infos.Count == 0) { return null; }
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
			return null;
			
			
		}

	}
}
