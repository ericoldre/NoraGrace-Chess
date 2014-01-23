using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinobyl.Engine;

namespace Sinobyl.CommandLine
{
	public class Winboard
	{

		ChessGamePlayerMurderhole player = new ChessGamePlayerMurderhole();
		ChessBoard board = new ChessBoard();
		ChessPlayer myplayer = ChessPlayer.Black;
		ChessTimeControl timeControl = ChessTimeControl.Blitz(5, 5);
		TimeSpan timeLeft = TimeSpan.FromMinutes(5);

		public Winboard()
		{
			player.MovePlayed += player_OnMove;
			player.Kibitzed += player_OnKibitz;
			player.Resigned += player_OnResign;
		}

		public ChessGamePlayerPersonality EnginePersonality
		{
			get
			{
				return player.Personality;
			}
			set
			{
				player.Personality = value;
			}
		}

		void player_OnResign(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}

		void player_OnKibitz(object sender, SearchProgressEventArgs e)
		{
            
            try
            {
                string pvstring = new ChessMoves(e.Progress.PrincipleVariation).ToString(new ChessBoard(e.Progress.FEN), true);
                string output = string.Format("{0} {1} {2} {3} {4}", e.Progress.Depth, e.Progress.Score, Math.Round(e.Progress.Time.TotalMilliseconds / 10), e.Progress.Nodes, pvstring);
                Program.ConsoleWriteline(output);
            }
            catch (Exception ex)
            {
                Program.LogException(ex);
            }
            
		}

		void player_OnMove(object sender, MoveEventArgs e)
		{
            
            try
            {
                Program.ConsoleWriteline(string.Format("move {0}", e.Move.ToString()));
                board.MoveApply(e.Move);
                GameDoneAnnounce();

            }
            catch (Exception ex)
            {
                Program.LogException(ex);
            }
            
			
		}
		public bool GameDoneAnnounce()
		{
			if (board.IsMate())
			{
				Program.ConsoleWriteline(board.WhosTurn == ChessPlayer.White ? "0-1 {Black Mates}" : "1-0 {White Mates}");
				return true;
			}
			else if (board.IsDrawBy50MoveRule())
			{
				Program.ConsoleWriteline("1/2-1/2 {Draw by 50 move rule}");
				return true;
			}
			else if (board.IsDrawByRepetition())
			{
				Program.ConsoleWriteline("1/2-1/2 {Draw by repetition}");
				return true;
			}
			else if (board.IsDrawByStalemate())
			{
				Program.ConsoleWriteline("1/2-1/2 {Draw by stalemate}");
				return true;
			}
			return false;
		}

		void StartThinking()
		{
			Program.LogInfo("THINKING", board.FEN.ToString());
			player.YourTurn(board, timeControl, timeLeft);
		}
	

