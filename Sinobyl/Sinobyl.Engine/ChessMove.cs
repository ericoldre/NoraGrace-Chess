using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;
using System.Linq;
namespace Sinobyl.Engine
{
	/// <summary>
	/// Summary description for ChessMove.
	/// </summary>
	/// 
	public enum ChessNotationType
	{
		Coord,
		San,
		Detailed
	}

	public class ChessMoves : List<ChessMove>
	{
		public ChessMoves()
		{

		}
		public ChessMoves(IEnumerable<ChessMove> moves)
			: base(moves)
		{

		}
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (ChessMove move in this)
            {
                sb.Append(move.Description()+ " ");
            }
            return sb.ToString();
        }
		public string ToString(ChessBoard board, bool isVariation)
		{
			StringBuilder sb = new StringBuilder();
			long zobInit = board.ZobristBoard;
			foreach (ChessMove move in this)
			{
				if (isVariation && board.WhosTurn==Player.White)
				{
					sb.Append(board.FullMoveCount.ToString() + ". ");
				}
				sb.Append(move.Description(board) + " ");
				if (isVariation)
				{
					board.MoveApply(move);
				}
			}
			if (isVariation)
			{
				foreach (ChessMove move in this)
				{
					board.MoveUndo();
				}
			}
			return sb.ToString();
		}

	}

    [System.Diagnostics.DebuggerDisplay(@"{Sinobyl.Engine.ChessMoveInfo.Description(this),nq}")]
    public enum ChessMove 
    {
        EMPTY = 0
    }

	public static partial class ChessMoveInfo
	{

        public static ChessMove Create(ChessPosition from, ChessPosition to)
        {
            System.Diagnostics.Debug.Assert((int)from >= 0 && (int)from <= 63);
            System.Diagnostics.Debug.Assert((int)to >= 0 && (int)to <= 63);

            return (ChessMove)((int)from | ((int)to << 6));
        }

        public static ChessMove Create(ChessPosition from, ChessPosition to, Piece promote)
        {
            System.Diagnostics.Debug.Assert((int)from >= 0 && (int)from <= 63);
            System.Diagnostics.Debug.Assert((int)to >= 0 && (int)to <= 63);
            System.Diagnostics.Debug.Assert(promote == Piece.WKnight || promote == Piece.WBishop || promote == Piece.WRook || promote == Piece.WQueen || promote == Piece.BKnight || promote == Piece.BBishop || promote == Piece.BRook || promote == Piece.BQueen);


            return (ChessMove)((int)from | ((int)to << 6) | ((int) promote << 12));
        }

        public static ChessPosition From(this ChessMove move)
        {
            return (ChessPosition)((int)move & 0x3F);
        }

        public static ChessPosition To(this ChessMove move)
        {
            return (ChessPosition)((int)move >> 6 & 0x3F);
        }

        public static Piece Promote(this ChessMove move)
        {
            return (Piece)((int)move >> 12 & 0x3F);
        }

        



		public static ChessMove Parse(ChessBoard board, string movetext)
		{
			Piece Promote = Piece.EMPTY;//unless changed below
			ChessPosition From = (ChessPosition.OUTOFBOUNDS);
			ChessPosition To = (ChessPosition.OUTOFBOUNDS);
			Regex regex = new Regex("");

			movetext = movetext.Replace("+", "");
			movetext = movetext.Replace("x", "");
			movetext = movetext.Replace("#", "");
			movetext = movetext.Replace("=", "");

			Player me = board.WhosTurn;
			Piece mypawn = board.WhosTurn == Player.White ? Piece.WPawn : Piece.BPawn;
			Piece myknight = board.WhosTurn == Player.White ? Piece.WKnight : Piece.BKnight;
			Piece mybishop = board.WhosTurn == Player.White ? Piece.WBishop : Piece.BBishop;
			Piece myrook = board.WhosTurn == Player.White ? Piece.WRook : Piece.BRook;
			Piece myqueen = board.WhosTurn == Player.White ? Piece.WQueen : Piece.BQueen;
			Piece myking = board.WhosTurn == Player.White ? Piece.WKing : Piece.BKing;

			Direction mynorth = board.WhosTurn == Player.White ? Direction.DirN : Direction.DirS;
			Direction mysouth = board.WhosTurn == Player.White ? Direction.DirS : Direction.DirN;
			Rank myrank4 = board.WhosTurn == Player.White ? Rank.Rank4 : Rank.Rank5;


			ChessPosition tmppos;
			Piece tmppiece;
			File tmpfile;
			Rank tmprank;

			if (Regex.IsMatch(movetext, "^[abcdefgh][12345678][abcdefgh][12345678]$", RegexOptions.IgnoreCase))
			{
				//coordinate notation, will not verify legality for now
                From = ChessPositionInfo.Parse(movetext.Substring(0, 2));
                To = ChessPositionInfo.Parse(movetext.Substring(2, 2));
			}
			else if (Regex.IsMatch(movetext, "^[abcdefgh][12345678][abcdefgh][12345678][BNRQK]$", RegexOptions.IgnoreCase))
			{
				//coordinate notation, with promotion
                From = ChessPositionInfo.Parse(movetext.Substring(0, 2));
				To = ChessPositionInfo.Parse(movetext.Substring(2, 2));
				Promote = movetext[4].ParseAsPiece(me);
                return Create(From, To, Promote);
			}
			else if (movetext == "0-0" || movetext == "O-O" || movetext == "o-o")
			{
				if (me == Player.White)
				{
					From = ChessPosition.E1;
					To = ChessPosition.G1;
				}
				else
				{
					From = ChessPosition.E8;
					To = ChessPosition.G8;
				}
			}
			else if (movetext == "0-0-0" || movetext == "O-O-O" || movetext == "o-o-o")
			{
				if (me == Player.White)
				{
					From = ChessPosition.E1;
					To = ChessPosition.C1;
				}
				else
				{
					From = ChessPosition.E8;
					To = ChessPosition.C8;
				}
			}
			else if (Regex.IsMatch(movetext, "^[abcdefgh][12345678]$"))
			{
				//pawn forward
                To = ChessPositionInfo.Parse(movetext);
                tmppos = To.PositionInDirection(mysouth);
				if (board.PieceAt(tmppos) == mypawn)
				{
					From = tmppos;
                    return Create(From, To);
				}
				else if (board.PieceAt(tmppos) == Piece.EMPTY && To.ToRank() == myrank4)
				{
                    tmppos = tmppos.PositionInDirection(mysouth);
					if (board.PieceAt(tmppos) == mypawn)
					{
						From = tmppos;
                        return Create(From, To);
					}
				}
                throw new ArgumentException("no pawn can move to " + movetext);

			}
			else if (Regex.IsMatch(movetext, "^[abcdefgh][12345678][BNRQK]$"))
			{
				//pawn forward, promotion
                To = ChessPositionInfo.Parse(movetext.Substring(0, 2));
                tmppos = To.PositionInDirection(mysouth);
				if (board.PieceAt(tmppos) == mypawn)
				{
					From = tmppos;
					Promote = movetext[2].ParseAsPiece(me);
                    return Create(From, To, Promote);
				}
                throw new ArgumentException("no pawn can promoted to " + movetext.Substring(0, 2));
			}
			else if (Regex.IsMatch(movetext, "^[abcdefgh][abcdefgh][12345678]$"))
			{
				//pawn attack
                To = ChessPositionInfo.Parse(movetext.Substring(1, 2));
				tmpfile = FileInfo.Parse(movetext[0]);
				From = ParseFilter(board, To, mypawn, tmpfile, Rank.EMPTY);
                return Create(From, To);
			}
			else if (Regex.IsMatch(movetext, "^[abcdefgh][abcdefgh][12345678][BNRQK]$"))
			{
				//pawn attack, promote
				To = ChessPositionInfo.Parse(movetext.Substring(1, 2));
                tmpfile = FileInfo.Parse(movetext[0]);
				From = ParseFilter(board, To, mypawn, tmpfile, Rank.EMPTY);
				Promote = movetext[3].ParseAsPiece(me);
                return Create(From, To, Promote);
			}
			else if (Regex.IsMatch(movetext, "^[BNRQK][abcdefgh][12345678]$"))
			{
				//normal attack
				To = ChessPositionInfo.Parse(movetext.Substring(1, 2));
				tmppiece = movetext[0].ParseAsPiece(me);
				From = ParseFilter(board, To, tmppiece, File.EMPTY, Rank.EMPTY);
                return Create(From, To);
			}
			else if (Regex.IsMatch(movetext, "^[BNRQK][abcdefgh][abcdefgh][12345678]$"))
			{
				//normal, specify file
				To = ChessPositionInfo.Parse(movetext.Substring(2, 2));
				tmppiece = movetext[0].ParseAsPiece(me);
                tmpfile = FileInfo.Parse(movetext[1]);
				From = ParseFilter(board, To, tmppiece, tmpfile, Rank.EMPTY);
                return Create(From, To);
			}
			else if (Regex.IsMatch(movetext, "^[BNRQK][12345678][abcdefgh][12345678]$"))
			{
				//normal, specify rank
				To = ChessPositionInfo.Parse(movetext.Substring(2, 2));
				tmppiece = movetext[0].ParseAsPiece(me);
                tmprank = RankInfo.Parse(movetext[1]);
				From = ParseFilter(board, To, tmppiece, File.EMPTY, tmprank);
                return Create(From, To);

			}
			else if (Regex.IsMatch(movetext, "^[BNRQK][abcdefgh][12345678][abcdefgh][12345678]$"))
			{
				//normal, specify rank and file
				To = ChessPositionInfo.Parse(movetext.Substring(3, 2));
				tmppiece = movetext[0].ParseAsPiece(me);
                tmpfile = FileInfo.Parse(movetext[1]);
                tmprank = RankInfo.Parse(movetext[2]);
				From = ParseFilter(board, To, tmppiece, tmpfile, tmprank);
                return Create(From, To);
			}
            return ChessMoveInfo.Create(From, To);
		}

		private static ChessPosition ParseFilter(ChessBoard board, ChessPosition attackto, Piece piece, File file, Rank rank)
		{
            List<ChessPosition> fits = new List<ChessPosition>();
            var attacksTo = board.AttacksTo(attackto, board.WhosTurn);
			foreach(ChessPosition pos in attacksTo.ToPositions())
			{
				if (piece != Piece.EMPTY && piece != board.PieceAt(pos))
				{
                    continue;
				}
				if (rank != Rank.EMPTY && rank != pos.ToRank())
				{
                    continue;
				}
				if (file != File.EMPTY && file != pos.ToFile())
				{
                    continue;
				}
                fits.Add(pos);
			}

            if (fits.Count > 1)
			{
				//ambigous moves, one is probably illegal, check against legal move list
                ChessMoves allLegal = new ChessMoves(ChessMoveInfo.GenMovesLegal(board));
                fits.Clear();
				foreach (ChessMove move in allLegal)
				{
					if (move.To() != attackto) { continue; }
					if (board.PieceAt(move.From()) != piece) { continue; }
					if (file != File.EMPTY && move.From().ToFile() != file) { continue; }
					if (rank != Rank.EMPTY && move.From().ToRank() != rank) { continue; }
                    fits.Add(move.From());
				}
			}

            if (fits.Count != 1)
			{
                throw new ArgumentException("invalid move input");
				
			}

            return fits[0];
		}

        public static bool IsLegal(this ChessMove move, ChessBoard board)
        {
            ChessMoves legalmoves = new ChessMoves(ChessMoveInfo.GenMovesLegal(board));
            foreach (ChessMove legalmove in legalmoves)
            {
                if (legalmove == move) { return true; }
            }
            return false;
        }

		public static string Description(this ChessMove move)
		{
			string retval = "";
			if (move.Promote() == Piece.EMPTY)
			{
				retval = move.From().PositionToString().ToLower() + move.To().PositionToString().ToLower();
			}
			else
			{
                retval = move.From().PositionToString().ToLower() + move.To().PositionToString().ToLower() + move.Promote().PieceToString().ToLower();
			}
			return retval;
		}

		
		public static string Description(this ChessMove move, ChessBoard board)
		{
			string retval = "";
            Piece piece = board.PieceAt(move.From());
            bool iscap = (board.PieceAt(move.To()) != Piece.EMPTY);

            Rank fromrank = move.From().ToRank();
            File fromfile = move.From().ToFile();
            bool isprom = move.Promote() != Piece.EMPTY;

            string sTo = (move.To().PositionToString());
            string sPiece = piece.PieceToString().ToUpper();
            string sRank = fromrank.RankToString().ToLower();
            string sFile = fromfile.FileToString().ToLower();
			string sProm = "";
			
			//enpassant cap
            if (move.To() == board.EnPassant && (piece == Piece.WPawn || piece == Piece.BPawn))
			{
				iscap = true;
			}
			
			if (isprom)
			{
                sProm = move.Promote().PieceToString().ToUpper();
			}

			if (piece == Piece.WPawn || piece == Piece.BPawn)
			{
				if (iscap)
				{
					retval += sFile + "x";
				}
				retval += sTo;
				if (isprom)
				{
					retval += sProm;
				}
			}
            else if (piece == Piece.WKing && move.From() == ChessPosition.E1 && move.To() == ChessPosition.G1)
			{
				retval += "O-O";
			}
            else if (piece == Piece.BKing && move.From() == ChessPosition.E8 && move.To() == ChessPosition.G8)
			{
				retval += "O-O";
			}
            else if (piece == Piece.WKing && move.From() == ChessPosition.E1 && move.To() == ChessPosition.C1)
			{
				retval += "O-O-O";
			}
            else if (piece == Piece.BKing && move.From() == ChessPosition.E8 && move.To() == ChessPosition.C8)
			{
				retval += "O-O-O";
			}
			else
			{
				bool pieceunique = true;
				bool fileunique = true;
				bool rankunique = true;
                foreach (ChessPosition pos in board.AttacksTo(move.To(), piece.PieceToPlayer()).ToPositions())
				{
                    if (pos == move.From()) { continue; }

					Piece otherpiece = board.PieceAt(pos);
					if (otherpiece == piece)
					{
						pieceunique = false;
						if (pos.ToRank() == fromrank)
						{
							rankunique = false;
						}
						if (pos.ToFile() == fromfile)
						{
							fileunique = false;
						}
					}
				}
				retval += sPiece;
				if (pieceunique)
				{
				}
				else if (fileunique)
				{
					retval += sFile;
				}
				else if (rankunique)
				{
					retval += sRank;
				}
				else
				{
					retval += sFile + sRank;
				}

				if (iscap)
				{
					retval += "x";
				}
				retval += sTo;
			}
            board.MoveApply(move);
			if (board.IsCheck())
			{
				if (ChessMoveInfo.GenMovesLegal(board).Any())
				{
					retval += "+";
				}
				else
				{
					retval += "#";
				}
			}
			board.MoveUndo();
			return retval;
		}


        public static bool IsPsuedoLegal(this ChessMove move, ChessBoard board)
        {
            if (move == ChessMove.EMPTY) { return false; }
            
            var from = move.From();
            var to = move.To();
            
            
            var piece = board.PieceAt(from);
            if (piece == Piece.EMPTY) { return false; }
            var pieceType = piece.ToPieceType();
            var me = piece.PieceToPlayer();

            if (board.WhosTurn != me) { return false; }            
            if (board[me].Contains(to)) { return false; }

            if (move.Promote() != Piece.EMPTY)
            {
                if (pieceType != ChessPieceType.Pawn) { return false; }
                if (to.ToRank() != Rank.Rank1 && to.ToRank() != Rank.Rank8) { return false; }
                if (move.Promote().PieceToPlayer() != me) { return false; }
            }

            var targets = ~board[me];

            switch (pieceType)
            {
                case ChessPieceType.Knight:
                    return (Attacks.KnightAttacks(from) & targets).Contains(to);
                case ChessPieceType.Bishop:
                    return (Attacks.BishopAttacks(from, board.PieceLocationsAll) & targets).Contains(to);
                case ChessPieceType.Rook:
                    return (Attacks.RookAttacks(from, board.PieceLocationsAll) & targets).Contains(to);
                case ChessPieceType.Queen:
                    return (Attacks.QueenAttacks(from, board.PieceLocationsAll) & targets).Contains(to);
                case ChessPieceType.King:
                    if ((Attacks.KingAttacks(from) & targets).Contains(to))
                    {
                        return true;
                    }
                    else if(me== Player.White
                        && from == ChessPosition.E1
                        && to == ChessPosition.G1
                        && board.PieceAt(ChessPosition.F1) == Piece.EMPTY
                        && board.PieceAt(ChessPosition.G1) == Piece.EMPTY
                        && (board.CastleRights & CastleFlags.WhiteShort) == CastleFlags.WhiteShort
                        && !board.IsCheck()
                        && !board.AttacksTo(ChessPosition.F1).Contains(targets)
                        && !board.AttacksTo(ChessPosition.G1).Contains(targets))
                    {
                        return true;
                    }
                    else if (me == Player.White
                        && from == ChessPosition.E1
                        && to == ChessPosition.C1
                        && board.PieceAt(ChessPosition.B1) == Piece.EMPTY
                        && board.PieceAt(ChessPosition.C1) == Piece.EMPTY
                        && board.PieceAt(ChessPosition.D1) == Piece.EMPTY
                        && (board.CastleRights & CastleFlags.WhiteLong) == CastleFlags.WhiteLong
                        && !board.IsCheck()
                        && !board.AttacksTo(ChessPosition.D1).Contains(targets)
                        && !board.AttacksTo(ChessPosition.C1).Contains(targets))
                    {
                        return true;
                    }
                    else if (me == Player.Black
                        && from == ChessPosition.E8
                        && to == ChessPosition.G8
                        && board.PieceAt(ChessPosition.F8) == Piece.EMPTY
                        && board.PieceAt(ChessPosition.G8) == Piece.EMPTY
                        && (board.CastleRights & CastleFlags.BlackShort) == CastleFlags.BlackShort
                        && !board.IsCheck()
                        && !board.AttacksTo(ChessPosition.F8).Contains(targets)
                        && !board.AttacksTo(ChessPosition.G8).Contains(targets))
                    {
                        return true;
                    }
                    else if (me == Player.Black
                        && from == ChessPosition.E8
                        && to == ChessPosition.C8
                        && board.PieceAt(ChessPosition.B8) == Piece.EMPTY
                        && board.PieceAt(ChessPosition.C8) == Piece.EMPTY
                        && board.PieceAt(ChessPosition.D8) == Piece.EMPTY
                        && (board.CastleRights & CastleFlags.BlackLong) == CastleFlags.BlackLong
                        && !board.IsCheck()
                        && !board.AttacksTo(ChessPosition.D8).Contains(targets)
                        && !board.AttacksTo(ChessPosition.C8).Contains(targets))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case ChessPieceType.Pawn:
                    if (Attacks.PawnAttacks(from, me).Contains(to))
                    {
                        return board[me.PlayerOther()].Contains(to) || to == board.EnPassant;
                    }
                    else if (from.PositionInDirection(me.MyNorth()) == to 
                        && board.PieceAt(to) == Piece.EMPTY)
                    {
                        return true;
                    }
                    else if(from.ToRank() == me.MyRank2()   //rank 2
                        && from.PositionInDirection(me.MyNorth()).PositionInDirection(me.MyNorth()) == to //is double jump
                        && board.PieceAt(to) == Piece.EMPTY //target empty
                        && board.PieceAt(from.PositionInDirection(me.MyNorth())) == Piece.EMPTY) //single jump empty
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                default:
                    return false;
            }

        }


	}


}
