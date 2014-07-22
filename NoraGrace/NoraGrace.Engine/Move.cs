using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
namespace NoraGrace.Engine
{
	/// <summary>
	/// Summary description for ChessMove.
	/// </summary>
	/// 
	public enum NotationType
	{
		Coord,
		San,
		Detailed
	}

    [System.Diagnostics.DebuggerDisplay(@"{NoraGrace.Engine.MoveInfo.Description(this),nq}")]
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

        


        public static bool IsLegal(this Move move, Board board)
        {
            List<Move> legalmoves = new List<Move>(MoveInfo.GenMovesLegal(board));
            foreach (Move legalmove in legalmoves)
            {
                if (legalmove == move) { return true; }
            }
            return false;
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
                    else if (me == Player.White
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
                    //check moves to last rank have promotion if pawn.
                    if (me == Player.White && to.ToRank() == Rank.Rank8 && move.Promote() == Piece.EMPTY)
                    {
                        return false;
                    }
                    else if (me == Player.Black && to.ToRank() == Rank.Rank1 && move.Promote() == Piece.EMPTY)
                    {
                        return false;
                    }

                    if (Attacks.PawnAttacks(from, me).Contains(to))
                    {
                        return board[me.PlayerOther()].Contains(to) || to == board.EnPassant;
                    }
                    else if (from.PositionInDirection(me.MyNorth()) == to
                        && board.PieceAt(to) == Piece.EMPTY)
                    {
                        return true;
                    }
                    else if (from.ToRank() == me.MyRank2()   //rank 2
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

        public static bool MakesThreat(this Move move, Board board)
        {
            Position from = move.From();
            Position to = move.To();
            Player player = board.WhosTurn;
            Player opponent = player.PlayerOther();
            Bitboard them = board[opponent];
            PieceType pieceType = board.PieceAt(from).ToPieceType();

            switch (pieceType)
            {
                case PieceType.Pawn:
                    return (Attacks.PawnAttacks(to, player) & them & ~board[PieceType.Pawn]) != Bitboard.Empty;
                case PieceType.Knight:
                    return (Attacks.KnightAttacks(to) & them & board.RookSliders) != Bitboard.Empty;
                case PieceType.Bishop:
                    return (Attacks.BishopAttacks(to, board.PieceLocationsAll) & them & board.RookSliders) != Bitboard.Empty;
                case PieceType.Rook:
                    return (Attacks.RookAttacks(to, board.PieceLocationsAll) & them & board[PieceType.Queen]) != Bitboard.Empty;
                default:
                    return false;
            }

        }

        public static bool CausesCheck(this Move move, Board board, ref CheckInfo oppenentCheckInfo)
        {
            Position from = move.From();
            Position to = move.To();
            PieceType pieceType = board.PieceAt(from).ToPieceType();

            if (oppenentCheckInfo.DirectAll.Contains(to))
            {
                bool direct = false;
                PieceType pieceTypeAfter = move.Promote() == Piece.EMPTY ? pieceType : move.Promote().ToPieceType(); //in case of promotion
                switch (pieceTypeAfter)
                {
                    case PieceType.Pawn:
                        direct = oppenentCheckInfo.PawnDirect.Contains(to);
                        break;
                    case PieceType.Knight:
                        direct = oppenentCheckInfo.KnightDirect.Contains(to);
                        break;
                    case PieceType.Bishop:
                        direct = oppenentCheckInfo.BishopDirect.Contains(to);
                        break;
                    case PieceType.Rook:
                        direct = oppenentCheckInfo.RookDirect.Contains(to);
                        break;
                    case PieceType.Queen:
                        direct = oppenentCheckInfo.BishopDirect.Contains(to) || oppenentCheckInfo.RookDirect.Contains(to);
                        break;
                }
                if (direct) { return true; }
            }

            bool isEnpassantCapture = pieceType == PieceType.Pawn && to == board.EnPassant;

            if (oppenentCheckInfo.PinnedOrDiscovered.Contains(from) || isEnpassantCapture)
            {
                Bitboard allAfterMove = (board.PieceLocationsAll & ~from.ToBitboard()) | to.ToBitboard();

                if (isEnpassantCapture)
                {
                    Position enpassantCapturedSq = board.WhosTurn.MyRank(Rank.Rank5).ToPosition(board.EnPassant.ToFile());
                    allAfterMove &= ~enpassantCapturedSq.ToBitboard(); 
                }

                Bitboard stmAll = board[board.WhosTurn];
                Bitboard revealed = (Attacks.BishopAttacks(oppenentCheckInfo.KingPosition, allAfterMove) & stmAll & board.BishopSliders)
                    | (Attacks.RookAttacks(oppenentCheckInfo.KingPosition, allAfterMove) & stmAll & board.RookSliders);
                if (revealed != Bitboard.Empty) { return true; }
            }

            //check if castling reveals check via rook
            if (pieceType == PieceType.King)
            {
                if (from == Position.E1 && to == Position.G1 && oppenentCheckInfo.RookDirect.Contains(Position.F1))
                {
                    return true;
                }
                if (from == Position.E1 && to == Position.C1 && oppenentCheckInfo.RookDirect.Contains(Position.D1))
                {
                    return true;
                }

                if (from == Position.E8 && to == Position.G8 && oppenentCheckInfo.RookDirect.Contains(Position.F8))
                {
                    return true;
                }
                if (from == Position.E8 && to == Position.C8 && oppenentCheckInfo.RookDirect.Contains(Position.D8))
                {
                    return true;
                }
            }

            return false;
        }

	}


}
