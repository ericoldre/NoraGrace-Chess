using System;
using System.Collections.Generic;
using System.Text;

using System.Text.RegularExpressions;
using System.Collections;
using System.Linq;

namespace Sinobyl.Engine
{
    [Flags]
    public enum CastleFlags
    {
        WhiteShort = 1,
        WhiteLong = 2, 
        BlackShort = 4,
        BlackLong = 8,
        All = WhiteShort | WhiteLong | BlackShort | BlackLong
    }

	public class ChessMoveHistory
	{
        public ChessMove Move;
        public ChessPiece PieceMoved;
		public ChessPiece Captured;
		public ChessPosition Enpassant;
        public CastleFlags Castle;
		public int FiftyCount;
		public int MovesSinceNull;
		public Int64 Zobrist;
		public Int64 ZobristPawn;
        public Int64 ZobristMaterial;

        public void Reset(ChessMove move, ChessPiece piecemoved, ChessPiece captured, ChessPosition enpassant, CastleFlags castle, int fifty, int sinceNull, Int64 zob, Int64 zobPawn, Int64 zobMaterial)
        {
            this.Move = move;
            this.PieceMoved = piecemoved;
            this.Captured = captured;
            this.Enpassant = enpassant;
            this.Castle = castle;
            this.FiftyCount = fifty;
            this.MovesSinceNull = sinceNull;
            this.Zobrist = zob;
            this.ZobristPawn = zobPawn;
            this.ZobristMaterial = zobMaterial;
        }
	}


	public sealed class ChessBoard
    {

        //#region changed event related

        //public class BoardChangedEventArgs : EventArgs
        //{
        //    public List<BoardChangeEventItemRemoved> Removed = new List<BoardChangeEventItemRemoved>();
        //    public List<BoardChangeEventItemMoved> Moved = new List<BoardChangeEventItemMoved>();
        //    public List<BoardChangeEventItemAdded> Added = new List<BoardChangeEventItemAdded>();
        //    public List<BoardChangeEventItemChanged> Changed = new List<BoardChangeEventItemChanged>();
        //}

        //public struct BoardChangeEventItemRemoved
        //{
        //    public readonly ChessPiece Piece;
        //    public readonly ChessPosition Position;
        //    public BoardChangeEventItemRemoved(ChessPiece piece, ChessPosition position)
        //    {
        //        Piece = piece;
        //        Position = position;
        //    }
        //}

        //public struct BoardChangeEventItemMoved
        //{
        //    public readonly ChessPiece Piece;
        //    public readonly ChessPosition OldPosition;
        //    public readonly ChessPosition NewPosition;
        //    public BoardChangeEventItemMoved(ChessPiece piece, ChessPosition oldPosition, ChessPosition newPosition)
        //    {
        //        Piece = piece;
        //        OldPosition = oldPosition;
        //        NewPosition = newPosition;
        //    }
        //}

        //public struct BoardChangeEventItemAdded
        //{
        //    public readonly ChessPiece Piece;
        //    public readonly ChessPosition Position;
        //    public BoardChangeEventItemAdded(ChessPiece piece, ChessPosition position)
        //    {
        //        Piece = piece;
        //        Position = position;
        //    }
        //}

        //public struct BoardChangeEventItemChanged
        //{
        //    public readonly ChessPiece OldPiece;
        //    public readonly ChessPiece NewPiece;
        //    public readonly ChessPosition Position;
        //    public BoardChangeEventItemChanged(ChessPiece oldPiece, ChessPiece newPiece, ChessPosition position)
        //    {
        //        OldPiece = oldPiece;
        //        NewPiece = newPiece;
        //        Position = position;
        //    }
        //}

        ////public event EventHandler<BoardChangedEventArgs> BoardChanged;

        //#endregion


        //private ChessPositionDictionary<ChessPiece> _pieceat = new ChessPositionDictionary<ChessPiece>();
		private ChessPiece[] _pieceat = new ChessPiece[65];

        //private ChessPieceDictionary<int> _pieceCount = new ChessPieceDictionary<int>();
		private int[] _pieceCount = new int[ChessPieceInfo.LookupArrayLength];

