using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;


namespace Sinobyl.Engine
{
    
    public class ChessGameOptions
    {
        public TimeControl TimeControl;
        public bool TimeControlEnforced = true;
        public ChessGamePlayerPersonality WhiteAI;
        public ChessGamePlayerPersonality BlackAI;

    }
    public class ChessGame
    {


        public event EventHandler GameStarted;
        public event EventHandler<MoveEventArgs> GameMoveApplied;
        public event EventHandler<MoveEventArgs> GameMoveUndo;
        public event EventHandler<SearchProgressPlayerEventArgs> PlayerKibitzed;
        public event EventHandler GameFinished;


        //properties that are locked once the game starts
        private ChessGamePlayer _white;
        private ChessGamePlayer _black;
        private TimeControl _timecontrol = TimeControl.Blitz(1, 0);
        private bool _timeControlEnforced = true;
        private ReadOnlyCollection<ChessMove> _startMoves = new ReadOnlyCollection<ChessMove>(new ChessMoves());
        private ChessFEN _startFEN = new ChessFEN(ChessFEN.FENStart);

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
                player.MovePlayed -= Player_OnMove;
                player.Kibitz -= player_OnKibitz;
                player.Resigned -= player_OnResign;
            }
        }
        private void PlayerHandlersAdd(ChessGamePlayer player)
        {
            player.Kibitz += player_OnKibitz;
            player.MovePlayed += Player_OnMove;
            player.Resigned += player_OnResign;
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
                    OnGameMoveUndo(moveUndoing);
                }
            }
            PlayerWhosTurn.YourTurn(this._startFEN, new ChessMoves(this.MoveHistory()), this.TimeControl, this.ClockTime(FENCurrent.whosturn));

        }

        protected virtual void OnGameMoveUndo(ChessMove move)
        {
            var eh = this.GameMoveUndo;
            if (eh != null) { eh(this, new MoveEventArgs(move)); }
        }

        protected virtual void OnGameFinished()
        {
            var eh = this.GameFinished;
            if (eh != null) { eh(this, EventArgs.Empty); }
        }

        void player_OnResign(object sender, EventArgs e)
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
            OnGameFinished();
        }

        void player_OnKibitz(object sender, SearchProgressEventArgs e)
        {
            var eh = this.PlayerKibitzed;

            if (eh != null)
            {
                Player color = sender == _white ? Player.White : Player.Black;
                eh(this, new SearchProgressPlayerEventArgs(e.Progress, color));
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
        public TimeControl TimeControl
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
                        throw new Exception(string.Format("error initializing game start moves {0} is not a valid position from {1}", move.Description(_board), _board.FEN.ToString()));
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

                if (_board.WhosTurn == Player.White) { _result = ChessResult.BlackWins; }
                if (_board.WhosTurn == Player.Black) { _result = ChessResult.WhiteWins; }
                _resultReason = ChessResultReason.OutOfTime;

                OnGameFinished();
                return; //game over
            }
        }

        public void GameStart()
        {
            if (_started) { throw new Exception("game already started"); }
            _started = true;

            _timeRemainingForPlayer[(int)Player.White] = _timecontrol.InitialAmount;
            _timeRemainingForPlayer[(int)Player.Black] = _timecontrol.InitialAmount;
            _timeMoveStarted = DateTime.Now;

            OnGameStarted();

            PlayerWhosTurn.YourTurn(this._startFEN, new ChessMoves(this.MoveHistory()), this.TimeControl, this.ClockTime(FENCurrent.whosturn));
        }

        protected virtual void OnGameStarted()
        {
            var eh = this.GameStarted;
            if (eh != null) { eh(this, new EventArgs()); }
        }
        public ChessGamePlayer PlayerWhosTurn
        {
            get
            {
                return _board.WhosTurn == Player.White ? _white : _black;
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

        public TimeSpan ClockTime(Player a_player)
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
            if (!sender.Equals(_board.WhosTurn == Player.White ? _white : _black))
            {
                throw new Exception("it is not your turn");
            }
            if (!move.IsLegal(_board))
            {
                throw new Exception(string.Format("{0} is not a legal move from this position", move.Description()));
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
                _result = _board.WhosTurn == Player.White ? ChessResult.BlackWins : ChessResult.WhiteWins;
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

            OnGameMoveApplied(move);

            if (_result == null)
            {
                //game still going
                PlayerWhosTurn.YourTurn(this._startFEN, new ChessMoves(this.MoveHistory()), this.TimeControl, this.ClockTime(FENCurrent.whosturn));
            }
            else
            {
                //game is done.
                OnGameFinished();
            }

        }

        public virtual void OnGameMoveApplied(ChessMove move)
        {
            var ehGameMove = this.GameMoveApplied;
            if (ehGameMove != null) { ehGameMove(this, new MoveEventArgs(move)); }
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
