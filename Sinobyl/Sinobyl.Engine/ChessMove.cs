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

    [System.Diagnostics.DebuggerDisplay(@"{Sinobyl.Engine.ChessMoveInfo.Description(this),nq}")]
    public enum Move 
    {
        EMPTY = 0
    }

	public static partial class MoveInfo
	{

        public static Move Create(Position from, Position to)
        {
            System.Diagnostics.Debug.Assert((int)from >= 0 && (int)from <= 63);
            System.Diagnostics.Debug.Assert((int)to >= 0 && (int)to <= 63);

            return (Move)((int)from | ((int)to << 6));
        }

        public static Move Create(Position from, Position to, Piece promote)
        {
            System.Diagnostics.Debug.Assert((int)from >= 0 && (int)from <= 63);
            System.Diagnostics.Debug.Assert((int)to >= 0 && (int)to <= 63);
            System.Diagnostics.Debug.Assert(promote == Piece.WKnight || promote == Piece.WBishop || promote == Piece.WRook || promote == Piece.WQueen || promote == Piece.BKnight || promote == Piece.BBishop || promote == Piece.BRook || promote == Piece.BQueen);


            return (Move)((int)from | ((int)to << 6) | ((int) promote << 12));
        }

        public static Position From(this Move move)
        {
            return (Position)((int)move & 0x3F);
        }

        public static Position To(this Move move)
        {
            return (Position)((int)move >> 6 & 0x3F);
        }

        public static Piece Promote(this Move move)
        {
            return (Piece)((int)move >> 12 & 0x3F);
        }

        



		public static Move Parse(Board board, string movetext)
		{
			Piece Promote = Piece.EMPTY;//unless changed below
			Position From = (Position.OUTOFBOUNDS);
			Position To = (Position.OUTOFBOUNDS);
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


			Position tmppos;
			Piece tmppiece;
			File tmpfile;
			Rank tmprank;

			if (Regex.IsMatch(movetext, "^[abcdefgh][12345678][abcdefgh][12345678]$", RegexOptions.IgnoreCase))
			{
				//coordinate notation, will not verify legality for now
                From = PositionInfo.Parse(movetext.Substring(0, 2));
                To = PositionInfo.Parse(movetext.Substring(2, 2));
			}
			else if (Regex.IsMatch(movetext, "^[abcdefgh][12345678][abcdefgh][12345678][BNRQK]$", RegexOptions.IgnoreCase))
			{
				//coordinate notation, with promotion
                From = PositionInfo.Parse(movetext.Substring(0, 2));
				To = PositionInfo.Parse(movetext.Substring(2, 2));
				Promote = movetext[4].ParseAsPiece(me);
                return Create(From, To, Promote);
			}
			else if (movetext == "0-0" || movetext == "O-O" || movetext == "o-o")
			{
				if (me == Player.White)
				{
					From = Position.E1;
					To = Position.G1;
				}
				else
				{
					From = Position.E8;
					To = Position.G8;
				}
			}
			else if (movetext == "0-0-0" || movetext == "O-O-O" || movetext == "o-o-o")
			{
				if (me == Player.White)
				{
					From = Position.E1;
					To = Position.C1;
				}
				else
				{
					From = Position.E8;
					To = Position.C8;
				}
			}
			else if (Regex.IsMatch(movetext, "^[abcdefgh][12345678]$"))
			{
				//pawn forward
                To = PositionInfo.Parse(movetext);
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
                To = PositionInfo.Parse(movetext.Substring(0, 2));
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
                To = PositionInfo.Parse(movetext.Substring(1, 2));
				tmpfile = FileInfo.Parse(movetext[0]);
				From = ParseFilter(board, To, mypawn, tmpfile, Rank.EMPTY);
                return Create(From, To);
			}
			else if (Regex.IsMatch(movetext, "^[abcdefgh][abcdefgh][12345678][BNRQK]$"))
			{
				//pawn attack, promote
				To = PositionInfo.Parse(movetext.Substring(1, 2));
                tmpfile = FileInfo.Parse(movetext[0]);
				From = ParseFilter(board, To, mypawn, tmpfile, Rank.EMPTY);
				Promote = movetext[3].ParseAsPiece(me);
                return Create(From, To, Promote);
			}
			else if (Regex.IsMatch(movetext, "^[BNRQK][abcdefgh][12345678]$"))
			{
				//normal attack
				To = PositionInfo.Parse(movetext.Substring(1, 2));
				tmppiece = movetext[0].ParseAsPiece(me);
				From = ParseFilter(board, To, tmppiece, File.EMPTY, Rank.EMPTY);
                return Create(From, To);
			}
			else if (Regex.IsMatch(movetext, "^[BNRQK][abcdefgh][abcdefgh][12345678]$"))
			{
				//normal, specify file
				To = PositionInfo.Parse(movetext.Substring(2, 2));
				tmppiece = movetext[0].ParseAsPiece(me);
                tmpfile = FileInfo.Parse(movetext[1]);
				From = ParseFilter(board, To, tmppiece, tmpfile, Rank.EMPTY);
                return Create(From, To);
			}
			else if (Regex.IsMatch(movetext, "^[BNRQK][12345678][abcdefgh][12345678]$"))
			{
				//normal, specify rank
				To = PositionInfo.Parse(movetext.Substring(2, 2));
				tmppiece = movetext[0].ParseAsPiece(me);
                tmprank = RankInfo.Parse(movetext[1]);
				From = ParseFilter(board, To, tmppiece, File.EMPTY, tmprank);
                return Create(From, To);

			}
			else if (Regex.IsMatch(movetext, "^[BNRQK][abcdefgh][12345678][abcdefgh][12345678]$"))
			{
				//normal, specify rank and file
				To = PositionInfo.Parse(movetext.Substring(3, 2));
				tmppiece = movetext[0].ParseAsPiece(me);
                tmpfile = FileInfo.Parse(movetext[1]);
                tmprank = RankInfo.Parse(movetext[2]);
				From = ParseFilter(board, To, tmppiece, tmpfile, tmprank);
                return Create(From, To);
			}
            return MoveInfo.Create(From, To);
		}

		private static Position ParseFilter(Board board, Position attackto, Piece piece, File file, Rank rank)
		{
            List<Position> fits = new List<Position>();
            var attacksTo = board.AttacksTo(attackto, board.WhosTurn);
			foreach(Position pos in attacksTo.ToPositions())
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
                List<Move> allLegal = new List<Move>(MoveInfo.GenMovesLegal(board));
                fits.Clear();
				foreach (Move move in allLegal)
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

        public static bool IsLegal(this Move move, Board board)
        {
            List<Move> legalmoves = new List<Move>(MoveInfo.GenMovesLegal(board));
            foreach (Move legalmove in legalmoves)
            {
                if (legalmove == move) { return true; }
            }
            return false;
        }

		public static string Description(this Move move)
		{
			string retval = "";
			if (move.Promote() == Piece.EMPTY)
			{
				retval = move.From().Name().ToLower() + move.To().Name().ToLower();
			}
			else
			{
                retval = move.From().Name().ToLower() + move.To().Name().ToLower() + move.Promote().PieceToString().ToLower();
			}
			return retval;
		}

		
		public static string Description(this Move move, Board board)
		{
			string retval = "";
            Piece piece = board.PieceAt(move.From());
            bool iscap = (board.PieceAt(move.To()) != Piece.EMPTY);

            Rank fromrank = move.From().ToRank();
            File fromfile = move.From().ToFile();
            bool isprom = move.Promote() != Piece.EMPTY;

            string sTo = (move.To().Name());
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
            else if (piece == Piece.WKing && move.From() == Position.E1 && move.To() == Position.G1)
			{
				retval += "O-O";
			}
            else if (piece == Piece.BKing && move.From() == Position.E8 && move.To() == Position.G8)
			{
				retval += "O-O";
			}
            else if (piece == Piece.WKing && move.From() == Position.E1 && move.To() == Position.C1)
			{
				retval += "O-O-O";
			}
            else if (piece == Piece.BKing && move.From() == Position.E8 && move.To() == Position.C8)
			{
				retval += "O-O-O";
			}
			else
			{
				bool pieceunique = true;
				bool fileunique = true;
				bool rankunique = true;
                foreach (Position pos in board.AttacksTo(move.To(), piece.PieceToPlayer()).ToPositions())
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
				if (MoveInfo.GenMovesLegal(board).Any())
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


        public static bool IsPsuedoLegal(this Move move, Board board)
        {
            if (move == Move.EMPTY) { return false; }
            
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
                if (pieceType != PieceType.Pawn) { return false; }
                if (to.ToRank() != Rank.Rank1 && to.ToRank() != Rank.Rank8) { return false; }
                if (move.Promote().PieceToPlayer() != me) { return false; }
            }

            var targets = ~board[me];

            switch (pieceType)
            {
                case PieceType.Knight:
                    return (Attacks.KnightAttacks(from) & targets).Contains(to);
                case PieceType.Bishop:
                    return (Attacks.BishopAttacks(from, board.PieceLocationsAll) & targets).Contains(to);
                case PieceType.Rook:
                    return (Attacks.RookAttacks(from, board.PieceLocationsAll) & targets).Contains(to);
                case PieceType.Queen:
                    return (Attacks.QueenAttacks(from, board.PieceLocationsAll) & targets).Contains(to);
                case PieceType.King:
                    if ((Attacks.KingAttacks(from) & targets).Contains(to))
                    {
                        return true;
                    }
                    else if(me== Player.White
                        && from == Position.E1
                        && to == Position.G1
                        && board.PieceAt(Position.F1) == Piece.EMPTY
                        && board.PieceAt(Position.G1) == Piece.EMPTY
                        && (board.CastleRights & CastleFlags.WhiteShort) == CastleFlags.WhiteShort
                        && !board.IsCheck()
                        && !board.AttacksTo(Position.F1).Contains(targets)
                        && !board.AttacksTo(Position.G1).Contains(targets))
                    {
                        return true;
                    }
                    else if (me == Player.White
                        && from == Position.E1
                        && to == Position.C1
                        && board.PieceAt(Position.B1) == Piece.EMPTY
                        && board.PieceAt(Position.C1) == Piece.EMPTY
                        && board.PieceAt(Position.D1) == Piece.EMPTY
                        && (board.CastleRights & CastleFlags.WhiteLong) == CastleFlags.WhiteLong
                        && !board.IsCheck()
                        && !board.AttacksTo(Position.D1).Contains(targets)
                        && !board.AttacksTo(Position.C1).Contains(targets))
                    {
                        return true;
                    }
                    else if (me == Player.Black
                        && from == Position.E8
                        && to == Position.G8
                        && board.PieceAt(Position.F8) == Piece.EMPTY
                        && board.PieceAt(Position.G8) == Piece.EMPTY
                        && (board.CastleRights & CastleFlags.BlackShort) == CastleFlags.BlackShort
                        && !board.IsCheck()
                        && !board.AttacksTo(Position.F8).Contains(targets)
                        && !board.AttacksTo(Position.G8).Contains(targets))
                    {
                        return true;
                    }
                    else if (me == Player.Black
                        && from == Position.E8
                        && to == Position.C8
                        && board.PieceAt(Position.B8) == Piece.EMPTY
                        && board.PieceAt(Position.C8) == Piece.EMPTY
                        && board.PieceAt(Position.D8) == Piece.EMPTY
                        && (board.CastleRights & CastleFlags.BlackLong) == CastleFlags.BlackLong
                        && !board.IsCheck()
                        && !board.AttacksTo(Position.D8).Contains(targets)
                        && !board.AttacksTo(Position.C8).Contains(targets))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case PieceType.Pawn:
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

        public static string Descriptions(this IEnumerable<Move> moves, Board board, bool isVariation)
        {
            StringBuilder sb = new StringBuilder();
            long zobInit = board.ZobristBoard;
            foreach (Move move in moves)
            {
                if (isVariation && board.WhosTurn == Player.White)
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
                foreach (Move move in moves)
                {
                    board.MoveUndo();
                }
            }
            return sb.ToString();
        }
	}


}
