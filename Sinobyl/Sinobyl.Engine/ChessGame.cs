using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;


namespace Sinobyl.Engine
{

	//public delegate void MsgMove(object sender, object move);
	
	public class ChessTimeControl
	{
		[System.Xml.Serialization.XmlIgnore]
		public TimeSpan InitialTime = TimeSpan.FromMinutes(1);
		[System.Xml.Serialization.XmlIgnore]
		public TimeSpan BonusAmount = TimeSpan.FromSeconds(1);

		public int BonusEveryXMoves = 1;

		public double SerializationInitTimeSeconds
		{
			get
			{
				return InitialTime.TotalSeconds;
			}
			set
			{
				InitialTime = TimeSpan.FromSeconds(value);
			}
		}
		public double SerializationBonusAmt
		{
			get
			{
				return BonusAmount.TotalSeconds;
			}
			set
			{
				BonusAmount = TimeSpan.FromSeconds(value);
			}
		}

		public ChessTimeControl()
		{

		}

	
		public ChessTimeControl(TimeSpan a_InitialTime, TimeSpan a_BonusAmount , int a_BonusEveryXMoves)
		{
			InitialTime = a_InitialTime;
			BonusEveryXMoves = a_BonusEveryXMoves;
			BonusAmount = a_BonusAmount;
		}
		public static ChessTimeControl Blitz(int a_Minutes, int a_Seconds)
		{
			return new ChessTimeControl(TimeSpan.FromMinutes(a_Minutes),TimeSpan.FromSeconds(a_Seconds) , 1);
		}
		public static ChessTimeControl TotalGame(int a_Minutes)
		{
			return new ChessTimeControl(TimeSpan.FromMinutes(a_Minutes), TimeSpan.FromSeconds(0), 0);
		}
		public static ChessTimeControl MovesInMinutes(int a_Minutes, int a_Moves)
		{
			return new ChessTimeControl(TimeSpan.FromMinutes(a_Minutes), TimeSpan.FromMinutes(a_Minutes), a_Moves);
		}
		
		
		public TimeSpan CalcNewTimeLeft(TimeSpan beforeMove, TimeSpan amountUsed, int moveNum)
		{
			TimeSpan retval = beforeMove-amountUsed;
			if (BonusEveryXMoves != 0 && moveNum % BonusEveryXMoves == 0)
			{
				retval += BonusAmount;
			}
			return retval;
		}

		public TimeSpan RecommendSearchTime(TimeSpan a_Remaining, int a_MoveNum)
		{
			TimeSpan ExtraPerMove = TimeSpan.FromMilliseconds(this.BonusAmount.TotalMilliseconds / this.BonusEveryXMoves);

			return TimeSpan.FromMilliseconds(a_Remaining.TotalMilliseconds / 30)+ExtraPerMove;
		}


	}

	public class ChessGameOptions
	{
		public ChessTimeControl TimeControl;
		public bool TimeControlEnforced = true;
		public ChessGamePlayerPersonality WhiteAI;
		public ChessGamePlayerPersonality BlackAI;

	}
	public class ChessGame
	{


		public event msgVoid OnGameStart;
		public event msgMove OnGameMove;
		public event msgMove OnGameMoveUndo;
		public event msgPlayerChessSearchProgress OnPlayerKibitz;
		public event msgVoid OnGameFinish;


		//properties that are locked once the game starts
		private ChessGamePlayer _white;
		private ChessGamePlayer _black;
		private ChessTimeControl _timecontrol = ChessTimeControl.Blitz(1,0);
		private bool _timeControlEnforced = true;
		private ReadOnlyCollection<ChessMove> _startMoves = new ReadOnlyCollection<ChessMove>(new ChessMoves());
		private ChessFEN _startFEN = new ChessFEN(Chess.FENStart);

		private bool _started = false;
		private readonly ChessBoard _board = new ChessBoard();
		private readonly ChessMoves _moves = new ChessMoves();
		public ChessResult? _result = null;
		public ChessResultReason _resultReason = ChessResultReason.NotDecided;
		private TimeSpan[] _timeRemainingForPlayer = new TimeSpan[2];
		private DateTime _timeMoveStarted;

		public ChessGame()
		{

		}

		#region Properties to be set before game start

