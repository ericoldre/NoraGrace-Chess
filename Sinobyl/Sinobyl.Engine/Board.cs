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



    [System.Diagnostics.DebuggerDisplay(@"{FENCurrent,nq}")]
	public sealed class Board
    {

        public class MoveHistory
        {
            public ChessMove Move;
            public Piece PieceMoved;
            public Piece Captured;
            public Position Enpassant;
            public CastleFlags Castle;
            public int FiftyCount;
            public int MovesSinceNull;
            public Int64 Zobrist;
            public Int64 ZobristPawn;
            public Int64 ZobristMaterial;
            public Bitboard Checkers;

            public void Reset(ChessMove move, Piece piecemoved, Piece captured, Position enpassant, CastleFlags castle, int fifty, int sinceNull, Int64 zob, Int64 zobPawn, Int64 zobMaterial, Bitboard checkers)
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

		private Piece[] _pieceat = new Piece[65];

        //private ChessPieceDictionary<int> _pieceCount = new ChessPieceDictionary<int>();
		private int[] _pieceCount = new int[PieceInfo.LookupArrayLength];

        //private ChessPlayerDictionary<ChessPosition> _kingpos = new ChessPlayerDictionary<ChessPosition>();
        private Position[] _kingpos = new Position[2];

        //private ChessPieceDictionary<ChessBitboard> _pieces = new ChessPieceDictionary<ChessBitboard>();
        private Bitboard[] _pieces = new Bitboard[PieceInfo.LookupArrayLength];

        //private ChessPieceTypeDictionary<ChessBitboard> _pieceTypes = new ChessPieceTypeDictionary<ChessBitboard>();
        private Bitboard[] _pieceTypes = new Bitboard[PieceTypeInfo.LookupArrayLength];

        //private ChessPlayerDictionary<ChessBitboard> _playerBoards = new ChessPlayerDictionary<ChessBitboard>();
        private Bitboard[] _playerBoards = new Bitboard[2];
        
        private Bitboard _allPieces = 0;
        private Bitboard _checkers = 0;
        //private Attacks.ChessBitboardRotatedVert _allPiecesVert = 0;
        //private Attacks.ChessBitboardRotatedA1H8 _allPiecesA1H8 = 0;
        //private Attacks.ChessBitboardRotatedH1A8 _allPiecesH1A8 = 0;

		private Player _whosturn;
        private CastleFlags _castleFlags;
		private Position _enpassant;
		private int _fiftymove = 0;
		private int _fullmove = 0;
		private Int64 _zob;
		private Int64 _zobPawn;
        private Int64 _zobMaterial;

        private MoveHistory[] _hist = new MoveHistory[25];
        private int _histUB;
        private int _histCount = 0;

		private int _movesSinceNull = 100;

        private readonly ChessEval _pcSqEvaluator;
        private PhasedScore _pcSq;

        public Board(ChessEval pcSqEvaluator = null)
            : this(new FEN(FEN.FENStart), pcSqEvaluator)
		{
            
		}
        public Board(string fen, ChessEval pcSqEvaluator = null)
            : this(new FEN(fen), pcSqEvaluator)
		{

		}
		public Board(FEN fen, ChessEval pcSqEvaluator = null)
		{
            _histUB = _hist.GetUpperBound(0);
            for (int i = 0; i <= _histUB; i++)
            {
                _hist[i] = new MoveHistory();
            }

            _pcSqEvaluator = pcSqEvaluator ?? ChessEval.Default;
            initPieceAtArray();

			this.FENCurrent = fen;
		}

        public Board(FEN fen, IEnumerable<ChessMove> prevMoves, ChessEval pcSqEvaluator = null)
            : this(fen, pcSqEvaluator)
        {
            foreach (ChessMove move in prevMoves)
            {
                this.MoveApply(move);
            }
        }



		private void initPieceAtArray()
		{
            foreach (Position pos in PositionInfo.AllPositions)
			{
				_pieceat[(int)pos] = Piece.EMPTY;
			}
            foreach (Piece piece in PieceInfo.AllPieces)
			{
                _pieceCount[(int)piece] = 0;
                _pieces[(int)piece] = 0;
			}
            foreach (PieceType piecetype in PieceTypeInfo.AllPieceTypes)
            {
                _pieceTypes[(int)piecetype] = Bitboard.Empty;
            }
            _playerBoards[(int)Player.White] = 0;
            _playerBoards[(int)Player.Black] = 0;
            _allPieces = 0;

            _pcSq = 0;
			
		}

		public Position EnPassant
		{
			get
			{
				return _enpassant;
			}
		}

		public Position KingPosition(Player kingplayer)
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
			Position kingpos = KingPosition(kingplayer);
            return PositionAttacked(kingpos, kingplayer.PlayerOther());
		}

        private void PieceMove(Position from, Position to)
        {
            Piece piece = _pieceat[(int)from];
            PieceRemove(from);
            PieceAdd(to, piece);
        }

        private void PieceChange(Position pos, Piece newPiece)
        {
            PieceRemove(pos);
            PieceAdd(pos, newPiece);
        }

        private void PieceAdd(Position pos, Piece piece)
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
        private void PieceRemove(Position pos)
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
        public int PieceCount(Player player, PieceType pieceType)
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
                MoveHistory movehist = _hist[i];
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


		public List<Position> PieceList(Piece piece)
		{
			List<Position> retval = new List<Position>();
            foreach (Position pos in PositionInfo.AllPositions)
			{
				if (PieceAt(pos) == piece)
				{
					retval.Add(pos);
				}
			}
			return retval;
		}

		public FEN FENCurrent
		{
			get
			{
				return new FEN(this);
			}
			set
			{

                //clear all existing pieces.
                foreach (var pos in PositionInfo.AllPositions)
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
                foreach (var pos in PositionInfo.AllPositions)
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

        public Bitboard this[PieceType pieceType]
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

        public Piece PieceAt(Position pos)
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
                    _hist[i] = new MoveHistory();
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
			Position from = move.From();
			Position to = move.To();
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
			if (piece == Piece.WKing && from == Position.E1 && to == Position.G1)
			{
                this.PieceMove(Position.H1, Position.F1);
			}
			if (piece == Piece.WKing && from == Position.E1 && to == Position.C1)
			{
                this.PieceMove(Position.A1, Position.D1);
			}
			if (piece == Piece.BKing && from == Position.E8 && to == Position.G8)
			{
                this.PieceMove(Position.H8, Position.F8);
			}
			if (piece == Piece.BKing && from == Position.E8 && to == Position.C8)
			{
                this.PieceMove(Position.A8, Position.D8);
			}

			//mark unavailability of castling
            if(_castleFlags != 0)
            {
                if (((_castleFlags & CastleFlags.WhiteShort) != 0) 
                    && (piece == Piece.WKing || from == Position.H1))
                {
                    _castleFlags &= ~CastleFlags.WhiteShort;
                    _zob ^= Zobrist.CastleWS;
                }

                if (((_castleFlags & CastleFlags.WhiteLong) != 0)
                    && (piece == Piece.WKing || from == Position.A1))
                {
                    _castleFlags &= ~CastleFlags.WhiteLong;
                    _zob ^= Zobrist.CastleWL;
                }

                if (((_castleFlags & CastleFlags.BlackShort) != 0)
                    && (piece == Piece.BKing || from == Position.H8))
                {
                    _castleFlags &= ~CastleFlags.BlackShort;
                    _zob ^= Zobrist.CastleBS;
                }

                if (((_castleFlags & CastleFlags.BlackLong) != 0)
                    && (piece == Piece.BKing || from == Position.A8))
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
				_enpassant = (Position.OUTOFBOUNDS);
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
            MoveHistory movehist = _hist[_histCount - 1];
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
            if (movehist.PieceMoved == Piece.WKing && moveUndoing.From() == Position.E1 && moveUndoing.To() == Position.G1)
			{
                this.PieceMove(Position.F1, Position.H1);
			}
            if (movehist.PieceMoved == Piece.WKing && moveUndoing.From() == Position.E1 && moveUndoing.To() == Position.C1)
			{
                this.PieceMove(Position.D1, Position.A1);
			}
            if (movehist.PieceMoved == Piece.BKing && moveUndoing.From() == Position.E8 && moveUndoing.To() == Position.G8)
			{
                this.PieceMove(Position.F8, Position.H8);
			}
            if (movehist.PieceMoved == Piece.BKing && moveUndoing.From() == Position.E8 && moveUndoing.To() == Position.C8)
			{
                this.PieceMove(Position.D8, Position.A8);
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
				_enpassant = (Position.OUTOFBOUNDS);
			}

			//switch whos turn
            _whosturn = _whosturn.PlayerOther();
			_zob ^= Zobrist.PlayerKey;
            _checkers = AttacksTo(_kingpos[(int)_whosturn]) & this[_whosturn.PlayerOther()];
		}
		public void MoveNullUndo()
		{
            MoveHistory movehist = _hist[_histCount - 1];
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

		public Piece PieceInDirection(Position from, Direction dir, ref Position pos)
		{
			int i = 0;
			return PieceInDirection(from, dir, ref pos, ref i);
		}
		public Piece PieceInDirection(Position from, Direction dir, ref Position pos, ref int dist)
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
        public Bitboard AttacksTo(Position to, Player by)
		{
            return AttacksTo(to) & this[by];
		}
        public Bitboard AttacksTo(Position to)
		{
            Bitboard retval = 0;
            retval |= Attacks.KnightAttacks(to) & this[PieceType.Knight];
            retval |= Attacks.RookAttacks(to, this.PieceLocationsAll) & (this[PieceType.Queen] | this[PieceType.Rook]);
            retval |= Attacks.BishopAttacks(to, this.PieceLocationsAll) & (this[PieceType.Queen] | this[PieceType.Bishop]);
            retval |= Attacks.KingAttacks(to) & (this[PieceType.King]);
            retval |= Attacks.PawnAttacks(to, Player.Black) & this[Piece.WPawn];
            retval |= Attacks.PawnAttacks(to, Player.White) & this[Piece.BPawn];
            return retval;
		}

		public bool PositionAttacked(Position to, Player byPlayer)
		{
            return !AttacksTo(to, byPlayer).Empty();
		}

        

	}
}
