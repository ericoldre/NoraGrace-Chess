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

	public static class ChessMoveInfo
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
            ChessMoveBuffer.MoveData[] array = new ChessMoveBuffer.MoveData[210];
			int count = GenMovesArray(array, board, false);
            for (int i = 0; i < count; i++)
            {
                yield return array[i].Move;
            }
		}

        public static int GenMovesArray(ChessMoveBuffer.MoveData[] array, ChessBoard board, bool capsOnly)
        {
            ChessBitboard mypieces = board[board.WhosTurn];
            ChessBitboard hispieces = board[board.WhosTurn.PlayerOther()];
            ChessBitboard kingAttacks = Attacks.KingAttacks(board.KingPosition(board.WhosTurn)) & ~mypieces;
            //return GenMoves(board, board[board.WhosTurn], capsOnly ? board[board.WhosTurn.PlayerOther()] : ~board[board.WhosTurn], !capsOnly);
            if (board.Checkers == ChessBitboard.Empty)
            {
                //not in check, normal logic
                if (capsOnly) { kingAttacks &= hispieces; }
                return GenMoves(array, board, mypieces, capsOnly ? hispieces : ~mypieces, !capsOnly, kingAttacks);
            }
            else
            {
                //return GenMoves(board, mypieces, capsOnly ? hispieces : ~mypieces, !capsOnly, kingAttacks);
                if (board.Checkers.BitCount() > 1)
                {
                    //multiple attackers, king move is only option.
                    return GenMoves(array, board, board.KingPosition(board.WhosTurn).Bitboard(), ~mypieces, false, kingAttacks);
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
                    return GenMoves(array, board, mypieces, attackLocs, false, kingAttacks);
                }
            }
        }

        private static int GenMoves(ChessMoveBuffer.MoveData[] array, ChessBoard board, ChessBitboard possibleMovers, ChessBitboard targetLocations, bool castling, ChessBitboard kingTargets)
        {
            System.Diagnostics.Debug.Assert((possibleMovers & ~board[board.WhosTurn]) == ChessBitboard.Empty); //possible movers must be subset of my pieces
            System.Diagnostics.Debug.Assert((targetLocations & board[board.WhosTurn]) == ChessBitboard.Empty); //targets may not include my pieces.
            System.Diagnostics.Debug.Assert((kingTargets & ~Attacks.KingAttacks(board.KingPosition(board.WhosTurn))) == ChessBitboard.Empty); //king targets is very specific, must filter before calling this

            int retval = 0;

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
                    array[retval++].Move = ChessMoveInfo.Create(piecepos, attackPos);
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
                            array[retval++].Move = ChessMoveInfo.Create(piecepos, targetpos, myqueen);
                            array[retval++].Move = ChessMoveInfo.Create(piecepos, targetpos, myrook);
                            array[retval++].Move = ChessMoveInfo.Create(piecepos, targetpos, mybishop);
                            array[retval++].Move = ChessMoveInfo.Create(piecepos, targetpos, myknight);
                        }
                        else
                        {
                            array[retval++].Move = ChessMoveInfo.Create(piecepos, targetpos);
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
                        array[retval++].Move = ChessMoveInfo.Create(piecepos, targetpos, myqueen);
                        array[retval++].Move = ChessMoveInfo.Create(piecepos, targetpos, myrook);
                        array[retval++].Move = ChessMoveInfo.Create(piecepos, targetpos, mybishop);
                        array[retval++].Move = ChessMoveInfo.Create(piecepos, targetpos, myknight);
                    }
                    else
                    {
                        array[retval++].Move = ChessMoveInfo.Create(piecepos, targetpos);
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
                    array[retval++].Move = ChessMoveInfo.Create(piecepos, targetpos);
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
                        array[retval++].Move = ChessMoveInfo.Create(ChessPosition.E1, ChessPosition.G1);
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
                        array[retval++].Move = ChessMoveInfo.Create(ChessPosition.E1, ChessPosition.C1);
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
                        array[retval++].Move = ChessMoveInfo.Create(ChessPosition.E8, ChessPosition.G8);
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
                        array[retval++].Move = ChessMoveInfo.Create(ChessPosition.E8, ChessPosition.C8);
                    }

                }
            }
            return retval;

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
                    if ((board.CastleRights & CastleFlags.WhiteShort) != 0
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
                        retval.Add(ChessMoveInfo.Create(ChessPosition.E1, ChessPosition.C1));
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
                        retval.Add(ChessMoveInfo.Create(ChessPosition.E8, ChessPosition.G8));
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
                        retval.Add(ChessMoveInfo.Create(ChessPosition.E8, ChessPosition.C8));
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
                    yield return ChessMoveInfo.Create(piecepos, attackPos);
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
                        yield return ChessMoveInfo.Create(ChessPosition.E1, ChessPosition.G1);
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
                        yield return ChessMoveInfo.Create(ChessPosition.E1, ChessPosition.C1);
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
                        yield return ChessMoveInfo.Create(ChessPosition.E8, ChessPosition.G8);
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




	}


}
