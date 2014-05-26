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
			long zobInit = board.Zobrist;
			foreach (ChessMove move in this)
			{
				if (isVariation && board.WhosTurn==ChessPlayer.White)
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

        public static ChessMove Create(ChessPosition from, ChessPosition to, ChessPiece promote)
        {
            System.Diagnostics.Debug.Assert((int)from >= 0 && (int)from <= 63);
            System.Diagnostics.Debug.Assert((int)to >= 0 && (int)to <= 63);
            System.Diagnostics.Debug.Assert(promote == ChessPiece.WKnight || promote == ChessPiece.WBishop || promote == ChessPiece.WRook || promote == ChessPiece.WQueen || promote == ChessPiece.BKnight || promote == ChessPiece.BBishop || promote == ChessPiece.BRook || promote == ChessPiece.BQueen);


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

        public static ChessPiece Promote(this ChessMove move)
        {
            return (ChessPiece)((int)move >> 12 & 0x3F);
        }

        



		public static ChessMove Parse(ChessBoard board, string movetext)
		{
			ChessPiece Promote = ChessPiece.EMPTY;//unless changed below
			ChessPosition From = (ChessPosition.OUTOFBOUNDS);
			ChessPosition To = (ChessPosition.OUTOFBOUNDS);
			Regex regex = new Regex("");

			movetext = movetext.Replace("+", "");
			movetext = movetext.Replace("x", "");
			movetext = movetext.Replace("#", "");
			movetext = movetext.Replace("=", "");

			ChessPlayer me = board.WhosTurn;
			ChessPiece mypawn = board.WhosTurn == ChessPlayer.White ? ChessPiece.WPawn : ChessPiece.BPawn;
			ChessPiece myknight = board.WhosTurn == ChessPlayer.White ? ChessPiece.WKnight : ChessPiece.BKnight;
			ChessPiece mybishop = board.WhosTurn == ChessPlayer.White ? ChessPiece.WBishop : ChessPiece.BBishop;
			ChessPiece myrook = board.WhosTurn == ChessPlayer.White ? ChessPiece.WRook : ChessPiece.BRook;
			ChessPiece myqueen = board.WhosTurn == ChessPlayer.White ? ChessPiece.WQueen : ChessPiece.BQueen;
			ChessPiece myking = board.WhosTurn == ChessPlayer.White ? ChessPiece.WKing : ChessPiece.BKing;

			ChessDirection mynorth = board.WhosTurn == ChessPlayer.White ? ChessDirection.DirN : ChessDirection.DirS;
			ChessDirection mysouth = board.WhosTurn == ChessPlayer.White ? ChessDirection.DirS : ChessDirection.DirN;
			ChessRank myrank4 = board.WhosTurn == ChessPlayer.White ? ChessRank.Rank4 : ChessRank.Rank5;


			ChessPosition tmppos;
			ChessPiece tmppiece;
			ChessFile tmpfile;
			ChessRank tmprank;

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
				if (me == ChessPlayer.White)
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
				if (me == ChessPlayer.White)
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
				else if (board.PieceAt(tmppos) == ChessPiece.EMPTY && To.GetRank() == myrank4)
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
				tmpfile = ChessFileInfo.Parse(movetext[0]);
				From = filter(board, To, mypawn, tmpfile, ChessRank.EMPTY);
                return Create(From, To);
			}
			else if (Regex.IsMatch(movetext, "^[abcdefgh][abcdefgh][12345678][BNRQK]$"))
			{
				//pawn attack, promote
				To = ChessPositionInfo.Parse(movetext.Substring(1, 2));
                tmpfile = ChessFileInfo.Parse(movetext[0]);
				From = filter(board, To, mypawn, tmpfile, ChessRank.EMPTY);
				Promote = movetext[3].ParseAsPiece(me);
                return Create(From, To, Promote);
			}
			else if (Regex.IsMatch(movetext, "^[BNRQK][abcdefgh][12345678]$"))
			{
				//normal attack
				To = ChessPositionInfo.Parse(movetext.Substring(1, 2));
				tmppiece = movetext[0].ParseAsPiece(me);
				From = filter(board, To, tmppiece, ChessFile.EMPTY, ChessRank.EMPTY);
                return Create(From, To);
			}
			else if (Regex.IsMatch(movetext, "^[BNRQK][abcdefgh][abcdefgh][12345678]$"))
			{
				//normal, specify file
				To = ChessPositionInfo.Parse(movetext.Substring(2, 2));
				tmppiece = movetext[0].ParseAsPiece(me);
                tmpfile = ChessFileInfo.Parse(movetext[1]);
				From = filter(board, To, tmppiece, tmpfile, ChessRank.EMPTY);
                return Create(From, To);
			}
			else if (Regex.IsMatch(movetext, "^[BNRQK][12345678][abcdefgh][12345678]$"))
			{
				//normal, specify rank
				To = ChessPositionInfo.Parse(movetext.Substring(2, 2));
				tmppiece = movetext[0].ParseAsPiece(me);
                tmprank = ChessRankInfo.Parse(movetext[1]);
				From = filter(board, To, tmppiece, ChessFile.EMPTY, tmprank);
                return Create(From, To);

			}
			else if (Regex.IsMatch(movetext, "^[BNRQK][abcdefgh][12345678][abcdefgh][12345678]$"))
			{
				//normal, specify rank and file
				To = ChessPositionInfo.Parse(movetext.Substring(3, 2));
				tmppiece = movetext[0].ParseAsPiece(me);
                tmpfile = ChessFileInfo.Parse(movetext[1]);
                tmprank = ChessRankInfo.Parse(movetext[2]);
				From = filter(board, To, tmppiece, tmpfile, tmprank);
                return Create(From, To);
			}
            return ChessMoveInfo.Create(From, To);
		}

		private static ChessPosition filter(ChessBoard board, ChessPosition attackto, ChessPiece piece, ChessFile file, ChessRank rank)
		{
            List<ChessPosition> fits = new List<ChessPosition>();
            var attacksTo = board.AttacksTo(attackto, board.WhosTurn);
			foreach(ChessPosition pos in attacksTo.ToPositions())
			{
				if (piece != ChessPiece.EMPTY && piece != board.PieceAt(pos))
				{
                    continue;
				}
				if (rank != ChessRank.EMPTY && rank != pos.GetRank())
				{
                    continue;
				}
				if (file != ChessFile.EMPTY && file != pos.GetFile())
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
					if (file != ChessFile.EMPTY && move.From().GetFile() != file) { continue; }
					if (rank != ChessRank.EMPTY && move.From().GetRank() != rank) { continue; }
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
			if (move.Promote() == ChessPiece.EMPTY)
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
            ChessPiece piece = board.PieceAt(move.From());
            bool iscap = (board.PieceAt(move.To()) != ChessPiece.EMPTY);

            ChessRank fromrank = move.From().GetRank();
            ChessFile fromfile = move.From().GetFile();
            bool isprom = move.Promote() != ChessPiece.EMPTY;

            string sTo = (move.To().PositionToString());
            string sPiece = piece.PieceToString().ToUpper();
            string sRank = fromrank.RankToString().ToLower();
            string sFile = fromfile.FileToString().ToLower();
			string sProm = "";
			
			//enpassant cap
            if (move.To() == board.EnPassant && (piece == ChessPiece.WPawn || piece == ChessPiece.BPawn))
			{
				iscap = true;
			}
			
			if (isprom)
			{
                sProm = move.Promote().PieceToString().ToUpper();
			}

			if (piece == ChessPiece.WPawn || piece == ChessPiece.BPawn)
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
            else if (piece == ChessPiece.WKing && move.From() == ChessPosition.E1 && move.To() == ChessPosition.G1)
			{
				retval += "O-O";
			}
            else if (piece == ChessPiece.BKing && move.From() == ChessPosition.E8 && move.To() == ChessPosition.G8)
			{
				retval += "O-O";
			}
            else if (piece == ChessPiece.WKing && move.From() == ChessPosition.E1 && move.To() == ChessPosition.C1)
			{
				retval += "O-O-O";
			}
            else if (piece == ChessPiece.BKing && move.From() == ChessPosition.E8 && move.To() == ChessPosition.C8)
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

					ChessPiece otherpiece = board.PieceAt(pos);
					if (otherpiece == piece)
					{
						pieceunique = false;
						if (pos.GetRank() == fromrank)
						{
							rankunique = false;
						}
						if (pos.GetFile() == fromfile)
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




	}


}
