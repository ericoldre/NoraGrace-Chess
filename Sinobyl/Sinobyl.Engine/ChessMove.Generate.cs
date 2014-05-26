using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinobyl.Engine
{

    public static partial class ChessMoveInfo
    {
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
            ChessMoveData[] array = new ChessMoveData[210];
            int count = GenMovesArray(array, board, false);
            for (int i = 0; i < count; i++)
            {
                yield return array[i].Move;
            }
        }

        public static int GenMovesArray(ChessMoveData[] array, ChessBoard board, bool capsOnly)
        {
            ChessBitboard mypieces = board[board.WhosTurn];
            ChessBitboard hispieces = board[board.WhosTurn.PlayerOther()];
            ChessBitboard kingAttacks = Attacks.KingAttacks(board.KingPosition(board.WhosTurn)) & ~mypieces;
            //return GenMoves(board, board[board.WhosTurn], capsOnly ? board[board.WhosTurn.PlayerOther()] : ~board[board.WhosTurn], !capsOnly);
            if (board.Checkers == ChessBitboard.Empty)
            {
                var caps = GenCapsNonCaps(array, board, true, 0);
                if (!capsOnly)
                {
                    return GenCapsNonCaps(array, board, false, caps);
                }
                else
                {
                    return caps;
                }
                ////not in check, normal logic
                //if (capsOnly) { kingAttacks &= hispieces; }
                //return GenMoves(array, board, mypieces, capsOnly ? hispieces : ~mypieces, !capsOnly, kingAttacks);
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

        private static int GenCapsNonCaps(ChessMoveData[] array, ChessBoard board, bool captures, int arrayIndex)
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
            ChessDirection mypawnsouth = board.WhosTurn == ChessPlayer.White ? ChessDirection.DirS : ChessDirection.DirN;
            ChessRank myrank8 = board.WhosTurn == ChessPlayer.White ? ChessRank.Rank8 : ChessRank.Rank1;
            ChessRank myrank2 = board.WhosTurn == ChessPlayer.White ? ChessRank.Rank2 : ChessRank.Rank7;

            ChessPlayer me = board.WhosTurn;

            ChessBitboard attacks = ChessBitboard.Empty;

            ChessBitboard targetLocations = captures ? board[me.PlayerOther()] : ~board.PieceLocationsAll;

            //loop through all non pawn locations
            ChessBitboard piecePositions = board[me] & ~board[ChessPieceType.Pawn];
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
                        attacks = Attacks.KingAttacks(piecepos) & targetLocations; ;
                        break;
                }
                while (attacks != ChessBitboard.Empty)
                {
                    ChessPosition attackPos = ChessBitboardInfo.PopFirst(ref attacks);
                    array[arrayIndex++].Move = ChessMoveInfo.Create(piecepos, attackPos);
                }
            }


            //pawn caps
            piecePositions = board[me] & board[ChessPieceType.Pawn];

            if (piecePositions != ChessBitboard.Empty)
            {
                if (captures)
                {
                    //pawn captures.
                    targetLocations = board[me.PlayerOther()] | (board.EnPassant.IsInBounds() ? board.EnPassant.Bitboard() : 0);

                    foreach (ChessDirection capDir in new ChessDirection[] { mypawneast, mypawnwest })
                    {
                        attacks = piecePositions.Shift(capDir) & targetLocations;
                        while (attacks != ChessBitboard.Empty)
                        {
                            ChessPosition targetpos = ChessBitboardInfo.PopFirst(ref attacks);
                            ChessPosition piecepos = targetpos.PositionInDirectionUnsafe(capDir.Opposite());
                            if (targetpos.GetRank() == myrank8)
                            {
                                array[arrayIndex++].Move = ChessMoveInfo.Create(piecepos, targetpos, myqueen);
                                array[arrayIndex++].Move = ChessMoveInfo.Create(piecepos, targetpos, myrook);
                                array[arrayIndex++].Move = ChessMoveInfo.Create(piecepos, targetpos, mybishop);
                                array[arrayIndex++].Move = ChessMoveInfo.Create(piecepos, targetpos, myknight);
                            }
                            else
                            {
                                array[arrayIndex++].Move = ChessMoveInfo.Create(piecepos, targetpos);
                            }
                        }
                    }
                }
                else
                {
                    //pawn jumps
                    targetLocations = ~board.PieceLocationsAll; //empty squares pawns could jump to

                    //find single jumpers.
                    attacks = targetLocations.Shift(mypawnsouth) & piecePositions;
                    while (attacks != ChessBitboard.Empty)
                    {
                        ChessPosition piecepos = ChessBitboardInfo.PopFirst(ref attacks);
                        ChessPosition targetpos = piecepos.PositionInDirectionUnsafe(mypawnnorth);
                        if (targetpos.GetRank() == myrank8)
                        {
                            array[arrayIndex++].Move = ChessMoveInfo.Create(piecepos, targetpos, myqueen);
                            array[arrayIndex++].Move = ChessMoveInfo.Create(piecepos, targetpos, myrook);
                            array[arrayIndex++].Move = ChessMoveInfo.Create(piecepos, targetpos, mybishop);
                            array[arrayIndex++].Move = ChessMoveInfo.Create(piecepos, targetpos, myknight);
                        }
                        else
                        {
                            array[arrayIndex++].Move = ChessMoveInfo.Create(piecepos, targetpos);
                        }
                    }

                    //pawn double jumps
                    attacks = myrank2.Bitboard()
                        & piecePositions
                        & targetLocations.Shift(mypawnsouth).Shift(mypawnsouth)
                        & ~board.PieceLocationsAll.Shift(mypawnsouth);
                    while (attacks != ChessBitboard.Empty)
                    {
                        ChessPosition piecepos = ChessBitboardInfo.PopFirst(ref attacks);
                        ChessPosition targetpos = piecepos.PositionInDirectionUnsafe(mypawnnorth).PositionInDirectionUnsafe(mypawnnorth);
                        array[arrayIndex++].Move = ChessMoveInfo.Create(piecepos, targetpos);
                    }
                }



            }

            if (!captures)
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
                        array[arrayIndex++].Move = ChessMoveInfo.Create(ChessPosition.E1, ChessPosition.G1);
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
                        array[arrayIndex++].Move = ChessMoveInfo.Create(ChessPosition.E1, ChessPosition.C1);
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
                        array[arrayIndex++].Move = ChessMoveInfo.Create(ChessPosition.E8, ChessPosition.G8);
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
                        array[arrayIndex++].Move = ChessMoveInfo.Create(ChessPosition.E8, ChessPosition.C8);
                    }

                }
            }
            return arrayIndex;

        }


        private static int GenMoves(ChessMoveData[] array, ChessBoard board, ChessBitboard possibleMovers, ChessBitboard targetLocations, bool castling, ChessBitboard kingTargets)
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
