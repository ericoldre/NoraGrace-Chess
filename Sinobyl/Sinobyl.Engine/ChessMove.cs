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
                sb.Append(move.ToString()+ " ");
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

	public struct ChessMove
	{
		public readonly ChessPosition From;
		public readonly ChessPosition To;
		public readonly ChessPiece Promote;
		//public int? EstScore;

		public override bool Equals(object obj)
		{
			ChessMove other = (ChessMove)obj;
            return other.From == this.From && other.To == this.To && other.Promote == this.Promote;
		}

        public override int GetHashCode()
        {
            //return (To.GetHashCode() ^ From.GetHashCode() >> 1) + this.Promote.GetHashCode();
            return (int)To | ((int)From << 8) | ((int)Promote << 16);
        }

        public static readonly ChessMove EMPTY = new ChessMove();

		public ChessMove(ChessPosition from, ChessPosition to)
		{
			this.Promote = ChessPiece.EMPTY;
			this.From = from;
			this.To = to;
		}
		public ChessMove(ChessPosition from, ChessPosition to, ChessPiece promote)
		{
			this.From = from;
			this.To = to;
			this.Promote = promote;
		}

        public static bool operator ==(ChessMove x, ChessMove y)
        {
            return x.From == y.From && x.To == y.To && x.Promote == y.Promote;
        }

        public static bool operator !=(ChessMove x, ChessMove y)
        {
            return !(x == y);
        }

		public ChessMove(ChessBoard board, string movetext)
		{
			this.Promote = ChessPiece.EMPTY;//unless changed below
			this.From = (ChessPosition.OUTOFBOUNDS);
			this.To = (ChessPosition.OUTOFBOUNDS);
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
                this.From = ChessPositionInfo.Parse(movetext.Substring(0, 2));
                this.To = ChessPositionInfo.Parse(movetext.Substring(2, 2));
			}
			else if (Regex.IsMatch(movetext, "^[abcdefgh][12345678][abcdefgh][12345678][BNRQK]$", RegexOptions.IgnoreCase))
			{
				//coordinate notation, with promotion
                this.From = ChessPositionInfo.Parse(movetext.Substring(0, 2));
				this.To = ChessPositionInfo.Parse(movetext.Substring(2, 2));
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
                this.To = ChessPositionInfo.Parse(movetext);
                tmppos = To.PositionInDirection(mysouth);
				if (board.PieceAt(tmppos) == mypawn)
				{
					From = tmppos;
					return;
				}
				else if (board.PieceAt(tmppos) == ChessPiece.EMPTY && To.GetRank() == myrank4)
				{
                    tmppos = tmppos.PositionInDirection(mysouth);
					if (board.PieceAt(tmppos) == mypawn)
					{
						From = tmppos;
						return;
					}
				}
                throw new ArgumentException("no pawn can move to " + movetext);

			}
			else if (Regex.IsMatch(movetext, "^[abcdefgh][12345678][BNRQK]$"))
			{
				//pawn forward, promotion
                this.To = ChessPositionInfo.Parse(movetext.Substring(0, 2));
                tmppos = To.PositionInDirection(mysouth);
				if (board.PieceAt(tmppos) == mypawn)
				{
					From = tmppos;
					Promote = movetext[2].ParseAsPiece(me);
					return;
				}
                throw new ArgumentException("no pawn can promoted to " + movetext.Substring(0, 2));
			}
			else if (Regex.IsMatch(movetext, "^[abcdefgh][abcdefgh][12345678]$"))
			{
				//pawn attack
                this.To = ChessPositionInfo.Parse(movetext.Substring(1, 2));
				tmpfile = ChessFileInfo.Parse(movetext[0]);
				this.From = filter(board, To, mypawn, tmpfile, ChessRank.EMPTY);
				return;
			}
			else if (Regex.IsMatch(movetext, "^[abcdefgh][abcdefgh][12345678][BNRQK]$"))
			{
				//pawn attack, promote
				this.To = ChessPositionInfo.Parse(movetext.Substring(1, 2));
                tmpfile = ChessFileInfo.Parse(movetext[0]);
				this.From = filter(board, To, mypawn, tmpfile, ChessRank.EMPTY);
				this.Promote = movetext[3].ParseAsPiece(me);
				return;
			}
			else if (Regex.IsMatch(movetext, "^[BNRQK][abcdefgh][12345678]$"))
			{
				//normal attack
				this.To = ChessPositionInfo.Parse(movetext.Substring(1, 2));
				tmppiece = movetext[0].ParseAsPiece(me);
				this.From = filter(board, To, tmppiece, ChessFile.EMPTY, ChessRank.EMPTY);
				return;
			}
			else if (Regex.IsMatch(movetext, "^[BNRQK][abcdefgh][abcdefgh][12345678]$"))
			{
				//normal, specify file
				this.To = ChessPositionInfo.Parse(movetext.Substring(2, 2));
				tmppiece = movetext[0].ParseAsPiece(me);
                tmpfile = ChessFileInfo.Parse(movetext[1]);
				this.From = filter(board, To, tmppiece, tmpfile, ChessRank.EMPTY);
				return;
			}
			else if (Regex.IsMatch(movetext, "^[BNRQK][12345678][abcdefgh][12345678]$"))
			{
				//normal, specify rank
				this.To = ChessPositionInfo.Parse(movetext.Substring(2, 2));
				tmppiece = movetext[0].ParseAsPiece(me);
                tmprank = ChessRankInfo.Parse(movetext[1]);
				this.From = filter(board, To, tmppiece, ChessFile.EMPTY, tmprank);
				return;

			}
			else if (Regex.IsMatch(movetext, "^[BNRQK][abcdefgh][12345678][abcdefgh][12345678]$"))
			{
				//normal, specify rank and file
				this.To = ChessPositionInfo.Parse(movetext.Substring(3, 2));
				tmppiece = movetext[0].ParseAsPiece(me);
                tmpfile = ChessFileInfo.Parse(movetext[1]);
                tmprank = ChessRankInfo.Parse(movetext[2]);
				this.From = filter(board, To, tmppiece, tmpfile, tmprank);
				return;
			}

		}
		private ChessPosition filter(ChessBoard board, ChessPosition attackto, ChessPiece piece, ChessFile file, ChessRank rank)
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
                ChessMoves allLegal = new ChessMoves(ChessMove.GenMovesLegal(board));
                fits.Clear();
				foreach (ChessMove move in allLegal)
				{
					if (move.To != attackto) { continue; }
					if (board.PieceAt(move.From) != piece) { continue; }
					if (file != ChessFile.EMPTY && move.From.GetFile() != file) { continue; }
					if (rank != ChessRank.EMPTY && move.From.GetRank() != rank) { continue; }
                    fits.Add(move.From);
				}
			}

            if (fits.Count != 1)
			{
                throw new ArgumentException("invalid move input");
				
			}

            return fits[0];
		}

		public override string ToString()
		{
			string retval = "";
			if (Promote == ChessPiece.EMPTY)
			{
				retval = From.PositionToString().ToLower() + To.PositionToString().ToLower();
			}
			else
			{
                retval = From.PositionToString().ToLower() + To.PositionToString().ToLower() + Promote.PieceToString().ToLower();
			}
			return retval;
		}
		public bool IsSameAs(ChessMove move)
		{
			return this.From == move.From && this.To == move.To && this.Promote == move.Promote;
		}
		public bool IsLegal(ChessBoard board)
		{
            ChessMoves legalmoves = new ChessMoves(ChessMove.GenMovesLegal(board));
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

			string sTo = (this.To.PositionToString());
            string sPiece = piece.PieceToString().ToUpper();
            string sRank = fromrank.RankToString().ToLower();
            string sFile = fromfile.FileToString().ToLower();
			string sProm = "";
			
			//enpassant cap
			if(this.To==board.EnPassant && (piece==ChessPiece.WPawn || piece==ChessPiece.BPawn))
			{
				iscap = true;
			}
			
			if (isprom)
			{
                sProm = this.Promote.PieceToString().ToUpper();
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
                foreach (ChessPosition pos in board.AttacksTo(this.To, piece.PieceToPlayer()).ToPositions())
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
				if (ChessMove.GenMovesLegal(board).Any())
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

        public static IEnumerable<ChessMove> GenMovesLegal(ChessBoard board)
		{
			ChessBoard workingboard = new ChessBoard(board.FEN);
			
			ChessPlayer me = board.WhosTurn;
			foreach (ChessMove move in GenMoves(workingboard))
			{
				workingboard.MoveApply(move);
                bool resultsInCheck = workingboard.IsCheck(me);
                workingboard.MoveUndo();
                if (!resultsInCheck) { yield return move; }
			}

		}

		public static IEnumerable<ChessMove> GenMoves(ChessBoard board)
		{
			return GenMoves(board, false);
		}

        public static IEnumerable<ChessMove> GenMoves(ChessBoard board, bool capsOnly)
        {
            ChessBitboard mypieces = board[board.WhosTurn];
            ChessBitboard hispieces = board[board.WhosTurn.PlayerOther()];
            ChessBitboard kingAttacks = Attacks.KingAttacks(board.KingPosition(board.WhosTurn)) & ~mypieces;
            //return GenMoves(board, board[board.WhosTurn], capsOnly ? board[board.WhosTurn.PlayerOther()] : ~board[board.WhosTurn], !capsOnly);
            if (board.Checkers == ChessBitboard.Empty)
            {
                //not in check, normal logic
                if (capsOnly) { kingAttacks &= hispieces; }
                return GenMoves(board, mypieces, capsOnly ? hispieces : ~mypieces, !capsOnly, kingAttacks);
            }
            else
            {
                //return GenMoves(board, mypieces, capsOnly ? hispieces : ~mypieces, !capsOnly, kingAttacks);
                if (board.Checkers.BitCount() > 1)
                {
                    //multiple attackers, king move is only option.
                    return GenMoves(board, board.KingPosition(board.WhosTurn).Bitboard(), ~mypieces, false, kingAttacks);
                }
                else
                {
                    ChessPosition attackerPos = board.Checkers.NorthMostPosition();
                    ChessPosition kingPos = board.KingPosition(board.WhosTurn);
                    ChessDirection dir = kingPos.DirectionTo(attackerPos);
                    ChessBitboard attackLocs = ChessBitboard.Empty;
                    while (kingPos != attackerPos)
                    {
                        kingPos = kingPos.PositionInDirectionUnsafe(dir);
                        attackLocs |= kingPos.Bitboard();
                    }
                    //move any piece to either location of attacker, or in path, or additionally move king out of harms way.
                    return GenMoves(board, mypieces, attackLocs, false, kingAttacks);
                }
            }
        }

        private static IEnumerable<ChessMove> GenMoves(ChessBoard board, ChessBitboard possibleMovers, ChessBitboard targetLocations, bool castling, ChessBitboard kingTargets)
        {
            System.Diagnostics.Debug.Assert((possibleMovers & ~board[board.WhosTurn]) == ChessBitboard.Empty); //possible movers must be subset of my pieces
            System.Diagnostics.Debug.Assert((targetLocations & board[board.WhosTurn]) == ChessBitboard.Empty); //targets may not include my pieces.
            System.Diagnostics.Debug.Assert((kingTargets & ~Attacks.KingAttacks(board.KingPosition(board.WhosTurn))) == ChessBitboard.Empty); //king targets is very specific, must filter before calling this

            ChessPiece mypawn = board.WhosTurn == ChessPlayer.White ? ChessPiece.WPawn : ChessPiece.BPawn;
            ChessPiece myknight = board.WhosTurn == ChessPlayer.White ? ChessPiece.WKnight : ChessPiece.BKnight;
            ChessPiece mybishop = board.WhosTurn == ChessPlayer.White ? ChessPiece.WBishop : ChessPiece.BBishop;
            ChessPiece myrook = board.WhosTurn == ChessPlayer.White ? ChessPiece.WRook : ChessPiece.BRook;
            ChessPiece myqueen = board.WhosTurn == ChessPlayer.White ? ChessPiece.WQueen : ChessPiece.BQueen;
            ChessPiece myking = board.WhosTurn == ChessPlayer.White ? ChessPiece.WKing : ChessPiece.BKing;

            ChessDirection mypawnwest = board.WhosTurn == ChessPlayer.White ? ChessDirection.DirNW : ChessDirection.DirSW;
            ChessDirection mypawneast = board.WhosTurn == ChessPlayer.White ? ChessDirection.DirNE : ChessDirection.DirSE;
            ChessDirection mypawnnorth = board.WhosTurn == ChessPlayer.White ? ChessDirection.DirN : ChessDirection.DirS;
            ChessDirection mypawnsouth = board.WhosTurn == ChessPlayer.White ? ChessDirection.DirS : ChessDirection.DirN;
            ChessRank myrank8 = board.WhosTurn == ChessPlayer.White ? ChessRank.Rank8 : ChessRank.Rank1;
            ChessRank myrank2 = board.WhosTurn == ChessPlayer.White ? ChessRank.Rank2 : ChessRank.Rank7;

            ChessBitboard attacks = ChessBitboard.Empty;


            //loop through all non pawn locations
            ChessBitboard piecePositions = possibleMovers & ~board[ChessPieceType.Pawn];
            while (piecePositions != ChessBitboard.Empty)// (ChessPosition piecepos in (board[board.WhosTurn] & ~board[ChessPieceType.Pawn]).ToPositions())
            {
                ChessPosition piecepos = ChessBitboardInfo.PopFirst(ref piecePositions);
                ChessPiece piece = board.PieceAt(piecepos);
                ChessPieceType pieceType = piece.ToPieceType();
                switch (pieceType)
                {
                    case ChessPieceType.Knight:
                        attacks = Attacks.KnightAttacks(piecepos) & targetLocations;
                        break;
                    case ChessPieceType.Bishop:
                        attacks = MagicBitboards.BishopAttacks(piecepos, board.PieceLocationsAll) & targetLocations;
                        break;
                    case ChessPieceType.Rook:
                        attacks = MagicBitboards.RookAttacks(piecepos, board.PieceLocationsAll) & targetLocations;
                        break;
                    case ChessPieceType.Queen:
                        attacks = MagicBitboards.QueenAttacks(piecepos, board.PieceLocationsAll) & targetLocations;
                        break;
                    case ChessPieceType.King:
                        attacks = kingTargets;
                        break;
                }
                while (attacks != ChessBitboard.Empty)
                {
                    ChessPosition attackPos = ChessBitboardInfo.PopFirst(ref attacks);
                    yield return new ChessMove(piecepos, attackPos);
                }
            }


            //pawn caps
            piecePositions = possibleMovers & board[ChessPieceType.Pawn];


            if (piecePositions != ChessBitboard.Empty)
            {
                ChessBitboard pawnTargets = (targetLocations & board.PieceLocationsAll) | (board.EnPassant.IsInBounds() ? board.EnPassant.Bitboard() : 0);

                //pawn captures.
                foreach (ChessDirection capDir in new ChessDirection[] { mypawneast, mypawnwest })
                {
                    attacks = piecePositions.Shift(capDir) & pawnTargets;
                    while (attacks != ChessBitboard.Empty)
                    {
                        ChessPosition targetpos = ChessBitboardInfo.PopFirst(ref attacks);
                        ChessPosition piecepos = targetpos.PositionInDirectionUnsafe(capDir.Opposite());
                        if (targetpos.GetRank() == myrank8)
                        {
                            yield return new ChessMove(piecepos, targetpos, myqueen);
                            yield return new ChessMove(piecepos, targetpos, myrook);
                            yield return new ChessMove(piecepos, targetpos, mybishop);
                            yield return new ChessMove(piecepos, targetpos, myknight);
                        }
                        else
                        {
                            yield return new ChessMove(piecepos, targetpos);
                        }
                    }
                }

                //pawn jumps
                pawnTargets = targetLocations & ~board.PieceLocationsAll; //empty squares pawns could jump to

                //find single jumpers.
                attacks = pawnTargets.Shift(mypawnsouth) & piecePositions;
                while (attacks != ChessBitboard.Empty)
                {
                    ChessPosition piecepos = ChessBitboardInfo.PopFirst(ref attacks);
                    ChessPosition targetpos = piecepos.PositionInDirectionUnsafe(mypawnnorth);
                    if (targetpos.GetRank() == myrank8)
                    {
                        yield return new ChessMove(piecepos, targetpos, myqueen);
                        yield return new ChessMove(piecepos, targetpos, myrook);
                        yield return new ChessMove(piecepos, targetpos, mybishop);
                        yield return new ChessMove(piecepos, targetpos, myknight);
                    }
                    else
                    {
                        yield return new ChessMove(piecepos, targetpos);
                    }
                }

                //pawn double jumps
                attacks = myrank2.Bitboard()
                    & piecePositions
                    & pawnTargets.Shift(mypawnsouth).Shift(mypawnsouth)
                    & ~board.PieceLocationsAll.Shift(mypawnsouth);
                while (attacks != ChessBitboard.Empty)
                {
                    ChessPosition piecepos = ChessBitboardInfo.PopFirst(ref attacks);
                    ChessPosition targetpos = piecepos.PositionInDirectionUnsafe(mypawnnorth).PositionInDirectionUnsafe(mypawnnorth);
                    yield return new ChessMove(piecepos, targetpos);
                }

            }




            if (castling)
            {


                //castling
                if (board.WhosTurn == ChessPlayer.White)
                {
                    if ((board.CastleRights & CastleFlags.WhiteShort) != 0
                        && board.PieceAt(ChessPosition.E1) == ChessPiece.WKing
                        && board.PieceAt(ChessPosition.H1) == ChessPiece.WRook
                        && board.PieceAt(ChessPosition.F1) == ChessPiece.EMPTY
                        && board.PieceAt(ChessPosition.G1) == ChessPiece.EMPTY
                        && !board.PositionAttacked(ChessPosition.E1, ChessPlayer.Black)
                        && !board.PositionAttacked(ChessPosition.F1, ChessPlayer.Black)
                        && !board.PositionAttacked(ChessPosition.G1, ChessPlayer.Black))
                    {
                        yield return new ChessMove(ChessPosition.E1, ChessPosition.G1);
                    }
                    if ((board.CastleRights & CastleFlags.WhiteLong) != 0
                        && board.PieceAt(ChessPosition.E1) == ChessPiece.WKing
                        && board.PieceAt(ChessPosition.A1) == ChessPiece.WRook
                        && board.PieceAt(ChessPosition.B1) == ChessPiece.EMPTY
                        && board.PieceAt(ChessPosition.C1) == ChessPiece.EMPTY
                        && board.PieceAt(ChessPosition.D1) == ChessPiece.EMPTY
                        && !board.PositionAttacked(ChessPosition.E1, ChessPlayer.Black)
                        && !board.PositionAttacked(ChessPosition.D1, ChessPlayer.Black)
                        && !board.PositionAttacked(ChessPosition.C1, ChessPlayer.Black))
                    {
                        yield return new ChessMove(ChessPosition.E1, ChessPosition.C1);
                    }
                }
                else
                {
                    if ((board.CastleRights & CastleFlags.BlackShort) != 0
                        && board.PieceAt(ChessPosition.E8) == ChessPiece.BKing
                        && board.PieceAt(ChessPosition.H8) == ChessPiece.BRook
                        && board.PieceAt(ChessPosition.F8) == ChessPiece.EMPTY
                        && board.PieceAt(ChessPosition.G8) == ChessPiece.EMPTY
                        && !board.PositionAttacked(ChessPosition.E8, ChessPlayer.White)
                        && !board.PositionAttacked(ChessPosition.F8, ChessPlayer.White)
                        && !board.PositionAttacked(ChessPosition.G8, ChessPlayer.White))
                    {
                        yield return new ChessMove(ChessPosition.E8, ChessPosition.G8);
                    }
                    if ((board.CastleRights & CastleFlags.BlackLong) != 0
                        && board.PieceAt(ChessPosition.E8) == ChessPiece.BKing
                        && board.PieceAt(ChessPosition.A8) == ChessPiece.BRook
                        && board.PieceAt(ChessPosition.B8) == ChessPiece.EMPTY
                        && board.PieceAt(ChessPosition.C8) == ChessPiece.EMPTY
                        && board.PieceAt(ChessPosition.D8) == ChessPiece.EMPTY
                        && !board.PositionAttacked(ChessPosition.E8, ChessPlayer.White)
                        && !board.PositionAttacked(ChessPosition.D8, ChessPlayer.White)
                        && !board.PositionAttacked(ChessPosition.C8, ChessPlayer.White))
                    {
                        yield return new ChessMove(ChessPosition.E8, ChessPosition.C8);
                    }

                }
            }

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

            foreach (ChessPosition piecepos in ChessPositionInfo.AllPositions)
            {
                ChessPiece piece = board.PieceAt(piecepos);
                if (piece == ChessPiece.EMPTY) { continue; }

                //knight attacks
                if (piece == myknight)
                {
                    foreach (ChessDirection dir in ChessDirectionInfo.AllDirectionsKnight)
                    {
                        AddDirection(retval, board, piecepos, dir, board.WhosTurn, 1, CapsOnly);
                    }
                    continue;
                }
                //bishop attacks
                if (piece == mybishop)
                {
                    foreach (ChessDirection dir in ChessDirectionInfo.AllDirectionsBishop)
                    {
                        AddDirection(retval, board, piecepos, dir, board.WhosTurn, 8, CapsOnly);
                    }
                    continue;
                }
                //rook attacks
                if (piece == myrook)
                {
                    foreach (ChessDirection dir in ChessDirectionInfo.AllDirectionsRook)
                    {
                        AddDirection(retval, board, piecepos, dir, board.WhosTurn, 8, CapsOnly);
                    }
                    continue;
                }
                //queen attacks
                if (piece == myqueen)
                {
                    foreach (ChessDirection dir in ChessDirectionInfo.AllDirectionsQueen)
                    {
                        AddDirection(retval, board, piecepos, dir, board.WhosTurn, 8, CapsOnly);
                    }
                    continue;
                }
                //king attacks
                if (piece == myking)
                {
                    foreach (ChessDirection dir in ChessDirectionInfo.AllDirectionsQueen)
                    {
                        AddDirection(retval, board, piecepos, dir, board.WhosTurn, 1, CapsOnly);
                    }
                    continue;
                }
                //pawn moves
                if (piece == mypawn)
                {
                    //pawn east caps
                    targetpos = piecepos.PositionInDirection(mypawneast);
                    targetpiece = board.PieceAt(targetpos);
                    if (targetpos.IsInBounds())
                    {
                        if (targetpiece.PieceToPlayer() == board.WhosTurn.PlayerOther())
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
                    targetpos = piecepos.PositionInDirection(mypawnwest);
                    targetpiece = board.PieceAt(targetpos);
                    if (targetpos.IsInBounds())
                    {
                        if (targetpiece.PieceToPlayer() == board.WhosTurn.PlayerOther())
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
                        targetpos = piecepos.PositionInDirection(mypawnnorth);
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
                                targetpos = targetpos.PositionInDirection(mypawnnorth);
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
                    if ((board.CastleRights & CastleFlags.WhiteShort) != 0
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
                    if ((board.CastleRights & CastleFlags.WhiteLong) != 0
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
                    if ((board.CastleRights & CastleFlags.BlackShort) != 0
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
                    if ((board.CastleRights & CastleFlags.BlackLong) != 0
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
        public static IEnumerable<ChessMove> GenMovesOld2(ChessBoard board, bool CapsOnly)
        {

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

            ChessBitboard attacks = ChessBitboard.Empty;



            //pawn caps
            ChessBitboard pawnTargets = board[board.WhosTurn.PlayerOther()] | (board.EnPassant.IsInBounds() ? board.EnPassant.Bitboard() : 0);
            foreach (ChessDirection capDir in new ChessDirection[] { mypawneast, mypawnwest })
            {
                attacks = board[mypawn].Shift(capDir) & pawnTargets;
                while (attacks != ChessBitboard.Empty)
                {
                    ChessPosition targetpos = ChessBitboardInfo.PopFirst(ref attacks);
                    ChessPosition piecepos = targetpos.PositionInDirectionUnsafe(capDir.Opposite());
                    if (targetpos.GetRank() == myrank8)
                    {
                        yield return new ChessMove(piecepos, targetpos, myqueen);
                        yield return new ChessMove(piecepos, targetpos, myrook);
                        yield return new ChessMove(piecepos, targetpos, mybishop);
                        yield return new ChessMove(piecepos, targetpos, myknight);
                    }
                    else
                    {
                        yield return new ChessMove(piecepos, targetpos);
                    }
                }
            }

            ChessBitboard targetLocations = board[board.WhosTurn.PlayerOther()];
            if (!CapsOnly)
            {
                targetLocations |= ~board.PieceLocationsAll;
            }

            //loop through all non pawn locations
            ChessBitboard piecePositions = board[board.WhosTurn] & ~board[ChessPieceType.Pawn];
            while(piecePositions != ChessBitboard.Empty)// (ChessPosition piecepos in (board[board.WhosTurn] & ~board[ChessPieceType.Pawn]).ToPositions())
            {
                ChessPosition piecepos = ChessBitboardInfo.PopFirst(ref piecePositions);
                ChessPiece piece = board.PieceAt(piecepos);
                ChessPieceType pieceType = piece.ToPieceType();
                switch (pieceType)
                {
                    case ChessPieceType.Knight:
                        attacks = Attacks.KnightAttacks(piecepos) & targetLocations;
                        break;
                    case ChessPieceType.Bishop:
                        attacks = MagicBitboards.BishopAttacks(piecepos, board.PieceLocationsAll) & targetLocations;
                        break;
                    case ChessPieceType.Rook:
                        attacks = MagicBitboards.RookAttacks(piecepos, board.PieceLocationsAll) & targetLocations;
                        break;
                    case ChessPieceType.Queen:
                        attacks = MagicBitboards.QueenAttacks(piecepos, board.PieceLocationsAll) & targetLocations;
                        break;
                    case ChessPieceType.King:
                        attacks = Attacks.KingAttacks(piecepos) & targetLocations;
                        break;
                }
                while (attacks != ChessBitboard.Empty)
                {
                    ChessPosition attackPos = ChessBitboardInfo.PopFirst(ref attacks);
                    yield return new ChessMove(piecepos, attackPos);
                }
            }

            

            

            if (!CapsOnly)
            {
                //pawn jumps
                attacks = (board[mypawn].Shift(mypawnnorth) & ~board.PieceLocationsAll);
                while (attacks != ChessBitboard.Empty)
                {
                    ChessPosition targetpos = ChessBitboardInfo.PopFirst(ref attacks);
                    ChessPosition piecepos = targetpos.PositionInDirectionUnsafe(mypawnnorth.Opposite());
                    if (targetpos.GetRank() == myrank8)
                    {
                        yield return new ChessMove(piecepos, targetpos, myqueen);
                        yield return new ChessMove(piecepos, targetpos, myrook);
                        yield return new ChessMove(piecepos, targetpos, mybishop);
                        yield return new ChessMove(piecepos, targetpos, myknight);
                    }
                    else
                    {
                        yield return new ChessMove(piecepos, targetpos);
                        if (piecepos.GetRank() == myrank2)
                        {
                            var doubleJumpPos = targetpos.PositionInDirectionUnsafe(mypawnnorth);
                            if (board.PieceAt(doubleJumpPos) == ChessPiece.EMPTY)
                            {
                                yield return new ChessMove(piecepos, doubleJumpPos);
                            }
                        }
                    }
                }

                //castling
                if (board.WhosTurn == ChessPlayer.White)
                {
                    if ((board.CastleRights & CastleFlags.WhiteShort) != 0
                        && board.PieceAt(ChessPosition.E1) == ChessPiece.WKing
                        && board.PieceAt(ChessPosition.H1) == ChessPiece.WRook
                        && board.PieceAt(ChessPosition.F1) == ChessPiece.EMPTY
                        && board.PieceAt(ChessPosition.G1) == ChessPiece.EMPTY
                        && !board.PositionAttacked(ChessPosition.E1, ChessPlayer.Black)
                        && !board.PositionAttacked(ChessPosition.F1, ChessPlayer.Black)
                        && !board.PositionAttacked(ChessPosition.G1, ChessPlayer.Black))
                    {
                        yield return new ChessMove(ChessPosition.E1, ChessPosition.G1);
                    }
                    if ((board.CastleRights & CastleFlags.WhiteLong) != 0
                        && board.PieceAt(ChessPosition.E1) == ChessPiece.WKing
                        && board.PieceAt(ChessPosition.A1) == ChessPiece.WRook
                        && board.PieceAt(ChessPosition.B1) == ChessPiece.EMPTY
                        && board.PieceAt(ChessPosition.C1) == ChessPiece.EMPTY
                        && board.PieceAt(ChessPosition.D1) == ChessPiece.EMPTY
                        && !board.PositionAttacked(ChessPosition.E1, ChessPlayer.Black)
                        && !board.PositionAttacked(ChessPosition.D1, ChessPlayer.Black)
                        && !board.PositionAttacked(ChessPosition.C1, ChessPlayer.Black))
                    {
                        yield return new ChessMove(ChessPosition.E1, ChessPosition.C1);
                    }
                }
                else
                {
                    if ((board.CastleRights & CastleFlags.BlackShort) != 0
                        && board.PieceAt(ChessPosition.E8) == ChessPiece.BKing
                        && board.PieceAt(ChessPosition.H8) == ChessPiece.BRook
                        && board.PieceAt(ChessPosition.F8) == ChessPiece.EMPTY
                        && board.PieceAt(ChessPosition.G8) == ChessPiece.EMPTY
                        && !board.PositionAttacked(ChessPosition.E8, ChessPlayer.White)
                        && !board.PositionAttacked(ChessPosition.F8, ChessPlayer.White)
                        && !board.PositionAttacked(ChessPosition.G8, ChessPlayer.White))
                    {
                        yield return new ChessMove(ChessPosition.E8, ChessPosition.G8);
                    }
                    if ((board.CastleRights & CastleFlags.BlackLong) != 0
                        && board.PieceAt(ChessPosition.E8) == ChessPiece.BKing
                        && board.PieceAt(ChessPosition.A8) == ChessPiece.BRook
                        && board.PieceAt(ChessPosition.B8) == ChessPiece.EMPTY
                        && board.PieceAt(ChessPosition.C8) == ChessPiece.EMPTY
                        && board.PieceAt(ChessPosition.D8) == ChessPiece.EMPTY
                        && !board.PositionAttacked(ChessPosition.E8, ChessPlayer.White)
                        && !board.PositionAttacked(ChessPosition.D8, ChessPlayer.White)
                        && !board.PositionAttacked(ChessPosition.C8, ChessPlayer.White))
                    {
                        yield return new ChessMove(ChessPosition.E8, ChessPosition.C8);
                    }

                }
            }

        }


		private static void AddDirection(ChessMoves retval, ChessBoard board, ChessPosition from, ChessDirection dir, ChessPlayer forwho, int maxdist, bool CapsOnly)
		{
            ChessPosition to = from.PositionInDirection(dir);
			int i = 1;
			while (to.IsInBounds() && i <= maxdist)
			{
				ChessPiece targetpiece = board.PieceAt(to);
				if (targetpiece == ChessPiece.EMPTY)
				{
					if (!CapsOnly) { retval.Add(new ChessMove(from, to)); }
				}
                else if (targetpiece.PieceToPlayer() == forwho)
				{
					break;
				}
				else
				{
					retval.Add(new ChessMove(from, to));
					break;
				}
				to = to.PositionInDirection(dir);
				i++;
			}
		}


		public class Comp : Comparer<ChessMove>
		{
			public readonly ChessBoard board;
			public readonly ChessMove tt_move;
			public readonly bool UseSEE;
            private readonly Dictionary<ChessMove, int> _dic = new Dictionary<ChessMove, int>();
			public Comp(ChessBoard a_board, ChessMove a_tt_move, bool a_see)
			{
				board = a_board;
				tt_move = a_tt_move;
				UseSEE = false;
			}

            private int GetScore(ChessMove move)
            {
                if (_dic.ContainsKey(move))
                {
                    return _dic[move];
                }
                else
                {
                    int score;
                    if (UseSEE)
                    {
                        score = CompEstScoreSEE(move, board);
                    }
                    else
                    {
                        score = CompEstScore(move, board);
                    }
                    _dic.Add(move, score);
                    return score;
                }
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

                int xScore = GetScore(x);
                int yScore = GetScore(y);
                if (xScore > yScore) { return -1; }
                if (xScore < yScore) { return 1; }
                return 0;
			}

			public static int CompEstScore(ChessMove move, ChessBoard board)
			{
				int retval = 0;

				ChessPiece mover = board.PieceAt(move.From);
				ChessPiece taken = board.PieceAt(move.To);
				ChessPlayer me = board.WhosTurn;

                retval -= eval._pcsqPiecePosStage[(int)mover, (int)move.From, (int)ChessGameStage.Opening];
                retval += eval._pcsqPiecePosStage[(int)mover, (int)move.To, (int)ChessGameStage.Opening];

				if (taken != ChessPiece.EMPTY)
				{
					retval -= eval._matPieceStage[(int)taken,(int)ChessGameStage.Opening];
				}

				if (me == ChessPlayer.Black) { retval = -retval; }
				return retval;
			}

			public static int CompEstScoreSEE(ChessMove move, ChessBoard board)
			{
                System.Diagnostics.Debug.Assert(move != ChessMove.EMPTY);
                //System.Diagnostics.Debug.Assert(ChessMove.GenMoves(board).Contains(move));

				int retval = 0;


				ChessPiece mover = board.PieceAt(move.From);
				ChessPiece taken = board.PieceAt(move.To);
                ChessPlayer me = mover.PieceToPlayer();

				

				if (taken != ChessPiece.EMPTY)
				{
                    retval += taken.PieceValBasic();
					//do see
					var attacks = board.AttacksTo(move.To);
                    attacks &= ~move.From.Bitboard();
                    retval -= attackswap(board, attacks, me.PlayerOther(), move.To, mover.PieceValBasic());
				}

                //int pieceSqVal = 0;
                //pieceSqVal -= eval._pcsqPiecePosStage[(int)mover, (int)move.From, (int)ChessGameStage.Opening];
                //pieceSqVal += eval._pcsqPiecePosStage[(int)mover, (int)move.To, (int)ChessGameStage.Opening];
                //if (me == ChessPlayer.Black) { pieceSqVal = -pieceSqVal; }
                //retval += pieceSqVal;


				return retval;
			}
			static int attackswap(ChessBoard board, ChessBitboard attacks, ChessPlayer player, ChessPosition positionattacked, int pieceontargetval)
			{
				int nextAttackPieceVal = 0;
				ChessPosition nextAttackPos = 0;

				bool HasAttack = attackpop(board, ref attacks, player, positionattacked, out nextAttackPos, out nextAttackPieceVal);
				if (!HasAttack) { return 0; }

                int moveval = pieceontargetval - attackswap(board, attacks, player.PlayerOther(), positionattacked, nextAttackPieceVal);

				if (moveval > 0)
				{
					return moveval;
				}
				else
				{
					return 0;
				}
			}

			static bool attackpop(ChessBoard board, ref ChessBitboard attacks, ChessPlayer player, ChessPosition positionattacked, out ChessPosition OutFrom, out int OutPieceVal)
			{
			
                OutFrom = ChessPosition.OUTOFBOUNDS;
				OutPieceVal = 0;

                ChessBitboard myAttacks = attacks & board[player];
                if ((myAttacks & board[ChessPieceType.Pawn]) != 0)
                {
                    OutFrom = (myAttacks & board[ChessPieceType.Pawn]).NorthMostPosition();
                    OutPieceVal = 100;
                }
                else if ((myAttacks & board[ChessPieceType.Knight]) != 0)
                {
                    OutFrom = (myAttacks & board[ChessPieceType.Knight]).NorthMostPosition();
                    OutPieceVal = 300;
                }
                else if ((myAttacks & board[ChessPieceType.Bishop]) != 0)
                {
                    OutFrom = (myAttacks & board[ChessPieceType.Bishop]).NorthMostPosition();
                    OutPieceVal = 300;
                }
                else if ((myAttacks & board[ChessPieceType.Rook]) != 0)
                {
                    OutFrom = (myAttacks & board[ChessPieceType.Rook]).NorthMostPosition();
                    OutPieceVal = 500;
                }
                else if ((myAttacks & board[ChessPieceType.Queen]) != 0)
                {
                    OutFrom = (myAttacks & board[ChessPieceType.Queen]).NorthMostPosition();
                    OutPieceVal = 900;
                }
                else if ((myAttacks & board[ChessPieceType.King]) != 0)
                {
                    OutFrom = (myAttacks & board[ChessPieceType.King]).NorthMostPosition();
                    OutPieceVal = 100000;
                }

				if (OutFrom == ChessPosition.OUTOFBOUNDS)
				{
					//i'm out of attacks to this position;
					return false;
				}

                ChessDirection addAttackFrom = positionattacked.DirectionTo(OutFrom);
                if (!addAttackFrom.IsDirectionKnight())
                {
                    ChessPosition AddPosition = 0;
                    ChessPiece AddPiece = board.PieceInDirection(OutFrom, addAttackFrom, ref AddPosition);
                    if (addAttackFrom.IsDirectionRook() && AddPiece.PieceIsSliderRook())
                    {
                        attacks |= AddPosition.Bitboard();
                    }
                    else if (addAttackFrom.IsDirectionBishop() && AddPiece.PieceIsSliderBishop())
                    {
                        attacks |= AddPosition.Bitboard();
                    }
                }
                attacks &= ~OutFrom.Bitboard();
				return true;

			}

		}
		private static ChessEval eval = new ChessEval();


	}

    public class ChessMoveBuffer
    {
        List<PlyBuffer> _plyBuffers = new List<PlyBuffer>();

        public ChessMoveBuffer(int plyCapacity = 50)
        {
            while (_plyBuffers.Count < plyCapacity)
            {
                _plyBuffers.Add(new PlyBuffer());
            }
        }

        public PlyBuffer this[int ply]
        {
            get
            {
                if (ply > _plyBuffers.Count)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        _plyBuffers.Add(new PlyBuffer());
                    }
                }
                return _plyBuffers[ply];
            }
        }

        public class KillerInfo
        {
            private ChessMove move1;
            private ChessMove move2;

            public void RegisterKiller(ChessMove move)
            {
                if (move != move1)
                {
                    move2 = move1;
                    move1 = move;
                }
            }

            public bool IsKiller(ChessMove move)
            {
                return move == move1 || move == move2;
            }
        }
        public class PlyBuffer
        {
            private readonly MoveInfo[] _array = new MoveInfo[192];
            private int moveCount;

            private readonly KillerInfo[] _playerKillers = new KillerInfo[2];

            public PlyBuffer()
            {
                _playerKillers[0] = new KillerInfo();
                _playerKillers[1] = new KillerInfo();
            }

            public void Initialize(ChessBoard board, bool capsOnly = false)
            {
                moveCount = 0;

                foreach (ChessMove genMove in ChessMove.GenMoves(board, capsOnly))
                {
                    _array[moveCount++].Move = genMove;// = new MoveInfo() { Move = genMove };
                }
            }

            public int MoveCount
            {
                get { return moveCount; }
            }

            public void RegisterCutoff(ChessBoard board, ChessMove move)
            {
                if (board.PieceAt(move.To) != ChessPiece.EMPTY)
                {
                    _playerKillers[(int)board.WhosTurn].RegisterKiller(move);
                }
            }

            public void Sort(ChessBoard board, bool useSEE, ChessMove ttMove)
            {
                //useSEE = true;
                var killers = _playerKillers[(int)board.WhosTurn];
                //first score moves
                for (int i = 0; i < moveCount; i++)
                {
                    ChessMove move = _array[i].Move;
                    ChessPiece piece = board.PieceAt(move.From);
                    bool isCap = board.PieceAt(move.To) != ChessPiece.EMPTY;

                    System.Diagnostics.Debug.Assert(move != ChessMove.EMPTY);

                    //calc pcsq value;
                    int pcSq = 0;
                    int dummyPcSq = 0;
                    board.PcSqEvaluator.PcSqValuesRemove(piece, move.From, ref pcSq, ref dummyPcSq);
                    board.PcSqEvaluator.PcSqValuesAdd(piece, move.To, ref pcSq, ref dummyPcSq);
                    if (board.WhosTurn == ChessPlayer.Black) { pcSq = -pcSq; }

                    int see = 0;

                    //calc flags
                    MoveFlags flags = 0;
                    if (move == ttMove) { flags |= MoveFlags.TransTable; }
                    if (move.Promote != ChessPiece.EMPTY) { flags |= MoveFlags.Promote; }

                    if (isCap)
                    {
                        flags |= MoveFlags.Capture;
                        see = ChessMove.Comp.CompEstScoreSEE(move, board);
                        //if (see > 0) { flags |= MoveFlags.CapturePositive; }
                        //if (see == 0) { flags |= MoveFlags.CaptureEqual; }
                    }

                    //if(piece.ToPieceType() == ChessPieceType.Pawn && (move.To.GetRank() == ChessRank.Rank7 || move.To.GetRank() == ChessRank.Rank2))
                    //{
                    //    flags |= MoveFlags.Pawn7th;
                    //}

                    //if (killers.IsKiller(move)) { flags |= MoveFlags.Killer; }

                    _array[i].SEE = see;
                    _array[i].PcSq = pcSq;
                    _array[i].Flags = flags;
                    _array[i].Score = _array[i].ScoreCalc();

                }

                //now sort array.
                for (int i = 1; i < moveCount; i++)
                {
                    for (int ii = i; ii > 0; ii--)
                    {
                        if (_array[ii].Score > _array[ii - 1].Score)
                        {
                            var tmp = _array[ii];
                            _array[ii] = _array[ii - 1];
                            _array[ii - 1] = tmp;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

            }

            public IEnumerable<ChessMove> SortedMoves()
            {
                for (int i = 0; i < moveCount; i++)
                {
                    yield return _array[i].Move;
                }
            }

        }

        [System.Diagnostics.DebuggerDisplay(@"{Move} SEE:{SEE} PcSq:{PcSq} Flags:{Flags}")]
        public struct MoveInfo
        {
            public ChessMove Move;
            public int SEE;
            public int PcSq;
            public MoveFlags Flags;
            public int Score;

            public int ScoreCalc()
            {
                return SEE + PcSq
                    + ((Flags & MoveFlags.TransTable) != 0 ? 10000 : 0)
                    + ((Flags & MoveFlags.Capture) != 0 ? 1000 : 0);
            }
            public static bool operator > (MoveInfo x, MoveInfo y)
            {
                return x.Score > y.Score;
                //first check if any of the flags used for ordering are different, if they are, order using the relevant flags value.
                //var xOrderFlags = x.Flags & _orderFlags;
                //var yOrderFlags = y.Flags & _orderFlags;
                if (x.Flags != y.Flags)
                {
                    //int x2 = (int)xOrderFlags;
                    //int y2 = (int)yOrderFlags;
                    return x.Flags > y.Flags;
                }
                else if (x.SEE != y.SEE)
                {
                    return x.SEE > y.SEE;
                }
                else
                {
                    //return false;
                    return x.PcSq > y.PcSq;
                }
            }
            public static bool operator < (MoveInfo x, MoveInfo y)
            {
                return !(x > y);
            }


            //public void SetScores(ChessMove move, int see, int pcSq, MoveFlags flags)
            //{
            //    Move = move;
            //    SEE = see;
            //    PcSq = pcSq;
            //    Flags = flags;
            //}

            //private const MoveFlags _orderFlags = MoveFlags.TransTable | MoveFlags.Promote | MoveFlags.CapturePositive | MoveFlags.CaptureEqual | MoveFlags.Killer;
        }

        [Flags]
        public enum MoveFlags
        {
            Killer = (1 << 0),
            Capture = (1 << 1),
            Promote = (1 << 2),
            TransTable = (1 << 3),
        }

    }
}
