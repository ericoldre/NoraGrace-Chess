using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.ComponentModel;
namespace Murderhole
{
	public abstract class ChessGamePlayer
	{
		public event msgMove OnMove;
		public event msgChessSearchProgress OnKibitz;
		public event msgVoid OnResign;

		public abstract void TurnStop();
		public abstract void YourTurn(ChessFEN initalPosition, ChessMoves prevMoves, ChessTimeControl timeControl, TimeSpan timeLeft);
		public abstract string Name { get; }

		protected void RaiseOnMove(ChessMove move)
		{
			msgMove e = this.OnMove;
			if (e != null) { e(this, move); }
		}
		protected void RaiseOnResign()
		{
			msgVoid e = this.OnResign;
			if (e != null) { e(this); }
		}
		protected void RaiseOnKibitz(ChessSearch.Progress prog)
		{
			msgChessSearchProgress e = this.OnKibitz;
			if (e != null) { e(this, prog); }
		}


		public void YourTurn(ChessBoard board, ChessTimeControl timeControl, TimeSpan timeLeft)
		{
			//need to extrapolate previous moves and initial position from board
			ChessMoves moves = board.HistoryMoves;
			int moveCount = moves.Count;
			for (int i = 0; i < moveCount; i++)
			{
				board.MoveUndo();
			}
			ChessFEN initialPosition = board.FEN;
			foreach (ChessMove move in moves)
			{
				board.MoveApply(move);
			}
			YourTurn(initialPosition, moves, timeControl, timeLeft);
		}


	}

	public class ChessGamePlayerPersonality
	{
		public int MaxDepth { get; set; }
		public int NodesPerSecond { get; set; }
		public String Name { get; set; }
		public ChessSearch.BlunderChance Blunder { get; set; }
		
		public ChessGamePlayerPersonality()
		{
			MaxDepth = int.MaxValue;
			NodesPerSecond = int.MaxValue;
			Name = "Default personality";
			Blunder = new ChessSearch.BlunderChance();
		}

		public static ChessGamePlayerPersonality FromStrength(float a_strength)
		{
			if(a_strength<0){a_strength=0;}
			if(a_strength>1){a_strength=1;}
			ChessGamePlayerPersonality retval = new ChessGamePlayerPersonality();

			retval.NodesPerSecond = (int)(10000 * a_strength) + 500;
			retval.MaxDepth = (int)(10 * a_strength) + 2;
			retval.Blunder.BlunderSkipCount = 0;
			retval.Blunder.BlunderReduceEnd = .1;
			retval.Blunder.BlunderPct = .85 - (.75 * a_strength);
			retval.Blunder.BlunderDepth = .05 + (.10 * a_strength);

			if (a_strength == 1)
			{
				retval.NodesPerSecond = int.MaxValue;
				retval.MaxDepth = int.MaxValue;
				retval.Blunder.BlunderPct = 0;
				retval.Blunder.BlunderSkipCount = int.MaxValue;
				retval.Blunder.BlunderReduceEnd = 1;
			}

			return retval;

		}


		
	}
	public class ChessGamePlayerMurderhole : ChessGamePlayer
	{

		private readonly ChessSearchAsync search;
		private ChessGamePlayerPersonality _personality = ChessGamePlayerPersonality.FromStrength(1);
		private readonly ChessTrans _transTable = new ChessTrans();
		private BackgroundWorker BookBackgroundWorker;

		public TimeSpan DelaySearch { get; set; }

		public ChessGamePlayerMurderhole()
		{
			DelaySearch = new TimeSpan(0);
			search = new ChessSearchAsync();
			search.OnProgress += new msgChessSearchProgress(search_OnProgress);
			search.OnFinish += new msgChessSearchProgress(search_OnFinish);
		}
		public ChessGamePlayerPersonality Personality
		{
			get
			{
				return _personality;
			}
			set
			{
				_personality = value;
			}
		}

		public override string Name
		{
			get
			{
				return Personality.Name;
			}
		}

		void search_OnFinish(object sender, ChessSearch.Progress progress)
		{
			this.RaiseOnMove(progress.PrincipleVariation[0]);
			//if (this.OnMove != null) { this.OnMove(this, progress.PrincipleVariation[0]); }
		}

		void search_OnProgress(object sender, ChessSearch.Progress progress)
		{
			this.RaiseOnKibitz(progress);
			//if (this.OnProgress != null) { this.OnProgress(this, progress); }
		}

		public override void TurnStop()
		{
			search.Abort(false);
		}

		public override void YourTurn(ChessFEN initalPosition, ChessMoves prevMoves, ChessTimeControl timeControl, TimeSpan timeLeft)
		{
			//ChessSearch a = new ChessSearch(game.FENCurrent);

			ChessOpening opening = new ChessOpening();//only here to debug static contructor

			ChessBoard currBoard = new ChessBoard(initalPosition,prevMoves);

			ChessFEN FenCurrent = currBoard.FEN;

			//check opening book
			ChessBookOpening book = new ChessBookOpening();
			ChessMove bookMove = book.FindMove(FenCurrent);
			if (bookMove != null)
			{
				if (BookBackgroundWorker == null)
				{
					BookBackgroundWorker = new BackgroundWorker();
					BookBackgroundWorker.DoWork += new DoWorkEventHandler(BookBackgroundWorker_DoWork);
					BookBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BookBackgroundWorker_RunWorkerCompleted);
					BookBackgroundWorker.WorkerSupportsCancellation = true;
				}
				BookBackgroundWorker.RunWorkerAsync(bookMove);
				return;
			}


			ChessSearch.Args args = new ChessSearch.Args();
			args.GameStartPosition = initalPosition;
			args.GameMoves = new ChessMoves(prevMoves);
			args.MaxDepth = _personality.MaxDepth;
			args.NodesPerSecond = _personality.NodesPerSecond;
			args.Blunder = _personality.Blunder;
			args.StopAtTime = DateTime.Now + timeControl.RecommendSearchTime(timeLeft, FenCurrent.fullmove);
			args.TransTable = _transTable;
			args.Delay = this.DelaySearch;
			
			search.SearchAsync(args);

		}

		public void ForceMove()
		{
			search.Abort(true);
		}


		void BookBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			ChessMove move = (ChessMove)e.Result;
			if (move != null)
			{
				RaiseOnMove(move);
			}
		}

		void BookBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			BackgroundWorker bw = (BackgroundWorker)sender;

			DateTime endtime = DateTime.Now.Add(this.DelaySearch);
			while (DateTime.Now < endtime)
			{
				System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(50));
				if (bw.CancellationPending)
				{
					return;
				}
			}
			ChessMove move = (ChessMove)e.Argument;
			e.Result = move;
		}




		void search_FoundMove(object sender, object move)
		{
			this.RaiseOnMove((ChessMove)move);
			//if (this.OnMove != null) { this.OnMove(this, move); }
		}
	}
}
