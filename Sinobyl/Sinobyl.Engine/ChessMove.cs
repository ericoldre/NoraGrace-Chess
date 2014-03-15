using System;

using System.Collections.Generic;
using System.Text;
using System.Linq;
namespace Sinobyl.Engine
{
    [Flags]
    public enum ChessMove
    {
        NULL_MOVE = 0,
        NOT_NULL_MOVE = 1,

        ToA1 = ChessPosition.A1 << ChessMoveInfo.SHIFT_TO,
        ToB1 = ChessPosition.B1 << ChessMoveInfo.SHIFT_TO,
        ToC1 = ChessPosition.C1 << ChessMoveInfo.SHIFT_TO,
        ToD1 = ChessPosition.D1 << ChessMoveInfo.SHIFT_TO,
        ToE1 = ChessPosition.E1 << ChessMoveInfo.SHIFT_TO,
        ToF1 = ChessPosition.F1 << ChessMoveInfo.SHIFT_TO,
        ToG1 = ChessPosition.G1 << ChessMoveInfo.SHIFT_TO,
        ToH1 = ChessPosition.H1 << ChessMoveInfo.SHIFT_TO,

        ToA2 = ChessPosition.A2 << ChessMoveInfo.SHIFT_TO,
        ToB2 = ChessPosition.B2 << ChessMoveInfo.SHIFT_TO,
        ToC2 = ChessPosition.C2 << ChessMoveInfo.SHIFT_TO,
        ToD2 = ChessPosition.D2 << ChessMoveInfo.SHIFT_TO,
        ToE2 = ChessPosition.E2 << ChessMoveInfo.SHIFT_TO,
        ToF2 = ChessPosition.F2 << ChessMoveInfo.SHIFT_TO,
        ToG2 = ChessPosition.G2 << ChessMoveInfo.SHIFT_TO,
        ToH2 = ChessPosition.H2 << ChessMoveInfo.SHIFT_TO,

        ToA3 = ChessPosition.A3 << ChessMoveInfo.SHIFT_TO,
        ToB3 = ChessPosition.B3 << ChessMoveInfo.SHIFT_TO,
        ToC3 = ChessPosition.C3 << ChessMoveInfo.SHIFT_TO,
        ToD3 = ChessPosition.D3 << ChessMoveInfo.SHIFT_TO,
        ToE3 = ChessPosition.E3 << ChessMoveInfo.SHIFT_TO,
        ToF3 = ChessPosition.F3 << ChessMoveInfo.SHIFT_TO,
        ToG3 = ChessPosition.G3 << ChessMoveInfo.SHIFT_TO,
        ToH3 = ChessPosition.H3 << ChessMoveInfo.SHIFT_TO,

        ToA4 = ChessPosition.A4 << ChessMoveInfo.SHIFT_TO,
        ToB4 = ChessPosition.B4 << ChessMoveInfo.SHIFT_TO,
        ToC4 = ChessPosition.C4 << ChessMoveInfo.SHIFT_TO,
        ToD4 = ChessPosition.D4 << ChessMoveInfo.SHIFT_TO,
        ToE4 = ChessPosition.E4 << ChessMoveInfo.SHIFT_TO,
        ToF4 = ChessPosition.F4 << ChessMoveInfo.SHIFT_TO,
        ToG4 = ChessPosition.G4 << ChessMoveInfo.SHIFT_TO,
        ToH4 = ChessPosition.H4 << ChessMoveInfo.SHIFT_TO,

        ToA5 = ChessPosition.A5 << ChessMoveInfo.SHIFT_TO,
        ToB5 = ChessPosition.B5 << ChessMoveInfo.SHIFT_TO,
        ToC5 = ChessPosition.C5 << ChessMoveInfo.SHIFT_TO,
        ToD5 = ChessPosition.D5 << ChessMoveInfo.SHIFT_TO,
        ToE5 = ChessPosition.E5 << ChessMoveInfo.SHIFT_TO,
        ToF5 = ChessPosition.F5 << ChessMoveInfo.SHIFT_TO,
        ToG5 = ChessPosition.G5 << ChessMoveInfo.SHIFT_TO,
        ToH5 = ChessPosition.H5 << ChessMoveInfo.SHIFT_TO,

        ToA6 = ChessPosition.A6 << ChessMoveInfo.SHIFT_TO,
        ToB6 = ChessPosition.B6 << ChessMoveInfo.SHIFT_TO,
        ToC6 = ChessPosition.C6 << ChessMoveInfo.SHIFT_TO,
        ToD6 = ChessPosition.D6 << ChessMoveInfo.SHIFT_TO,
        ToE6 = ChessPosition.E6 << ChessMoveInfo.SHIFT_TO,
        ToF6 = ChessPosition.F6 << ChessMoveInfo.SHIFT_TO,
        ToG6 = ChessPosition.G6 << ChessMoveInfo.SHIFT_TO,
        ToH6 = ChessPosition.H6 << ChessMoveInfo.SHIFT_TO,