        //private ChessPlayerDictionary<ChessPosition> _kingpos = new ChessPlayerDictionary<ChessPosition>();
        private ChessPosition[] _kingpos = new ChessPosition[2];

        //private ChessPieceDictionary<ChessBitboard> _pieces = new ChessPieceDictionary<ChessBitboard>();
        private ChessBitboard[] _pieces = new ChessBitboard[ChessPieceInfo.LookupArrayLength];

        //private ChessPieceTypeDictionary<ChessBitboard> _pieceTypes = new ChessPieceTypeDictionary<ChessBitboard>();
        private ChessBitboard[] _pieceTypes = new ChessBitboard[ChessPieceTypeInfo.LookupArrayLength];

        //private ChessPlayerDictionary<ChessBitboard> _playerBoards = new ChessPlayerDictionary<ChessBitboard>();
        private ChessBitboard[] _playerBoards = new ChessBitboard[2];
        
        private ChessBitboard _allPieces = 0;
        //private Attacks.ChessBitboardRotatedVert _allPiecesVert = 0;
        //private Attacks.ChessBitboardRotatedA1H8 _allPiecesA1H8 = 0;
        //private Attacks.ChessBitboardRotatedH1A8 _allPiecesH1A8 = 0;

		private ChessPlayer _whosturn;
        private CastleFlags _castleFlags;
		private ChessPosition _enpassant;
		private int _fiftymove = 0;
		private int _fullmove = 0;
		private Int64 _zob;
		private Int64 _zobPawn;
        private Int64 _zobMaterial;

        private ChessMoveHistory[] _hist = new ChessMoveHistory[25];
        private int _histUB;
        private int _histCount = 0;

		private int _movesSinceNull = 100;

        private readonly ChessEval _pcSqEvaluator;
        private int _pcSqStart;
        private int _pcSqEnd;

        public ChessBoard(ChessEval pcSqEvaluator = null)
            : this(new ChessFEN(ChessFEN.FENStart), pcSqEvaluator)
		{
            
		}
        public ChessBoard(string fen, ChessEval pcSqEvaluator = null)
            : this(new ChessFEN(fen), pcSqEvaluator)
		{

		}
		public ChessBoard(ChessFEN fen, ChessEval pcSqEvaluator = null)
		{
            _histUB = _hist.GetUpperBound(0);
            for (int i = 0; i <= _histUB; i++)
            {
                _hist[i] = new ChessMoveHistory();
            }

            _pcSqEvaluator = pcSqEvaluator ?? ChessEval.Default;
            initPieceAtArray();

			this.FEN = fen;
		}

        public ChessBoard(ChessFEN fen, IEnumerable<ChessMove> prevMoves, ChessEval pcSqEvaluator = null)
            : this(fen, pcSqEvaluator)
        {
            foreach (ChessMove move in prevMoves)
            {
                this.MoveApply(move);
            }
        }



