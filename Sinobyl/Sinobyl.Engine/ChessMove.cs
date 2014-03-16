using System;

using System.Collections.Generic;
using System.Text;
using System.Linq;
namespace Sinobyl.Engine
{


    public static partial class ChessMoveInfo
    {
        public const int SHIFT_TO = 1;
        public const int SHIFT_FROM = 7;
        public const int SHIFT_PROM = 13;
        public const int MASK_SIXBITS = 0x3F;
        public const int SHIFT_TOTAL = 19;
        public const int TOTAL_MASK = 0x7FFFF;

        public static ChessMove Create(ChessPosition from, ChessPosition to)
        {
            return Create(from, to, ChessPiece.EMPTY);

        }

        public static ChessMove Create(ChessPosition from, ChessPosition to, ChessPiece promote)
        {

            if (((int)from & MASK_SIXBITS) != (int)from) { throw new ArgumentOutOfRangeException("from"); }
            if (((int)to & MASK_SIXBITS) != (int)to) { throw new ArgumentOutOfRangeException("to"); }
            if (((int)promote & MASK_SIXBITS) != (int)promote) { throw new ArgumentOutOfRangeException("promote"); }

            int ito = (int)to << SHIFT_TO;
            int ifrom = (int)from << SHIFT_FROM;
            int iprom = (int)promote << SHIFT_PROM;

            return (ChessMove)(ito | ifrom | iprom) | ChessMove.NOT_NULL_MOVE;
            //return ChessMoveInfo.Create(from, to, promote);
        }

        public static ChessPosition To(this ChessMove move)
        {
            int mask = ((int)move >> SHIFT_TO) & MASK_SIXBITS;
            return (ChessPosition)mask;
        }

        public static ChessPosition From(this ChessMove move)
        {
            var shift = (int)move >> SHIFT_FROM;
            int mask = shift & MASK_SIXBITS;
            return (ChessPosition)mask;
        }

        public static ChessPiece Promote(this ChessMove move)
        {
            var shift = (int)move >> SHIFT_PROM;
            int mask = shift & MASK_SIXBITS;
            return (ChessPiece)mask;
        }

        public static int? EstScore(this ChessMove move)
        {
            return 0;
        }


		public static bool IsSameAs(this ChessMove move, ChessMove otherMove)
		{
            return ((int)move & TOTAL_MASK) == ((int)otherMove & TOTAL_MASK);
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



    }

    [Flags]
    public enum ChessMove
    {
        NULL_MOVE = 0,
        NOT_NULL_MOVE = 1,
        //TO_FLAG = 1 << 19,
        //FROM_MARKER = 1 << 20,
        //PROM_MARKER = 1 << 21,

        //ToA1 = (ChessPosition.A1 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToB1 = (ChessPosition.B1 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToC1 = (ChessPosition.C1 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToD1 = (ChessPosition.D1 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToE1 = (ChessPosition.E1 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToF1 = (ChessPosition.F1 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToG1 = (ChessPosition.G1 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToH1 = (ChessPosition.H1 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,

        //ToA2 = (ChessPosition.A2 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToB2 = (ChessPosition.B2 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToC2 = (ChessPosition.C2 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToD2 = (ChessPosition.D2 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToE2 = (ChessPosition.E2 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToF2 = (ChessPosition.F2 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToG2 = (ChessPosition.G2 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToH2 = (ChessPosition.H2 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,

        //ToA3 = (ChessPosition.A3 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToB3 = (ChessPosition.B3 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToC3 = (ChessPosition.C3 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToD3 = (ChessPosition.D3 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToE3 = (ChessPosition.E3 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToF3 = (ChessPosition.F3 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToG3 = (ChessPosition.G3 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToH3 = (ChessPosition.H3 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,

        //ToA4 = (ChessPosition.A4 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToB4 = (ChessPosition.B4 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToC4 = (ChessPosition.C4 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToD4 = (ChessPosition.D4 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToE4 = (ChessPosition.E4 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToF4 = (ChessPosition.F4 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToG4 = (ChessPosition.G4 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToH4 = (ChessPosition.H4 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,

        //ToA5 = (ChessPosition.A5 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToB5 = (ChessPosition.B5 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToC5 = (ChessPosition.C5 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToD5 = (ChessPosition.D5 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToE5 = (ChessPosition.E5 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToF5 = (ChessPosition.F5 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToG5 = (ChessPosition.G5 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToH5 = (ChessPosition.H5 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,

