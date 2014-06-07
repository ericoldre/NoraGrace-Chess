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
        public Piece PieceMoved;
		public Piece Captured;
		public ChessPosition Enpassant;
        public CastleFlags Castle;
		public int FiftyCount;
		public int MovesSinceNull;
		public Int64 Zobrist;
		public Int64 ZobristPawn;
        public Int64 ZobristMaterial;
        public Bitboard Checkers;

        public void Reset(ChessMove move, Piece piecemoved, Piece captured, ChessPosition enpassant, CastleFlags castle, int fifty, int sinceNull, Int64 zob, Int64 zobPawn, Int64 zobMaterial, Bitboard checkers)
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
            this.Checkers = checkers;
        }
	}

    [System.Diagnostics.DebuggerDisplay(@"{FEN,nq}")]
	public sealed class ChessBoard
    {

		private Piece[] _pieceat = new Piece[65];

        //private ChessPieceDictionary<int> _pieceCount = new ChessPieceDictionary<int>();
		private int[] _pieceCount = new int[PieceInfo.LookupArrayLength];

        //private ChessPlayerDictionary<ChessPosition> _kingpos = new ChessPlayerDictionary<ChessPosition>();
        private ChessPosition[] _kingpos = new ChessPosition[2];

        //private ChessPieceDictionary<ChessBitboard> _pieces = new ChessPieceDictionary<ChessBitboard>();
        private Bitboard[] _pieces = new Bitboard[PieceInfo.LookupArrayLength];

        //private ChessPieceTypeDictionary<ChessBitboard> _pieceTypes = new ChessPieceTypeDictionary<ChessBitboard>();
        private Bitboard[] _pieceTypes = new Bitboard[ChessPieceTypeInfo.LookupArrayLength];

        //private ChessPlayerDictionary<ChessBitboard> _playerBoards = new ChessPlayerDictionary<ChessBitboard>();
        private Bitboard[] _playerBoards = new Bitboard[2];
        
        private Bitboard _allPieces = 0;
        private Bitboard _checkers = 0;
        //private Attacks.ChessBitboardRotatedVert _allPiecesVert = 0;
        //private Attacks.ChessBitboardRotatedA1H8 _allPiecesA1H8 = 0;
        //private Attacks.ChessBitboardRotatedH1A8 _allPiecesH1A8 = 0;

		private Player _whosturn;
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
        private PhasedScore _pcSq;

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
				_pieceat[(int)pos] = Piece.EMPTY;
			}
            foreach (Piece piece in PieceInfo.AllPieces)
			{
                _pieceCount[(int)piece] = 0;
                _pieces[(int)piece] = 0;
			}
            foreach (ChessPieceType piecetype in ChessPieceTypeInfo.AllPieceTypes)
            {
                _pieceTypes[(int)piecetype] = Bitboard.Empty;
            }
            _playerBoards[(int)Player.White] = 0;
            _playerBoards[(int)Player.Black] = 0;
            _allPieces = 0;

            _pcSq = 0;
			
		}

		public ChessPosition EnPassant
		{
			get
			{
				return _enpassant;
			}
		}

		public ChessPosition KingPosition(Player kingplayer)
		{
            return _kingpos[(int)kingplayer];
		}

        public Bitboard Checkers
        {
            get { return _checkers; }
        }

		public bool IsCheck()
		{
            System.Diagnostics.Debug.Assert(_checkers == (AttacksTo(KingPosition(WhosTurn)) & this[WhosTurn.PlayerOther()]));
            return _checkers != Bitboard.Empty;
			//return IsCheck(_whosturn);
		}
		public bool IsCheck(Player kingplayer)
		{
			ChessPosition kingpos = KingPosition(kingplayer);
            return PositionAttacked(kingpos, kingplayer.PlayerOther());
		}

        private void PieceMove(ChessPosition from, ChessPosition to)
        {
            Piece piece = _pieceat[(int)from];
            PieceRemove(from);
            PieceAdd(to, piece);
        }

        private void PieceChange(ChessPosition pos, Piece newPiece)
        {
            PieceRemove(pos);
            PieceAdd(pos, newPiece);
        }

        private void PieceAdd(ChessPosition pos, Piece piece)
		{
            _pieceat[(int)pos] = piece;
            _zob ^= Zobrist.PiecePosition(piece, pos);
            _zobMaterial ^= Zobrist.Material(piece, _pieceCount[(int)piece]);
            _pieceCount[(int)piece]++;
            _pcSqEvaluator.PcSqValuesAdd(piece, pos, ref _pcSq);

            Bitboard posBits = pos.ToBitboard();
            _pieces[(int)piece] |= posBits;
            _pieceTypes[(int)piece.ToPieceType()] |= posBits;
            _allPieces |= posBits;
            _playerBoards[(int)piece.PieceToPlayer()] |= posBits;

			if (piece == Piece.WPawn || piece == Piece.BPawn)
			{
                _zobPawn ^= Zobrist.PiecePosition(piece, pos);
			}
			else if (piece == Piece.WKing)
			{
                _kingpos[(int)Player.White] = pos;
			}
			else if (piece == Piece.BKing)
			{
                _kingpos[(int)Player.Black] = pos;
			}
		}
        private void PieceRemove(ChessPosition pos)
		{
            Piece piece = PieceAt(pos);

            _pieceat[(int)pos] = Piece.EMPTY;
			_zob ^= Zobrist.PiecePosition(piece, pos);
            _zobMaterial ^= Zobrist.Material(piece, _pieceCount[(int)piece] - 1);
            _pieceCount[(int)piece]--;
            _pcSqEvaluator.PcSqValuesRemove(piece, pos, ref _pcSq);

            Bitboard notPosBits = ~pos.ToBitboard();
            _pieces[(int)piece] &= notPosBits;
            _pieceTypes[(int)piece.ToPieceType()] &= notPosBits;
            _allPieces &= notPosBits;
            _playerBoards[(int)piece.PieceToPlayer()] &= notPosBits;

			if (piece == Piece.WPawn || piece == Piece.BPawn)
			{
                _zobPawn ^= Zobrist.PiecePosition(piece, pos); 
			}
		}
		public int PieceCount(Piece piece)
		{
            return _pieceCount[(int)piece];
		}
        public int PieceCount(Player player, ChessPieceType pieceType)
        {
            return _pieceCount[(int)pieceType.ForPlayer(player)];
        }

        public PhasedScore PcSqValue
        {
            get { return _pcSq; }
        }

        public ChessEval PcSqEvaluator
        {
            get { return _pcSqEvaluator; }
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
            return PositionRepetitionCount() >= 3;
		}

        public int PositionRepetitionCount()
        {
            Int64 currzob = this.ZobristBoard;
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
                    return repcount;
                }
                if (movehist.Captured != Piece.EMPTY)
                {
                    break;
                }
                if (movehist.PieceMoved == Piece.WPawn || movehist.PieceMoved == Piece.BPawn)
                {
                    break;
                }
            }
            return repcount;
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


		public List<ChessPosition> PieceList(Piece piece)
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
                    if (_pieceat[(int)pos] != Piece.EMPTY)
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
                    if (value.pieceat[pos.GetIndex64()] != Piece.EMPTY)
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
				_zob = Zobrist.BoardZob(this);
				_zobPawn = Zobrist.BoardZobPawn(this);
                _zobMaterial = Zobrist.BoardZobMaterial(this);
                _checkers = AttacksTo(_kingpos[(int)_whosturn]) & this[_whosturn.PlayerOther()];

			}
		}

		public Int64 ZobristBoard
		{
            get { return _zob; }
		}

        public Int64 ZobristPrevious
        {
            get { return _hist[_histCount - 1].Zobrist; }
        }

		public Int64 ZobristPawn
		{
            get { return _zobPawn; }
		}

        public Int64 ZobristMaterial
        {
            get { return _zobMaterial; }
        }

        public Bitboard this[Piece piece]
        {
            get { return _pieces[(int)piece]; }
        }

        public Bitboard this[ChessPieceType pieceType]
        {
            get { return _pieceTypes[(int)pieceType]; }
        }

        public Bitboard this[Player player]
        {
            get
            {
                return _playerBoards[(int)player];
            }
        }

        public Bitboard PieceLocationsAll
        {
            get
            {
                return _allPieces;
            }
        }

        public Piece PieceAt(ChessPosition pos)
		{
            return _pieceat[(int)pos];
		}

        public CastleFlags CastleRights
        {
            get { return _castleFlags; }
        }
		
		public Player WhosTurn
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
			ChessMove move = ChessMoveInfo.Parse(this, movedesc);
			this.MoveApply(move);
		}

		public void MoveApply(ChessMove move)
		{
            System.Diagnostics.Debug.Assert(move != ChessMove.EMPTY);
			ChessPosition from = move.From();
			ChessPosition to = move.To();
			Piece piece = this.PieceAt(from);
			Piece capture = this.PieceAt(to);
            Piece promote = move.Promote();
            Rank fromrank = from.ToRank();
			Rank torank = to.ToRank();
			File fromfile = from.ToFile();
			File tofile = to.ToFile();

            if (_histCount > _histUB) { HistResize(); }
			_hist[_histCount++].Reset(move, piece, capture, _enpassant, _castleFlags, _fiftymove,_movesSinceNull, _zob, _zobPawn, _zobMaterial, _checkers);

			//increment since null count;
			_movesSinceNull++;

			//remove captured piece
			if (capture != Piece.EMPTY)
			{
				this.PieceRemove(to);
			}

            //move piece, promote if needed
            this.PieceMove(from, to);
            if (promote != Piece.EMPTY)
            {
                this.PieceChange(to, promote);
            }
			
			//if castle, move rook
			if (piece == Piece.WKing && from == ChessPosition.E1 && to == ChessPosition.G1)
			{
                this.PieceMove(ChessPosition.H1, ChessPosition.F1);
			}
			if (piece == Piece.WKing && from == ChessPosition.E1 && to == ChessPosition.C1)
			{
                this.PieceMove(ChessPosition.A1, ChessPosition.D1);
			}
			if (piece == Piece.BKing && from == ChessPosition.E8 && to == ChessPosition.G8)
			{
                this.PieceMove(ChessPosition.H8, ChessPosition.F8);
			}
			if (piece == Piece.BKing && from == ChessPosition.E8 && to == ChessPosition.C8)
			{
                this.PieceMove(ChessPosition.A8, ChessPosition.D8);
			}

			//mark unavailability of castling
            if(_castleFlags != 0)
            {
                if (((_castleFlags & CastleFlags.WhiteShort) != 0) 
                    && (piece == Piece.WKing || from == ChessPosition.H1))
                {
                    _castleFlags &= ~CastleFlags.WhiteShort;
                    _zob ^= Zobrist.CastleWS;
                }

                if (((_castleFlags & CastleFlags.WhiteLong) != 0)
                    && (piece == Piece.WKing || from == ChessPosition.A1))
                {
                    _castleFlags &= ~CastleFlags.WhiteLong;
                    _zob ^= Zobrist.CastleWL;
                }

                if (((_castleFlags & CastleFlags.BlackShort) != 0)
                    && (piece == Piece.BKing || from == ChessPosition.H8))
                {
                    _castleFlags &= ~CastleFlags.BlackShort;
                    _zob ^= Zobrist.CastleBS;
                }

                if (((_castleFlags & CastleFlags.BlackLong) != 0)
                    && (piece == Piece.BKing || from == ChessPosition.A8))
                {
                    _castleFlags &= ~CastleFlags.BlackLong;
                    _zob ^= Zobrist.CastleBL;
                }
            }



			//if enpassant move then remove captured pawn
			if (piece == Piece.WPawn && to == this._enpassant)
			{
                this.PieceRemove(tofile.ToPosition(Rank.Rank5));
			}
			if (piece == Piece.BPawn && to == this._enpassant)
			{
                this.PieceRemove(tofile.ToPosition(Rank.Rank4));
			}

			//unmark enpassant sq
			if (_enpassant.IsInBounds())
			{
				_zob ^= Zobrist.Enpassant(_enpassant);
				_enpassant = (ChessPosition.OUTOFBOUNDS);
			}

			//mark enpassant sq if pawn double jump
			if (piece == Piece.WPawn && fromrank == Rank.Rank2 && torank == Rank.Rank4)
			{
				_enpassant = fromfile.ToPosition(Rank.Rank3);
                _zob ^= Zobrist.Enpassant(_enpassant);
			}
			else if (piece == Piece.BPawn && fromrank == Rank.Rank7 && torank == Rank.Rank5)
			{
				_enpassant = fromfile.ToPosition(Rank.Rank6);
                _zob ^= Zobrist.Enpassant(_enpassant);
			}

			//increment the move count
			if (_whosturn == Player.Black)
			{
				_fullmove++;
			}
			if (piece != Piece.WPawn && piece != Piece.BPawn && capture == Piece.EMPTY)
			{
				_fiftymove++;
			}
			else
			{
				_fiftymove = 0;
			}

			//switch whos turn
            _whosturn = _whosturn.PlayerOther();
			_zob ^= Zobrist.PlayerKey;
            _checkers = AttacksTo(_kingpos[(int)_whosturn]) & this[_whosturn.PlayerOther()];

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
            if (movehist.Move.Promote() != Piece.EMPTY)
            {
                PieceChange(moveUndoing.To(), movehist.PieceMoved);
            }
			
            //move piece to it's original location
            PieceMove(moveUndoing.To(), moveUndoing.From());



			//replace the captured piece
			if (movehist.Captured != Piece.EMPTY)
			{
                PieceAdd(moveUndoing.To(), movehist.Captured);
			}

			//move rook back if castle
            if (movehist.PieceMoved == Piece.WKing && moveUndoing.From() == ChessPosition.E1 && moveUndoing.To() == ChessPosition.G1)
			{
                this.PieceMove(ChessPosition.F1, ChessPosition.H1);
			}
            if (movehist.PieceMoved == Piece.WKing && moveUndoing.From() == ChessPosition.E1 && moveUndoing.To() == ChessPosition.C1)
			{
                this.PieceMove(ChessPosition.D1, ChessPosition.A1);
			}
            if (movehist.PieceMoved == Piece.BKing && moveUndoing.From() == ChessPosition.E8 && moveUndoing.To() == ChessPosition.G8)
			{
                this.PieceMove(ChessPosition.F8, ChessPosition.H8);
			}
            if (movehist.PieceMoved == Piece.BKing && moveUndoing.From() == ChessPosition.E8 && moveUndoing.To() == ChessPosition.C8)
			{
                this.PieceMove(ChessPosition.D8, ChessPosition.A8);
			}

			//put back pawn if enpassant capture
            if (movehist.PieceMoved == Piece.WPawn && moveUndoing.To() == movehist.Enpassant)
			{
                File tofile = moveUndoing.To().ToFile();
                PieceAdd(tofile.ToPosition(Rank.Rank5), Piece.BPawn);
			}
            if (movehist.PieceMoved == Piece.BPawn && moveUndoing.To() == movehist.Enpassant)
			{
                File tofile = moveUndoing.To().ToFile();
                PieceAdd(tofile.ToPosition(Rank.Rank4), Piece.WPawn);
			}

            this._castleFlags = movehist.Castle;
			this._enpassant = movehist.Enpassant;
			this._fiftymove = movehist.FiftyCount;
			this._movesSinceNull = movehist.MovesSinceNull;

			if (_whosturn == Player.White)
			{
				_fullmove--;
			}

            _whosturn = _whosturn.PlayerOther();
			_zob = movehist.Zobrist;
			_zobPawn = movehist.ZobristPawn;
            _zobMaterial = movehist.ZobristMaterial;
            _checkers = movehist.Checkers;
		}

		public void MoveNullApply()
		{
			//save move history
            if (_histCount > _histUB) { HistResize(); }
            _hist[_histCount++].Reset(ChessMove.EMPTY, (Piece.EMPTY), (Piece.EMPTY), _enpassant, _castleFlags, _fiftymove, _movesSinceNull, _zob, _zobPawn, _zobMaterial, _checkers);

			//reset since null count;
			_movesSinceNull = 0;

			//unmark enpassant sq
			if (_enpassant.IsInBounds())
			{
				_zob ^= Zobrist.Enpassant(_enpassant);
				_enpassant = (ChessPosition.OUTOFBOUNDS);
			}

			//switch whos turn
            _whosturn = _whosturn.PlayerOther();
			_zob ^= Zobrist.PlayerKey;
            _checkers = AttacksTo(_kingpos[(int)_whosturn]) & this[_whosturn.PlayerOther()];
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
            _checkers = movehist.Checkers;
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

		public Piece PieceInDirection(ChessPosition from, Direction dir, ref ChessPosition pos)
		{
			int i = 0;
			return PieceInDirection(from, dir, ref pos, ref i);
		}
		public Piece PieceInDirection(ChessPosition from, Direction dir, ref ChessPosition pos, ref int dist)
		{

			Piece piece;
            pos = from.PositionInDirection(dir);
			dist = 1;
			while (pos.IsInBounds())
			{
				piece = this.PieceAt(pos);
				if (piece != Piece.EMPTY) { return piece; }

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
			return Piece.EMPTY;
		}
        public Bitboard AttacksTo(ChessPosition to, Player by)
		{
            return AttacksTo(to) & this[by];
		}
        public Bitboard AttacksTo(ChessPosition to)
		{
            Bitboard retval = 0;
            retval |= Attacks.KnightAttacks(to) & this[ChessPieceType.Knight];
            retval |= Attacks.RookAttacks(to, this.PieceLocationsAll) & (this[ChessPieceType.Queen] | this[ChessPieceType.Rook]);
            retval |= Attacks.BishopAttacks(to, this.PieceLocationsAll) & (this[ChessPieceType.Queen] | this[ChessPieceType.Bishop]);
            retval |= Attacks.KingAttacks(to) & (this[ChessPieceType.King]);
            retval |= Attacks.PawnAttacks(to, Player.Black) & this[Piece.WPawn];
            retval |= Attacks.PawnAttacks(to, Player.White) & this[Piece.BPawn];
            return retval;
		}

		public bool PositionAttacked(ChessPosition to, Player byPlayer)
		{
            return !AttacksTo(to, byPlayer).Empty();
		}

        

	}
}
