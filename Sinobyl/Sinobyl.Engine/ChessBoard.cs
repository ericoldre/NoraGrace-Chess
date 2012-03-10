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
		//public event msgMove OnMoveApply;
		//public event msgVoid OnMoveUndo;
		//public event msgVoid OnReset;


		private ChessPiece[] _pieceat = new ChessPiece[120];
		private int[] _pieceCount = new int[12];
		private ChessPosition[] _kingpos = new ChessPosition[2];

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
			this.FEN = new ChessFEN(Chess.FENStart);
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
			for (int i = 0; i <= 119; i++)
			{
				_pieceat[i] = ChessPiece.OOB;
			}
			foreach (ChessPosition pos in Chess.AllPositions)
			{
				_pieceat[(int)pos] = ChessPiece.EMPTY;
			}
			for (int i = 0; i < 12; i++)
			{
				_pieceCount[i] = 0;
			}
			
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
			return PositionAttacked(kingpos, Chess.PlayerOther(kingplayer));
		}

		private void PieceAdd(ChessPosition pos, ChessPiece piece)
		{
			_pieceat[(int)pos] = piece;
			_zob ^= ChessZobrist._piecepos[(int)piece, (int)pos];
			_pieceCount[(int)piece]++;

			if (piece == ChessPiece.WPawn || piece == ChessPiece.BPawn)
			{
				_zobPawn ^= ChessZobrist._piecepos[(int)piece, (int)pos];
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
		private void PieceRemove(ChessPosition pos, ChessPiece piece)
		{
			_pieceat[(int)pos] = ChessPiece.EMPTY;
			_zob ^= ChessZobrist._piecepos[(int)piece, (int)pos];
			_pieceCount[(int)piece]--;

			if (piece == ChessPiece.WPawn || piece == ChessPiece.BPawn)
			{
				_zobPawn ^= ChessZobrist._piecepos[(int)piece, (int)pos];
			}
		}
		public int PieceCount(ChessPiece piece)
		{
			return _pieceCount[(int)piece];
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
				this._hist.Clear();
				initPieceAtArray();

				for (int i = 0; i < 120; i++)
				{
					if (((ChessPosition)i).IsInBounds() && value.pieceat[i]!=ChessPiece.EMPTY)
					{
						this.PieceAdd((ChessPosition)i,value.pieceat[i]);
					}
					//_pieceat[i] = value.pieceat[i];
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
		public ChessPiece PieceAt(ChessPosition pos)
		{
			return _pieceat[(int)pos];
		}
		private void Reset()
		{
			foreach (ChessPosition pos in Chess.AllPositions)
			{
				_pieceat[(int)pos] = ChessPiece.EMPTY;
			}
			_whosturn = ChessPlayer.White;
			_castleWS = false;
			_castleWL = false;
			_castleBS = false;
			_castleBL = false;
			_enpassant = (ChessPosition)0;
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
				this.PieceRemove(to, capture);
			}
			//move piece, promote if needed
			this.PieceRemove(from, piece);
			this.PieceAdd(to, promote == ChessPiece.EMPTY ? piece : promote);

			//if castle, move rook
			if (piece == ChessPiece.WKing && from == ChessPosition.E1 && to == ChessPosition.G1)
			{
				this.PieceRemove(ChessPosition.H1, ChessPiece.WRook);
				this.PieceAdd(ChessPosition.F1, ChessPiece.WRook);
			}
			if (piece == ChessPiece.WKing && from == ChessPosition.E1 && to == ChessPosition.C1)
			{
				this.PieceRemove(ChessPosition.A1, ChessPiece.WRook);
				this.PieceAdd(ChessPosition.D1, ChessPiece.WRook);
			}
			if (piece == ChessPiece.BKing && from == ChessPosition.E8 && to == ChessPosition.G8)
			{
				this.PieceRemove(ChessPosition.H8, ChessPiece.BRook);
				this.PieceAdd(ChessPosition.F8, ChessPiece.BRook);
			}
			if (piece == ChessPiece.BKing && from == ChessPosition.E8 && to == ChessPosition.C8)
			{
				this.PieceRemove(ChessPosition.A8, ChessPiece.BRook);
				this.PieceAdd(ChessPosition.D8, ChessPiece.BRook);
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
				this.PieceRemove(tofile.ToPosition(ChessRank.Rank5), ChessPiece.BPawn);
			}
			if (piece == ChessPiece.BPawn && to == this._enpassant)
			{
				this.PieceRemove(tofile.ToPosition(ChessRank.Rank4), ChessPiece.WPawn);
			}

			//unmark enpassant sq
			if (_enpassant.IsInBounds())
			{
				_zob ^= ChessZobrist._enpassant[(int)_enpassant];
				_enpassant = (ChessPosition)0;
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
			_whosturn = Chess.PlayerOther(_whosturn);
			_zob ^= ChessZobrist._player;
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

			ChessMoveHistory movehist = _hist[_hist.Count - 1];
			_hist.RemoveAt(_hist.Count - 1);


			//move piece to it's original location
			PieceRemove(movehist.To, movehist.PieceMoved);
			PieceAdd(movehist.From, movehist.PieceMoved);

			//replace the captured piece
			if (movehist.Captured != ChessPiece.EMPTY)
			{
				PieceAdd(movehist.To, movehist.Captured);
			}

			//move rook back if castle
			if (movehist.PieceMoved == ChessPiece.WKing && movehist.From == ChessPosition.E1 && movehist.To == ChessPosition.G1)
			{
				this.PieceRemove(ChessPosition.F1, ChessPiece.WRook);
				this.PieceAdd(ChessPosition.H1, ChessPiece.WRook);
			}
			if (movehist.PieceMoved == ChessPiece.WKing && movehist.From == ChessPosition.E1 && movehist.To == ChessPosition.C1)
			{
				this.PieceRemove(ChessPosition.D1, ChessPiece.WRook);
				this.PieceAdd(ChessPosition.A1, ChessPiece.WRook);
			}
			if (movehist.PieceMoved == ChessPiece.BKing && movehist.From == ChessPosition.E8 && movehist.To == ChessPosition.G8)
			{
				this.PieceRemove(ChessPosition.F8, ChessPiece.BRook);
				this.PieceAdd(ChessPosition.H8, ChessPiece.BRook);
			}
			if (movehist.PieceMoved == ChessPiece.BKing && movehist.From == ChessPosition.E8 && movehist.To == ChessPosition.C8)
			{
				this.PieceRemove(ChessPosition.D8, ChessPiece.BRook);
				this.PieceAdd(ChessPosition.A8, ChessPiece.BRook);
			}

			//put back pawn if enpassant capture
			if (movehist.PieceMoved == ChessPiece.WPawn && movehist.To == movehist.Enpassant)
			{
                ChessFile tofile = movehist.To.GetFile();
				PieceAdd(tofile.ToPosition(ChessRank.Rank5), ChessPiece.BPawn);
			}
			if (movehist.PieceMoved == ChessPiece.BPawn && movehist.To == movehist.Enpassant)
			{
				ChessFile tofile = movehist.To.GetFile();
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

			_whosturn = Chess.PlayerOther(_whosturn);
			_zob = movehist.Zobrist;
			_zobPawn = movehist.ZobristPawn;

		}

		public void MoveNullApply()
		{
			//save move history
			ChessMoveHistory histobj = new ChessMoveHistory((ChessPosition)0, (ChessPosition)0, (ChessPiece)0, (ChessPiece)0, (ChessPiece)0, _enpassant, _castleWS, _castleWL, _castleBS, _castleBL, _fiftymove, _movesSinceNull, _zob, _zobPawn);
			_hist.Add(histobj);

			//reset since null count;
			_movesSinceNull = 0;

			//unmark enpassant sq
			if (_enpassant.IsInBounds())
			{
				_zob ^= ChessZobrist._enpassant[(int)_enpassant];
				_enpassant = (ChessPosition)0;
			}

			//switch whos turn
			_whosturn = Chess.PlayerOther(_whosturn);
			_zob ^= ChessZobrist._player;

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
		
			_whosturn = Chess.PlayerOther(_whosturn);
			_zob = movehist.Zobrist;
			_zobPawn = movehist.ZobristPawn;

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
				&& this._hist[_hist.Count - 2].From == (ChessPosition)0;
		}

		public ChessPiece PieceInDirection(ChessPosition from, ChessDirection dir, ref ChessPosition pos)
		{
			int i = 0;
			return PieceInDirection(from, dir, ref pos, ref i);
		}
		public ChessPiece PieceInDirection(ChessPosition from, ChessDirection dir, ref ChessPosition pos, ref int dist)
		{

			ChessPiece piece;
			pos = Chess.PositionInDirection(from, dir);
			dist = 1;
			while (pos.IsInBounds())
			{
				piece = this.PieceAt(pos);
				if (piece != ChessPiece.EMPTY) { return piece; }

				dist++;
				if (Chess.IsDirectionKnight(dir))
				{
					break;
				}
				else
				{
					pos = Chess.PositionInDirection(pos, dir);
				}
			}
			return ChessPiece.EMPTY;
		}
		public List<ChessPosition> AttacksTo(ChessPosition to, ChessPlayer by)
		{
			List<ChessPosition> allattacks = AttacksTo(to);
			List<ChessPosition> retval = new List<ChessPosition>();
			foreach (ChessPosition pos in allattacks)
			{
				if (Chess.PieceToPlayer(PieceAt(pos)) == by)
				{
					retval.Add(pos);
				}
			}
			return retval;

		}
		public List<ChessPosition> AttacksTo(ChessPosition to)
		{
			List<ChessPosition> retval = new List<ChessPosition>();
			ChessPosition pos = (ChessPosition)0;
			ChessPiece piece;
			int dist = 0;
			foreach (ChessDirection dir in Chess.AllDirectionsKnight)
			{
				piece = this.PieceInDirection(to, dir, ref pos);
				if (piece == ChessPiece.WKnight || piece == ChessPiece.BKnight)
				{
					retval.Add(pos);
				}
			}
			foreach (ChessDirection dir in Chess.AllDirectionsRook)
			{
				piece = this.PieceInDirection(to, dir, ref pos, ref dist);
				if (Chess.PieceIsSliderRook(piece))
				{
					retval.Add(pos);
				}
				if (dist == 1 && (piece == ChessPiece.WKing || piece == ChessPiece.BKing))
				{
					retval.Add(pos);
				}

			}
			foreach (ChessDirection dir in Chess.AllDirectionsBishop)
			{
				piece = this.PieceInDirection(to, dir, ref pos, ref dist);
				if (Chess.PieceIsSliderBishop(piece))
				{
					retval.Add(pos);
				}
				if (dist == 1 && (piece == ChessPiece.WKing || piece == ChessPiece.BKing))
				{
					retval.Add(pos);
				}
			}
			if (PieceAt(Chess.PositionInDirection(to, ChessDirection.DirSW)) == ChessPiece.WPawn)
			{
				retval.Add(Chess.PositionInDirection(to, ChessDirection.DirSW));
			}
			if (PieceAt(Chess.PositionInDirection(to, ChessDirection.DirSE)) == ChessPiece.WPawn)
			{
				retval.Add(Chess.PositionInDirection(to, ChessDirection.DirSE));
			}
			if (PieceAt(Chess.PositionInDirection(to, ChessDirection.DirNW)) == ChessPiece.BPawn)
			{
				retval.Add(Chess.PositionInDirection(to, ChessDirection.DirNW));
			}
			if (PieceAt(Chess.PositionInDirection(to, ChessDirection.DirNE)) == ChessPiece.BPawn)
			{
				retval.Add(Chess.PositionInDirection(to, ChessDirection.DirNE));
			}

			return retval;
		}


		public bool PositionAttacked(ChessPosition to, ChessPlayer byPlayer)
		{


			ChessPiece myknight = ChessPiece.WKnight;
			ChessPiece mybishop = ChessPiece.WBishop;
			ChessPiece myrook = ChessPiece.WRook;
			ChessPiece myqueen = ChessPiece.WQueen;
			ChessPiece myking = ChessPiece.WKing;

			if (byPlayer == ChessPlayer.Black)
			{
				myknight = ChessPiece.BKnight;
				mybishop = ChessPiece.BBishop;
				myrook = ChessPiece.BRook;
				myqueen = ChessPiece.BQueen;
				myking = ChessPiece.BKing;
			}

			ChessPosition pos = (ChessPosition)0;
			ChessPiece piece;
			int dist = 0;
			foreach (ChessDirection dir in Chess.AllDirectionsKnight)
			{
				piece = this.PieceInDirection(to, dir, ref pos);
				if (piece == myknight)
				{
					return true;
				}
			}
			foreach (ChessDirection dir in Chess.AllDirectionsRook)
			{
				piece = this.PieceInDirection(to, dir, ref pos, ref dist);
				if (piece == myrook || piece == myqueen)
				{
					return true;
				}
				
				if (dist == 1 && (piece == myking))
				{
					return true;
				}

			}
			foreach (ChessDirection dir in Chess.AllDirectionsBishop)
			{
				piece = this.PieceInDirection(to, dir, ref pos, ref dist);
				if (piece == mybishop || piece == myqueen)
				{
					return true;
				}

				if (dist == 1 && (piece == myking))
				{
					return true;
				}
			}
			if (byPlayer == ChessPlayer.White)
			{
				if (PieceAt(Chess.PositionInDirection(to, ChessDirection.DirSW)) == ChessPiece.WPawn)
				{
					return true;
				}
				if (PieceAt(Chess.PositionInDirection(to, ChessDirection.DirSE)) == ChessPiece.WPawn)
				{
					return true;
				}
			}
			else
			{
				if (PieceAt(Chess.PositionInDirection(to, ChessDirection.DirNW)) == ChessPiece.BPawn)
				{
					return true;
				}
				if (PieceAt(Chess.PositionInDirection(to, ChessDirection.DirNE)) == ChessPiece.BPawn)
				{
					return true;
				}
			}
			
			return false;
		}


	}
}