        //ToA6 = (ChessPosition.A6 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToB6 = (ChessPosition.B6 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToC6 = (ChessPosition.C6 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToD6 = (ChessPosition.D6 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToE6 = (ChessPosition.E6 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToF6 = (ChessPosition.F6 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToG6 = (ChessPosition.G6 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToH6 = (ChessPosition.H6 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,

        //ToA7 = (ChessPosition.A7 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToB7 = (ChessPosition.B7 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToC7 = (ChessPosition.C7 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToD7 = (ChessPosition.D7 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToE7 = (ChessPosition.E7 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToF7 = (ChessPosition.F7 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToG7 = (ChessPosition.G7 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToH7 = (ChessPosition.H7 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,

        //ToA8 = (ChessPosition.A8 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToB8 = (ChessPosition.B8 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToC8 = (ChessPosition.C8 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToD8 = (ChessPosition.D8 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToE8 = (ChessPosition.E8 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToF8 = (ChessPosition.F8 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToG8 = (ChessPosition.G8 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,
        //ToH8 = (ChessPosition.H8 << ChessMoveInfo.SHIFT_TO) | TO_FLAG,


        ////from

        //FromA1 = ChessPosition.A1 << ChessMoveInfo.SHIFT_FROM,
        //FromB1 = ChessPosition.B1 << ChessMoveInfo.SHIFT_FROM,
        //FromC1 = ChessPosition.C1 << ChessMoveInfo.SHIFT_FROM,
        //FromD1 = ChessPosition.D1 << ChessMoveInfo.SHIFT_FROM,
        //FromE1 = ChessPosition.E1 << ChessMoveInfo.SHIFT_FROM,
        //FromF1 = ChessPosition.F1 << ChessMoveInfo.SHIFT_FROM,
        //FromG1 = ChessPosition.G1 << ChessMoveInfo.SHIFT_FROM,
        //FromH1 = ChessPosition.H1 << ChessMoveInfo.SHIFT_FROM,

        //FromA2 = ChessPosition.A2 << ChessMoveInfo.SHIFT_FROM,
        //FromB2 = ChessPosition.B2 << ChessMoveInfo.SHIFT_FROM,
        //FromC2 = ChessPosition.C2 << ChessMoveInfo.SHIFT_FROM,
        //FromD2 = ChessPosition.D2 << ChessMoveInfo.SHIFT_FROM,
        //FromE2 = ChessPosition.E2 << ChessMoveInfo.SHIFT_FROM,
        //FromF2 = ChessPosition.F2 << ChessMoveInfo.SHIFT_FROM,
        //FromG2 = ChessPosition.G2 << ChessMoveInfo.SHIFT_FROM,
        //FromH2 = ChessPosition.H2 << ChessMoveInfo.SHIFT_FROM,

        //FromA3 = ChessPosition.A3 << ChessMoveInfo.SHIFT_FROM,
        //FromB3 = ChessPosition.B3 << ChessMoveInfo.SHIFT_FROM,
        //FromC3 = ChessPosition.C3 << ChessMoveInfo.SHIFT_FROM,
        //FromD3 = ChessPosition.D3 << ChessMoveInfo.SHIFT_FROM,
        //FromE3 = ChessPosition.E3 << ChessMoveInfo.SHIFT_FROM,
        //FromF3 = ChessPosition.F3 << ChessMoveInfo.SHIFT_FROM,
        //FromG3 = ChessPosition.G3 << ChessMoveInfo.SHIFT_FROM,
        //FromH3 = ChessPosition.H3 << ChessMoveInfo.SHIFT_FROM,

        //FromA4 = ChessPosition.A4 << ChessMoveInfo.SHIFT_FROM,
        //FromB4 = ChessPosition.B4 << ChessMoveInfo.SHIFT_FROM,
        //FromC4 = ChessPosition.C4 << ChessMoveInfo.SHIFT_FROM,
        //FromD4 = ChessPosition.D4 << ChessMoveInfo.SHIFT_FROM,
        //FromE4 = ChessPosition.E4 << ChessMoveInfo.SHIFT_FROM,
        //FromF4 = ChessPosition.F4 << ChessMoveInfo.SHIFT_FROM,
        //FromG4 = ChessPosition.G4 << ChessMoveInfo.SHIFT_FROM,
        //FromH4 = ChessPosition.H4 << ChessMoveInfo.SHIFT_FROM,

