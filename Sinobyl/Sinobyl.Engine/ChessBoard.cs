using System;
using System.Collections.Generic;
using System.Text;

using System.Text.RegularExpressions;
using System.Collections;

namespace Sinobyl.Engine
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>

	public class ChessMoveHistory
	{
		public readonly ChessPosition From;
		public readonly ChessPosition To;
		public readonly ChessPiece PieceMoved;
		public readonly ChessPiece Promote;
		public readonly ChessPiece Captured;
		public readonly ChessPosition Enpassant;
		public readonly bool CastleWS;
		public readonly bool CastleWL;
		public readonly bool CastleBS;
		public readonly bool CastleBL;
		public readonly int FiftyCount;
		public readonly int MovesSinceNull;
		public readonly Int64 Zobrist;
		public readonly Int64 ZobristPawn;
		public ChessMoveHistory(ChessPosition from, ChessPosition to, ChessPiece piecemoved, ChessPiece promote, ChessPiece captured, ChessPosition enpassant, bool cws, bool cwl, bool cbs, bool cbl, int fifty, int sinceNull, Int64 zob, Int64 zobPawn)
		{
			this.From = from;
			this.To = to;
			this.PieceMoved = piecemoved;
			this.Promote = promote;
			this.Captured = captured;
			this.Enpassant = enpassant;
			this.CastleWS = cws;
			this.CastleWL = cwl;
			this.CastleBS = cbs;
			this.CastleBL = cbl;
			this.FiftyCount = fifty;
			this.MovesSinceNull = sinceNull;
			this.Zobrist = zob;
			this.ZobristPawn = zobPawn;
		}
	}

	public class ChessMoveHistoryCollection : List<ChessMoveHistory>
	{

	}

	public class ChessBoard
	{

        public class BoardChangedEventArgs : EventArgs
        {
            public List<BoardChangeEventItemRemoved> Removed = new List<BoardChangeEventItemRemoved>();
            public List<BoardChangeEventItemMoved> Moved = new List<BoardChangeEventItemMoved>();
            public List<BoardChangeEventItemAdded> Added = new List<BoardChangeEventItemAdded>();
            public List<BoardChangeEventItemChanged> Changed = new List<BoardChangeEventItemChanged>();
        }
        public struct BoardChangeEventItemRemoved
        {
            public readonly ChessPiece Piece;
            public readonly ChessPosition Position;
            public BoardChangeEventItemRemoved(ChessPiece piece, ChessPosition position)
            {
                Piece = piece;
                Position = position;
            }
        }
        public struct BoardChangeEventItemMoved
        {
            public readonly ChessPiece Piece;
            public readonly ChessPosition OldPosition;
            public readonly ChessPosition NewPosition;
            public BoardChangeEventItemMoved(ChessPiece piece, ChessPosition oldPosition, ChessPosition newPosition)
            {
                Piece = piece;
                OldPosition = oldPosition;
                NewPosition = newPosition;
            }
        }
        public struct BoardChangeEventItemAdded
        {
            public readonly ChessPiece Piece;
            public readonly ChessPosition Position;
            public BoardChangeEventItemAdded(ChessPiece piece, ChessPosition position)
            {
                Piece = piece;
                Position = position;
            }
        }
        public struct BoardChangeEventItemChanged
        {
            public readonly ChessPiece OldPiece;
            public readonly ChessPiece NewPiece;
            public readonly ChessPosition Position;
            public BoardChangeEventItemChanged(ChessPiece oldPiece, ChessPiece newPiece, ChessPosition position)
            {
                OldPiece = oldPiece;
                NewPiece = newPiece;
                Position = position;
            }
        }

        public event EventHandler<BoardChangedEventArgs> BoardChanged;

		//public event msgMove OnMoveApply;
		//public event msgVoid OnMoveUndo;
		//public event msgVoid OnReset;

        private ChessPositionDictionary<ChessPiece> _pieceat = new ChessPositionDictionary<ChessPiece>();
		//private ChessPiece[] _pieceat = new ChessPiece[65];

        private ChessPieceDictionary<int> _pieceCount = new ChessPieceDictionary<int>();
		//private int[] _pieceCount = new int[12];

        private ChessPlayerDictionary<ChessPosition> _kingpos = new ChessPlayerDictionary<ChessPosition>();

        //private ChessPosition[] _kingpos = new ChessPosition[2];

        private ChessPieceDictionary<ChessBitboard> _pieces = new ChessPieceDictionary<ChessBitboard>();
        //private ChessBitboard[] _pieces = new ChessBitboard[12];

        private ChessPlayerDictionary<ChessBitboard> _playerBoards = new ChessPlayerDictionary<ChessBitboard>();
        //private ChessBitboard[] _playerBoards = new ChessBitboard[2];
        
        private ChessBitboard _allPieces = 0;
        private Attacks.ChessBitboardRotatedVert _allPiecesVert = 0;
        private Attacks.ChessBitboardRotatedA1H8 _allPiecesA1H8 = 0;
        private Attacks.ChessBitboardRotatedH1A8 _allPiecesH1A8 = 0;

		private ChessPlayer _whosturn;
		private bool _castleWS;
		private bool _castleWL;
		private bool _castleBS;
		private bool _castleBL;
		private ChessPosition _enpassant;
		private int _fiftymove = 0;
		private int _fullmove = 0;
		private Int64 _zob;
		private Int64 _zobPawn;
		private ChessMoveHistoryCollection _hist = new ChessMoveHistoryCollection();
		private int _movesSinceNull = 100;

		public ChessBoard()
		{
			initPieceAtArray();
            this.FEN = new ChessFEN(ChessFEN.FENStart);
		}
		public ChessBoard(string fen)
		{
			initPieceAtArray();
			this.FEN = new ChessFEN(fen);
		}
		public ChessBoard(ChessFEN fen)
		{
			this.FEN = fen;
		}
		public ChessBoard(ChessFEN fen, IEnumerable<ChessMove> prevMoves)
		{
			this.FEN = fen;
			foreach (ChessMove move in prevMoves)
			{
				this.MoveApply(move);
			}
		}



		private void initPieceAtArray()
		{
			foreach (ChessPosition pos in Chess.AllPositions)
			{
				_pieceat[pos] = ChessPiece.EMPTY;
			}
			foreach(ChessPiece piece in Chess.AllPieces)
			{
                _pieceCount[piece] = 0;
                _pieces[piece] = 0;
			}
            _playerBoards[ChessPlayer.White] = 0;
            _playerBoards[ChessPlayer.Black] = 0;
            _allPieces = 0;
            _allPiecesVert = 0;
            _allPiecesA1H8 = 0;
            _allPiecesH1A8 = 0;
			
		}

		public ChessPosition EnPassant
		{
			get
			{
				return _enpassant;
			}
		}

		public ChessPosition KingPosition(ChessPlayer kingplayer)
		{
			return _kingpos[kingplayer];
		}
		public bool IsCheck()
		{
			return IsCheck(_whosturn);
		}
		public bool IsCheck(ChessPlayer kingplayer)
		{
			ChessPosition kingpos = KingPosition(kingplayer);
            return PositionAttacked(kingpos, kingplayer.PlayerOther());
		}

        private void PieceMove(ChessPosition from, ChessPosition to, BoardChangedEventArgs log)
        {
            ChessPiece piece = _pieceat[from];
            PieceRemove(from, null);
            PieceAdd(to, piece, null);

            if (log != null)
            {
                log.Moved.Add(new BoardChangeEventItemMoved(piece, from, to));
            }
        }
        private void PieceChange(ChessPosition pos, ChessPiece newPiece, BoardChangedEventArgs log)
        {
            if (log != null)
            {
                ChessPiece oldPiece = _pieceat[pos];
                log.Changed.Add(new BoardChangeEventItemChanged(oldPiece, newPiece, pos));
            }
            PieceRemove(pos, null);
            PieceAdd(pos, newPiece, null);
        }
        private void PieceAdd(ChessPosition pos, ChessPiece piece, BoardChangedEventArgs log)
		{
            if (log != null)
            {
                log.Added.Add(new BoardChangeEventItemAdded(piece, pos));
            }
            _pieceat[pos] = piece;
			_zob ^= ChessZobrist._piecepos[(int)piece, (int)pos];
			_pieceCount[piece]++;

            _pieces[piece] |= pos.Bitboard();
            _allPieces |= pos.Bitboard();
            _playerBoards[piece.PieceToPlayer()] |= pos.Bitboard();
            _allPiecesVert |= Attacks.RotateVert(pos);
            _allPiecesA1H8 |= Attacks.RotateA1H8(pos);
            _allPiecesH1A8 |= Attacks.RotateH1A8(pos);

			if (piece == ChessPiece.WPawn || piece == ChessPiece.BPawn)
			{
				_zobPawn ^= ChessZobrist._piecepos[(int)piece, (int)pos];
			}
			else if (piece == ChessPiece.WKing)
			{
				_kingpos[ChessPlayer.White] = pos;
			}
			else if (piece == ChessPiece.BKing)
			{
				_kingpos[ChessPlayer.Black] = pos;
			}
		}
		private void PieceRemove(ChessPosition pos, BoardChangedEventArgs log)
		{
            ChessPiece piece = PieceAt(pos);

            if (log != null)
            {
                log.Removed.Add(new BoardChangeEventItemRemoved(piece, pos));
            }
            
			_pieceat[pos] = ChessPiece.EMPTY;
			_zob ^= ChessZobrist._piecepos[(int)piece, (int)pos];
			_pieceCount[piece]--;

            _pieces[piece] &= ~pos.Bitboard();
            _allPieces &= ~pos.Bitboard();
            _playerBoards[piece.PieceToPlayer()] &= ~pos.Bitboard();
            _allPiecesVert &= ~Attacks.RotateVert(pos);
            _allPiecesA1H8 &= ~Attacks.RotateA1H8(pos);
            _allPiecesH1A8 &= ~Attacks.RotateH1A8(pos);

			if (piece == ChessPiece.WPawn || piece == ChessPiece.BPawn)
			{
				_zobPawn ^= ChessZobrist._piecepos[(int)piece, (int)pos];
			}
		}
		public int PieceCount(ChessPiece piece)
		{
			return _pieceCount[piece];
		}

		public bool IsMate()
		{
			if (IsCheck())
			{
				if (ChessMove.GenMovesLegal(this).Count == 0)
				{
					return true;
				}
			}
			return false;
		}

		public bool IsDrawByStalemate()
		{
			if (!IsCheck())
			{
				if (ChessMove.GenMovesLegal(this).Count == 0)
				{
					return true;
				}
			}
			return false;
		}
		public bool IsDrawBy50MoveRule()
		{
			return (_fiftymove >= 100);
		}
		public bool IsDrawByRepetition()
		{
			Int64 currzob = this.Zobrist;
			int repcount = 1;
			for (int i = _hist.Count - 1; i >= 0; i--)
			{
				ChessMoveHistory movehist = _hist[i];
				if (movehist.Zobrist == currzob)
				{
					repcount++;
				}
				if (repcount >= 3)
				{
					return true;
				}
				if (movehist.Captured != ChessPiece.EMPTY)
				{
					break;
				}
				if (movehist.PieceMoved == ChessPiece.WPawn || movehist.PieceMoved == ChessPiece.BPawn)
				{
					break;
				}
			}
			return false;
		}
		public int FiftyMovePlyCount
		{
			get
			{
				return _fiftymove;
			}
		}
		public int FullMoveCount
		{
			get
			{
				return _fullmove;
			}
		}


		public List<ChessPosition> PieceList(ChessPiece piece)
		{
			List<ChessPosition> retval = new List<ChessPosition>();
			foreach (ChessPosition pos in Chess.AllPositions)
			{
				if (PieceAt(pos) == piece)
				{
					retval.Add(pos);
				}
			}
			return retval;
		}

		public ChessFEN FEN
		{
			get
			{
				return new ChessFEN(this);
			}
			set
			{
                //initialize event handler and log.
                var handler = this.BoardChanged;
                BoardChangedEventArgs log = null;
                if (handler != null)
                {
                    log = new BoardChangedEventArgs();
                }

                //clear all existing pieces.
                foreach (var pos in Chess.AllPositions)
                {
                    if (_pieceat[pos] != ChessPiece.EMPTY)
                    {
                        this.PieceRemove(pos, log);
                    }
                }

                //reset board
				this._hist.Clear();
				initPieceAtArray();

                //add pieces
				foreach(var pos in Chess.AllPositions)
				{
                    if (value.pieceat[pos.GetIndex64()] != ChessPiece.EMPTY)
					{
						this.PieceAdd(pos,value.pieceat[pos.GetIndex64()], log);
					}
				}
				_castleWS = value.castleWS;
				_castleWL = value.castleWL;
				_castleBS = value.castleBS;
				_castleBL = value.castleBL;
				_whosturn = value.whosturn;
				_enpassant = value.enpassant;
				_fiftymove = value.fiftymove;
				_fullmove = value.fullmove;
				_zob = ChessZobrist.BoardZob(this);
				_zobPawn = ChessZobrist.BoardZobPawn(this);

                //raise changed event.
                if (handler != null)
                {
                    handler(this, log);
                }
			}
		}
		//public string FenString
		//{
		//    set
		//    {
		//        this.FEN = new ChessFEN(value);
		//    }
		//    get
		//    {
		//        return this.FEN.ToString();
		//    }
		//}
		public Int64 Zobrist
		{
			get
			{
				return _zob;
			}
		}
		public Int64 ZobristPawn
		{
			get
			{
				return _zobPawn;
			}
		}
        public ChessBitboard PieceLocations(ChessPiece piece)
        {
            return _pieces[piece];
        }
        public ChessBitboard PlayerLocations(ChessPlayer player)
        {
            return _playerBoards[player];
        }
        public ChessBitboard PieceLocationsAll
        {
            get
            {
                return _allPieces;
            }
        }
        public Attacks.ChessBitboardRotatedVert PieceLocationsAllVert
        {
            get
            {
                return _allPiecesVert;
            }
        }
        public Attacks.ChessBitboardRotatedA1H8 PieceLocationsAllA1H8
        {
            get
            {
                return _allPiecesA1H8;
            }
        }

        public Attacks.ChessBitboardRotatedH1A8 PieceLocationsAllH1A8
        {
            get
            {
                return _allPiecesH1A8;
            }
        }
        
        public ChessPiece PieceAt(ChessPosition pos)
		{
			return _pieceat[pos];
		}
		private void Reset()
		{
			foreach (ChessPosition pos in Chess.AllPositions)
			{
				_pieceat[pos] = ChessPiece.EMPTY;
			}
			_whosturn = ChessPlayer.White;
			_castleWS = false;
			_castleWL = false;
			_castleBS = false;
			_castleBL = false;
			_enpassant = (ChessPosition.OUTOFBOUNDS);
			_fiftymove = 0;
			_fullmove = 1;
			_zob = 0;
			_zobPawn = 0;
		}
		
		public bool CastleAvailWS
		{
			get
			{
				return _castleWS;
			}
		}
		public bool CastleAvailWL
		{
			get
			{
				return _castleWL;
			}
		}
		public bool CastleAvailBS
		{
			get
			{
				return _castleBS;
			}
		}
		public bool CastleAvailBL
		{
			get
			{
				return _castleBL;
			}
		}
		public ChessPlayer WhosTurn
		{
			get
			{
				return _whosturn;
			}
		}

		public void MoveApply(string movedesc)
		{
			ChessMove move = new ChessMove(this, movedesc);
			this.MoveApply(move);
		}

		public void MoveApply(ChessMove move)
		{
            //initialize event handler and log.
            var handler = this.BoardChanged;
            BoardChangedEventArgs log = null;
            if (handler != null)
            {
                log = new BoardChangedEventArgs();
            }

			ChessPosition from = move.From;
			ChessPosition to = move.To;
			ChessPiece piece = this.PieceAt(from);
			ChessPiece capture = this.PieceAt(to);
			ChessPiece promote = move.Promote;
            ChessRank fromrank = from.GetRank();
			ChessRank torank = to.GetRank();
			ChessFile fromfile = from.GetFile();
			ChessFile tofile = to.GetFile();

			ChessMoveHistory histobj = new ChessMoveHistory(from, to, piece, promote, capture, _enpassant, _castleWS, _castleWL, _castleBS, _castleBL, _fiftymove,_movesSinceNull, _zob, _zobPawn);
			_hist.Add(histobj);

			//increment since null count;
			_movesSinceNull++;

			//remove captured piece
			if (capture != ChessPiece.EMPTY)
			{
				this.PieceRemove(to, log);
			}

            //move piece, promote if needed
            this.PieceMove(from, to, log);
            if (promote != ChessPiece.EMPTY)
            {
                this.PieceChange(to, promote, log);
            }
			
			//if castle, move rook
			if (piece == ChessPiece.WKing && from == ChessPosition.E1 && to == ChessPosition.G1)
			{
                this.PieceMove(ChessPosition.H1, ChessPosition.F1, log);
			}
			if (piece == ChessPiece.WKing && from == ChessPosition.E1 && to == ChessPosition.C1)
			{
                this.PieceMove(ChessPosition.A1, ChessPosition.D1, log);
			}
			if (piece == ChessPiece.BKing && from == ChessPosition.E8 && to == ChessPosition.G8)
			{
                this.PieceMove(ChessPosition.H8, ChessPosition.F8, log);
			}
			if (piece == ChessPiece.BKing && from == ChessPosition.E8 && to == ChessPosition.C8)
			{
                this.PieceMove(ChessPosition.A8, ChessPosition.D8, log);
			}

			//mark unavailability of castling
			if (piece == ChessPiece.WKing)
			{
				if (this._castleWS)
				{
					this._castleWS = false;
					_zob ^= ChessZobrist._castleWS;
				}
				if (this._castleWL)
				{
					this._castleWL = false;
					_zob ^= ChessZobrist._castleWL;
				}
			}
			if (piece == ChessPiece.BKing)
			{
				if (this._castleBS)
				{
					this._castleBS = false;
					_zob ^= ChessZobrist._castleBS;
				}
				if (this._castleBL)
				{
					this._castleBL = false;
					_zob ^= ChessZobrist._castleBL;
				}
			}
			if (from == ChessPosition.H1 && this._castleWS)
			{
				this._castleWS = false;
				_zob ^= ChessZobrist._castleWS;
			}
			if (from == ChessPosition.A1 && this._castleWL)
			{
				this._castleWL = false;
				_zob ^= ChessZobrist._castleWL;
			}
			if (from == ChessPosition.H8 && this._castleBS)
			{
				this._castleBS = false;
				_zob ^= ChessZobrist._castleBS;
			}
			if (from == ChessPosition.A8 && this._castleBL)
			{
				this._castleBL = false;
				_zob ^= ChessZobrist._castleBL;
			}



			//if enpassant move then remove captured pawn
			if (piece == ChessPiece.WPawn && to == this._enpassant)
			{
				this.PieceRemove(tofile.ToPosition(ChessRank.Rank5), log);
			}
			if (piece == ChessPiece.BPawn && to == this._enpassant)
			{
				this.PieceRemove(tofile.ToPosition(ChessRank.Rank4), log);
			}

			//unmark enpassant sq
			if (_enpassant.IsInBounds())
			{
				_zob ^= ChessZobrist._enpassant[(int)_enpassant];
				_enpassant = (ChessPosition.OUTOFBOUNDS);
			}

			//mark enpassant sq if pawn double jump
			if (piece == ChessPiece.WPawn && fromrank == ChessRank.Rank2 && torank == ChessRank.Rank4)
			{
				_enpassant = fromfile.ToPosition(ChessRank.Rank3);
				_zob ^= ChessZobrist._enpassant[(int)_enpassant];
			}
			else if (piece == ChessPiece.BPawn && fromrank == ChessRank.Rank7 && torank == ChessRank.Rank5)
			{
				_enpassant = fromfile.ToPosition(ChessRank.Rank6);
				_zob ^= ChessZobrist._enpassant[(int)_enpassant];
			}

			//increment the move count
			if (_whosturn == ChessPlayer.Black)
			{
				_fullmove++;
			}
			if (piece != ChessPiece.WPawn && piece != ChessPiece.BPawn && capture == ChessPiece.EMPTY)
			{
				_fiftymove++;
			}
			else
			{
				_fiftymove = 0;
			}

			//switch whos turn
            _whosturn = _whosturn.PlayerOther();
			_zob ^= ChessZobrist._player;

            if (handler != null)
            {
                handler(this, log);
            }
		}

		public int HistoryCount
		{
			get
			{
				return _hist.Count;
			}
		}
		public ChessMoves HistoryMoves
		{
			get
			{
				ChessMoves retval = new ChessMoves();
				foreach (ChessMoveHistory hist in _hist)
				{
					retval.Add(new ChessMove(hist.From, hist.To, hist.Promote));
				}
				return retval;
			}
		}
		public ChessMove HistMove(int MovesAgo)
		{
			ChessMoveHistory hist = _hist[_hist.Count - MovesAgo];
			return new ChessMove(hist.From, hist.To, hist.Promote);
		}

		public void MoveUndo()
		{
            //initialize event handler and log.
            var handler = this.BoardChanged;
            BoardChangedEventArgs log = null;
            if (handler != null)
            {
                log = new BoardChangedEventArgs();
            }

            //undo move history
			ChessMoveHistory movehist = _hist[_hist.Count - 1];
			_hist.RemoveAt(_hist.Count - 1);

            //undo promotion
            if (movehist.Promote != ChessPiece.EMPTY)
            {
                PieceChange(movehist.To, movehist.PieceMoved, log);
            }
			
            //move piece to it's original location
            PieceMove(movehist.To, movehist.From, log);



			//replace the captured piece
			if (movehist.Captured != ChessPiece.EMPTY)
			{
				PieceAdd(movehist.To, movehist.Captured, log);
			}

			//move rook back if castle
			if (movehist.PieceMoved == ChessPiece.WKing && movehist.From == ChessPosition.E1 && movehist.To == ChessPosition.G1)
			{
                this.PieceMove(ChessPosition.F1, ChessPosition.H1, log);
			}
			if (movehist.PieceMoved == ChessPiece.WKing && movehist.From == ChessPosition.E1 && movehist.To == ChessPosition.C1)
			{
                this.PieceMove(ChessPosition.D1, ChessPosition.A1, log);
			}
			if (movehist.PieceMoved == ChessPiece.BKing && movehist.From == ChessPosition.E8 && movehist.To == ChessPosition.G8)
			{
                this.PieceMove(ChessPosition.F8, ChessPosition.H8, log);
			}
			if (movehist.PieceMoved == ChessPiece.BKing && movehist.From == ChessPosition.E8 && movehist.To == ChessPosition.C8)
			{
                this.PieceMove(ChessPosition.D8, ChessPosition.A8, log);
			}

			//put back pawn if enpassant capture
			if (movehist.PieceMoved == ChessPiece.WPawn && movehist.To == movehist.Enpassant)
			{
                ChessFile tofile = movehist.To.GetFile();
                PieceAdd(tofile.ToPosition(ChessRank.Rank5), ChessPiece.BPawn, log);
			}
			if (movehist.PieceMoved == ChessPiece.BPawn && movehist.To == movehist.Enpassant)
			{
				ChessFile tofile = movehist.To.GetFile();
				PieceAdd(tofile.ToPosition(ChessRank.Rank4), ChessPiece.WPawn, log);
			}

			_castleWS = movehist.CastleWS;
			_castleWL = movehist.CastleWL;
			_castleBS = movehist.CastleBS;
			_castleBL = movehist.CastleBL;
			this._enpassant = movehist.Enpassant;
			this._fiftymove = movehist.FiftyCount;
			this._movesSinceNull = movehist.MovesSinceNull;

			if (_whosturn == ChessPlayer.White)
			{
				_fullmove--;
			}

            _whosturn = _whosturn.PlayerOther();
			_zob = movehist.Zobrist;
			_zobPawn = movehist.ZobristPawn;

            if (handler != null)
            {
                handler(this, log);
            }

		}

		public void MoveNullApply()
		{
			//save move history
            ChessMoveHistory histobj = new ChessMoveHistory((ChessPosition.OUTOFBOUNDS), (ChessPosition.OUTOFBOUNDS), (ChessPiece.EMPTY), (ChessPiece.EMPTY), (ChessPiece.EMPTY), _enpassant, _castleWS, _castleWL, _castleBS, _castleBL, _fiftymove, _movesSinceNull, _zob, _zobPawn);
			_hist.Add(histobj);

			//reset since null count;
			_movesSinceNull = 0;

			//unmark enpassant sq
			if (_enpassant.IsInBounds())
			{
				_zob ^= ChessZobrist._enpassant[(int)_enpassant];
				_enpassant = (ChessPosition.OUTOFBOUNDS);
			}

			//switch whos turn
            _whosturn = _whosturn.PlayerOther();
			_zob ^= ChessZobrist._player;

            var handler = this.BoardChanged;
            if (handler != null)
            {
                handler(this, new BoardChangedEventArgs());
            }

		}
		public void MoveNullUndo()
		{
			ChessMoveHistory movehist = _hist[_hist.Count - 1];
			_hist.RemoveAt(_hist.Count - 1);

			_castleWS = movehist.CastleWS;
			_castleWL = movehist.CastleWL;
			_castleBS = movehist.CastleBS;
			_castleBL = movehist.CastleBL;
			this._enpassant = movehist.Enpassant;
			this._fiftymove = movehist.FiftyCount;
			this._movesSinceNull = movehist.MovesSinceNull;

            _whosturn = _whosturn.PlayerOther();
			_zob = movehist.Zobrist;
			_zobPawn = movehist.ZobristPawn;

            var handler = this.BoardChanged;
            if (handler != null)
            {
                handler(this, new BoardChangedEventArgs());
            }
		}

		public int MovesSinceNull
		{
			get
			{
				return this._movesSinceNull;
			}
		}
		public bool LastTwoMovesNull()
		{
			return MovesSinceNull == 0 
				&& this._hist.Count>=2 
				&& this._hist[_hist.Count - 2].From == (ChessPosition.OUTOFBOUNDS);
		}

		public ChessPiece PieceInDirection(ChessPosition from, ChessDirection dir, out ChessPosition pos)
		{
			int i = 0;
			return PieceInDirection(from, dir, out pos, out i);
		}
		public ChessPiece PieceInDirection(ChessPosition from, ChessDirection dir, out ChessPosition pos, out int dist)
		{

			ChessPiece piece;
            pos = from.PositionInDirection(dir);
			dist = 1;
			while (pos.IsInBounds())
			{
				piece = this.PieceAt(pos);
				if (piece != ChessPiece.EMPTY) { return piece; }

				dist++;
				if (dir.IsDirectionKnight())
				{
					break;
				}
				else
				{
                    pos = pos.PositionInDirection(dir);
				}
			}
			return ChessPiece.EMPTY;
		}
        public ChessBitboard AttacksTo(ChessPosition to, ChessPlayer by)
		{
            return AttacksTo(to) & this.PlayerLocations(by);
		}
        public ChessBitboard AttacksTo(ChessPosition to)
		{
            ChessBitboard retval = 0;
            retval |= Attacks.KnightAttacks(to) & (this.PieceLocations(ChessPiece.WKnight) | this.PieceLocations(ChessPiece.BKnight));
            retval |= Attacks.RookAttacks(to, this.PieceLocationsAll, this.PieceLocationsAllVert) & (this.PieceLocations(ChessPiece.WQueen) | this.PieceLocations(ChessPiece.BQueen) | this.PieceLocations(ChessPiece.WRook) | this.PieceLocations(ChessPiece.BRook));
            retval |= Attacks.BishopAttacks(to, this.PieceLocationsAllA1H8, this.PieceLocationsAllH1A8) & (this.PieceLocations(ChessPiece.WQueen) | this.PieceLocations(ChessPiece.BQueen) | this.PieceLocations(ChessPiece.WBishop) | this.PieceLocations(ChessPiece.BBishop));
            retval |= Attacks.KingAttacks(to) & (this.PieceLocations(ChessPiece.WKing) | this.PieceLocations(ChessPiece.BKing));
            retval |= Attacks.PawnAttacksBlack(to) & this.PieceLocations(ChessPiece.WPawn);
            retval |= Attacks.PawnAttacksWhite(to) & this.PieceLocations(ChessPiece.BPawn);
            return retval;
		}

		public bool PositionAttacked(ChessPosition to, ChessPlayer byPlayer)
		{
            return !AttacksTo(to, byPlayer).Empty();
		}


	}
}