		private void PlayerHandlersRemove(ChessGamePlayer player)
		{
			if (player != null)
			{
				player.OnMove -= new msgMove(Player_OnMove);
				player.OnKibitz -= new msgChessSearchProgress(player_OnKibitz);
				player.OnResign -= new msgVoid(player_OnResign);
			}
		}
		private void PlayerHandlersAdd(ChessGamePlayer player)
		{
			player.OnKibitz += new msgChessSearchProgress(player_OnKibitz);
			player.OnMove += new msgMove(Player_OnMove);
			player.OnResign += new msgVoid(player_OnResign);
		}

		public void TakebackMoves(int count)
		{
			PlayerWhosTurn.TurnStop();
			for (int i = 0; i < count; i++)
			{
				if (_moves.Count > 0)
				{
					ChessMove moveUndoing = _moves[_moves.Count - 1];
					_board.MoveUndo();
					_moves.RemoveAt(_moves.Count - 1);
					if (this.OnGameMoveUndo != null) { this.OnGameMoveUndo(this, moveUndoing); }
				}
			}
			PlayerWhosTurn.YourTurn(this._startFEN, new ChessMoves(this.MoveHistory()), this.TimeControl, this.ClockTime(FENCurrent.whosturn));

		}

		void player_OnResign(object sender)
		{
			if (sender.Equals(PlayerBlack))
			{
				_result = ChessResult.WhiteWins;
			}
			else if (sender.Equals(PlayerWhite))
			{
				_result = ChessResult.BlackWins;
			}
			_resultReason = ChessResultReason.Resign;
			if (this.OnGameFinish != null) { this.OnGameFinish(this); }
		}

		void player_OnKibitz(object sender, ChessSearch.Progress progress)
		{
			if (this.OnPlayerKibitz != null)
			{
				ChessPlayer color = sender == _white ? ChessPlayer.White : ChessPlayer.Black;
				this.OnPlayerKibitz(this, color, progress);
			}
		}
		public ChessGamePlayer PlayerWhite
		{
			get
			{
				return _white;
			}
			set
			{
				if (_started) { throw new Exception("game already started"); }
				PlayerHandlersRemove(_white);
				_white = value;
				PlayerHandlersAdd(_white);
			}
		}
		public ChessGamePlayer PlayerBlack
		{
			get
			{
				return _black;
			}
			set
			{
				if (_started) { throw new Exception("game already started"); }
				PlayerHandlersRemove(_black);
				_black = value;
				PlayerHandlersAdd(_black);
			}
		}
		public ChessTimeControl TimeControl
		{
			get
			{
				return _timecontrol;
			}
			set
			{
				if (_started) { throw new Exception("game already started"); }
				_timecontrol = value;
			}
		}
		public bool TimeControlEnforced
		{
			get
			{
				return _timeControlEnforced;
			}
			set
			{
				if (_started) { throw new Exception("game already started"); }
				_timeControlEnforced = value;
			}
		}
		public ChessFEN StartFEN
		{
			get
			{
				return _startFEN;
			}
			set
			{
				if (_started) { throw new Exception("game already started"); }
				_startFEN = value;
				StartMoves = StartMoves; //reinit board state start moves;
			}
		}
		public ReadOnlyCollection<ChessMove> StartMoves
		{
			get
			{
				return _startMoves;
			}
			set
			{
				if (_started) { throw new Exception("game already started"); }
				//apply these moves to the board;
				_board.FEN = _startFEN;
				foreach (ChessMove move in value)
				{
					if (move.IsLegal(_board))
					{
						_board.MoveApply(move);
					}
					else
					{
						throw new Exception(string.Format("error initializing game start moves {0} is not a valid position from {1}", move.ToString(_board), _board.FEN.ToString()));
					}
				}
				_startMoves = value;
			}
		}
		
		#endregion

		public void CheckClocks()
		{
			//check if out of time
			if (_timeControlEnforced && this.ClockTime(_board.WhosTurn) < new TimeSpan(0))
			{
				_timeRemainingForPlayer[(int)_board.WhosTurn] = this.ClockTime(_board.WhosTurn);

				if (_board.WhosTurn == ChessPlayer.White) { _result = ChessResult.BlackWins; }
				if (_board.WhosTurn == ChessPlayer.Black) { _result = ChessResult.WhiteWins; }
				_resultReason = ChessResultReason.OutOfTime;

				if (this.OnGameFinish != null) { this.OnGameFinish(this); }
				return; //game over
			}
		}

