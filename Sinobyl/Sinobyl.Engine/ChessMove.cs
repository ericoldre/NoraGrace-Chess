using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;

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
				sb.Append(move.ToString(board) + " ");
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

	public class ChessMove
	{
		public readonly ChessPosition From;
		public readonly ChessPosition To;
		public readonly ChessPiece Promote;
		public int? EstScore;

		public override bool Equals(object obj)
		{
			ChessMove other = (ChessMove)obj;
			if (other.From==this.From && other.To==this.To && other.Promote==this.Promote)
			{
				return true;
			}
			return false;
		}
        public override int GetHashCode()
        {
            return (To.GetHashCode() ^ From.GetHashCode()>>1) + this.Promote.GetHashCode();
        }

		public ChessMove()
		{
			this.Promote = ChessPiece.EMPTY;
			this.From = (ChessPosition)0;
			this.To = (ChessPosition)0;
			this.EstScore = null;
		}
		public ChessMove(ChessPosition from, ChessPosition to)
		{
			this.Promote = ChessPiece.EMPTY;
			this.From = from;
			this.To = to;
			this.EstScore = null;
		}
		public ChessMove(ChessPosition from, ChessPosition to, ChessPiece promote)
		{
			this.From = from;
			this.To = to;
			this.Promote = promote;
			this.EstScore = null;
		}
		public ChessMove(ChessBoard board, string movetext)
		{
			this.Promote = ChessPiece.EMPTY;//unless changed below
			this.From = (ChessPosition)0;
			this.To = (ChessPosition)0;
			this.EstScore = null;
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
                this.From = movetext.Substring(0, 2).ParseAsPosition();
                this.To = movetext.Substring(2, 2).ParseAsPosition();
			}
			else if (Regex.IsMatch(movetext, "^[abcdefgh][12345678][abcdefgh][12345678][BNRQK]$", RegexOptions.IgnoreCase))
			{
				//coordinate notation, with promotion
                this.From = movetext.Substring(0, 2).ParseAsPosition();
				this.To = movetext.Substring(2, 2).ParseAsPosition();
				this.Promote = movetext[4].ParseAsPiece(me);
			}
			else if (movetext == "0-0" || movetext == "O-O" || movetext == "o-o")
			{
				if (me == ChessPlayer.White)
				{
					this.From = ChessPosition.E1;
					this.To = ChessPosition.G1;
				}
				else
				{
					this.From = ChessPosition.E8;
					this.To = ChessPosition.G8;
				}
			}
			else if (movetext == "0-0-0" || movetext == "O-O-O" || movetext == "o-o-o")
			{
				if (me == ChessPlayer.White)
				{
					this.From = ChessPosition.E1;
					this.To = ChessPosition.C1;
				}
				else
				{
					this.From = ChessPosition.E8;
					this.To = ChessPosition.C8;
				}
			}
			else if (Regex.IsMatch(movetext, "^[abcdefgh][12345678]$"))
			{
				//pawn forward
                this.To = movetext.ParseAsPosition();
				tmppos = Chess.PositionInDirection(To, mysouth);
				if (board.PieceAt(tmppos) == mypawn)
				{
					From = tmppos;
					return;
				}
				else if (board.PieceAt(tmppos) == ChessPiece.EMPTY && To.GetRank() == myrank4)
				{
					tmppos = Chess.PositionInDirection(tmppos, mysouth);
					if (board.PieceAt(tmppos) == mypawn)
					{
						From = tmppos;
						return;
					}
				}
				throw new ChessException("no pawn can move to " + movetext);

			}
			else if (Regex.IsMatch(movetext, "^[abcdefgh][12345678][BNRQK]$"))
			{
				//pawn forward, promotion
                this.To = movetext.Substring(0, 2).ParseAsPosition();
				tmppos = Chess.PositionInDirection(To, mysouth);
				if (board.PieceAt(tmppos) == mypawn)
				{
					From = tmppos;
					Promote = movetext[2].ParseAsPiece(me);
					return;
				}
				throw new ChessException("no pawn can promoted to " + movetext.Substring(0, 2));
			}
			else if (Regex.IsMatch(movetext, "^[abcdefgh][abcdefgh][12345678]$"))
			{
				//pawn attack
                this.To = movetext.Substring(1, 2).ParseAsPosition();
				tmpfile = movetext[0].ParseAsFile();
				this.From = filter(board, To, mypawn, tmpfile, ChessRank.EMPTY);
				return;
			}
			else if (Regex.IsMatch(movetext, "^[abcdefgh][abcdefgh][12345678][BNRQK]$"))
			{
				//pawn attack, promote
				this.To = movetext.Substring(1, 2).ParseAsPosition();
				tmpfile = movetext[0].ParseAsFile();
				this.From = filter(board, To, mypawn, tmpfile, ChessRank.EMPTY);
				this.Promote = movetext[3].ParseAsPiece(me);
				return;
			}
			else if (Regex.IsMatch(movetext, "^[BNRQK][abcdefgh][12345678]$"))
			{
				//normal attack
				this.To = movetext.Substring(1, 2).ParseAsPosition();
				tmppiece = movetext[0].ParseAsPiece(me);
				this.From = filter(board, To, tmppiece, ChessFile.EMPTY, ChessRank.EMPTY);
				return;
			}
			else if (Regex.IsMatch(movetext, "^[BNRQK][abcdefgh][abcdefgh][12345678]$"))
			{
				//normal, specify file
				this.To = movetext.Substring(2, 2).ParseAsPosition();
				tmppiece = movetext[0].ParseAsPiece(me);
                tmpfile = movetext[1].ParseAsFile();
				this.From = filter(board, To, tmppiece, tmpfile, ChessRank.EMPTY);
				return;
			}
			else if (Regex.IsMatch(movetext, "^[BNRQK][12345678][abcdefgh][12345678]$"))
			{
				//normal, specify rank
				this.To = movetext.Substring(2, 2).ParseAsPosition();
				tmppiece = movetext[0].ParseAsPiece(me);
				tmprank = movetext[1].ParseAsRank();
				this.From = filter(board, To, tmppiece, ChessFile.EMPTY, tmprank);
				return;

			}
			else if (Regex.IsMatch(movetext, "^[BNRQK][abcdefgh][12345678][abcdefgh][12345678]$"))
			{
				//normal, specify rank and file
				this.To = movetext.Substring(3, 2).ParseAsPosition();
				tmppiece = movetext[0].ParseAsPiece(me);
                tmpfile = movetext[1].ParseAsFile();
                tmprank = movetext[2].ParseAsRank();
				this.From = filter(board, To, tmppiece, tmpfile, tmprank);
				return;
			}

		}
		private ChessPosition filter(ChessBoard board, ChessPosition attackto, ChessPiece piece, ChessFile file, ChessRank rank)
		{
			List<ChessPosition> attlist = board.AttacksTo(attackto, board.WhosTurn);

			for (int i = 0; i < attlist.Count; i++)
			{
				ChessPosition pos = attlist[i];
				bool isfit = true;
				if (piece != ChessPiece.EMPTY && piece != board.PieceAt(pos))
				{
					isfit = false;
				}
				if (rank != ChessRank.EMPTY && rank != pos.GetRank())
				{
					isfit = false;
				}
				if (file != ChessFile.EMPTY && file != pos.GetFile())
				{
					isfit = false;
				}
				if (!isfit)
				{
					attlist.RemoveAt(i);
					i--; //subtract the counter so it doesn't skip one.
				}
			}

			if (attlist.Count > 1)
			{
				//ambigous moves, one is probably illegal, check against legal move list
				ChessMoves allLegal = ChessMove.GenMovesLegal(board);
				attlist.Clear();
				foreach (ChessMove move in allLegal)
				{
					if (move.To != attackto) { continue; }
					if (board.PieceAt(move.From) != piece) { continue; }
					if (file != ChessFile.EMPTY && move.From.GetFile() != file) { continue; }
					if (rank != ChessRank.EMPTY && move.From.GetRank() != rank) { continue; }
					attlist.Add(move.From);
				}
			}

			if (attlist.Count != 1)
			{
				throw new ChessException("invalid move input");
				
			}

			return attlist[0];
		}

		public override string ToString()
		{
			string retval = "";
			if (Promote == ChessPiece.EMPTY)
			{
				retval = Chess.PositionToString(From).ToLower() + Chess.PositionToString(To).ToLower();
			}
			else
			{
				retval = Chess.PositionToString(From).ToLower() + Chess.PositionToString(To).ToLower() + Chess.PieceToString(Promote).ToLower();
			}
			return retval;
		}
		public bool IsSameAs(ChessMove move)
		{
			return this.From == move.From && this.To == move.To && this.Promote == move.Promote;
		}
		public bool IsLegal(ChessBoard board)
		{
			ChessMoves legalmoves = ChessMove.GenMovesLegal(board);
			foreach (ChessMove legalmove in legalmoves)
			{
				if (legalmove.IsSameAs(this)) { return true; }
			}
			return false;
		}
		public string ToString(ChessBoard board)
		{
			string retval = "";
			ChessPiece piece = board.PieceAt(this.From);
			bool iscap = (board.PieceAt(this.To) != ChessPiece.EMPTY);
			
			ChessRank fromrank = this.From.GetRank();
			ChessFile fromfile = this.From.GetFile();
			bool isprom = this.Promote != ChessPiece.EMPTY;

			string sTo = Chess.PositionToString(this.To);
			string sPiece = Chess.PieceToString(piece).ToUpper();
			string sRank = Chess.RankToString(fromrank).ToLower();
			string sFile = Chess.FileToString(fromfile).ToLower();
			string sProm = "";
			
			//enpassant cap
			if(this.To==board.EnPassant && (piece==ChessPiece.WPawn || piece==ChessPiece.BPawn))
			{
				iscap = true;
			}
			
			if (isprom)
			{
				sProm = Chess.PieceToString(this.Promote).ToUpper();
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
			else if (piece == ChessPiece.WKing && this.From == ChessPosition.E1 && this.To == ChessPosition.G1)
			{
				retval += "O-O";
			}
			else if (piece == ChessPiece.BKing && this.From == ChessPosition.E8 && this.To == ChessPosition.G8)
			{
				retval += "O-O";
			}
			else if (piece == ChessPiece.WKing && this.From == ChessPosition.E1 && this.To == ChessPosition.C1)
			{
				retval += "O-O-O";
			}
			else if (piece == ChessPiece.BKing && this.From == ChessPosition.E8 && this.To == ChessPosition.C8)
			{
				retval += "O-O-O";
			}
			else
			{
				bool pieceunique = true;
				bool fileunique = true;
				bool rankunique = true;
				List<ChessPosition> attacks = board.AttacksTo(this.To, Chess.PieceToPlayer(piece));
				foreach (ChessPosition pos in attacks)
				{
					if (pos == this.From) { continue; }

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
			board.MoveApply(this);
			if (board.IsCheck())
			{
				if (ChessMove.GenMovesLegal(board).Count > 0)
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

		public static ChessMoves GenMovesLegal(ChessBoard board)
		{
			ChessBoard workingboard = new ChessBoard(board.FEN);
			ChessMoves retval = new ChessMoves();
			ChessMoves semilegal = GenMoves(workingboard);
			ChessPlayer me = board.WhosTurn;
			foreach (ChessMove move in semilegal)
			{
				workingboard.MoveApply(move);
				if (!workingboard.IsCheck(me))
				{
					retval.Add(move);
				}
				workingboard.MoveUndo();
			}
			return retval;
		}
		public static ChessMoves GenMoves(ChessBoard board)
		{
			return GenMoves(board, false);
		}
		public static ChessMoves GenMovesOld(ChessBoard board, bool CapsOnly)
		{
			ChessMoves retval = new ChessMoves();

			ChessPiece mypawn = board.WhosTurn == ChessPlayer.White ? ChessPiece.WPawn : ChessPiece.BPawn;
			ChessPiece myknight = board.WhosTurn == ChessPlayer.White ? ChessPiece.WKnight : ChessPiece.BKnight;
			ChessPiece mybishop = board.WhosTurn == ChessPlayer.White ? ChessPiece.WBishop : ChessPiece.BBishop;
			ChessPiece myrook = board.WhosTurn == ChessPlayer.White ? ChessPiece.WRook : ChessPiece.BRook;
			ChessPiece myqueen = board.WhosTurn == ChessPlayer.White ? ChessPiece.WQueen : ChessPiece.BQueen;
			ChessPiece myking = board.WhosTurn == ChessPlayer.White ? ChessPiece.WKing : ChessPiece.BKing;

			ChessDirection mypawnwest = board.WhosTurn == ChessPlayer.White ? ChessDirection.DirNW : ChessDirection.DirSW;
			ChessDirection mypawneast = board.WhosTurn == ChessPlayer.White ? ChessDirection.DirNE : ChessDirection.DirSE;
			ChessDirection mypawnnorth = board.WhosTurn == ChessPlayer.White ? ChessDirection.DirN : ChessDirection.DirS;
			ChessRank myrank8 = board.WhosTurn == ChessPlayer.White ? ChessRank.Rank8 : ChessRank.Rank1;
			ChessRank myrank2 = board.WhosTurn == ChessPlayer.White ? ChessRank.Rank2 : ChessRank.Rank7;

			ChessPosition targetpos;
			ChessPiece targetpiece;

			foreach (ChessPosition piecepos in Chess.AllPositions)
			{
				ChessPiece piece = board.PieceAt(piecepos);
				if (piece == ChessPiece.EMPTY) { continue; }

				//knight attacks
				if (piece == myknight)
				{
					foreach (ChessDirection dir in Chess.AllDirectionsKnight)
					{
						AddDirection(retval, board, piecepos, dir, board.WhosTurn, 1, CapsOnly);
					}
					continue;
				}
				//bishop attacks
				if (piece == mybishop)
				{
					foreach (ChessDirection dir in Chess.AllDirectionsBishop)
					{
						AddDirection(retval, board, piecepos, dir, board.WhosTurn, 8, CapsOnly);
					}
					continue;
				}
				//rook attacks
				if (piece == myrook)
				{
					foreach (ChessDirection dir in Chess.AllDirectionsRook)
					{
						AddDirection(retval, board, piecepos, dir, board.WhosTurn, 8, CapsOnly);
					}
					continue;
				}
				//queen attacks
				if (piece == myqueen)
				{
					foreach (ChessDirection dir in Chess.AllDirectionsQueen)
					{
						AddDirection(retval, board, piecepos, dir, board.WhosTurn, 8, CapsOnly);
					}
					continue;
				}
				//king attacks
				if (piece == myking)
				{
					foreach (ChessDirection dir in Chess.AllDirectionsQueen)
					{
						AddDirection(retval, board, piecepos, dir, board.WhosTurn, 1, CapsOnly);
					}
					continue;
				}
				//pawn moves
				if (piece == mypawn)
				{
					//pawn east caps
					targetpos = Chess.PositionInDirection(piecepos, mypawneast);
					targetpiece = board.PieceAt(targetpos);
					if (targetpos.IsInBounds())
					{
						if (Chess.PieceToPlayer(targetpiece) == Chess.PlayerOther(board.WhosTurn))
						{
							if (targetpos.GetRank() == myrank8)
							{
								retval.Add(new ChessMove(piecepos, targetpos, myqueen));
								retval.Add(new ChessMove(piecepos, targetpos, myrook));
								retval.Add(new ChessMove(piecepos, targetpos, mybishop));
								retval.Add(new ChessMove(piecepos, targetpos, myknight));
							}
							else
							{
								retval.Add(new ChessMove(piecepos, targetpos));
							}
						}
						else if (targetpos == board.EnPassant)
						{
							retval.Add(new ChessMove(piecepos, targetpos));
						}
					}

					//pawn west caps
					targetpos = Chess.PositionInDirection(piecepos, mypawnwest);
					targetpiece = board.PieceAt(targetpos);
					if (targetpos.IsInBounds())
					{
						if (Chess.PieceToPlayer(targetpiece) == Chess.PlayerOther(board.WhosTurn))
						{
							if (targetpos.GetRank() == myrank8)
							{
								retval.Add(new ChessMove(piecepos, targetpos, myqueen));
								retval.Add(new ChessMove(piecepos, targetpos, myrook));
								retval.Add(new ChessMove(piecepos, targetpos, mybishop));
								retval.Add(new ChessMove(piecepos, targetpos, myknight));
							}
							else
							{
								retval.Add(new ChessMove(piecepos, targetpos));
							}
						}
						else if (targetpos == board.EnPassant)
						{
							retval.Add(new ChessMove(piecepos, targetpos));
						}
					}

					if (!CapsOnly)
					{
						//pawn jump
						targetpos = Chess.PositionInDirection(piecepos, mypawnnorth);
						targetpiece = board.PieceAt(targetpos);
						if (targetpiece == ChessPiece.EMPTY)
						{
							if (targetpos.GetRank() == myrank8)
							{
								retval.Add(new ChessMove(piecepos, targetpos, myqueen));
								retval.Add(new ChessMove(piecepos, targetpos, myrook));
								retval.Add(new ChessMove(piecepos, targetpos, mybishop));
								retval.Add(new ChessMove(piecepos, targetpos, myknight));
							}
							else
							{
								retval.Add(new ChessMove(piecepos, targetpos));
							}

							//double jump
							if (piecepos.GetRank() == myrank2)
							{
								targetpos = Chess.PositionInDirection(targetpos, mypawnnorth);
								targetpiece = board.PieceAt(targetpos);
								if (targetpiece == ChessPiece.EMPTY)
								{
									retval.Add(new ChessMove(piecepos, targetpos));
								}
							}
						}
					}
				}
			}

			
			
			if (!CapsOnly)
			{
				//castling
				if (board.WhosTurn == ChessPlayer.White)
				{
					if (board.CastleAvailWS
						&& board.PieceAt(ChessPosition.E1) == ChessPiece.WKing
						&& board.PieceAt(ChessPosition.H1) == ChessPiece.WRook
						&& board.PieceAt(ChessPosition.F1) == ChessPiece.EMPTY
						&& board.PieceAt(ChessPosition.G1) == ChessPiece.EMPTY
						&& !board.PositionAttacked(ChessPosition.E1, ChessPlayer.Black)
						&& !board.PositionAttacked(ChessPosition.F1, ChessPlayer.Black)
						&& !board.PositionAttacked(ChessPosition.G1, ChessPlayer.Black))
					{
						retval.Add(new ChessMove(ChessPosition.E1, ChessPosition.G1));
					}
					if (board.CastleAvailWL
						&& board.PieceAt(ChessPosition.E1) == ChessPiece.WKing
						&& board.PieceAt(ChessPosition.A1) == ChessPiece.WRook
						&& board.PieceAt(ChessPosition.B1) == ChessPiece.EMPTY
						&& board.PieceAt(ChessPosition.C1) == ChessPiece.EMPTY
						&& board.PieceAt(ChessPosition.D1) == ChessPiece.EMPTY
						&& !board.PositionAttacked(ChessPosition.E1, ChessPlayer.Black)
						&& !board.PositionAttacked(ChessPosition.D1, ChessPlayer.Black)
						&& !board.PositionAttacked(ChessPosition.C1, ChessPlayer.Black))
					{
						retval.Add(new ChessMove(ChessPosition.E1, ChessPosition.C1));
					}
				}
				else
				{
					if (board.CastleAvailBS
						&& board.PieceAt(ChessPosition.E8) == ChessPiece.BKing
						&& board.PieceAt(ChessPosition.H8) == ChessPiece.BRook
						&& board.PieceAt(ChessPosition.F8) == ChessPiece.EMPTY
						&& board.PieceAt(ChessPosition.G8) == ChessPiece.EMPTY
						&& !board.PositionAttacked(ChessPosition.E8, ChessPlayer.White)
						&& !board.PositionAttacked(ChessPosition.F8, ChessPlayer.White)
						&& !board.PositionAttacked(ChessPosition.G8, ChessPlayer.White))
					{
						retval.Add(new ChessMove(ChessPosition.E8, ChessPosition.G8));
					}
					if (board.CastleAvailBL
						&& board.PieceAt(ChessPosition.E8) == ChessPiece.BKing
						&& board.PieceAt(ChessPosition.A8) == ChessPiece.BRook
						&& board.PieceAt(ChessPosition.B8) == ChessPiece.EMPTY
						&& board.PieceAt(ChessPosition.C8) == ChessPiece.EMPTY
						&& board.PieceAt(ChessPosition.D8) == ChessPiece.EMPTY
						&& !board.PositionAttacked(ChessPosition.E8, ChessPlayer.White)
						&& !board.PositionAttacked(ChessPosition.D8, ChessPlayer.White)
						&& !board.PositionAttacked(ChessPosition.C8, ChessPlayer.White))
					{
						retval.Add(new ChessMove(ChessPosition.E8, ChessPosition.C8));
					}

				}
			}
			
			return retval;

		}


        public static ChessMoves GenMoves(ChessBoard board, bool CapsOnly)
        {
            ChessMoves retval = new ChessMoves();

            ChessPiece mypawn = board.WhosTurn == ChessPlayer.White ? ChessPiece.WPawn : ChessPiece.BPawn;
            ChessPiece myknight = board.WhosTurn == ChessPlayer.White ? ChessPiece.WKnight : ChessPiece.BKnight;
            ChessPiece mybishop = board.WhosTurn == ChessPlayer.White ? ChessPiece.WBishop : ChessPiece.BBishop;
            ChessPiece myrook = board.WhosTurn == ChessPlayer.White ? ChessPiece.WRook : ChessPiece.BRook;
            ChessPiece myqueen = board.WhosTurn == ChessPlayer.White ? ChessPiece.WQueen : ChessPiece.BQueen;
            ChessPiece myking = board.WhosTurn == ChessPlayer.White ? ChessPiece.WKing : ChessPiece.BKing;

            ChessDirection mypawnwest = board.WhosTurn == ChessPlayer.White ? ChessDirection.DirNW : ChessDirection.DirSW;
            ChessDirection mypawneast = board.WhosTurn == ChessPlayer.White ? ChessDirection.DirNE : ChessDirection.DirSE;
            ChessDirection mypawnnorth = board.WhosTurn == ChessPlayer.White ? ChessDirection.DirN : ChessDirection.DirS;
            ChessRank myrank8 = board.WhosTurn == ChessPlayer.White ? ChessRank.Rank8 : ChessRank.Rank1;
            ChessRank myrank2 = board.WhosTurn == ChessPlayer.White ? ChessRank.Rank2 : ChessRank.Rank7;

            ChessPosition targetpos;
            ChessPiece targetpiece;

            ChessBitboard targetLocations = board.PlayerLocations(Chess.PlayerOther(board.WhosTurn));
            if (!CapsOnly)
            {
                targetLocations |= ~board.PieceLocationsAll;
            }

            ChessBitboard pawnTargets = board.PlayerLocations(Chess.PlayerOther(board.WhosTurn)) | (board.EnPassant.IsInBounds() ? board.EnPassant.Bitboard() : 0);

            foreach (ChessPosition piecepos in Chess.AllPositions)
            {
                ChessPiece piece = board.PieceAt(piecepos);
                if (piece == ChessPiece.EMPTY) { continue; }

                //knight attacks
                if (piece == myknight)
                {
                    foreach (ChessPosition attackPos in (Attacks.KnightAttacks(piecepos) & targetLocations).ToPositions())
                    {
                        retval.Add(new ChessMove(piecepos, attackPos));
                    }
                    continue;
                }
                //bishop attacks
                if (piece == mybishop)
                {
                    foreach (ChessPosition attackPos in (Attacks.BishopAttacks(piecepos, board.PieceLocationsAllA1H8, board.PieceLocationsAllH1A8) & targetLocations).ToPositions())
                    {
                        retval.Add(new ChessMove(piecepos, attackPos));
                    }
                    continue;
                }
                //rook attacks
                if (piece == myrook)
                {
                    foreach (ChessPosition attackPos in (Attacks.RookAttacks(piecepos, board.PieceLocationsAll, board.PieceLocationsAllVert) & targetLocations).ToPositions())
                    {
                        retval.Add(new ChessMove(piecepos, attackPos));
                    }
                    continue;
                }
                //queen attacks
                if (piece == myqueen)
                {
                    foreach (ChessPosition attackPos in (Attacks.QueenAttacks(piecepos, board.PieceLocationsAll, board.PieceLocationsAllVert, board.PieceLocationsAllA1H8, board.PieceLocationsAllH1A8) & targetLocations).ToPositions())
                    {
                        retval.Add(new ChessMove(piecepos, attackPos));
                    }
                    continue;
                }
                //king attacks
                if (piece == myking)
                {
                    foreach (ChessPosition attackPos in (Attacks.KingAttacks(piecepos) & targetLocations).ToPositions())
                    {
                        retval.Add(new ChessMove(piecepos, attackPos));
                    }
                    continue;
                }
                //pawn moves
                if (piece == mypawn)
                {
                    //pawn attacks
                    foreach (var attackPos in (Attacks.PawnAttacks(piecepos, board.WhosTurn) & pawnTargets).ToPositions())
                    {
                        if (attackPos.GetRank() == myrank8)
                        {
                            retval.Add(new ChessMove(piecepos, attackPos, myqueen));
                            retval.Add(new ChessMove(piecepos, attackPos, myrook));
                            retval.Add(new ChessMove(piecepos, attackPos, mybishop));
                            retval.Add(new ChessMove(piecepos, attackPos, myknight));
                        }
                        else
                        {
                            retval.Add(new ChessMove(piecepos, attackPos));
                        }
                    }

                    //pawn jumps
                    if (!CapsOnly)
                    {
                        //pawn jump
                        targetpos = Chess.PositionInDirection(piecepos, mypawnnorth);
                        targetpiece = board.PieceAt(targetpos);
                        if (targetpiece == ChessPiece.EMPTY)
                        {
                            if (targetpos.GetRank() == myrank8)
                            {
                                retval.Add(new ChessMove(piecepos, targetpos, myqueen));
                                retval.Add(new ChessMove(piecepos, targetpos, myrook));
                                retval.Add(new ChessMove(piecepos, targetpos, mybishop));
                                retval.Add(new ChessMove(piecepos, targetpos, myknight));
                            }
                            else
                            {
                                retval.Add(new ChessMove(piecepos, targetpos));
                            }

                            //double jump
                            if (piecepos.GetRank() == myrank2)
                            {
                                targetpos = Chess.PositionInDirection(targetpos, mypawnnorth);
                                targetpiece = board.PieceAt(targetpos);
                                if (targetpiece == ChessPiece.EMPTY)
                                {
                                    retval.Add(new ChessMove(piecepos, targetpos));
                                }
                            }
                        }
                    }
                }
            }



            if (!CapsOnly)
            {
                //castling
                if (board.WhosTurn == ChessPlayer.White)
                {
                    if (board.CastleAvailWS
                        && board.PieceAt(ChessPosition.E1) == ChessPiece.WKing
                        && board.PieceAt(ChessPosition.H1) == ChessPiece.WRook
                        && board.PieceAt(ChessPosition.F1) == ChessPiece.EMPTY
                        && board.PieceAt(ChessPosition.G1) == ChessPiece.EMPTY
                        && !board.PositionAttacked(ChessPosition.E1, ChessPlayer.Black)
                        && !board.PositionAttacked(ChessPosition.F1, ChessPlayer.Black)
                        && !board.PositionAttacked(ChessPosition.G1, ChessPlayer.Black))
                    {
                        retval.Add(new ChessMove(ChessPosition.E1, ChessPosition.G1));
                    }
                    if (board.CastleAvailWL
                        && board.PieceAt(ChessPosition.E1) == ChessPiece.WKing
                        && board.PieceAt(ChessPosition.A1) == ChessPiece.WRook
                        && board.PieceAt(ChessPosition.B1) == ChessPiece.EMPTY
                        && board.PieceAt(ChessPosition.C1) == ChessPiece.EMPTY
                        && board.PieceAt(ChessPosition.D1) == ChessPiece.EMPTY
                        && !board.PositionAttacked(ChessPosition.E1, ChessPlayer.Black)
                        && !board.PositionAttacked(ChessPosition.D1, ChessPlayer.Black)
                        && !board.PositionAttacked(ChessPosition.C1, ChessPlayer.Black))
                    {
                        retval.Add(new ChessMove(ChessPosition.E1, ChessPosition.C1));
                    }
                }
                else
                {
                    if (board.CastleAvailBS
                        && board.PieceAt(ChessPosition.E8) == ChessPiece.BKing
                        && board.PieceAt(ChessPosition.H8) == ChessPiece.BRook
                        && board.PieceAt(ChessPosition.F8) == ChessPiece.EMPTY
                        && board.PieceAt(ChessPosition.G8) == ChessPiece.EMPTY
                        && !board.PositionAttacked(ChessPosition.E8, ChessPlayer.White)
                        && !board.PositionAttacked(ChessPosition.F8, ChessPlayer.White)
                        && !board.PositionAttacked(ChessPosition.G8, ChessPlayer.White))
                    {
                        retval.Add(new ChessMove(ChessPosition.E8, ChessPosition.G8));
                    }
                    if (board.CastleAvailBL
                        && board.PieceAt(ChessPosition.E8) == ChessPiece.BKing
                        && board.PieceAt(ChessPosition.A8) == ChessPiece.BRook
                        && board.PieceAt(ChessPosition.B8) == ChessPiece.EMPTY
                        && board.PieceAt(ChessPosition.C8) == ChessPiece.EMPTY
                        && board.PieceAt(ChessPosition.D8) == ChessPiece.EMPTY
                        && !board.PositionAttacked(ChessPosition.E8, ChessPlayer.White)
                        && !board.PositionAttacked(ChessPosition.D8, ChessPlayer.White)
                        && !board.PositionAttacked(ChessPosition.C8, ChessPlayer.White))
                    {
                        retval.Add(new ChessMove(ChessPosition.E8, ChessPosition.C8));
                    }

                }
            }

            return retval;

        }


		private static void AddDirection(ChessMoves retval, ChessBoard board, ChessPosition from, ChessDirection dir, ChessPlayer forwho, int maxdist, bool CapsOnly)
		{
			ChessPosition to = (ChessPosition)((int)from + (int)dir);
			int i = 1;
			while (to.IsInBounds() && i <= maxdist)
			{
				ChessPiece targetpiece = board.PieceAt(to);
				if (targetpiece == ChessPiece.EMPTY)
				{
					if (!CapsOnly) { retval.Add(new ChessMove(from, to)); }
				}
				else if (Chess.PieceToPlayer(targetpiece) == forwho)
				{
					break;
				}
				else
				{
					retval.Add(new ChessMove(from, to));
					break;
				}
				to = (ChessPosition)((int)to + (int)dir);
				i++;
			}
		}


		public class Comp : Comparer<ChessMove>
		{
			public readonly ChessBoard board;
			public readonly ChessMove tt_move;
			public readonly bool UseSEE;
			public Comp(ChessBoard a_board, ChessMove a_tt_move, bool a_see)
			{
				board = a_board;
				tt_move = a_tt_move;
				UseSEE = false;
			}
			public override int Compare(ChessMove x, ChessMove y)
			{
				if (x.Equals(tt_move) && y.Equals(tt_move))
				{
					return 0;
				}
				if(x.Equals(tt_move))
				{
					return -1;
				}
				if (y.Equals(tt_move))
				{
					return 1;
				}
				//captures 1st
				if (board.PieceAt(x.To) != ChessPiece.EMPTY && board.PieceAt(y.To) == ChessPiece.EMPTY)
				{
					return -1;
				}
				if (board.PieceAt(y.To) != ChessPiece.EMPTY && board.PieceAt(x.To) == ChessPiece.EMPTY)
				{
					return 1;
				}

				if (!x.EstScore.HasValue)
				{
					if (UseSEE)
					{
						x.EstScore = CompEstScoreSEE(x, board);
					}
					else
					{
						x.EstScore = CompEstScore(x, board);
					}
				}
				if (!y.EstScore.HasValue)
				{
					if (UseSEE)
					{
						y.EstScore = CompEstScoreSEE(y, board);
					}
					else
					{
						y.EstScore = CompEstScore(y, board);
					}
				}
				if (x.EstScore > y.EstScore) { return -1; }
				if (x.EstScore < y.EstScore) { return 1; }
				return 0;
			}

			public static int CompEstScore(ChessMove move, ChessBoard board)
			{
				int retval = 0;

				ChessPiece mover = board.PieceAt(move.From);
				ChessPiece taken = board.PieceAt(move.To);
				ChessPlayer me = board.WhosTurn;

				retval -= eval.PieceSquareStartEndVals[(int)mover, (int)move.From, 0];
				retval += eval.PieceSquareStartEndVals[(int)mover, (int)move.To, 0];

				if (taken != ChessPiece.EMPTY)
				{
					retval -= eval.PieceValue[(int)taken];
				}

				if (me == ChessPlayer.Black) { retval = -retval; }
				return retval;
			}

			public static int CompEstScoreSEE(ChessMove move, ChessBoard board)
			{

				int retval = 0;


				ChessPiece mover = board.PieceAt(move.From);
				ChessPiece taken = board.PieceAt(move.To);
				ChessPlayer me = Chess.PieceToPlayer(mover);

				int pieceSqVal = 0;
				pieceSqVal -= eval.PieceSquareStartEndVals[(int)mover, (int)move.From, 0];
				pieceSqVal += eval.PieceSquareStartEndVals[(int)mover, (int)move.To, 0];
				if (me == ChessPlayer.Black) { pieceSqVal = -pieceSqVal; }
				retval += pieceSqVal;

				if (taken != ChessPiece.EMPTY)
				{
					retval += Chess.PieceValBasic(taken);
					//do see
					List<ChessPosition> attacks = board.AttacksTo(move.To);
					attacks.Remove(move.From);
					retval -= attackswap(board, attacks, Chess.PlayerOther(me), move.To, Chess.PieceValBasic(mover));


				}

				return retval;



			}
			static int attackswap(ChessBoard board, List<ChessPosition> attacks, ChessPlayer player, ChessPosition positionattacked, int pieceontargetval)
			{
				int nextAttackPieceVal = 0;
				ChessPosition nextAttackPos = 0;

				bool HasAttack = attackpop(board, attacks, player, positionattacked, out nextAttackPos, out nextAttackPieceVal);
				if (!HasAttack) { return 0; }

				int moveval = pieceontargetval - attackswap(board, attacks, Chess.PlayerOther(player), positionattacked, nextAttackPieceVal);

				if (moveval > 0)
				{
					return moveval;
				}
				else
				{
					return 0;
				}
			}

			static bool attackpop(ChessBoard board, List<ChessPosition> attacks, ChessPlayer player, ChessPosition positionattacked, out ChessPosition OutFrom, out int OutPieceVal)
			{


				ChessPosition mypawn = 0;
				ChessPosition myknight = 0;
				ChessPosition mybishop = 0;
				ChessPosition myrook = 0;
				ChessPosition myqueen = 0;
				ChessPosition myking = 0;


				if (player == ChessPlayer.White)
				{
					foreach (ChessPosition attackPos in attacks)
					{
						ChessPiece attackPiece = board.PieceAt(attackPos);
						switch (attackPiece)
						{
							case ChessPiece.WPawn:
								mypawn = attackPos; break;
							case ChessPiece.WKnight:
								myknight = attackPos; break;
							case ChessPiece.WBishop:
								mybishop = attackPos; break;
							case ChessPiece.WRook:
								myrook = attackPos; break;
							case ChessPiece.WQueen:
								myqueen = attackPos; break;
							case ChessPiece.WKing:
								myking = attackPos; break;
						}
					}
				}
				else
				{
					foreach (ChessPosition attackPos in attacks)
					{
						ChessPiece attackPiece = board.PieceAt(attackPos);
						switch (attackPiece)
						{
							case ChessPiece.BPawn:
								mypawn = attackPos; break;
							case ChessPiece.BKnight:
								myknight = attackPos; break;
							case ChessPiece.BBishop:
								mybishop = attackPos; break;
							case ChessPiece.BRook:
								myrook = attackPos; break;
							case ChessPiece.BQueen:
								myqueen = attackPos; break;
							case ChessPiece.BKing:
								myking = attackPos; break;
						}
					}
				}

				OutFrom = 0;
				OutPieceVal = 0;

				if (mypawn != 0)
				{
					OutFrom = mypawn;
					OutPieceVal = 100;
				}
				else if (myknight != 0)
				{
					OutFrom = myknight;
					OutPieceVal = 300;
				}
				else if (mybishop != 0)
				{
					OutFrom = mybishop;
					OutPieceVal = 300;
				}
				else if (myrook != 0)
				{
					OutFrom = myrook;
					OutPieceVal = 500;
				}
				else if (myqueen != 0)
				{
					OutFrom = myqueen;
					OutPieceVal = 900;
				}
				else if (myking != 0)
				{
					OutFrom = myking;
					OutPieceVal = 100000;
				}

				if (OutFrom == 0)
				{
					//i'm out of attacks to this position;
					return false;
				}
				ChessDirection addAttackFrom = Chess.DirectionFromTo(positionattacked, OutFrom);
				if (!Chess.IsDirectionKnight(addAttackFrom))
				{
					ChessPosition AddPosition = 0;
					ChessPiece AddPiece = board.PieceInDirection(OutFrom, addAttackFrom, ref AddPosition);
					if (Chess.IsDirectionRook(addAttackFrom) && Chess.PieceIsSliderRook(AddPiece))
					{
						attacks.Add(AddPosition);
					}
					else if (Chess.IsDirectionBishop(addAttackFrom) && Chess.PieceIsSliderBishop(AddPiece))
					{
						attacks.Add(AddPosition);
					}
				}
				attacks.Remove(OutFrom);
				return true;

			}

		}
		private static ChessEval eval = new ChessEval();


	}


}