		public void ProcessCmd(string input)
		{
            
            //if getting only move switch to usermove format
            if (input.Length >= 4
                && string.Format("abcdefgh").IndexOf(input.Substring(0, 1)) >= 0
                && string.Format("12345678").IndexOf(input.Substring(1, 1)) >= 0
                && string.Format("abcdefgh").IndexOf(input.Substring(2, 1)) >= 0
                && string.Format("12345678").IndexOf(input.Substring(3, 1)) >= 0)
            {
                input = "usermove " + input;
            }

            string command = input;
            string argument = "";

            int spaceIdx = input.IndexOf(' ');
            if (spaceIdx >= 0)
            {
                command = input.Substring(0, spaceIdx);
                argument = input.Substring(spaceIdx + 1);
            }
            command = command.ToLower();

            Program.LogInfo("COMMAND", input);

            switch (command)
            {
                case "protover":
                    Program.ConsoleWriteline("feature usermove=1");
                    Program.ConsoleWriteline("feature setboard=1");
                    Program.ConsoleWriteline("feature analyze=1");
                    Program.ConsoleWriteline("feature done=1");
                    break;
                case "new":
                    board.FEN = new ChessFEN(ChessFEN.FENStart);
                    myplayer = ChessPlayer.Black;
                    break;
                case "force":
                    myplayer = ChessPlayer.None;
                    break;
                case "go":
                    myplayer = board.WhosTurn;
                    StartThinking();
                    break;
                case "time":
                    timeLeft = TimeSpan.FromMilliseconds(int.Parse(argument) * 10);
                    break;
                case "usermove":
                    ChessMove usermove = new ChessMove(board, argument);
                    board.MoveApply(usermove);
                    bool done = GameDoneAnnounce();
                    if (!done && board.WhosTurn == myplayer)
                    {
                        StartThinking();
                    }
                    break;
                case "?":
                    //implement later
                    //player.ForceMove();
                    //Move now. If your engine is thinking, it should move immediately; otherwise, the command should be ignored (treated as a no-op).
                    break;
                case "draw":
                    //The engine's opponent offers the engine a draw. To accept the draw, send "offer draw". To decline, ignore the offer (that is, send nothing). 
                    //If you're playing on ICS, it's possible for the draw offer to have been withdrawn by the time you accept it, so don't assume the game is over because you accept a draw offer. Continue playing until xboard tells you the game is over. See also "offer draw" below.
                    break;
                case "setboard":
                    board.FEN = new ChessFEN(argument);
                    break;
                case "undo":
                    board.MoveUndo();
                    break;
                case "remove":
                    board.MoveUndo();
                    board.MoveUndo();
                    break;
                case "level":
                    string[] args = argument.Split(' ');
                    timeControl = new ChessTimeControl();
                    timeControl.BonusEveryXMoves = int.Parse(args[0]);
                    timeControl.InitialTime = TimeSpan.FromMinutes(int.Parse(args[1]));
                    timeControl.BonusAmount = TimeSpan.FromSeconds(int.Parse(args[2]));
                    if (timeControl.BonusAmount.TotalSeconds > 0) { timeControl.BonusEveryXMoves = 1; }
                    break;
                case "analyze":
                    player.YourTurn(board, new ChessTimeControl(TimeSpan.FromDays(365), TimeSpan.FromDays(1), 0), TimeSpan.FromDays(365));
                    break;
                //custom stuff for my debugging
                case "setpos1":
                    board.FEN = new ChessFEN("3k4/1PR5/8/P7/7P/4K3/2P2P2/6NR w - - 1 48");
                    break;
                case "eval":
                    ChessEval eval = new ChessEval();
                    int e = eval.EvalFor(board, board.WhosTurn);
                    break;
                case "nodecounttest":
                    NodeCountTest();
                    break;
            }
            
		}
		public void SimulateUsermove(string usermoveTxt)
		{
			ChessMove usermove = new ChessMove(board, usermoveTxt);
			board.MoveApply(usermove);
			bool done = GameDoneAnnounce();


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


		}

		public void NodeCountTest()
		{
			using (System.IO.StreamReader reader = new System.IO.StreamReader("c:\\chess\\pgn\\gm2600.pgn"))
			{
				int gameCount = 0;
				ChessTrans trans = new ChessTrans();
				DateTime timeStart = DateTime.Now;

				while(!reader.EndOfStream)
				{
					ChessPGN pgn = ChessPGN.NextGame(reader);
					if (pgn == null) { break; }


					
					ChessBoard board1 = new ChessBoard(pgn.StartingPosition);
					foreach (ChessMove move in pgn.Moves)
					{

						if (board1.HistoryCount % 4 == 0)
						{
							ChessSearch.Args args = new ChessSearch.Args();
							args.MaxDepth = 7;
							args.TransTable = trans;
							args.GameStartPosition = new ChessFEN(pgn.StartingPosition);
							args.GameMoves = board1.HistoryMoves;
							args.Blunder = new ChessSearch.BlunderChance();
							args.StopAtTime = DateTime.Now.AddHours(1);

							ChessSearch search = new ChessSearch(args);
							search.Search();

							Program.ConsoleWriteline(string.Format("Move:{0}\tNodes:{1}\tTotal:{2}", board1.HistoryMoves.Count, search.CountAIValSearch + search.CountAIQSearch, ChessSearch.CountTotalAINodes));

						}
						
						board1.MoveApply(move);
					}

					gameCount++;
					if (gameCount >= 1) { break; }

				}
				DateTime timeEnd = DateTime.Now;
				TimeSpan timeUsed = timeEnd - timeStart;

				Program.ConsoleWriteline(string.Format("DONEALL:\tNodes:{0}\tTime:{1}", ChessSearch.CountTotalAINodes, timeUsed.TotalSeconds));


			}
		}
	}
}