		private void initPieceAtArray()
		{
            foreach (ChessPosition pos in ChessPositionInfo.AllPositions)
			{
				_pieceat[(int)pos] = ChessPiece.EMPTY;
			}
            foreach (ChessPiece piece in ChessPieceInfo.AllPieces)
			{
                _pieceCount[(int)piece] = 0;
                _pieces[(int)piece] = 0;
			}
            foreach (ChessPieceType piecetype in ChessPieceTypeInfo.AllPieceTypes)
            {
                _pieceTypes[(int)piecetype] = ChessBitboard.Empty;
            }
            _playerBoards[(int)ChessPlayer.White] = 0;
            _playerBoards[(int)ChessPlayer.Black] = 0;
            _allPieces = 0;

            _pcSqStart = 0;
            _pcSqEnd = 0;
			
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
            return _kingpos[(int)kingplayer];
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

        private void PieceMove(ChessPosition from, ChessPosition to)
        {
            ChessPiece piece = _pieceat[(int)from];
            PieceRemove(from);
            PieceAdd(to, piece);
        }

        private void PieceChange(ChessPosition pos, ChessPiece newPiece)
        {
            PieceRemove(pos);
            PieceAdd(pos, newPiece);
        }

        private void PieceAdd(ChessPosition pos, ChessPiece piece)
		{
            _pieceat[(int)pos] = piece;
            _zob ^= ChessZobrist.PiecePosition(piece, pos);
            _zobMaterial ^= ChessZobrist.Material(piece, _pieceCount[(int)piece]);
            _pieceCount[(int)piece]++;
            _pcSqEvaluator.PcSqValuesAdd(piece, pos, ref _pcSqStart, ref _pcSqEnd);

            ChessBitboard posBits = pos.Bitboard();
            _pieces[(int)piece] |= posBits;
            _pieceTypes[(int)piece.ToPieceType()] |= posBits;
            _allPieces |= posBits;
            _playerBoards[(int)piece.PieceToPlayer()] |= posBits;

			if (piece == ChessPiece.WPawn || piece == ChessPiece.BPawn)
			{
                _zobPawn ^= ChessZobrist.PiecePosition(piece, pos);
			}
			else if (piece == ChessPiece.WKing)
			{
                _kingpos[(int)ChessPlayer.White] = pos;
			}
			else if (piece == ChessPiece.BKing)
			{
                _kingpos[(int)ChessPlayer.Black] = pos;
			}
		}
        private void PieceRemove(ChessPosition pos)
		{
            ChessPiece piece = PieceAt(pos);

            _pieceat[(int)pos] = ChessPiece.EMPTY;
			_zob ^= ChessZobrist.PiecePosition(piece, pos);
            _zobMaterial ^= ChessZobrist.Material(piece, _pieceCount[(int)piece] - 1);
            _pieceCount[(int)piece]--;
            _pcSqEvaluator.PcSqValuesRemove(piece, pos, ref _pcSqStart, ref _pcSqEnd);

            ChessBitboard notPosBits = ~pos.Bitboard();
            _pieces[(int)piece] &= notPosBits;
            _pieceTypes[(int)piece.ToPieceType()] &= notPosBits;
            _allPieces &= notPosBits;
            _playerBoards[(int)piece.PieceToPlayer()] &= notPosBits;

			if (piece == ChessPiece.WPawn || piece == ChessPiece.BPawn)
			{
                _zobPawn ^= ChessZobrist.PiecePosition(piece, pos); 
			}
		}
		public int PieceCount(ChessPiece piece)
		{
            return _pieceCount[(int)piece];
		}
        public int PieceCount(ChessPlayer player, ChessPieceType pieceType)
        {
            return _pieceCount[(int)pieceType.ForPlayer(player)];
        }

        public int PcSqValueStart
        {
            get { return _pcSqStart; }
        }

        public int PcSqValueEnd
        {
            get { return _pcSqEnd; }
        }

        public ChessEval PcSqEvaluator
        {
            get { return _pcSqEvaluator; }
        }

		public bool IsMate()
		{
			if (IsCheck())
			{
				if (!ChessMove.GenMovesLegal(this).Any())
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
				if (!ChessMove.GenMovesLegal(this).Any())
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
			for (int i = _histCount - 1; i >= 0; i--)
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
            foreach (ChessPosition pos in ChessPositionInfo.AllPositions)
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

                //clear all existing pieces.
                foreach (var pos in ChessPositionInfo.AllPositions)
                {
                    if (_pieceat[(int)pos] != ChessPiece.EMPTY)
                    {
                        this.PieceRemove(pos);
                    }
                }

                //reset board
                _histCount = 0;
				initPieceAtArray();

                //add pieces
                foreach (var pos in ChessPositionInfo.AllPositions)
				{
                    if (value.pieceat[pos.GetIndex64()] != ChessPiece.EMPTY)
					{
						this.PieceAdd(pos,value.pieceat[(int)pos]);
					}
				}

                _castleFlags = 0;
                _castleFlags |= value.castleWS ? CastleFlags.WhiteShort : 0;
                _castleFlags |= value.castleWL ? CastleFlags.WhiteLong : 0;
                _castleFlags |= value.castleBS ? CastleFlags.BlackShort : 0;
                _castleFlags |= value.castleBL ? CastleFlags.BlackLong : 0;

				_whosturn = value.whosturn;
				_enpassant = value.enpassant;
				_fiftymove = value.fiftymove;
				_fullmove = value.fullmove;
				_zob = ChessZobrist.BoardZob(this);
				_zobPawn = ChessZobrist.BoardZobPawn(this);
                _zobMaterial = ChessZobrist.BoardZobMaterial(this);

			}
		}

		public Int64 Zobrist
		{
            get { return _zob; }
		}

		public Int64 ZobristPawn
		{
            get { return _zobPawn; }
		}

        public Int64 ZobristMaterial
        {
            get { return _zobMaterial; }
        }

        public ChessBitboard this[ChessPiece piece]
        {
            get { return _pieces[(int)piece]; }
        }

        public ChessBitboard this[ChessPieceType pieceType]
        {
            get { return _pieceTypes[(int)pieceType]; }
        }

        public ChessBitboard this[ChessPlayer player]
        {
            get
            {
                return _playerBoards[(int)player];
            }
        }

        public ChessBitboard PieceLocationsAll
        {
            get
            {
                return _allPieces;
            }
        }

        public ChessPiece PieceAt(ChessPosition pos)
		{
            return _pieceat[(int)pos];
		}

        public CastleFlags CastleRights
        {
            get { return _castleFlags; }
        }
		
		public ChessPlayer WhosTurn
		{
			get
			{
				return _whosturn;
			}
		}

        private void HistResize()
        {
            if (_histCount > _histUB)
            {
                Array.Resize(ref _hist, (_histCount + 10) * 2);
                _histUB = _hist.GetUpperBound(0);
                for (int i = _histCount; i <= _histUB; i++)
                {
                    _hist[i] = new ChessMoveHistory();
                }
                _histUB = _hist.GetUpperBound(0);
            }
        }

		public void MoveApply(string movedesc)
		{
			ChessMove move = new ChessMove(this, movedesc);
			this.MoveApply(move);
		}

		public void MoveApply(ChessMove move)
		{

			ChessPosition from = move.From;
			ChessPosition to = move.To;
			ChessPiece piece = this.PieceAt(from);
			ChessPiece capture = this.PieceAt(to);
			ChessPiece promote = move.Promote;
            ChessRank fromrank = from.GetRank();
			ChessRank torank = to.GetRank();
			ChessFile fromfile = from.GetFile();
			ChessFile tofile = to.GetFile();

            if (_histCount > _histUB) { HistResize(); }
			_hist[_histCount++].Reset(move, piece, capture, _enpassant, _castleFlags, _fiftymove,_movesSinceNull, _zob, _zobPawn, _zobMaterial);

			//increment since null count;
			_movesSinceNull++;

			//remove captured piece
			if (capture != ChessPiece.EMPTY)
			{
				this.PieceRemove(to);
			}

            //move piece, promote if needed
            this.PieceMove(from, to);
            if (promote != ChessPiece.EMPTY)
            {
                this.PieceChange(to, promote);
            }
			
			//if castle, move rook
			if (piece == ChessPiece.WKing && from == ChessPosition.E1 && to == ChessPosition.G1)
			{
                this.PieceMove(ChessPosition.H1, ChessPosition.F1);
			}
			if (piece == ChessPiece.WKing && from == ChessPosition.E1 && to == ChessPosition.C1)
			{
                this.PieceMove(ChessPosition.A1, ChessPosition.D1);
			}
			if (piece == ChessPiece.BKing && from == ChessPosition.E8 && to == ChessPosition.G8)
			{
                this.PieceMove(ChessPosition.H8, ChessPosition.F8);
			}
			if (piece == ChessPiece.BKing && from == ChessPosition.E8 && to == ChessPosition.C8)
			{
                this.PieceMove(ChessPosition.A8, ChessPosition.D8);
			}

			//mark unavailability of castling
            if(_castleFlags != 0)
            {
                if (((_castleFlags & CastleFlags.WhiteShort) != 0) 
                    && (piece == ChessPiece.WKing || from == ChessPosition.H1))
                {
                    _castleFlags &= ~CastleFlags.WhiteShort;
                    _zob ^= ChessZobrist.CastleWS;
                }

                if (((_castleFlags & CastleFlags.WhiteLong) != 0)
                    && (piece == ChessPiece.WKing || from == ChessPosition.A1))
                {
                    _castleFlags &= ~CastleFlags.WhiteLong;
                    _zob ^= ChessZobrist.CastleWL;
                }

                if (((_castleFlags & CastleFlags.BlackShort) != 0)
                    && (piece == ChessPiece.BKing || from == ChessPosition.H8))
                {
                    _castleFlags &= ~CastleFlags.BlackShort;
                    _zob ^= ChessZobrist.CastleBS;
                }

                if (((_castleFlags & CastleFlags.BlackLong) != 0)
                    && (piece == ChessPiece.BKing || from == ChessPosition.A8))
                {
                    _castleFlags &= ~CastleFlags.BlackLong;
                    _zob ^= ChessZobrist.CastleBL;
                }
            }



			//if enpassant move then remove captured pawn
			if (piece == ChessPiece.WPawn && to == this._enpassant)
			{
                this.PieceRemove(tofile.ToPosition(ChessRank.Rank5));
			}
			if (piece == ChessPiece.BPawn && to == this._enpassant)
			{
                this.PieceRemove(tofile.ToPosition(ChessRank.Rank4));
			}

			//unmark enpassant sq
			if (_enpassant.IsInBounds())
			{
				_zob ^= ChessZobrist.Enpassant(_enpassant);
				_enpassant = (ChessPosition.OUTOFBOUNDS);
			}

			//mark enpassant sq if pawn double jump
			if (piece == ChessPiece.WPawn && fromrank == ChessRank.Rank2 && torank == ChessRank.Rank4)
			{
				_enpassant = fromfile.ToPosition(ChessRank.Rank3);
                _zob ^= ChessZobrist.Enpassant(_enpassant);
			}
			else if (piece == ChessPiece.BPawn && fromrank == ChessRank.Rank7 && torank == ChessRank.Rank5)
			{
				_enpassant = fromfile.ToPosition(ChessRank.Rank6);
                _zob ^= ChessZobrist.Enpassant(_enpassant);
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
			_zob ^= ChessZobrist.Player;

		}

		public int HistoryCount
		{
			get
			{
                return _histCount;
			}
		}
		public ChessMoves HistoryMoves
		{
			get
			{
				ChessMoves retval = new ChessMoves();
                for (int i = 0; i < _histCount; i++)
                {
                    retval.Add(_hist[i].Move);
                }
				return retval;
			}
		}
		public ChessMove HistMove(int MovesAgo)
		{
            return _hist[_histCount - MovesAgo].Move;
		}

		public void MoveUndo()
		{
            //undo move history
            ChessMoveHistory movehist = _hist[_histCount - 1];
            _histCount--;

            ChessMove moveUndoing = movehist.Move;
            //undo promotion
            if (movehist.Move.Promote != ChessPiece.EMPTY)
            {
                PieceChange(moveUndoing.To, movehist.PieceMoved);
            }
			
            //move piece to it's original location
            PieceMove(moveUndoing.To, moveUndoing.From);



			//replace the captured piece
			if (movehist.Captured != ChessPiece.EMPTY)
			{
                PieceAdd(moveUndoing.To, movehist.Captured);
			}

			//move rook back if castle
            if (movehist.PieceMoved == ChessPiece.WKing && moveUndoing.From == ChessPosition.E1 && moveUndoing.To == ChessPosition.G1)
			{
                this.PieceMove(ChessPosition.F1, ChessPosition.H1);
			}
            if (movehist.PieceMoved == ChessPiece.WKing && moveUndoing.From == ChessPosition.E1 && moveUndoing.To == ChessPosition.C1)
			{
                this.PieceMove(ChessPosition.D1, ChessPosition.A1);
			}
            if (movehist.PieceMoved == ChessPiece.BKing && moveUndoing.From == ChessPosition.E8 && moveUndoing.To == ChessPosition.G8)
			{
                this.PieceMove(ChessPosition.F8, ChessPosition.H8);
			}
            if (movehist.PieceMoved == ChessPiece.BKing && moveUndoing.From == ChessPosition.E8 && moveUndoing.To == ChessPosition.C8)
			{
                this.PieceMove(ChessPosition.D8, ChessPosition.A8);
			}

			//put back pawn if enpassant capture
            if (movehist.PieceMoved == ChessPiece.WPawn && moveUndoing.To == movehist.Enpassant)
			{
                ChessFile tofile = moveUndoing.To.GetFile();
                PieceAdd(tofile.ToPosition(ChessRank.Rank5), ChessPiece.BPawn);
			}
            if (movehist.PieceMoved == ChessPiece.BPawn && moveUndoing.To == movehist.Enpassant)
			{
                ChessFile tofile = moveUndoing.To.GetFile();
                PieceAdd(tofile.ToPosition(ChessRank.Rank4), ChessPiece.WPawn);
			}

            this._castleFlags = movehist.Castle;
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
            _zobMaterial = movehist.ZobristMaterial;
		}

		public void MoveNullApply()
		{
			//save move history
            if (_histCount > _histUB) { HistResize(); }
            _hist[_histCount++].Reset(ChessMove.EMPTY, (ChessPiece.EMPTY), (ChessPiece.EMPTY), _enpassant, _castleFlags, _fiftymove, _movesSinceNull, _zob, _zobPawn, _zobMaterial);

			//reset since null count;
			_movesSinceNull = 0;

			//unmark enpassant sq
			if (_enpassant.IsInBounds())
			{
				_zob ^= ChessZobrist.Enpassant(_enpassant);
				_enpassant = (ChessPosition.OUTOFBOUNDS);
			}

			//switch whos turn
            _whosturn = _whosturn.PlayerOther();
			_zob ^= ChessZobrist.Player;

		}
		public void MoveNullUndo()
		{
            ChessMoveHistory movehist = _hist[_histCount - 1];
            _histCount--;

            this._castleFlags = movehist.Castle;
			this._enpassant = movehist.Enpassant;
			this._fiftymove = movehist.FiftyCount;
			this._movesSinceNull = movehist.MovesSinceNull;

            _whosturn = _whosturn.PlayerOther();
			_zob = movehist.Zobrist;
			_zobPawn = movehist.ZobristPawn;
            _zobMaterial = movehist.ZobristMaterial;
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
                && this._histCount >= 2
                && this._hist[_histCount - 2].Move == ChessMove.EMPTY;
		}

		public ChessPiece PieceInDirection(ChessPosition from, ChessDirection dir, ref ChessPosition pos)
		{
			int i = 0;
			return PieceInDirection(from, dir, ref pos, ref i);
		}
		public ChessPiece PieceInDirection(ChessPosition from, ChessDirection dir, ref ChessPosition pos, ref int dist)
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
            return AttacksTo(to) & this[by];
		}
        public ChessBitboard AttacksTo(ChessPosition to)
		{
            ChessBitboard retval = 0;
            retval |= Attacks.KnightAttacks(to) & this[ChessPieceType.Knight];
            retval |= MagicBitboards.RookAttacks(to, this.PieceLocationsAll) & (this[ChessPieceType.Queen] | this[ChessPieceType.Rook]);
            retval |= MagicBitboards.BishopAttacks(to, this.PieceLocationsAll) & (this[ChessPieceType.Queen] | this[ChessPieceType.Bishop]);
            retval |= Attacks.KingAttacks(to) & (this[ChessPieceType.King]);
            retval |= Attacks.PawnAttacksBlack(to) & this[ChessPiece.WPawn];
            retval |= Attacks.PawnAttacksWhite(to) & this[ChessPiece.BPawn];
            return retval;
		}

		public bool PositionAttacked(ChessPosition to, ChessPlayer byPlayer)
		{
            return !AttacksTo(to, byPlayer).Empty();
		}

        

	}
}