        //FromA5 = ChessPosition.A5 << ChessMoveInfo.SHIFT_FROM,
        //FromB5 = ChessPosition.B5 << ChessMoveInfo.SHIFT_FROM,
        //FromC5 = ChessPosition.C5 << ChessMoveInfo.SHIFT_FROM,
        //FromD5 = ChessPosition.D5 << ChessMoveInfo.SHIFT_FROM,
        //FromE5 = ChessPosition.E5 << ChessMoveInfo.SHIFT_FROM,
        //FromF5 = ChessPosition.F5 << ChessMoveInfo.SHIFT_FROM,
        //FromG5 = ChessPosition.G5 << ChessMoveInfo.SHIFT_FROM,
        //FromH5 = ChessPosition.H5 << ChessMoveInfo.SHIFT_FROM,

        //FromA6 = ChessPosition.A6 << ChessMoveInfo.SHIFT_FROM,
        //FromB6 = ChessPosition.B6 << ChessMoveInfo.SHIFT_FROM,
        //FromC6 = ChessPosition.C6 << ChessMoveInfo.SHIFT_FROM,
        //FromD6 = ChessPosition.D6 << ChessMoveInfo.SHIFT_FROM,
        //FromE6 = ChessPosition.E6 << ChessMoveInfo.SHIFT_FROM,
        //FromF6 = ChessPosition.F6 << ChessMoveInfo.SHIFT_FROM,
        //FromG6 = ChessPosition.G6 << ChessMoveInfo.SHIFT_FROM,
        //FromH6 = ChessPosition.H6 << ChessMoveInfo.SHIFT_FROM,

        //FromA7 = ChessPosition.A7 << ChessMoveInfo.SHIFT_FROM,
        //FromB7 = ChessPosition.B7 << ChessMoveInfo.SHIFT_FROM,
        //FromC7 = ChessPosition.C7 << ChessMoveInfo.SHIFT_FROM,
        //FromD7 = ChessPosition.D7 << ChessMoveInfo.SHIFT_FROM,
        //FromE7 = ChessPosition.E7 << ChessMoveInfo.SHIFT_FROM,
        //FromF7 = ChessPosition.F7 << ChessMoveInfo.SHIFT_FROM,
        //FromG7 = ChessPosition.G7 << ChessMoveInfo.SHIFT_FROM,
        //FromH7 = ChessPosition.H7 << ChessMoveInfo.SHIFT_FROM,

        //FromA8 = ChessPosition.A8 << ChessMoveInfo.SHIFT_FROM,
        //FromB8 = ChessPosition.B8 << ChessMoveInfo.SHIFT_FROM,
        //FromC8 = ChessPosition.C8 << ChessMoveInfo.SHIFT_FROM,
        //FromD8 = ChessPosition.D8 << ChessMoveInfo.SHIFT_FROM,
        //FromE8 = ChessPosition.E8 << ChessMoveInfo.SHIFT_FROM,
        //FromF8 = ChessPosition.F8 << ChessMoveInfo.SHIFT_FROM,
        //FromG8 = ChessPosition.G8 << ChessMoveInfo.SHIFT_FROM,
        //FromH8 = ChessPosition.H8 << ChessMoveInfo.SHIFT_FROM,

        //PromWPawn = ChessPiece.WPawn << ChessMoveInfo.SHIFT_PROM,
        //PromWBishop = ChessPiece.WBishop << ChessMoveInfo.SHIFT_PROM,
        //PromWKnight = ChessPiece.WKnight << ChessMoveInfo.SHIFT_PROM,
        //PromWRook = ChessPiece.WRook << ChessMoveInfo.SHIFT_PROM,
        //PromWQueen = ChessPiece.WQueen << ChessMoveInfo.SHIFT_PROM,
        //PromWKing = ChessPiece.WKing << ChessMoveInfo.SHIFT_PROM,

        //PromBPawn = ChessPiece.BPawn << ChessMoveInfo.SHIFT_PROM,
        //PromBBishop = ChessPiece.BBishop << ChessMoveInfo.SHIFT_PROM,
        //PromBKnight = ChessPiece.BKnight << ChessMoveInfo.SHIFT_PROM,
        //PromBRook = ChessPiece.BRook << ChessMoveInfo.SHIFT_PROM,
        //PromBQueen = ChessPiece.BQueen << ChessMoveInfo.SHIFT_PROM,
        //PromBKing = ChessPiece.BKing << ChessMoveInfo.SHIFT_PROM,

    }
}
