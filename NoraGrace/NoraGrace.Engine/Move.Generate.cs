using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoraGrace.Engine
{

    public static partial class MoveInfo
    {
        public static IEnumerable<Move> GenMovesLegal(Board board)
        {
            Board workingboard = new Board(board.FENCurrent);

            Player me = board.WhosTurn;
            foreach (Move move in GenMoves(workingboard))
            {
                workingboard.MoveApply(move);
                bool resultsInCheck = workingboard.IsCheck(me);
                workingboard.MoveUndo();
                if (!resultsInCheck) { yield return move; }
            }

        }

        public static IEnumerable<Move> GenMoves(Board board)
        {

            ChessMoveData[] array = new ChessMoveData[210];
            var caps = GenCapsNonCaps(array, board, true, 0);
            var count = GenCapsNonCaps(array, board, false, caps);
            for (int i = 0; i < count; i++)
            {
                yield return array[i].Move;
            }
        }


        public static int GenCapsNonCaps(ChessMoveData[] array, Board board, bool captures, int arrayIndex)
        {

            //ChessPiece mypawn = board.WhosTurn == ChessPlayer.White ? ChessPiece.WPawn : ChessPiece.BPawn;
            //ChessPiece myknight = board.WhosTurn == ChessPlayer.White ? ChessPiece.WKnight : ChessPiece.BKnight;
            //ChessPiece mybishop = board.WhosTurn == ChessPlayer.White ? ChessPiece.WBishop : ChessPiece.BBishop;
            //ChessPiece myrook = board.WhosTurn == ChessPlayer.White ? ChessPiece.WRook : ChessPiece.BRook;
            //ChessPiece myqueen = board.WhosTurn == ChessPlayer.White ? ChessPiece.WQueen : ChessPiece.BQueen;
            //ChessPiece myking = board.WhosTurn == ChessPlayer.White ? ChessPiece.WKing : ChessPiece.BKing;

            Direction mypawnwest = board.WhosTurn == Player.White ? Direction.DirNW : Direction.DirSW;
            Direction mypawneast = board.WhosTurn == Player.White ? Direction.DirNE : Direction.DirSE;
            Direction mypawnnorth = board.WhosTurn == Player.White ? Direction.DirN : Direction.DirS;
            Direction mypawnsouth = board.WhosTurn == Player.White ? Direction.DirS : Direction.DirN;
            Rank myrank8 = board.WhosTurn == Player.White ? Rank.Rank8 : Rank.Rank1;
            Rank myrank2 = board.WhosTurn == Player.White ? Rank.Rank2 : Rank.Rank7;

            Player me = board.WhosTurn;

            Bitboard attacks = Bitboard.Empty;

            Bitboard targetLocations = captures ? board[me.PlayerOther()] : ~board.PieceLocationsAll;

            //first generate king attacks, these are the same 
            Position kingPos = board.KingPosition(me);
            attacks = Attacks.KingAttacks(kingPos) & targetLocations;
            while (attacks != Bitboard.Empty)
            {
                Position attackPos = BitboardInfo.PopFirst(ref attacks);
                array[arrayIndex++].Move = MoveInfo.Create(kingPos, attackPos, board.PieceAt(kingPos), board.PieceAt(attackPos));
            }

            //determine check state.
            Bitboard evasionTargets = ~Bitboard.Empty;
            if (board.IsCheck())
            {
                int checkerCount = board.Checkers.BitCount();
                if (checkerCount == 1)
                {
                    Position checkerPos = board.Checkers.NorthMostPosition();
                    evasionTargets = kingPos.Between(checkerPos) | checkerPos.ToBitboard();
                    targetLocations &= evasionTargets;
                }
                else
                {
                    return arrayIndex; //nothing else to do.
                }
            }

            //loop through all sliders/knights locations
            Bitboard piecePositions = board[me] & ~board[PieceType.Pawn] & ~board[PieceType.King];
            while (piecePositions != Bitboard.Empty //for each slider/knight
                && targetLocations != Bitboard.Empty) //if there are no valid targets just skip this whole segment.
            {
                Position piecepos = BitboardInfo.PopFirst(ref piecePositions);
                Piece piece = board.PieceAt(piecepos);
                PieceType pieceType = piece.ToPieceType();
                switch (pieceType)
                {
                    case PieceType.Knight:
                        attacks = Attacks.KnightAttacks(piecepos) & targetLocations;
                        break;
                    case PieceType.Bishop:
                        attacks = Attacks.BishopAttacks(piecepos, board.PieceLocationsAll) & targetLocations;
                        break;
                    case PieceType.Rook:
                        attacks = Attacks.RookAttacks(piecepos, board.PieceLocationsAll) & targetLocations;
                        break;
                    case PieceType.Queen:
                        attacks = Attacks.QueenAttacks(piecepos, board.PieceLocationsAll) & targetLocations;
                        break;
                    case PieceType.King:
                        attacks = Attacks.KingAttacks(piecepos) & targetLocations; ;
                        break;
                    default:
                        System.Diagnostics.Debug.Assert(false);
                        break;
                }
                while (attacks != Bitboard.Empty)
                {
                    Position attackPos = BitboardInfo.PopFirst(ref attacks);
                    array[arrayIndex++].Move = MoveInfo.Create(piecepos, attackPos, board.PieceAt(piecepos), board.PieceAt(attackPos));
                }
            }

            



            //pawn caps
            piecePositions = board[me] & board[PieceType.Pawn];

            if (piecePositions != Bitboard.Empty)
            {
                if (captures)
                {
                    //pawn captures.
                    //must include enpassant in evasion target locations. possible could be an evasion cap such as in response to 
                    //8/4r3/R4n2/2pPk3/p1P1B1p1/3K2P1/5P2/8 w - - 4 54 

                    targetLocations = board[me.PlayerOther()] | (board.EnPassant.IsInBounds() ? board.EnPassant.ToBitboard() : 0);
                    targetLocations &= evasionTargets | (board.EnPassant.IsInBounds() ? board.EnPassant.ToBitboard() : 0);
                    
                    foreach (Direction capDir in new Direction[] { mypawneast, mypawnwest })
                    {
                        attacks = piecePositions.Shift(capDir) & targetLocations;
                        while (attacks != Bitboard.Empty)
                        {
                            Position targetpos = BitboardInfo.PopFirst(ref attacks);
                            Position piecepos = targetpos.PositionInDirectionUnsafe(capDir.Opposite());
                            if (targetpos.ToRank() == myrank8)
                            {
                                array[arrayIndex++].Move = MoveInfo.Create(piecepos, targetpos, board.PieceAt(piecepos), board.PieceAt(targetpos), PieceType.Queen);
                                array[arrayIndex++].Move = MoveInfo.Create(piecepos, targetpos, board.PieceAt(piecepos), board.PieceAt(targetpos), PieceType.Rook);
                                array[arrayIndex++].Move = MoveInfo.Create(piecepos, targetpos, board.PieceAt(piecepos), board.PieceAt(targetpos), PieceType.Bishop);
                                array[arrayIndex++].Move = MoveInfo.Create(piecepos, targetpos, board.PieceAt(piecepos), board.PieceAt(targetpos), PieceType.Knight);
                            }
                            else
                            {
                                array[arrayIndex++].Move = MoveInfo.Create(piecepos, targetpos, board.PieceAt(piecepos), board.PieceAt(targetpos));
                            }
                        }
                    }
                }
                else
                {
                    //pawn jumps
                    targetLocations = ~board.PieceLocationsAll; //empty squares pawns could jump to
                    targetLocations &= evasionTargets;

                    //find single jumpers.
                    attacks = targetLocations.Shift(mypawnsouth) & piecePositions;
                    while (attacks != Bitboard.Empty)
                    {
                        Position piecepos = BitboardInfo.PopFirst(ref attacks);
                        Position targetpos = piecepos.PositionInDirectionUnsafe(mypawnnorth);
                        if (targetpos.ToRank() == myrank8)
                        {
                            array[arrayIndex++].Move = MoveInfo.Create(piecepos, targetpos, board.PieceAt(piecepos), board.PieceAt(targetpos), PieceType.Queen);
                            array[arrayIndex++].Move = MoveInfo.Create(piecepos, targetpos, board.PieceAt(piecepos), board.PieceAt(targetpos), PieceType.Rook);
                            array[arrayIndex++].Move = MoveInfo.Create(piecepos, targetpos, board.PieceAt(piecepos), board.PieceAt(targetpos), PieceType.Bishop);
                            array[arrayIndex++].Move = MoveInfo.Create(piecepos, targetpos, board.PieceAt(piecepos), board.PieceAt(targetpos), PieceType.Knight);
                        }
                        else
                        {
                            array[arrayIndex++].Move = MoveInfo.Create(piecepos, targetpos, board.PieceAt(piecepos), board.PieceAt(targetpos));
                        }
                    }

                    //pawn double jumps
                    attacks = myrank2.ToBitboard()
                        & piecePositions
                        & targetLocations.Shift(mypawnsouth).Shift(mypawnsouth)
                        & ~board.PieceLocationsAll.Shift(mypawnsouth);
                    while (attacks != Bitboard.Empty)
                    {
                        Position piecepos = BitboardInfo.PopFirst(ref attacks);
                        Position targetpos = piecepos.PositionInDirectionUnsafe(mypawnnorth).PositionInDirectionUnsafe(mypawnnorth);
                        array[arrayIndex++].Move = MoveInfo.Create(piecepos, targetpos, board.PieceAt(piecepos), board.PieceAt(targetpos));
                    }
                }



            }

            if (!captures && !board.IsCheck())
            {


                //castling
                if (board.WhosTurn == Player.White)
                {
                    if ((board.CastleRights & CastleFlags.WhiteShort) != 0
                        && board.PieceAt(Position.E1) == Piece.WKing
                        && board.PieceAt(Position.H1) == Piece.WRook
                        && board.PieceAt(Position.F1) == Piece.EMPTY
                        && board.PieceAt(Position.G1) == Piece.EMPTY
                        && !board.PositionAttacked(Position.E1, Player.Black)
                        && !board.PositionAttacked(Position.F1, Player.Black)
                        && !board.PositionAttacked(Position.G1, Player.Black))
                    {
                        array[arrayIndex++].Move = MoveInfo.Create(Position.E1, Position.G1, board.PieceAt(Position.E1), board.PieceAt(Position.G1));
                    }
                    if ((board.CastleRights & CastleFlags.WhiteLong) != 0
                        && board.PieceAt(Position.E1) == Piece.WKing
                        && board.PieceAt(Position.A1) == Piece.WRook
                        && board.PieceAt(Position.B1) == Piece.EMPTY
                        && board.PieceAt(Position.C1) == Piece.EMPTY
                        && board.PieceAt(Position.D1) == Piece.EMPTY
                        && !board.PositionAttacked(Position.E1, Player.Black)
                        && !board.PositionAttacked(Position.D1, Player.Black)
                        && !board.PositionAttacked(Position.C1, Player.Black))
                    {
                        array[arrayIndex++].Move = MoveInfo.Create(Position.E1, Position.C1, board.PieceAt(Position.E1), board.PieceAt(Position.C1));
                    }
                }
                else
                {
                    if ((board.CastleRights & CastleFlags.BlackShort) != 0
                        && board.PieceAt(Position.E8) == Piece.BKing
                        && board.PieceAt(Position.H8) == Piece.BRook
                        && board.PieceAt(Position.F8) == Piece.EMPTY
                        && board.PieceAt(Position.G8) == Piece.EMPTY
                        && !board.PositionAttacked(Position.E8, Player.White)
                        && !board.PositionAttacked(Position.F8, Player.White)
                        && !board.PositionAttacked(Position.G8, Player.White))
                    {
                        array[arrayIndex++].Move = MoveInfo.Create(Position.E8, Position.G8, board.PieceAt(Position.E8), board.PieceAt(Position.G8));
                    }
                    if ((board.CastleRights & CastleFlags.BlackLong) != 0
                        && board.PieceAt(Position.E8) == Piece.BKing
                        && board.PieceAt(Position.A8) == Piece.BRook
                        && board.PieceAt(Position.B8) == Piece.EMPTY
                        && board.PieceAt(Position.C8) == Piece.EMPTY
                        && board.PieceAt(Position.D8) == Piece.EMPTY
                        && !board.PositionAttacked(Position.E8, Player.White)
                        && !board.PositionAttacked(Position.D8, Player.White)
                        && !board.PositionAttacked(Position.C8, Player.White))
                    {
                        array[arrayIndex++].Move = MoveInfo.Create(Position.E8, Position.C8, board.PieceAt(Position.E8), board.PieceAt(Position.C8));
                    }

                }
            }
            return arrayIndex;

        }



        public static IEnumerable<Move> GenMovesOld(Board board, bool CapsOnly)
        {

            Piece mypawn = board.WhosTurn == Player.White ? Piece.WPawn : Piece.BPawn;
            Piece myknight = board.WhosTurn == Player.White ? Piece.WKnight : Piece.BKnight;
            Piece mybishop = board.WhosTurn == Player.White ? Piece.WBishop : Piece.BBishop;
            Piece myrook = board.WhosTurn == Player.White ? Piece.WRook : Piece.BRook;
            Piece myqueen = board.WhosTurn == Player.White ? Piece.WQueen : Piece.BQueen;
            Piece myking = board.WhosTurn == Player.White ? Piece.WKing : Piece.BKing;

            Direction mypawnwest = board.WhosTurn == Player.White ? Direction.DirNW : Direction.DirSW;
            Direction mypawneast = board.WhosTurn == Player.White ? Direction.DirNE : Direction.DirSE;
            Direction mypawnnorth = board.WhosTurn == Player.White ? Direction.DirN : Direction.DirS;
            Rank myrank8 = board.WhosTurn == Player.White ? Rank.Rank8 : Rank.Rank1;
            Rank myrank2 = board.WhosTurn == Player.White ? Rank.Rank2 : Rank.Rank7;

            Bitboard attacks = Bitboard.Empty;

            Player us = board.WhosTurn;
            Player them = us.PlayerOther();

            //pawn caps
            Bitboard pawnTargets = board[them] | (board.EnPassant.IsInBounds() ? board.EnPassant.ToBitboard() : 0);
            foreach (Direction capDir in new Direction[] { mypawneast, mypawnwest })
            {
                attacks = board[us, PieceType.Pawn].Shift(capDir) & pawnTargets;
                while (attacks != Bitboard.Empty)
                {
                    Position targetpos = BitboardInfo.PopFirst(ref attacks);
                    Position piecepos = targetpos.PositionInDirectionUnsafe(capDir.Opposite());
                    if (targetpos.ToRank() == myrank8)
                    {
                        yield return MoveInfo.Create(piecepos, targetpos, board.PieceAt(piecepos), board.PieceAt(targetpos), PieceType.Queen);
                        yield return MoveInfo.Create(piecepos, targetpos, board.PieceAt(piecepos), board.PieceAt(targetpos), PieceType.Rook);
                        yield return MoveInfo.Create(piecepos, targetpos, board.PieceAt(piecepos), board.PieceAt(targetpos), PieceType.Bishop);
                        yield return MoveInfo.Create(piecepos, targetpos, board.PieceAt(piecepos), board.PieceAt(targetpos), PieceType.Knight);
                    }
                    else
                    {
                        yield return MoveInfo.Create(piecepos, targetpos, board.PieceAt(piecepos), board.PieceAt(targetpos));
                    }
                }
            }

            Bitboard targetLocations = board[them];
            if (!CapsOnly)
            {
                targetLocations |= ~board.PieceLocationsAll;
            }

            //loop through all non pawn locations
            Bitboard piecePositions = board[board.WhosTurn] & ~board[PieceType.Pawn];
            while (piecePositions != Bitboard.Empty)// (ChessPosition piecepos in (board[board.WhosTurn] & ~board[ChessPieceType.Pawn]).ToPositions())
            {
                Position piecepos = BitboardInfo.PopFirst(ref piecePositions);
                Piece piece = board.PieceAt(piecepos);
                PieceType pieceType = piece.ToPieceType();
                switch (pieceType)
                {
                    case PieceType.Knight:
                        attacks = Attacks.KnightAttacks(piecepos) & targetLocations;
                        break;
                    case PieceType.Bishop:
                        attacks = Attacks.BishopAttacks(piecepos, board.PieceLocationsAll) & targetLocations;
                        break;
                    case PieceType.Rook:
                        attacks = Attacks.RookAttacks(piecepos, board.PieceLocationsAll) & targetLocations;
                        break;
                    case PieceType.Queen:
                        attacks = Attacks.QueenAttacks(piecepos, board.PieceLocationsAll) & targetLocations;
                        break;
                    case PieceType.King:
                        attacks = Attacks.KingAttacks(piecepos) & targetLocations;
                        break;
                }
                while (attacks != Bitboard.Empty)
                {
                    Position attackPos = BitboardInfo.PopFirst(ref attacks);
                    yield return MoveInfo.Create(piecepos, attackPos, board.PieceAt(piecepos), board.PieceAt(attackPos));
                }
            }





            if (!CapsOnly)
            {
                //pawn jumps
                attacks = (board[us, PieceType.Pawn].Shift(mypawnnorth) & ~board.PieceLocationsAll);
                while (attacks != Bitboard.Empty)
                {
                    Position targetpos = BitboardInfo.PopFirst(ref attacks);
                    Position piecepos = targetpos.PositionInDirectionUnsafe(mypawnnorth.Opposite());
                    if (targetpos.ToRank() == myrank8)
                    {
                        yield return MoveInfo.Create(piecepos, targetpos, board.PieceAt(piecepos), board.PieceAt(targetpos), PieceType.Queen);
                        yield return MoveInfo.Create(piecepos, targetpos, board.PieceAt(piecepos), board.PieceAt(targetpos), PieceType.Rook);
                        yield return MoveInfo.Create(piecepos, targetpos, board.PieceAt(piecepos), board.PieceAt(targetpos), PieceType.Bishop);
                        yield return MoveInfo.Create(piecepos, targetpos, board.PieceAt(piecepos), board.PieceAt(targetpos), PieceType.Knight);
                    }
                    else
                    {
                        yield return MoveInfo.Create(piecepos, targetpos, board.PieceAt(piecepos), board.PieceAt(targetpos));
                        if (piecepos.ToRank() == myrank2)
                        {
                            var doubleJumpPos = targetpos.PositionInDirectionUnsafe(mypawnnorth);
                            if (board.PieceAt(doubleJumpPos) == Piece.EMPTY)
                            {
                                yield return MoveInfo.Create(piecepos, doubleJumpPos, board.PieceAt(piecepos), board.PieceAt(doubleJumpPos));
                            }
                        }
                    }
                }

                //castling
                if (board.WhosTurn == Player.White)
                {
                    if ((board.CastleRights & CastleFlags.WhiteShort) != 0
                        && board.PieceAt(Position.E1) == Piece.WKing
                        && board.PieceAt(Position.H1) == Piece.WRook
                        && board.PieceAt(Position.F1) == Piece.EMPTY
                        && board.PieceAt(Position.G1) == Piece.EMPTY
                        && !board.PositionAttacked(Position.E1, Player.Black)
                        && !board.PositionAttacked(Position.F1, Player.Black)
                        && !board.PositionAttacked(Position.G1, Player.Black))
                    {
                        yield return MoveInfo.Create(Position.E1, Position.G1, board.PieceAt(Position.E1), board.PieceAt(Position.G1));
                    }
                    if ((board.CastleRights & CastleFlags.WhiteLong) != 0
                        && board.PieceAt(Position.E1) == Piece.WKing
                        && board.PieceAt(Position.A1) == Piece.WRook
                        && board.PieceAt(Position.B1) == Piece.EMPTY
                        && board.PieceAt(Position.C1) == Piece.EMPTY
                        && board.PieceAt(Position.D1) == Piece.EMPTY
                        && !board.PositionAttacked(Position.E1, Player.Black)
                        && !board.PositionAttacked(Position.D1, Player.Black)
                        && !board.PositionAttacked(Position.C1, Player.Black))
                    {
                        yield return MoveInfo.Create(Position.E1, Position.C1, board.PieceAt(Position.E1), board.PieceAt(Position.C1));
                    }
                }
                else
                {
                    if ((board.CastleRights & CastleFlags.BlackShort) != 0
                        && board.PieceAt(Position.E8) == Piece.BKing
                        && board.PieceAt(Position.H8) == Piece.BRook
                        && board.PieceAt(Position.F8) == Piece.EMPTY
                        && board.PieceAt(Position.G8) == Piece.EMPTY
                        && !board.PositionAttacked(Position.E8, Player.White)
                        && !board.PositionAttacked(Position.F8, Player.White)
                        && !board.PositionAttacked(Position.G8, Player.White))
                    {
                        yield return MoveInfo.Create(Position.E8, Position.G8, board.PieceAt(Position.E8), board.PieceAt(Position.G8));
                    }
                    if ((board.CastleRights & CastleFlags.BlackLong) != 0
                        && board.PieceAt(Position.E8) == Piece.BKing
                        && board.PieceAt(Position.A8) == Piece.BRook
                        && board.PieceAt(Position.B8) == Piece.EMPTY
                        && board.PieceAt(Position.C8) == Piece.EMPTY
                        && board.PieceAt(Position.D8) == Piece.EMPTY
                        && !board.PositionAttacked(Position.E8, Player.White)
                        && !board.PositionAttacked(Position.D8, Player.White)
                        && !board.PositionAttacked(Position.C8, Player.White))
                    {
                        yield return MoveInfo.Create(Position.E8, Position.C8, board.PieceAt(Position.E8), board.PieceAt(Position.C8));
                    }

                }
            }

        }


    }
}
