using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinobyl.Engine;

namespace Sinobyl.CommandLine
{
	public class Winboard: IDisposable
	{
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(Winboard));
		private readonly ChessGamePlayerMurderhole _player = new ChessGamePlayerMurderhole();
		private readonly Board _board = new Board();
		private Player _myplayer = Player.Black;
        private TimeControl _timeControl = TimeControl.Blitz(5, 5);
		private TimeSpan _timeLeft = TimeSpan.FromMinutes(5);
        private readonly ChessEval _staticEval = new ChessEval();
		public Winboard()
		{
			_player.MovePlayed += player_OnMove;
			_player.Kibitz += player_OnKibitz;
			_player.Resigned += player_OnResign;
		}

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _player.Dispose();
                // Dispose any managed objects
            }
            // Now disposed of any unmanaged objects
        }

		public ChessGamePlayerPersonality EnginePersonality
		{
			get
			{
				return _player.Personality;
			}
			set
			{
				_player.Personality = value;
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
                Board board = new Board(e.Progress.FEN);

                string pvstring = e.Progress.PrincipleVariation.Descriptions(board, true);
                
                string output = string.Format("{0} {1} {2} {3} {4}", e.Progress.Depth, e.Progress.Score, Math.Round(e.Progress.Time.TotalMilliseconds / 10), e.Progress.Nodes, pvstring);
                Program.ConsoleWriteline(output);
            }
            catch (Exception ex)
            {
                //throw ex;
                Program.LogException(ex);
            }
            
		}

		void player_OnMove(object sender, MoveEventArgs e)
		{
            
            try
            {
                Program.ConsoleWriteline(string.Format("move {0}", e.Move.Description()));
                _board.MoveApply(e.Move);
                GameDoneAnnounce();

            }
            catch (Exception ex)
            {
                //throw ex;
                Program.LogException(ex);
            }
            
			
		}
		public bool GameDoneAnnounce()
		{
			if (_board.IsMate())
			{
				Program.ConsoleWriteline(_board.WhosTurn == Player.White ? "0-1 {Black Mates}" : "1-0 {White Mates}");
				return true;
			}
			else if (_board.IsDrawBy50MoveRule())
			{
				Program.ConsoleWriteline("1/2-1/2 {Draw by 50 move rule}");
				return true;
			}
			else if (_board.IsDrawByRepetition())
			{
				Program.ConsoleWriteline("1/2-1/2 {Draw by repetition}");
				return true;
			}
			else if (_board.IsDrawByStalemate())
			{
				Program.ConsoleWriteline("1/2-1/2 {Draw by stalemate}");
				return true;
			}
			return false;
		}

		void StartThinking()
		{
            if (_log.IsInfoEnabled)
            {
                _log.InfoFormat("THINKING:{0}", _board.FENCurrent);
            }
			_player.YourTurn(_board, _timeControl, _timeLeft);
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


            if (_log.IsDebugEnabled)
            {
                _log.DebugFormat("COMMAND:{0}", input);
            }

            switch (command)
            {
                case "protover":
                    Program.ConsoleWriteline("feature usermove=1");
                    Program.ConsoleWriteline("feature setboard=1");
                    Program.ConsoleWriteline("feature analyze=1");
                    Program.ConsoleWriteline("feature done=1");
                    break;
                case "new":
                    _board.FENCurrent = new FEN(FEN.FENStart);
                    _myplayer = Player.Black;
                    break;
                case "force":
                    _myplayer = Player.None;
                    break;
                case "go":
                    _myplayer = _board.WhosTurn;
                    StartThinking();
                    break;
                case "time":
                    _timeLeft = TimeSpan.FromMilliseconds(int.Parse(argument) * 10);
                    break;
                case "usermove":
                    Move usermove = MoveInfo.Parse(_board, argument);
                    _board.MoveApply(usermove);
                    bool done = GameDoneAnnounce();
                    if (!done && _board.WhosTurn == _myplayer)
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
                    _board.FENCurrent = new FEN(argument);
                    break;
                case "undo":
                    _board.MoveUndo();
                    break;
                case "remove":
                    _board.MoveUndo();
                    _board.MoveUndo();
                    break;
                case "level":
                    string[] args = argument.Split(' ');
                    _timeControl = new TimeControl();
                    _timeControl.BonusEveryXMoves = int.Parse(args[0]);
                    _timeControl.InitialAmount = TimeSpan.FromMinutes(int.Parse(args[1]));
                    _timeControl.BonusAmount = TimeSpan.FromSeconds(int.Parse(args[2]));
                    if (_timeControl.BonusAmount.TotalSeconds > 0) { _timeControl.BonusEveryXMoves = 1; }
                    break;
                case "analyze":
                    _player.YourTurn(_board, new TimeControl(TimeSpan.FromDays(365), TimeSpan.FromDays(1), 0), TimeSpan.FromDays(365));
                    break;
                //custom stuff for debugging
                case "setpos1":
                    _board.FENCurrent = new FEN("8/4r3/R4n2/2pPk3/p1P1B1p1/3K2P1/5P2/8 w - - 4 54 ");
                    break;
                case "eval":
                    ChessEval eval = new ChessEval();
                    int e = eval.EvalFor(_board, _board.WhosTurn);
                    break;
            }
            
		}

		public void SimulateUsermove(string usermoveTxt)
		{
			Move usermove = MoveInfo.Parse(_board, usermoveTxt);
			_board.MoveApply(usermove);
			bool done = GameDoneAnnounce();


			//need to extrapolate previous moves and initial position from board
            List<Move> moves = _board.HistoryMoves.ToList();
			int moveCount = moves.Count;
			for (int i = 0; i < moveCount; i++)
			{
				_board.MoveUndo();
			}
			FEN initialPosition = _board.FENCurrent;
			foreach (Move move in moves)
			{
				_board.MoveApply(move);
			}


		}

	}
}
