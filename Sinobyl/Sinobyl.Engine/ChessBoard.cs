using System;
using System.Collections.Generic;
using System.Text;

using System.Text.RegularExpressions;
using System.Collections;
using System.Linq;

namespace Sinobyl.Engine
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>

	public sealed class ChessMoveHistory
	{
        public ChessMove Move;


        public ChessPiece PieceMoved;
		
		public ChessPiece Captured;
		public ChessPosition Enpassant;
		public bool CastleWS;
		public bool CastleWL;
		public bool CastleBS;
		public bool CastleBL;
		public int FiftyCount;
		public int MovesSinceNull;
		public Int64 Zobrist;
		public Int64 ZobristPawn;
        public Int64 ZobristMaterial;

        //public ChessMoveHistory(ChessMove move, ChessPiece piecemoved, ChessPiece captured, ChessPosition enpassant, bool cws, bool cwl, bool cbs, bool cbl, int fifty, int sinceNull, Int64 zob, Int64 zobPawn, Int64 zobMaterial)
        //{
        //    this.Move = move;
        //    this.PieceMoved = piecemoved;
        //    this.Captured = captured;
        //    this.Enpassant = enpassant;
        //    this.CastleWS = cws;
        //    this.CastleWL = cwl;
        //    this.CastleBS = cbs;
        //    this.CastleBL = cbl;
        //    this.FiftyCount = fifty;
        //    this.MovesSinceNull = sinceNull;
        //    this.Zobrist = zob;
        //    this.ZobristPawn = zobPawn;
        //    this.ZobristMaterial = zobMaterial;
        //}

        public void SetValues(ChessMove move, ChessPiece piecemoved, ChessPiece captured, ChessPosition enpassant, bool cws, bool cwl, bool cbs, bool cbl, int fifty, int sinceNull, Int64 zob, Int64 zobPawn, Int64 zobMaterial)
        {
            this.Move = move;
            this.PieceMoved = piecemoved;
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
            this.ZobristMaterial = zobMaterial;
        }
	}


    public sealed class ObservableChessBoard
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

    }
	public sealed class ChessBoard
	{




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
        private Int64 _zobMaterial;

        private ChessMoveHistory[] _histArray = new ChessMoveHistory[100];
        private int _histArrayCount = 0;

		//private ChessMoveHistoryCollection _hist = new ChessMoveHistoryCollection();
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
            initPieceAtArray();
			this.FEN = fen;
		}
		public ChessBoard(ChessFEN fen, IEnumerable<ChessMove> prevMoves)
		{
            initPieceAtArray();
			this.FEN = fen;
			foreach (ChessMove move in prevMoves)
			{
				this.MoveApply(move);
			}
		}



		private void initPieceAtArray()
		{
            foreach (ChessPosition pos in ChessPositionInfo.AllPositions)
			{
				_pieceat[pos] = ChessPiece.EMPTY;
			}
            foreach (ChessPiece piece in ChessPieceInfo.AllPieces)
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

            for (int i = 0; i < _histArray.Length; i++)
            {
                _histArray[i] = new ChessMoveHistory();
            }
            _histArrayCount = 0;
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

        private void PieceMove(ChessPosition from, ChessPosition to)
        {
            ChessPiece piece = _pieceat[from];
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

            _pieceat[pos] = piece;
            _zob ^= ChessZobrist.PiecePosition(piece, pos);
            _zobMaterial ^= ChessZobrist.Material(piece, _pieceCount[piece]);
			_pieceCount[piece]++;

            _pieces[piece] |= pos.Bitboard();
            _allPieces |= pos.Bitboard();
            _playerBoards[piece.PieceToPlayer()] |= pos.Bitboard();
            _allPiecesVert |= Attacks.RotateVert(pos);
            _allPiecesA1H8 |= Attacks.RotateA1H8(pos);
            _allPiecesH1A8 |= Attacks.RotateH1A8(pos);

			if (piece == ChessPiece.WPawn || piece == ChessPiece.BPawn)
			{
                _zobPawn ^= ChessZobrist.PiecePosition(piece, pos);
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
		private void PieceRemove(ChessPosition pos)
		{
            ChessPiece piece = PieceAt(pos);

            
			_pieceat[pos] = ChessPiece.EMPTY;
			_zob ^= ChessZobrist.PiecePosition(piece, pos);
            _zobMaterial ^= ChessZobrist.Material(piece, _pieceCount[piece] - 1);
			_pieceCount[piece]--;

            _pieces[piece] &= ~pos.Bitboard();
            _allPieces &= ~pos.Bitboard();
            _playerBoards[piece.PieceToPlayer()] &= ~pos.Bitboard();
            _allPiecesVert &= ~Attacks.RotateVert(pos);
            _allPiecesA1H8 &= ~Attacks.RotateA1H8(pos);
            _allPiecesH1A8 &= ~Attacks.RotateH1A8(pos);

			if (piece == ChessPiece.WPawn || piece == ChessPiece.BPawn)
			{
                _zobPawn ^= ChessZobrist.PiecePosition(piece, pos); 
			}
		}
		public int PieceCount(ChessPiece piece)
		{
			return _pieceCount[piece];
		}
        public int PieceCount(ChessPlayer player, ChessPieceType pieceType)
        {
            return _pieceCount[pieceType.ForPlayer(player)];
        }

		public bool IsMate()
		{
			if (IsCheck())
			{
                if (!ChessMoveInfo.GenMovesLegal(this).Any())
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
                if (!ChessMoveInfo.GenMovesLegal(this).Any())
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
			for (int i = _histArrayCount - 1; i >= 0; i--)
			{
                ChessMoveHistory movehist = _histArray[i];
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
                    if (_pieceat[pos] != ChessPiece.EMPTY)
                    {
                        this.PieceRemove(pos);
                    }
                }

                //reset board
                this._histArrayCount = 0;
				initPieceAtArray();

                //add pieces
                foreach (var pos in ChessPositionInfo.AllPositions)
				{
                    if (value.pieceat[pos.GetIndex64()] != ChessPiece.EMPTY)
					{
						this.PieceAdd(pos,value.pieceat[pos.GetIndex64()]);
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
                _zobMaterial = ChessZobrist.BoardZobMaterial(this);

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

        public ChessBitboard PieceLocations(ChessPiece piece)
        {
            return _pieces[piece];
        }
        public ChessBitboard PieceLocations(ChessPieceType pieceType, ChessPlayer player)
        {
            return _pieces[pieceType.ForPlayer(player)];
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
            foreach (ChessPosition pos in ChessPositionInfo.AllPositions)
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
            _zobMaterial = 0;
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


		public void MoveApply(ChessMove move)
		{
            //initialize event handler and log.

            ChessPosition from = move.From();
            ChessPosition to = move.To();
			ChessPiece piece = this.PieceAt(from);
			ChessPiece capture = this.PieceAt(to);
            ChessPiece promote = move.Promote();
            ChessRank fromrank = from.GetRank();
			ChessRank torank = to.GetRank();
			ChessFile fromfile = from.GetFile();
			ChessFile tofile = to.GetFile();

            if (_histArrayCount > _histArray.GetUpperBound(0)) 
            {
                Array.Resize(ref _histArray, _histArrayCount + 50);
                for (int i = _histArrayCount; i < _histArrayCount + 50; i++) 
                { 
                    _histArray[i] = new ChessMoveHistory(); 
                }
            }
            ChessMoveHistory histobj = _histArray[_histArrayCount++];
            histobj.SetValues(move, piece, capture, _enpassant, _castleWS, _castleWL, _castleBS, _castleBL, _fiftymove, _movesSinceNull, _zob, _zobPawn, _zobMaterial);
            
            //_hist.Add(histobj);

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
			if (piece == ChessPiece.WKing)
			{
				if (this._castleWS)
				{
					this._castleWS = false;
					_zob ^= ChessZobrist.CastleWS;
				}
				if (this._castleWL)
				{
					this._castleWL = false;
					_zob ^= ChessZobrist.CastleWL;
				}
			}
			if (piece == ChessPiece.BKing)
			{
				if (this._castleBS)
				{
					this._castleBS = false;
					_zob ^= ChessZobrist.CastleBS;
				}
				if (this._castleBL)
				{
					this._castleBL = false;
					_zob ^= ChessZobrist.CastleBL;
				}
			}
			if (from == ChessPosition.H1 && this._castleWS)
			{
				this._castleWS = false;
				_zob ^= ChessZobrist.CastleWS;
			}
			if (from == ChessPosition.A1 && this._castleWL)
			{
				this._castleWL = false;
				_zob ^= ChessZobrist.CastleWL;
			}
			if (from == ChessPosition.H8 && this._castleBS)
			{
				this._castleBS = false;
				_zob ^= ChessZobrist.CastleBS;
			}
			if (from == ChessPosition.A8 && this._castleBL)
			{
				this._castleBL = false;
				_zob ^= ChessZobrist.CastleBL;
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
				return _histArrayCount;
			}
		}
		public ChessMoves HistoryMoves
		{
			get
			{
				ChessMoves retval = new ChessMoves();
                for (int i = 0; i < _histArrayCount; i++)
                {
                    retval.Add(_histArray[i].Move);
                }
				return retval;
			}
		}

		public ChessMove HistMove(int MovesAgo)
		{
			ChessMoveHistory hist = _histArray[_histArrayCount - MovesAgo];
            return hist.Move;
		}

		public void MoveUndo()
		{
            //undo move history
            ChessMoveHistory movehist = _histArray[--_histArrayCount];

            ChessMove prevMove = movehist.Move;

            var prevMoveFrom = prevMove.From();
            var prevMoveTo = prevMove.To();
            var prevMoveProm = prevMove.Promote();


            //undo promotion
            if (prevMoveProm != ChessPiece.EMPTY)
            {
                PieceChange(prevMoveTo, movehist.PieceMoved);
            }
			
            //move piece to it's original location
            PieceMove(prevMoveTo, prevMoveFrom);



			//replace the captured piece
			if (movehist.Captured != ChessPiece.EMPTY)
			{
                PieceAdd(prevMoveTo, movehist.Captured);
			}

			//move rook back if castle
            if (movehist.PieceMoved == ChessPiece.WKing && prevMoveFrom == ChessPosition.E1 && prevMoveTo == ChessPosition.G1)
			{
                this.PieceMove(ChessPosition.F1, ChessPosition.H1);
			}
            if (movehist.PieceMoved == ChessPiece.WKing && prevMoveFrom == ChessPosition.E1 && prevMoveTo == ChessPosition.C1)
			{
                this.PieceMove(ChessPosition.D1, ChessPosition.A1);
			}
            if (movehist.PieceMoved == ChessPiece.BKing && prevMoveFrom == ChessPosition.E8 && prevMoveTo == ChessPosition.G8)
			{
                this.PieceMove(ChessPosition.F8, ChessPosition.H8);
			}
            if (movehist.PieceMoved == ChessPiece.BKing && prevMoveFrom == ChessPosition.E8 && prevMoveTo == ChessPosition.C8)
			{
                this.PieceMove(ChessPosition.D8, ChessPosition.A8);
			}

			//put back pawn if enpassant capture
            if (movehist.PieceMoved == ChessPiece.WPawn && prevMoveTo == movehist.Enpassant)
			{
                ChessFile tofile = prevMoveTo.GetFile();
                PieceAdd(tofile.ToPosition(ChessRank.Rank5), ChessPiece.BPawn);
			}
            if (movehist.PieceMoved == ChessPiece.BPawn && prevMoveTo == movehist.Enpassant)
			{
                ChessFile tofile = prevMoveTo.GetFile();
				PieceAdd(tofile.ToPosition(ChessRank.Rank4), ChessPiece.WPawn);
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
            _zobMaterial = movehist.ZobristMaterial;

		}

		public void MoveNullApply()
		{
            if (_histArrayCount > _histArray.GetUpperBound(0))
            {
                Array.Resize(ref _histArray, _histArrayCount + 50);
                for (int i = _histArrayCount; i < _histArrayCount + 50; i++)
                {
                    _histArray[i] = new ChessMoveHistory();
                }
            }

			//save move history
            ChessMoveHistory histobj = _histArray[_histArrayCount++]; //= new ChessMoveHistory(ChessMove.NULL_MOVE, (ChessPiece.EMPTY), (ChessPiece.EMPTY), _enpassant, _castleWS, _castleWL, _castleBS, _castleBL, _fiftymove, _movesSinceNull, _zob, _zobPawn, _zobMaterial);
            histobj.SetValues(ChessMove.NULL_MOVE, (ChessPiece.EMPTY), (ChessPiece.EMPTY), _enpassant, _castleWS, _castleWL, _castleBS, _castleBL, _fiftymove, _movesSinceNull, _zob, _zobPawn, _zobMaterial);
			//_hist.Add(histobj);

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
            ChessMoveHistory movehist = _histArray[--_histArrayCount];

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
                && this._histArrayCount >= 2
                && this._histArray[_histArrayCount - 2].Move == ChessMove.NULL_MOVE;
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
