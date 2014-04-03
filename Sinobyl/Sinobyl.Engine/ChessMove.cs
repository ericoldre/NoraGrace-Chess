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

    public enum ChessMove
    {
        EMPTY = 0
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



	public static class ChessMoveInfo
	{
		//public readonly ChessPosition From;
		//public readonly ChessPosition To;
		//public readonly ChessPiece Promote;
		//public int? EstScore;




        

		public static ChessMove Create(ChessPosition from, ChessPosition to)
		{
            System.Diagnostics.Debug.Assert(from.IsInBounds());
            System.Diagnostics.Debug.Assert(to.IsInBounds());

            return (ChessMove)((int)from | ((int)to << 8));

		}

		public static ChessMove Create(ChessPosition from, ChessPosition to, ChessPiece promote)
		{

            System.Diagnostics.Debug.Assert(from.IsInBounds());
            System.Diagnostics.Debug.Assert(to.IsInBounds());

            return (ChessMove)((int)from | ((int)to << 8) | (1 << 16) | ((int)promote << 17));
		}

        public static ChessPosition From(this ChessMove move)
        {
            return (ChessPosition)((int)move & 255);
        }

        public static ChessPosition To(this ChessMove move)
        {
            return (ChessPosition)(((int)move >> 8) & 255);
        }

        public static bool IsPromote(this ChessMove move, out ChessPiece promote)
        {
            if(((int)move & 1 << 16) != 0)
            {
                promote = (ChessPiece)(((int)move << 17) & 0xF);
                return true;
            }
            else
            {
                promote = ChessPiece.EMPTY;
                return false;
            }
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

            return ChessMove.EMPTY;

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

		public static string Description(this ChessMove move)
		{
            ChessPiece promotePiece;
			if (move.IsPromote(out promotePiece))
			{
                return move.From().PositionToString().ToLower() + move.To().PositionToString().ToLower() + promotePiece.PieceToString().ToLower();
			}
			else
			{
                return move.From().PositionToString().ToLower() + move.To().PositionToString().ToLower();
			}
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


		public static string ToString(this ChessMove move, ChessBoard board)
		{
			string retval = "";
            ChessPiece piece = board.PieceAt(move.From());
            bool iscap = (board.PieceAt(move.To()) != ChessPiece.EMPTY);

            ChessRank fromrank = move.From().GetRank();
            ChessFile fromfile = move.From().GetFile();
            ChessPiece promote;

            bool isprom = move.IsPromote(out promote);

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
                sProm = promote.PieceToString().ToUpper();
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
                                retval.Add(ChessMoveInfo.Create(piecepos, targetpos, myqueen));
                                retval.Add(ChessMoveInfo.Create(piecepos, targetpos, myrook));
                                retval.Add(ChessMoveInfo.Create(piecepos, targetpos, mybishop));
                                retval.Add(ChessMoveInfo.Create(piecepos, targetpos, myknight));
                            }
                            else
                            {
                                retval.Add(ChessMoveInfo.Create(piecepos, targetpos));
                            }
                        }
                        else if (targetpos == board.EnPassant)
                        {
                            retval.Add(ChessMoveInfo.Create(piecepos, targetpos));
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
                                retval.Add(ChessMoveInfo.Create(piecepos, targetpos, myqueen));
                                retval.Add(ChessMoveInfo.Create(piecepos, targetpos, myrook));
                                retval.Add(ChessMoveInfo.Create(piecepos, targetpos, mybishop));
                                retval.Add(ChessMoveInfo.Create(piecepos, targetpos, myknight));
                            }
                            else
                            {
                                retval.Add(ChessMoveInfo.Create(piecepos, targetpos));
                            }
                        }
                        else if (targetpos == board.EnPassant)
                        {
                            retval.Add(ChessMoveInfo.Create(piecepos, targetpos));
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
                                retval.Add(ChessMoveInfo.Create(piecepos, targetpos, myqueen));
                                retval.Add(ChessMoveInfo.Create(piecepos, targetpos, myrook));
                                retval.Add(ChessMoveInfo.Create(piecepos, targetpos, mybishop));
                                retval.Add(ChessMoveInfo.Create(piecepos, targetpos, myknight));
                            }
                            else
                            {
                                retval.Add(ChessMoveInfo.Create(piecepos, targetpos));
                            }

                            //double jump
                            if (piecepos.GetRank() == myrank2)
                            {
                                targetpos = targetpos.PositionInDirection(mypawnnorth);
                                targetpiece = board.PieceAt(targetpos);
                                if (targetpiece == ChessPiece.EMPTY)
                                {
                                    retval.Add(ChessMoveInfo.Create(piecepos, targetpos));
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
                        retval.Add(ChessMoveInfo.Create(ChessPosition.E1, ChessPosition.G1));
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
                        retval.Add(ChessMoveInfo.Create(ChessPosition.E1, ChessPosition.C1));
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
                        retval.Add(ChessMoveInfo.Create(ChessPosition.E8, ChessPosition.G8));
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
                        retval.Add(ChessMoveInfo.Create(ChessPosition.E8, ChessPosition.C8));
                    }

                }
            }

            return retval;

        }
        public static IEnumerable<ChessMove> GenMoves(ChessBoard board, bool CapsOnly)
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


            ChessBitboard targetLocations = board.PlayerLocations(board.WhosTurn.PlayerOther());
            if (!CapsOnly)
            {
                targetLocations |= ~board.PieceLocationsAll;
            }

            ChessBitboard pawnTargets = board.PlayerLocations(board.WhosTurn.PlayerOther()) | (board.EnPassant.IsInBounds() ? board.EnPassant.Bitboard() : 0);

            //loop through all non pawn locations
            foreach (ChessPosition piecepos in (board.PlayerLocations(board.WhosTurn) & ~board.PieceLocations(mypawn)).ToPositions())
            {
                ChessPiece piece = board.PieceAt(piecepos);

                //knight attacks
                if (piece == myknight)
                {
                    foreach (ChessPosition attackPos in (Attacks.KnightAttacks(piecepos) & targetLocations).ToPositions())
                    {
                        yield return ChessMoveInfo.Create(piecepos, attackPos);
                    }
                    continue;
                }
                //bishop attacks
                if (piece == mybishop)
                {
                    foreach (ChessPosition attackPos in (Attacks.BishopAttacks(piecepos, board.PieceLocationsAllA1H8, board.PieceLocationsAllH1A8) & targetLocations).ToPositions())
                    {
                        yield return ChessMoveInfo.Create(piecepos, attackPos);
                    }
                    continue;
                }
                //rook attacks
                if (piece == myrook)
                {
                    foreach (ChessPosition attackPos in (Attacks.RookAttacks(piecepos, board.PieceLocationsAll, board.PieceLocationsAllVert) & targetLocations).ToPositions())
                    {
                        yield return ChessMoveInfo.Create(piecepos, attackPos);
                    }
                    continue;
                }
                //queen attacks
                if (piece == myqueen)
                {
                    foreach (ChessPosition attackPos in (Attacks.QueenAttacks(piecepos, board.PieceLocationsAll, board.PieceLocationsAllVert, board.PieceLocationsAllA1H8, board.PieceLocationsAllH1A8) & targetLocations).ToPositions())
                    {
                        yield return ChessMoveInfo.Create(piecepos, attackPos);
                    }
                    continue;
                }
                //king attacks
                if (piece == myking)
                {
                    foreach (ChessPosition attackPos in (Attacks.KingAttacks(piecepos) & targetLocations).ToPositions())
                    {
                        yield return ChessMoveInfo.Create(piecepos, attackPos);
                    }
                    continue;
                }
                //pawn moves
                //if (piece == mypawn)
                //{
                //    //pawn attacks
                //    foreach (var attackPos in (Attacks.PawnAttacks(piecepos, board.WhosTurn) & pawnTargets).ToPositions())
                //    {
                //        if (attackPos.GetRank() == myrank8)
                //        {
                //            retval.Add(ChessMoveInfo.Create(piecepos, attackPos, myqueen));
                //            retval.Add(ChessMoveInfo.Create(piecepos, attackPos, myrook));
                //            retval.Add(ChessMoveInfo.Create(piecepos, attackPos, mybishop));
                //            retval.Add(ChessMoveInfo.Create(piecepos, attackPos, myknight));
                //        }
                //        else
                //        {
                //            retval.Add(ChessMoveInfo.Create(piecepos, attackPos));
                //        }
                //    }

                //    ////pawn jumps
                //    //if (!CapsOnly)
                //    //{
                //    //    //pawn jump
                //    //    ChessPosition targetpos = Chess.PositionInDirection(piecepos, mypawnnorth);
                //    //    targetpiece = board.PieceAt(targetpos);
                //    //    if (targetpiece == ChessPiece.EMPTY)
                //    //    {
                //    //        if (targetpos.GetRank() == myrank8)
                //    //        {
                //    //            retval.Add(ChessMoveInfo.Create(piecepos, targetpos, myqueen));
                //    //            retval.Add(ChessMoveInfo.Create(piecepos, targetpos, myrook));
                //    //            retval.Add(ChessMoveInfo.Create(piecepos, targetpos, mybishop));
                //    //            retval.Add(ChessMoveInfo.Create(piecepos, targetpos, myknight));
                //    //        }
                //    //        else
                //    //        {
                //    //            retval.Add(ChessMoveInfo.Create(piecepos, targetpos));
                //    //        }

                //    //        //double jump
                //    //        if (piecepos.GetRank() == myrank2)
                //    //        {
                //    //            targetpos = Chess.PositionInDirection(targetpos, mypawnnorth);
                //    //            targetpiece = board.PieceAt(targetpos);
                //    //            if (targetpiece == ChessPiece.EMPTY)
                //    //            {
                //    //                retval.Add(ChessMoveInfo.Create(piecepos, targetpos));
                //    //            }
                //    //        }
                //    //    }
                //    //}
                //}
            }

            

            //pawn caps
            foreach (ChessDirection capDir in new ChessDirection[] { mypawneast, mypawnwest })
            {
                foreach (ChessPosition targetpos in (board.PieceLocations(mypawn).Shift(capDir) & pawnTargets).ToPositions())
                {
                    ChessPosition piecepos = targetpos.PositionInDirectionUnsafe(capDir.Opposite());
                    if (targetpos.GetRank() == myrank8)
                    {
                        yield return ChessMoveInfo.Create(piecepos, targetpos, myqueen);
                        yield return ChessMoveInfo.Create(piecepos, targetpos, myrook);
                        yield return ChessMoveInfo.Create(piecepos, targetpos, mybishop);
                        yield return ChessMoveInfo.Create(piecepos, targetpos, myknight);
                    }
                    else
                    {
                        yield return ChessMoveInfo.Create(piecepos, targetpos);
                    }
                }
            }
            if (!CapsOnly)
            {
                //pawn jumps
                foreach (ChessPosition targetpos in (board.PieceLocations(mypawn).Shift(mypawnnorth) & ~board.PieceLocationsAll).ToPositions())
                {
                    ChessPosition piecepos = targetpos.PositionInDirectionUnsafe(mypawnnorth.Opposite());
                    if (targetpos.GetRank() == myrank8)
                    {
                        yield return ChessMoveInfo.Create(piecepos, targetpos, myqueen);
                        yield return ChessMoveInfo.Create(piecepos, targetpos, myrook);
                        yield return ChessMoveInfo.Create(piecepos, targetpos, mybishop);
                        yield return ChessMoveInfo.Create(piecepos, targetpos, myknight);
                    }
                    else
                    {
                        yield return ChessMoveInfo.Create(piecepos, targetpos);
                        if (piecepos.GetRank() == myrank2)
                        {
                            var doubleJumpPos = targetpos.PositionInDirectionUnsafe(mypawnnorth);
                            if (board.PieceAt(doubleJumpPos) == ChessPiece.EMPTY)
                            {
                                yield return ChessMoveInfo.Create(piecepos, doubleJumpPos);
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
                        yield return ChessMoveInfo.Create(ChessPosition.E1, ChessPosition.G1);
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
                        yield return ChessMoveInfo.Create(ChessPosition.E1, ChessPosition.C1);
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
                        yield return ChessMoveInfo.Create(ChessPosition.E8, ChessPosition.G8);
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
                        yield return ChessMoveInfo.Create(ChessPosition.E8, ChessPosition.C8);
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
					if (!CapsOnly) { retval.Add(ChessMoveInfo.Create(from, to)); }
				}
                else if (targetpiece.PieceToPlayer() == forwho)
				{
					break;
				}
				else
				{
					retval.Add(ChessMoveInfo.Create(from, to));
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

            private int GetScore(ChessMove x)
            {
                if (_dic.ContainsKey(x))
                {
                    return _dic[x];
                }
                else
                {
                    int retval;
                    if (UseSEE)
                    {
                        retval = CompEstScoreSEE(x, board);
                    }
                    else
                    {
                        retval = CompEstScore(x, board);
                    }
                    _dic.Add(x, retval);
                    return retval;
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
                if (board.PieceAt(x.To()) != ChessPiece.EMPTY && board.PieceAt(y.To()) == ChessPiece.EMPTY)
				{
					return -1;
				}
                if (board.PieceAt(y.To()) != ChessPiece.EMPTY && board.PieceAt(x.To()) == ChessPiece.EMPTY)
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

				ChessPiece mover = board.PieceAt(move.From());
                ChessPiece taken = board.PieceAt(move.To());
				ChessPlayer me = board.WhosTurn;

                retval -= eval._pcsqPiecePosStage[(int)mover, (int)move.From(), (int)ChessGameStage.Opening];
                retval += eval._pcsqPiecePosStage[(int)mover, (int)move.To(), (int)ChessGameStage.Opening];

				if (taken != ChessPiece.EMPTY)
				{
					retval -= eval._matPieceStage[(int)taken,(int)ChessGameStage.Opening];
				}

				if (me == ChessPlayer.Black) { retval = -retval; }
				return retval;
			}

			public static int CompEstScoreSEE(ChessMove move, ChessBoard board)
			{

				int retval = 0;


                ChessPiece mover = board.PieceAt(move.From());
                ChessPiece taken = board.PieceAt(move.To());
                ChessPlayer me = mover.PieceToPlayer();

				int pieceSqVal = 0;
                pieceSqVal -= eval._pcsqPiecePosStage[(int)mover, (int)move.From(), (int)ChessGameStage.Opening];
                pieceSqVal += eval._pcsqPiecePosStage[(int)mover, (int)move.To(), (int)ChessGameStage.Opening];
				if (me == ChessPlayer.Black) { pieceSqVal = -pieceSqVal; }
				retval += pieceSqVal;

				if (taken != ChessPiece.EMPTY)
				{
                    retval += taken.PieceValBasic();
					//do see
                    var attacks = board.AttacksTo(move.To());
                    attacks &= ~move.From().Bitboard();
                    retval -= attackswap(board, attacks, me.PlayerOther(), move.To(), mover.PieceValBasic());


				}

				return retval;



			}
			static int attackswap(ChessBoard board, ChessBitboard attacks, ChessPlayer player, ChessPosition positionattacked, int pieceontargetval)
			{
				int nextAttackPieceVal = 0;
				ChessPosition nextAttackPos = 0;

				bool HasAttack = attackpop(board, attacks, player, positionattacked, out nextAttackPos, out nextAttackPieceVal);
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

			static bool attackpop(ChessBoard board, ChessBitboard attacks, ChessPlayer player, ChessPosition positionattacked, out ChessPosition OutFrom, out int OutPieceVal)
			{


				ChessPosition mypawn = 0;
				ChessPosition myknight = 0;
				ChessPosition mybishop = 0;
				ChessPosition myrook = 0;
				ChessPosition myqueen = 0;
				ChessPosition myking = 0;


				if (player == ChessPlayer.White)
				{
					foreach (ChessPosition attackPos in attacks.ToPositions())
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
					foreach (ChessPosition attackPos in attacks.ToPositions())
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

				OutFrom = (ChessPosition)(int)-1;
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


}