		public void GameStart()
		{
			if (_started) { throw new Exception("game already started"); }
			_started = true;
			
			_timeRemainingForPlayer[(int)ChessPlayer.White] = _timecontrol.InitialTime;
			_timeRemainingForPlayer[(int)ChessPlayer.Black] = _timecontrol.InitialTime;
			_timeMoveStarted = DateTime.Now;

			if (this.OnGameStart != null) { this.OnGameStart(this); }

			PlayerWhosTurn.YourTurn(this._startFEN,new ChessMoves(this.MoveHistory()),this.TimeControl,this.ClockTime(FENCurrent.whosturn));
		}
		public ChessGamePlayer PlayerWhosTurn
		{
			get
			{
				return _board.WhosTurn == ChessPlayer.White ? _white : _black;
			}
		}
		
		public ChessResult? Result
		{
			get { return _result; }
		}
		public ChessResultReason ResultReason
		{
			get 
			{
				if (_result != null && _resultReason == ChessResultReason.NotDecided)
				{
					return ChessResultReason.Unknown;
				}
				else if (_result == null)
				{
					return ChessResultReason.NotDecided;
				}
				return _resultReason; 
			}
		}

		public TimeSpan ClockTime(ChessPlayer a_player)
		{
			if (this._moves.Count == 0 || _result != null)
			{
				//no moves played yet.
				return _timeRemainingForPlayer[(int)a_player];
			}
			if (a_player == _board.WhosTurn)
			{
				return _timeRemainingForPlayer[(int)a_player] - (DateTime.Now - _timeMoveStarted);
			}
			else
			{
				return _timeRemainingForPlayer[(int)a_player];
			}
		}
		void Player_OnMove(object sender, object moveObj)
		{
			//check if game already over.
			if (_result != null)
			{
				return;
			}
			
			//check legality of call
			ChessMove move = (ChessMove)moveObj;
			if (!sender.Equals(_board.WhosTurn==ChessPlayer.White?_white:_black))
			{
				throw new Exception("it is not your turn");
			}
			if (!move.IsLegal(_board))
			{
				throw new Exception(string.Format("{0} is not a legal move from this position", move.ToString()));
			}

			//check if out of time
			this.CheckClocks();
			if (_result != null)
			{
				return;
			}

			//adjust last players time
			if (_moves.Count == 0) { _timeMoveStarted = DateTime.Now; }//if this was the first move of the game, ignore amount of time thinking
			_timeRemainingForPlayer[(int)_board.WhosTurn] = _timecontrol.CalcNewTimeLeft(_timeRemainingForPlayer[(int)_board.WhosTurn], DateTime.Now - _timeMoveStarted, _board.FullMoveCount);
			_timeMoveStarted = DateTime.Now;


			_board.MoveApply(move);
			_moves.Add(move);

			if (_board.IsMate())
			{
				_result = _board.WhosTurn == ChessPlayer.White ? ChessResult.BlackWins : ChessResult.WhiteWins;
				_resultReason = ChessResultReason.Checkmate;
			}
			else if (_board.IsDrawBy50MoveRule())
			{
				_result = ChessResult.Draw;
				_resultReason = ChessResultReason.FiftyMoveRule;
			}
			else if (_board.IsDrawByRepetition())
			{
				_result = ChessResult.Draw;
				_resultReason = ChessResultReason.Repetition;
			}
			else if (_board.IsDrawByStalemate())
			{
				_result = ChessResult.Draw;
				_resultReason = ChessResultReason.Stalemate;
			}

			if (this.OnGameMove != null) { this.OnGameMove(this, move); }

			if (_result == null)
			{
				//game still going
				PlayerWhosTurn.YourTurn(this._startFEN, new ChessMoves(this.MoveHistory()), this.TimeControl, this.ClockTime(FENCurrent.whosturn));
			}
			else
			{
				//game is done.
				if (this.OnGameFinish != null) { this.OnGameFinish(this); }
			}
			

		}
		public ChessFEN FENCurrent
		{
			get
			{
				return _board.FEN;
			}
		}
		public ReadOnlyCollection<ChessMove> MoveHistory()
		{
			ChessMoves retval = new ChessMoves(_startMoves);
			retval.AddRange(_moves);
			return new ReadOnlyCollection<ChessMove>(retval);
		}
		
		
	}


}