        ToA7 = ChessPosition.A7 << ChessMoveInfo.SHIFT_TO,
        ToB7 = ChessPosition.B7 << ChessMoveInfo.SHIFT_TO,
        ToC7 = ChessPosition.C7 << ChessMoveInfo.SHIFT_TO,
        ToD7 = ChessPosition.D7 << ChessMoveInfo.SHIFT_TO,
        ToE7 = ChessPosition.E7 << ChessMoveInfo.SHIFT_TO,
        ToF7 = ChessPosition.F7 << ChessMoveInfo.SHIFT_TO,
        ToG7 = ChessPosition.G7 << ChessMoveInfo.SHIFT_TO,
        ToH7 = ChessPosition.H7 << ChessMoveInfo.SHIFT_TO,

        ToA8 = ChessPosition.A8 << ChessMoveInfo.SHIFT_TO,
        ToB8 = ChessPosition.B8 << ChessMoveInfo.SHIFT_TO,
        ToC8 = ChessPosition.C8 << ChessMoveInfo.SHIFT_TO,
        ToD8 = ChessPosition.D8 << ChessMoveInfo.SHIFT_TO,
        ToE8 = ChessPosition.E8 << ChessMoveInfo.SHIFT_TO,
        ToF8 = ChessPosition.F8 << ChessMoveInfo.SHIFT_TO,
        ToG8 = ChessPosition.G8 << ChessMoveInfo.SHIFT_TO,
        ToH8 = ChessPosition.H8 << ChessMoveInfo.SHIFT_TO,


        //from

        FromA1 = ChessPosition.A1 << ChessMoveInfo.SHIFT_FROM,
        FromB1 = ChessPosition.B1 << ChessMoveInfo.SHIFT_FROM,
        FromC1 = ChessPosition.C1 << ChessMoveInfo.SHIFT_FROM,
        FromD1 = ChessPosition.D1 << ChessMoveInfo.SHIFT_FROM,
        FromE1 = ChessPosition.E1 << ChessMoveInfo.SHIFT_FROM,
        FromF1 = ChessPosition.F1 << ChessMoveInfo.SHIFT_FROM,
        FromG1 = ChessPosition.G1 << ChessMoveInfo.SHIFT_FROM,
        FromH1 = ChessPosition.H1 << ChessMoveInfo.SHIFT_FROM,

        FromA2 = ChessPosition.A2 << ChessMoveInfo.SHIFT_FROM,
        FromB2 = ChessPosition.B2 << ChessMoveInfo.SHIFT_FROM,
        FromC2 = ChessPosition.C2 << ChessMoveInfo.SHIFT_FROM,
        FromD2 = ChessPosition.D2 << ChessMoveInfo.SHIFT_FROM,
        FromE2 = ChessPosition.E2 << ChessMoveInfo.SHIFT_FROM,
        FromF2 = ChessPosition.F2 << ChessMoveInfo.SHIFT_FROM,
        FromG2 = ChessPosition.G2 << ChessMoveInfo.SHIFT_FROM,
        FromH2 = ChessPosition.H2 << ChessMoveInfo.SHIFT_FROM,

        FromA3 = ChessPosition.A3 << ChessMoveInfo.SHIFT_FROM,
        FromB3 = ChessPosition.B3 << ChessMoveInfo.SHIFT_FROM,
        FromC3 = ChessPosition.C3 << ChessMoveInfo.SHIFT_FROM,
        FromD3 = ChessPosition.D3 << ChessMoveInfo.SHIFT_FROM,
        FromE3 = ChessPosition.E3 << ChessMoveInfo.SHIFT_FROM,
        FromF3 = ChessPosition.F3 << ChessMoveInfo.SHIFT_FROM,
        FromG3 = ChessPosition.G3 << ChessMoveInfo.SHIFT_FROM,
        FromH3 = ChessPosition.H3 << ChessMoveInfo.SHIFT_FROM,

        FromA4 = ChessPosition.A4 << ChessMoveInfo.SHIFT_FROM,
        FromB4 = ChessPosition.B4 << ChessMoveInfo.SHIFT_FROM,
        FromC4 = ChessPosition.C4 << ChessMoveInfo.SHIFT_FROM,
        FromD4 = ChessPosition.D4 << ChessMoveInfo.SHIFT_FROM,
        FromE4 = ChessPosition.E4 << ChessMoveInfo.SHIFT_FROM,
        FromF4 = ChessPosition.F4 << ChessMoveInfo.SHIFT_FROM,
        FromG4 = ChessPosition.G4 << ChessMoveInfo.SHIFT_FROM,
        FromH4 = ChessPosition.H4 << ChessMoveInfo.SHIFT_FROM,

