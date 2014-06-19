using System;
using System.Collections.Generic;
using System.Text;


namespace NoraGrace.Engine
{
	/// <summary>
	/// Summary description for ChessZobrist.
	/// </summary>
	public class Zobrist
	{
        private static readonly Int64[,] _piecepos = new Int64[PieceInfo.LookupArrayLength, 64];
		private static readonly Int64[] _enpassant = new Int64[64];

        private static readonly Int64 _castleWS;
        private static readonly Int64 _castleWL;
        private static readonly Int64 _castleBS;
        private static readonly Int64 _castleBL;
        private static readonly Int64 _player;

		static Zobrist()
		{
			Random rand = new Random(12345);

			//initialize the castling zob keys
			_castleWS = Rand64(rand);
			_castleWL = Rand64(rand);
			_castleBS = Rand64(rand);
			_castleBL = Rand64(rand);
			_player = Rand64(rand);

			//initialize the piecepos and enpassant zob keys
            foreach (Position pos in PositionInfo.AllPositions)
			{
				_enpassant[(int)pos] = Rand64(rand);
				foreach (Piece piece in PieceInfo.AllPieces)
				{
					_piecepos[(int)piece, (int)pos] = Rand64(rand);
				}
			}
		}
        

		private static Int64 Rand64(Random rand)
		{
			byte[] bytes = new byte[8];
			rand.NextBytes(bytes);
			Int64 retval = 0;
			for (int i = 0; i <= 7; i++)
			{
				//Int64 ibyte = (Int64)bytes[i]&256;
				Int64 ibyte = (Int64)bytes[i];
				retval |= ibyte << (i * 8);
			}
			return retval;
		}
		public static Int64 PiecePosition(Piece piece, Position pos)
		{
			//Chess.AssertPiece(piece);
			//Chess.AssertPosition(pos);
			return _piecepos[(int)piece, (int)pos];
		}
		public static Int64 Enpassant(Position pos)
		{
			//Chess.AssertPosition(pos);
			return _enpassant[(int)pos];
		}

        public static Int64 Material(Piece piece, int pieceCountBesidesThis)
        {
            return _piecepos[(int)piece, pieceCountBesidesThis];
        }
		public static Int64 CastleWS
		{
            get { return _castleWS; }
		}
		public static Int64 CastleWL
		{
            get { return _castleWL; }
		}
		public static Int64 CastleBS
		{
            get { return _castleBS; }
		}
		public static Int64 CastleBL
		{
            get { return _castleBL; }
			
		}
		public static Int64 PlayerKey
		{
            get { return _player; }
		}
		public static Int64 BoardZobPawn(Board board)
		{
			Int64 retval = 0;
            foreach (Position pos in PositionInfo.AllPositions)
			{
				Piece piece = board.PieceAt(pos);
				if (piece == Piece.WPawn || piece == Piece.BPawn)
				{
					retval ^= PiecePosition(piece, pos);
				}
			}
			return retval;
		}
        public static Int64 BoardZobMaterial(Board board)
        {
            Int64 retval = 0;
            foreach (Piece piece in PieceInfo.AllPieces)
            {
                for (int i = 0; i < board.PieceCount(piece); i++)
                {
                    retval ^= Material(piece, i);
                }
            }
            return retval;

        }
		public static Int64 BoardZob(Board board)
		{
			Int64 retval = 0;
            foreach (Position pos in PositionInfo.AllPositions)
			{
				Piece piece = board.PieceAt(pos);
				if (piece != Piece.EMPTY)
				{
					retval ^= PiecePosition(piece, pos);
				}
			}
			if (board.WhosTurn == Player.Black)
			{
				retval ^= Zobrist.PlayerKey;
			}
            if ((board.CastleRights & CastleFlags.WhiteShort) != 0)
			{
				retval ^= Zobrist.CastleWS;
			}
            if ((board.CastleRights & CastleFlags.WhiteLong) != 0)
			{
				retval ^= Zobrist.CastleWL;
			}
            if ((board.CastleRights & CastleFlags.BlackShort) != 0)
			{
				retval ^= Zobrist.CastleBS;
			}
            if ((board.CastleRights & CastleFlags.BlackLong) != 0)
			{
				retval ^= Zobrist.CastleBL;
			}
			if (board.EnPassant.IsInBounds())
			{
				retval ^= Zobrist.Enpassant(board.EnPassant);
			}
			return retval;

		}
	}
}