using System;
using System.Collections.Generic;
using System.Text;


namespace Sinobyl.Engine
{
	/// <summary>
	/// Summary description for ChessZobrist.
	/// </summary>
	public class ChessZobrist
	{
		private static readonly Int64[,] _piecepos = new Int64[12, 64];
		private static readonly Int64[] _enpassant = new Int64[64];

        private static readonly Int64 _castleWS;
        private static readonly Int64 _castleWL;
        private static readonly Int64 _castleBS;
        private static readonly Int64 _castleBL;
        private static readonly Int64 _player;

		static ChessZobrist()
		{
			Random rand = new Random(12345);

			//initialize the castling zob keys
			_castleWS = Rand64(rand);
			_castleWL = Rand64(rand);
			_castleBS = Rand64(rand);
			_castleBL = Rand64(rand);
			_player = Rand64(rand);

			//initialize the piecepos and enpassant zob keys
            foreach (ChessPosition pos in ChessPositionInfo.AllPositions)
			{
				_enpassant[(int)pos] = Rand64(rand);
				foreach (ChessPiece piece in ChessPieceInfo.AllPieces)
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
		public static Int64 PiecePosition(ChessPiece piece, ChessPosition pos)
		{
			//Chess.AssertPiece(piece);
			//Chess.AssertPosition(pos);
			return _piecepos[(int)piece, (int)pos];
		}
		public static Int64 Enpassant(ChessPosition pos)
		{
			//Chess.AssertPosition(pos);
			return _enpassant[(int)pos];
		}

        public static Int64 Material(ChessPiece piece)
        {
            return _piecepos[(int)piece, 0];
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
		public static Int64 Player
		{
            get { return _player; }
		}
		public static Int64 BoardZobPawn(ChessBoard board)
		{
			Int64 retval = 0;
            foreach (ChessPosition pos in ChessPositionInfo.AllPositions)
			{
				ChessPiece piece = board.PieceAt(pos);
				if (piece == ChessPiece.WPawn || piece == ChessPiece.BPawn)
				{
					retval ^= PiecePosition(piece, pos);
				}
			}
			return retval;
		}
        public static Int64 BoardZobMaterial(ChessBoard board)
        {
            Int64 retval = 0;
            foreach (ChessPosition pos in ChessPositionInfo.AllPositions)
            {
                ChessPiece piece = board.PieceAt(pos);
                if (piece != ChessPiece.EMPTY)
                {
                    retval ^= Material(piece);
                }
            }
            return retval;

        }
		public static Int64 BoardZob(ChessBoard board)
		{
			Int64 retval = 0;
            foreach (ChessPosition pos in ChessPositionInfo.AllPositions)
			{
				ChessPiece piece = board.PieceAt(pos);
				if (piece != ChessPiece.EMPTY)
				{
					retval ^= PiecePosition(piece, pos);
				}
			}
			if (board.WhosTurn == ChessPlayer.Black)
			{
				retval ^= ChessZobrist.Player;
			}
			if (board.CastleAvailWS)
			{
				retval ^= ChessZobrist.CastleWS;
			}
			if (board.CastleAvailWL)
			{
				retval ^= ChessZobrist.CastleWL;
			}
			if (board.CastleAvailBS)
			{
				retval ^= ChessZobrist.CastleBS;
			}
			if (board.CastleAvailBL)
			{
				retval ^= ChessZobrist.CastleBL;
			}
			if (board.EnPassant.IsInBounds())
			{
				retval ^= ChessZobrist.Enpassant(board.EnPassant);
			}
			return retval;

		}
	}
}