        FromA5 = ChessPosition.A5 << ChessMoveInfo.SHIFT_FROM,
        FromB5 = ChessPosition.B5 << ChessMoveInfo.SHIFT_FROM,
        FromC5 = ChessPosition.C5 << ChessMoveInfo.SHIFT_FROM,
        FromD5 = ChessPosition.D5 << ChessMoveInfo.SHIFT_FROM,
        FromE5 = ChessPosition.E5 << ChessMoveInfo.SHIFT_FROM,
        FromF5 = ChessPosition.F5 << ChessMoveInfo.SHIFT_FROM,
        FromG5 = ChessPosition.G5 << ChessMoveInfo.SHIFT_FROM,
        FromH5 = ChessPosition.H5 << ChessMoveInfo.SHIFT_FROM,

        FromA6 = ChessPosition.A6 << ChessMoveInfo.SHIFT_FROM,
        FromB6 = ChessPosition.B6 << ChessMoveInfo.SHIFT_FROM,
        FromC6 = ChessPosition.C6 << ChessMoveInfo.SHIFT_FROM,
        FromD6 = ChessPosition.D6 << ChessMoveInfo.SHIFT_FROM,
        FromE6 = ChessPosition.E6 << ChessMoveInfo.SHIFT_FROM,
        FromF6 = ChessPosition.F6 << ChessMoveInfo.SHIFT_FROM,
        FromG6 = ChessPosition.G6 << ChessMoveInfo.SHIFT_FROM,
        FromH6 = ChessPosition.H6 << ChessMoveInfo.SHIFT_FROM,

        FromA7 = ChessPosition.A7 << ChessMoveInfo.SHIFT_FROM,
        FromB7 = ChessPosition.B7 << ChessMoveInfo.SHIFT_FROM,
        FromC7 = ChessPosition.C7 << ChessMoveInfo.SHIFT_FROM,
        FromD7 = ChessPosition.D7 << ChessMoveInfo.SHIFT_FROM,
        FromE7 = ChessPosition.E7 << ChessMoveInfo.SHIFT_FROM,
        FromF7 = ChessPosition.F7 << ChessMoveInfo.SHIFT_FROM,
        FromG7 = ChessPosition.G7 << ChessMoveInfo.SHIFT_FROM,
        FromH7 = ChessPosition.H7 << ChessMoveInfo.SHIFT_FROM,

        FromA8 = ChessPosition.A8 << ChessMoveInfo.SHIFT_FROM,
        FromB8 = ChessPosition.B8 << ChessMoveInfo.SHIFT_FROM,
        FromC8 = ChessPosition.C8 << ChessMoveInfo.SHIFT_FROM,
        FromD8 = ChessPosition.D8 << ChessMoveInfo.SHIFT_FROM,
        FromE8 = ChessPosition.E8 << ChessMoveInfo.SHIFT_FROM,
        FromF8 = ChessPosition.F8 << ChessMoveInfo.SHIFT_FROM,
        FromG8 = ChessPosition.G8 << ChessMoveInfo.SHIFT_FROM,
        FromH8 = ChessPosition.H8 << ChessMoveInfo.SHIFT_FROM,

        PromWPawn = ChessPiece.WPawn << ChessMoveInfo.SHIFT_PROM,
        PromWBishop = ChessPiece.WBishop << ChessMoveInfo.SHIFT_PROM,
        PromWKnight = ChessPiece.WKnight << ChessMoveInfo.SHIFT_PROM,
        PromWRook = ChessPiece.WRook << ChessMoveInfo.SHIFT_PROM,
        PromWQueen = ChessPiece.WQueen << ChessMoveInfo.SHIFT_PROM,
        PromWKing = ChessPiece.WKing << ChessMoveInfo.SHIFT_PROM,

        PromBPawn = ChessPiece.BPawn << ChessMoveInfo.SHIFT_PROM,
        PromBBishop = ChessPiece.BBishop << ChessMoveInfo.SHIFT_PROM,
        PromBKnight = ChessPiece.BKnight << ChessMoveInfo.SHIFT_PROM,
        PromBRook = ChessPiece.BRook << ChessMoveInfo.SHIFT_PROM,
        PromBQueen = ChessPiece.BQueen << ChessMoveInfo.SHIFT_PROM,
        PromBKing = ChessPiece.BKing << ChessMoveInfo.SHIFT_PROM,

    }



	public class ChessMoveOld
	{
		public readonly ChessPosition From;
		public readonly ChessPosition To;
		public readonly ChessPiece Promote;
		public int? EstScore;

        //public override bool Equals(object obj)
        //{
        //    ChessMove other = (ChessMove)obj;
        //    if (other.From==this.From && other.To==this.To && other.Promote==this.Promote)
        //    {
        //        return true;
        //    }
        //    return false;
        //}
        //public override int GetHashCode()
        //{
        //    //return (To.GetHashCode() ^ From.GetHashCode() >> 1) + this.Promote.GetHashCode();
        //    return (int)To | ((int)From << 8) | ((int)Promote << 16);
        //}

		public ChessMoveOld()
		{
			this.Promote = ChessPiece.EMPTY;
			this.From = (ChessPosition.OUTOFBOUNDS);
			this.To = (ChessPosition.OUTOFBOUNDS);
			this.EstScore = null;
		}
		public ChessMoveOld(ChessPosition from, ChessPosition to)
		{
			this.Promote = ChessPiece.EMPTY;
			this.From = from;
			this.To = to;
			this.EstScore = null;
		}
        public ChessMoveOld(ChessPosition from, ChessPosition to, ChessPiece promote)
		{
			this.From = from;
			this.To = to;
			this.Promote = promote;
			this.EstScore = null;
		}

        public override string ToString()
        {
            return base.ToString();
        }


	}

    public static partial class ChessMoveInfo
    {
        public const int SHIFT_TO = 1;
        public const int SHIFT_FROM = 7;
        public const int SHIFT_PROM = 13;

        public static ChessMove Create(ChessPosition from, ChessPosition to)
        {
            return Create(from, to, ChessPiece.EMPTY);

        }

        public static ChessMove Create(ChessPosition from, ChessPosition to, ChessPiece promote)
        {
            int mask = 0x3F;

            
            if (((int)from & mask) != (int)from) { throw new ArgumentOutOfRangeException("from"); }
            if (((int)to & mask) != (int)to) { throw new ArgumentOutOfRangeException("to"); }
            if (((int)promote & mask) != (int)promote) { throw new ArgumentOutOfRangeException("promote"); }

            int ito = (int)to << 1;
            int ifrom = (int)from << 7;
            int iprom = (int)promote << 13;

            return (ChessMove)(ito | ifrom | iprom | 1);
            //return ChessMoveInfo.Create(from, to, promote);
        }

        public static ChessPosition To(this ChessMove move)
        {
            int mask = ((int)move >> 1) & 0x3F;
            return (ChessPosition)mask;
        }

        public static ChessPosition From(this ChessMove move)
        {
            var shift = (int)move >> 7;
            int mask = shift & 0x3F;
            return (ChessPosition)mask;
        }

        public static ChessPiece Promote(this ChessMove move)
        {
            var shift = (int)move >> 13;
            int mask = shift & 0x3F;
            return (ChessPiece)mask;
        }

        public static int? EstScore(this ChessMove move)
        {
            return 0;
        }


		public static bool IsSameAs(this ChessMove move, ChessMove otherMove)
		{
            return move.From() == otherMove.From() && move.To() == otherMove.To() && move.Promote() == otherMove.Promote();
		}

		public static bool IsLegal(this ChessMove move, ChessBoard board)
		{
            ChessMoves legalmoves = new ChessMoves(ChessMoveInfo.GenMovesLegal(board));
			foreach (ChessMove legalmove in legalmoves)
			{
				if (legalmove.IsSameAs(move)) { return true; }
			}
			return false;
		}

		public static string Write(this ChessMove move, ChessBoard board)
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
                if (board.PieceAt(x.To()) != ChessPiece.EMPTY && board.PieceAt(y.To()) == ChessPiece.EMPTY)
				{
					return -1;
				}
                if (board.PieceAt(y.To()) != ChessPiece.EMPTY && board.PieceAt(x.To()) == ChessPiece.EMPTY)
				{
					return 1;
				}

                if (!x.EstScore().HasValue)
				{
					if (UseSEE)
					{
						//x.EstScore = CompEstScoreSEE(x, board);
					}
					else
					{
						//x.EstScore = CompEstScore(x, board);
					}
				}
                if (!y.EstScore().HasValue)
				{
					if (UseSEE)
					{
						//y.EstScore = CompEstScoreSEE(y, board);
					}
					else
					{
						//y.EstScore = CompEstScore(y, board);
					}
				}
                if (x.EstScore() > y.EstScore()) { return -1; }
                if (x.EstScore() < y.EstScore()) { return 1; }
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
