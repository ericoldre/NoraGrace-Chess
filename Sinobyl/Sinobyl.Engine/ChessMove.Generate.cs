﻿using System;
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

            Player me = board.WhosTurn;
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
            Bitboard mypieces = board[board.WhosTurn];
            Bitboard hispieces = board[board.WhosTurn.PlayerOther()];
            Bitboard kingAttacks = Attacks.KingAttacks(board.KingPosition(board.WhosTurn)) & ~mypieces;
            //return GenMoves(board, board[board.WhosTurn], capsOnly ? board[board.WhosTurn.PlayerOther()] : ~board[board.WhosTurn], !capsOnly);
            if (true || board.Checkers == Bitboard.Empty)
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
                    return GenMoves(array, board, board.KingPosition(board.WhosTurn).ToBitboard(), ~mypieces, false, kingAttacks);
                }
                else
                {
                    ChessPosition attackerPos = board.Checkers.NorthMostPosition();
                    ChessPosition kingPos = board.KingPosition(board.WhosTurn);
                    Direction dir = kingPos.DirectionTo(attackerPos);
                    Bitboard attackLocs = Bitboard.Empty;
                    while (kingPos != attackerPos)
                    {
                        kingPos = kingPos.PositionInDirectionUnsafe(dir);
                        attackLocs |= kingPos.ToBitboard();
                    }
                    //move any piece to either location of attacker, or in path, or additionally move king out of harms way.
                    return GenMoves(array, board, mypieces, attackLocs, false, kingAttacks);
                }
            }
        }

        public static int GenCapsNonCaps(ChessMoveData[] array, ChessBoard board, bool captures, int arrayIndex)
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
            ChessPosition kingPos = board.KingPosition(me);
            attacks = Attacks.KingAttacks(kingPos) & targetLocations;
            while (attacks != Bitboard.Empty)
            {
                ChessPosition attackPos = BitboardInfo.PopFirst(ref attacks);
                array[arrayIndex++].Move = ChessMoveInfo.Create(kingPos, attackPos);
            }

            //determine check state.
            Bitboard evasionTargets = ~Bitboard.Empty;
            if (board.IsCheck())
            {
                int checkerCount = board.Checkers.BitCount();
                if (checkerCount == 1)
                {
                    ChessPosition checkerPos = board.Checkers.NorthMostPosition();
                    evasionTargets = kingPos.Between(checkerPos) | checkerPos.ToBitboard();
                    targetLocations &= evasionTargets;
                }
                else
                {
                    return arrayIndex; //nothing else to do.
                }
            }

            //loop through all sliders/knights locations
            Bitboard piecePositions = board[me] & ~board[ChessPieceType.Pawn] & ~board[ChessPieceType.King];
            while (piecePositions != Bitboard.Empty //for each slider/knight
                && targetLocations != Bitboard.Empty) //if there are no valid targets just skip this whole segment.
            {
                ChessPosition piecepos = BitboardInfo.PopFirst(ref piecePositions);
                Piece piece = board.PieceAt(piecepos);
                ChessPieceType pieceType = piece.ToPieceType();
                switch (pieceType)
                {
                    case ChessPieceType.Knight:
                        attacks = Attacks.KnightAttacks(piecepos) & targetLocations;
                        break;
                    case ChessPieceType.Bishop:
                        attacks = Attacks.BishopAttacks(piecepos, board.PieceLocationsAll) & targetLocations;
                        break;
                    case ChessPieceType.Rook:
                        attacks = Attacks.RookAttacks(piecepos, board.PieceLocationsAll) & targetLocations;
                        break;
                    case ChessPieceType.Queen:
                        attacks = Attacks.QueenAttacks(piecepos, board.PieceLocationsAll) & targetLocations;
                        break;
                    case ChessPieceType.King:
                        attacks = Attacks.KingAttacks(piecepos) & targetLocations; ;
                        break;
                    default:
                        System.Diagnostics.Debug.Assert(false);
                        break;
                }
                while (attacks != Bitboard.Empty)
                {
                    ChessPosition attackPos = BitboardInfo.PopFirst(ref attacks);
                    array[arrayIndex++].Move = ChessMoveInfo.Create(piecepos, attackPos);
                }
            }

            



            //pawn caps
            piecePositions = board[me] & board[ChessPieceType.Pawn];

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
                            ChessPosition targetpos = BitboardInfo.PopFirst(ref attacks);
                            ChessPosition piecepos = targetpos.PositionInDirectionUnsafe(capDir.Opposite());
                            if (targetpos.ToRank() == myrank8)
                            {
                                array[arrayIndex++].Move = ChessMoveInfo.Create(piecepos, targetpos, ChessPieceType.Queen.ForPlayer(me));
                                array[arrayIndex++].Move = ChessMoveInfo.Create(piecepos, targetpos, ChessPieceType.Rook.ForPlayer(me));
                                array[arrayIndex++].Move = ChessMoveInfo.Create(piecepos, targetpos, ChessPieceType.Bishop.ForPlayer(me));
                                array[arrayIndex++].Move = ChessMoveInfo.Create(piecepos, targetpos, ChessPieceType.Knight.ForPlayer(me));
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
                    targetLocations &= evasionTargets;

                    //find single jumpers.
                    attacks = targetLocations.Shift(mypawnsouth) & piecePositions;
                    while (attacks != Bitboard.Empty)
                    {
                        ChessPosition piecepos = BitboardInfo.PopFirst(ref attacks);
                        ChessPosition targetpos = piecepos.PositionInDirectionUnsafe(mypawnnorth);
                        if (targetpos.ToRank() == myrank8)
                        {
                            array[arrayIndex++].Move = ChessMoveInfo.Create(piecepos, targetpos, ChessPieceType.Queen.ForPlayer(me));
                            array[arrayIndex++].Move = ChessMoveInfo.Create(piecepos, targetpos, ChessPieceType.Rook.ForPlayer(me));
                            array[arrayIndex++].Move = ChessMoveInfo.Create(piecepos, targetpos, ChessPieceType.Bishop.ForPlayer(me));
                            array[arrayIndex++].Move = ChessMoveInfo.Create(piecepos, targetpos, ChessPieceType.Knight.ForPlayer(me));
                        }
                        else
                        {
                            array[arrayIndex++].Move = ChessMoveInfo.Create(piecepos, targetpos);
                        }
                    }

                    //pawn double jumps
                    attacks = myrank2.ToBitboard()
                        & piecePositions
                        & targetLocations.Shift(mypawnsouth).Shift(mypawnsouth)
                        & ~board.PieceLocationsAll.Shift(mypawnsouth);
                    while (attacks != Bitboard.Empty)
                    {
                        ChessPosition piecepos = BitboardInfo.PopFirst(ref attacks);
                        ChessPosition targetpos = piecepos.PositionInDirectionUnsafe(mypawnnorth).PositionInDirectionUnsafe(mypawnnorth);
                        array[arrayIndex++].Move = ChessMoveInfo.Create(piecepos, targetpos);
                    }
                }



            }

            if (!captures && !board.IsCheck())
            {


                //castling
                if (board.WhosTurn == Player.White)
                {
                    if ((board.CastleRights & CastleFlags.WhiteShort) != 0
                        && board.PieceAt(ChessPosition.E1) == Piece.WKing
                        && board.PieceAt(ChessPosition.H1) == Piece.WRook
                        && board.PieceAt(ChessPosition.F1) == Piece.EMPTY
                        && board.PieceAt(ChessPosition.G1) == Piece.EMPTY
                        && !board.PositionAttacked(ChessPosition.E1, Player.Black)
                        && !board.PositionAttacked(ChessPosition.F1, Player.Black)
                        && !board.PositionAttacked(ChessPosition.G1, Player.Black))
                    {
                        array[arrayIndex++].Move = ChessMoveInfo.Create(ChessPosition.E1, ChessPosition.G1);
                    }
                    if ((board.CastleRights & CastleFlags.WhiteLong) != 0
                        && board.PieceAt(ChessPosition.E1) == Piece.WKing
                        && board.PieceAt(ChessPosition.A1) == Piece.WRook
                        && board.PieceAt(ChessPosition.B1) == Piece.EMPTY
                        && board.PieceAt(ChessPosition.C1) == Piece.EMPTY
                        && board.PieceAt(ChessPosition.D1) == Piece.EMPTY
                        && !board.PositionAttacked(ChessPosition.E1, Player.Black)
                        && !board.PositionAttacked(ChessPosition.D1, Player.Black)
                        && !board.PositionAttacked(ChessPosition.C1, Player.Black))
                    {
                        array[arrayIndex++].Move = ChessMoveInfo.Create(ChessPosition.E1, ChessPosition.C1);
                    }
                }
                else
                {
                    if ((board.CastleRights & CastleFlags.BlackShort) != 0
                        && board.PieceAt(ChessPosition.E8) == Piece.BKing
                        && board.PieceAt(ChessPosition.H8) == Piece.BRook
                        && board.PieceAt(ChessPosition.F8) == Piece.EMPTY
                        && board.PieceAt(ChessPosition.G8) == Piece.EMPTY
                        && !board.PositionAttacked(ChessPosition.E8, Player.White)
                        && !board.PositionAttacked(ChessPosition.F8, Player.White)
                        && !board.PositionAttacked(ChessPosition.G8, Player.White))
                    {
                        array[arrayIndex++].Move = ChessMoveInfo.Create(ChessPosition.E8, ChessPosition.G8);
                    }
                    if ((board.CastleRights & CastleFlags.BlackLong) != 0
                        && board.PieceAt(ChessPosition.E8) == Piece.BKing
                        && board.PieceAt(ChessPosition.A8) == Piece.BRook
                        && board.PieceAt(ChessPosition.B8) == Piece.EMPTY
                        && board.PieceAt(ChessPosition.C8) == Piece.EMPTY
                        && board.PieceAt(ChessPosition.D8) == Piece.EMPTY
                        && !board.PositionAttacked(ChessPosition.E8, Player.White)
                        && !board.PositionAttacked(ChessPosition.D8, Player.White)
                        && !board.PositionAttacked(ChessPosition.C8, Player.White))
                    {
                        array[arrayIndex++].Move = ChessMoveInfo.Create(ChessPosition.E8, ChessPosition.C8);
                    }

                }
            }
            return arrayIndex;

        }


        private static int GenMoves(ChessMoveData[] array, ChessBoard board, Bitboard possibleMovers, Bitboard targetLocations, bool castling, Bitboard kingTargets)
        {
            System.Diagnostics.Debug.Assert((possibleMovers & ~board[board.WhosTurn]) == Bitboard.Empty); //possible movers must be subset of my pieces
            System.Diagnostics.Debug.Assert((targetLocations & board[board.WhosTurn]) == Bitboard.Empty); //targets may not include my pieces.
            System.Diagnostics.Debug.Assert((kingTargets & ~Attacks.KingAttacks(board.KingPosition(board.WhosTurn))) == Bitboard.Empty); //king targets is very specific, must filter before calling this

            int retval = 0;

            Piece mypawn = board.WhosTurn == Player.White ? Piece.WPawn : Piece.BPawn;
            Piece myknight = board.WhosTurn == Player.White ? Piece.WKnight : Piece.BKnight;
            Piece mybishop = board.WhosTurn == Player.White ? Piece.WBishop : Piece.BBishop;
            Piece myrook = board.WhosTurn == Player.White ? Piece.WRook : Piece.BRook;
            Piece myqueen = board.WhosTurn == Player.White ? Piece.WQueen : Piece.BQueen;
            Piece myking = board.WhosTurn == Player.White ? Piece.WKing : Piece.BKing;

            Direction mypawnwest = board.WhosTurn == Player.White ? Direction.DirNW : Direction.DirSW;
            Direction mypawneast = board.WhosTurn == Player.White ? Direction.DirNE : Direction.DirSE;
            Direction mypawnnorth = board.WhosTurn == Player.White ? Direction.DirN : Direction.DirS;
            Direction mypawnsouth = board.WhosTurn == Player.White ? Direction.DirS : Direction.DirN;
            Rank myrank8 = board.WhosTurn == Player.White ? Rank.Rank8 : Rank.Rank1;
            Rank myrank2 = board.WhosTurn == Player.White ? Rank.Rank2 : Rank.Rank7;

            Bitboard attacks = Bitboard.Empty;


            //loop through all non pawn locations
            Bitboard piecePositions = possibleMovers & ~board[ChessPieceType.Pawn];
            while (piecePositions != Bitboard.Empty)// (ChessPosition piecepos in (board[board.WhosTurn] & ~board[ChessPieceType.Pawn]).ToPositions())
            {
                ChessPosition piecepos = BitboardInfo.PopFirst(ref piecePositions);
                Piece piece = board.PieceAt(piecepos);
                ChessPieceType pieceType = piece.ToPieceType();
                switch (pieceType)
                {
                    case ChessPieceType.Knight:
                        attacks = Attacks.KnightAttacks(piecepos) & targetLocations;
                        break;
                    case ChessPieceType.Bishop:
                        attacks = Attacks.BishopAttacks(piecepos, board.PieceLocationsAll) & targetLocations;
                        break;
                    case ChessPieceType.Rook:
                        attacks = Attacks.RookAttacks(piecepos, board.PieceLocationsAll) & targetLocations;
                        break;
                    case ChessPieceType.Queen:
                        attacks = Attacks.QueenAttacks(piecepos, board.PieceLocationsAll) & targetLocations;
                        break;
                    case ChessPieceType.King:
                        attacks = kingTargets;
                        break;
                }
                while (attacks != Bitboard.Empty)
                {
                    ChessPosition attackPos = BitboardInfo.PopFirst(ref attacks);
                    array[retval++].Move = ChessMoveInfo.Create(piecepos, attackPos);
                }
            }


            //pawn caps
            piecePositions = possibleMovers & board[ChessPieceType.Pawn];


            if (piecePositions != Bitboard.Empty)
            {
                Bitboard pawnTargets = (targetLocations & board.PieceLocationsAll) | (board.EnPassant.IsInBounds() ? board.EnPassant.ToBitboard() : 0);

                //pawn captures.
                foreach (Direction capDir in new Direction[] { mypawneast, mypawnwest })
                {
                    attacks = piecePositions.Shift(capDir) & pawnTargets;
                    while (attacks != Bitboard.Empty)
                    {
                        ChessPosition targetpos = BitboardInfo.PopFirst(ref attacks);
                        ChessPosition piecepos = targetpos.PositionInDirectionUnsafe(capDir.Opposite());
                        if (targetpos.ToRank() == myrank8)
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
                while (attacks != Bitboard.Empty)
                {
                    ChessPosition piecepos = BitboardInfo.PopFirst(ref attacks);
                    ChessPosition targetpos = piecepos.PositionInDirectionUnsafe(mypawnnorth);
                    if (targetpos.ToRank() == myrank8)
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
                attacks = myrank2.ToBitboard()
                    & piecePositions
                    & pawnTargets.Shift(mypawnsouth).Shift(mypawnsouth)
                    & ~board.PieceLocationsAll.Shift(mypawnsouth);
                while (attacks != Bitboard.Empty)
                {
                    ChessPosition piecepos = BitboardInfo.PopFirst(ref attacks);
                    ChessPosition targetpos = piecepos.PositionInDirectionUnsafe(mypawnnorth).PositionInDirectionUnsafe(mypawnnorth);
                    array[retval++].Move = ChessMoveInfo.Create(piecepos, targetpos);
                }

            }




            if (castling)
            {


                //castling
                if (board.WhosTurn == Player.White)
                {
                    if ((board.CastleRights & CastleFlags.WhiteShort) != 0
                        && board.PieceAt(ChessPosition.E1) == Piece.WKing
                        && board.PieceAt(ChessPosition.H1) == Piece.WRook
                        && board.PieceAt(ChessPosition.F1) == Piece.EMPTY
                        && board.PieceAt(ChessPosition.G1) == Piece.EMPTY
                        && !board.PositionAttacked(ChessPosition.E1, Player.Black)
                        && !board.PositionAttacked(ChessPosition.F1, Player.Black)
                        && !board.PositionAttacked(ChessPosition.G1, Player.Black))
                    {
                        array[retval++].Move = ChessMoveInfo.Create(ChessPosition.E1, ChessPosition.G1);
                    }
                    if ((board.CastleRights & CastleFlags.WhiteLong) != 0
                        && board.PieceAt(ChessPosition.E1) == Piece.WKing
                        && board.PieceAt(ChessPosition.A1) == Piece.WRook
                        && board.PieceAt(ChessPosition.B1) == Piece.EMPTY
                        && board.PieceAt(ChessPosition.C1) == Piece.EMPTY
                        && board.PieceAt(ChessPosition.D1) == Piece.EMPTY
                        && !board.PositionAttacked(ChessPosition.E1, Player.Black)
                        && !board.PositionAttacked(ChessPosition.D1, Player.Black)
                        && !board.PositionAttacked(ChessPosition.C1, Player.Black))
                    {
                        array[retval++].Move = ChessMoveInfo.Create(ChessPosition.E1, ChessPosition.C1);
                    }
                }
                else
                {
                    if ((board.CastleRights & CastleFlags.BlackShort) != 0
                        && board.PieceAt(ChessPosition.E8) == Piece.BKing
                        && board.PieceAt(ChessPosition.H8) == Piece.BRook
                        && board.PieceAt(ChessPosition.F8) == Piece.EMPTY
                        && board.PieceAt(ChessPosition.G8) == Piece.EMPTY
                        && !board.PositionAttacked(ChessPosition.E8, Player.White)
                        && !board.PositionAttacked(ChessPosition.F8, Player.White)
                        && !board.PositionAttacked(ChessPosition.G8, Player.White))
                    {
                        array[retval++].Move = ChessMoveInfo.Create(ChessPosition.E8, ChessPosition.G8);
                    }
                    if ((board.CastleRights & CastleFlags.BlackLong) != 0
                        && board.PieceAt(ChessPosition.E8) == Piece.BKing
                        && board.PieceAt(ChessPosition.A8) == Piece.BRook
                        && board.PieceAt(ChessPosition.B8) == Piece.EMPTY
                        && board.PieceAt(ChessPosition.C8) == Piece.EMPTY
                        && board.PieceAt(ChessPosition.D8) == Piece.EMPTY
                        && !board.PositionAttacked(ChessPosition.E8, Player.White)
                        && !board.PositionAttacked(ChessPosition.D8, Player.White)
                        && !board.PositionAttacked(ChessPosition.C8, Player.White))
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

            ChessPosition targetpos;
            Piece targetpiece;

            foreach (ChessPosition piecepos in ChessPositionInfo.AllPositions)
            {
                Piece piece = board.PieceAt(piecepos);
                if (piece == Piece.EMPTY) { continue; }

                //knight attacks
                if (piece == myknight)
                {
                    foreach (Direction dir in DirectionInfo.AllDirectionsKnight)
                    {
                        AddDirection(retval, board, piecepos, dir, board.WhosTurn, 1, CapsOnly);
                    }
                    continue;
                }
                //bishop attacks
                if (piece == mybishop)
                {
                    foreach (Direction dir in DirectionInfo.AllDirectionsBishop)
                    {
                        AddDirection(retval, board, piecepos, dir, board.WhosTurn, 8, CapsOnly);
                    }
                    continue;
                }
                //rook attacks
                if (piece == myrook)
                {
                    foreach (Direction dir in DirectionInfo.AllDirectionsRook)
                    {
                        AddDirection(retval, board, piecepos, dir, board.WhosTurn, 8, CapsOnly);
                    }
                    continue;
                }
                //queen attacks
                if (piece == myqueen)
                {
                    foreach (Direction dir in DirectionInfo.AllDirectionsQueen)
                    {
                        AddDirection(retval, board, piecepos, dir, board.WhosTurn, 8, CapsOnly);
                    }
                    continue;
                }
                //king attacks
                if (piece == myking)
                {
                    foreach (Direction dir in DirectionInfo.AllDirectionsQueen)
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
                            if (targetpos.ToRank() == myrank8)
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
                            if (targetpos.ToRank() == myrank8)
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
                        if (targetpiece == Piece.EMPTY)
                        {
                            if (targetpos.ToRank() == myrank8)
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
                            if (piecepos.ToRank() == myrank2)
                            {
                                targetpos = targetpos.PositionInDirection(mypawnnorth);
                                targetpiece = board.PieceAt(targetpos);
                                if (targetpiece == Piece.EMPTY)
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
                if (board.WhosTurn == Player.White)
                {
                    if ((board.CastleRights & CastleFlags.WhiteShort) != 0
                        && board.PieceAt(ChessPosition.E1) == Piece.WKing
                        && board.PieceAt(ChessPosition.H1) == Piece.WRook
                        && board.PieceAt(ChessPosition.F1) == Piece.EMPTY
                        && board.PieceAt(ChessPosition.G1) == Piece.EMPTY
                        && !board.PositionAttacked(ChessPosition.E1, Player.Black)
                        && !board.PositionAttacked(ChessPosition.F1, Player.Black)
                        && !board.PositionAttacked(ChessPosition.G1, Player.Black))
                    {
                        retval.Add(ChessMoveInfo.Create(ChessPosition.E1, ChessPosition.G1));
                    }
                    if ((board.CastleRights & CastleFlags.WhiteLong) != 0
                        && board.PieceAt(ChessPosition.E1) == Piece.WKing
                        && board.PieceAt(ChessPosition.A1) == Piece.WRook
                        && board.PieceAt(ChessPosition.B1) == Piece.EMPTY
                        && board.PieceAt(ChessPosition.C1) == Piece.EMPTY
                        && board.PieceAt(ChessPosition.D1) == Piece.EMPTY
                        && !board.PositionAttacked(ChessPosition.E1, Player.Black)
                        && !board.PositionAttacked(ChessPosition.D1, Player.Black)
                        && !board.PositionAttacked(ChessPosition.C1, Player.Black))
                    {
                        retval.Add(ChessMoveInfo.Create(ChessPosition.E1, ChessPosition.C1));
                    }
                }
                else
                {
                    if ((board.CastleRights & CastleFlags.BlackShort) != 0
                        && board.PieceAt(ChessPosition.E8) == Piece.BKing
                        && board.PieceAt(ChessPosition.H8) == Piece.BRook
                        && board.PieceAt(ChessPosition.F8) == Piece.EMPTY
                        && board.PieceAt(ChessPosition.G8) == Piece.EMPTY
                        && !board.PositionAttacked(ChessPosition.E8, Player.White)
                        && !board.PositionAttacked(ChessPosition.F8, Player.White)
                        && !board.PositionAttacked(ChessPosition.G8, Player.White))
                    {
                        retval.Add(ChessMoveInfo.Create(ChessPosition.E8, ChessPosition.G8));
                    }
                    if ((board.CastleRights & CastleFlags.BlackLong) != 0
                        && board.PieceAt(ChessPosition.E8) == Piece.BKing
                        && board.PieceAt(ChessPosition.A8) == Piece.BRook
                        && board.PieceAt(ChessPosition.B8) == Piece.EMPTY
                        && board.PieceAt(ChessPosition.C8) == Piece.EMPTY
                        && board.PieceAt(ChessPosition.D8) == Piece.EMPTY
                        && !board.PositionAttacked(ChessPosition.E8, Player.White)
                        && !board.PositionAttacked(ChessPosition.D8, Player.White)
                        && !board.PositionAttacked(ChessPosition.C8, Player.White))
                    {
                        retval.Add(ChessMoveInfo.Create(ChessPosition.E8, ChessPosition.C8));
                    }

                }
            }

            return retval;

        }
        public static IEnumerable<ChessMove> GenMovesOld2(ChessBoard board, bool CapsOnly)
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



            //pawn caps
            Bitboard pawnTargets = board[board.WhosTurn.PlayerOther()] | (board.EnPassant.IsInBounds() ? board.EnPassant.ToBitboard() : 0);
            foreach (Direction capDir in new Direction[] { mypawneast, mypawnwest })
            {
                attacks = board[mypawn].Shift(capDir) & pawnTargets;
                while (attacks != Bitboard.Empty)
                {
                    ChessPosition targetpos = BitboardInfo.PopFirst(ref attacks);
                    ChessPosition piecepos = targetpos.PositionInDirectionUnsafe(capDir.Opposite());
                    if (targetpos.ToRank() == myrank8)
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

            Bitboard targetLocations = board[board.WhosTurn.PlayerOther()];
            if (!CapsOnly)
            {
                targetLocations |= ~board.PieceLocationsAll;
            }

            //loop through all non pawn locations
            Bitboard piecePositions = board[board.WhosTurn] & ~board[ChessPieceType.Pawn];
            while (piecePositions != Bitboard.Empty)// (ChessPosition piecepos in (board[board.WhosTurn] & ~board[ChessPieceType.Pawn]).ToPositions())
            {
                ChessPosition piecepos = BitboardInfo.PopFirst(ref piecePositions);
                Piece piece = board.PieceAt(piecepos);
                ChessPieceType pieceType = piece.ToPieceType();
                switch (pieceType)
                {
                    case ChessPieceType.Knight:
                        attacks = Attacks.KnightAttacks(piecepos) & targetLocations;
                        break;
                    case ChessPieceType.Bishop:
                        attacks = Attacks.BishopAttacks(piecepos, board.PieceLocationsAll) & targetLocations;
                        break;
                    case ChessPieceType.Rook:
                        attacks = Attacks.RookAttacks(piecepos, board.PieceLocationsAll) & targetLocations;
                        break;
                    case ChessPieceType.Queen:
                        attacks = Attacks.QueenAttacks(piecepos, board.PieceLocationsAll) & targetLocations;
                        break;
                    case ChessPieceType.King:
                        attacks = Attacks.KingAttacks(piecepos) & targetLocations;
                        break;
                }
                while (attacks != Bitboard.Empty)
                {
                    ChessPosition attackPos = BitboardInfo.PopFirst(ref attacks);
                    yield return ChessMoveInfo.Create(piecepos, attackPos);
                }
            }





            if (!CapsOnly)
            {
                //pawn jumps
                attacks = (board[mypawn].Shift(mypawnnorth) & ~board.PieceLocationsAll);
                while (attacks != Bitboard.Empty)
                {
                    ChessPosition targetpos = BitboardInfo.PopFirst(ref attacks);
                    ChessPosition piecepos = targetpos.PositionInDirectionUnsafe(mypawnnorth.Opposite());
                    if (targetpos.ToRank() == myrank8)
                    {
                        yield return ChessMoveInfo.Create(piecepos, targetpos, myqueen);
                        yield return ChessMoveInfo.Create(piecepos, targetpos, myrook);
                        yield return ChessMoveInfo.Create(piecepos, targetpos, mybishop);
                        yield return ChessMoveInfo.Create(piecepos, targetpos, myknight);
                    }
                    else
                    {
                        yield return ChessMoveInfo.Create(piecepos, targetpos);
                        if (piecepos.ToRank() == myrank2)
                        {
                            var doubleJumpPos = targetpos.PositionInDirectionUnsafe(mypawnnorth);
                            if (board.PieceAt(doubleJumpPos) == Piece.EMPTY)
                            {
                                yield return ChessMoveInfo.Create(piecepos, doubleJumpPos);
                            }
                        }
                    }
                }

                //castling
                if (board.WhosTurn == Player.White)
                {
                    if ((board.CastleRights & CastleFlags.WhiteShort) != 0
                        && board.PieceAt(ChessPosition.E1) == Piece.WKing
                        && board.PieceAt(ChessPosition.H1) == Piece.WRook
                        && board.PieceAt(ChessPosition.F1) == Piece.EMPTY
                        && board.PieceAt(ChessPosition.G1) == Piece.EMPTY
                        && !board.PositionAttacked(ChessPosition.E1, Player.Black)
                        && !board.PositionAttacked(ChessPosition.F1, Player.Black)
                        && !board.PositionAttacked(ChessPosition.G1, Player.Black))
                    {
                        yield return ChessMoveInfo.Create(ChessPosition.E1, ChessPosition.G1);
                    }
                    if ((board.CastleRights & CastleFlags.WhiteLong) != 0
                        && board.PieceAt(ChessPosition.E1) == Piece.WKing
                        && board.PieceAt(ChessPosition.A1) == Piece.WRook
                        && board.PieceAt(ChessPosition.B1) == Piece.EMPTY
                        && board.PieceAt(ChessPosition.C1) == Piece.EMPTY
                        && board.PieceAt(ChessPosition.D1) == Piece.EMPTY
                        && !board.PositionAttacked(ChessPosition.E1, Player.Black)
                        && !board.PositionAttacked(ChessPosition.D1, Player.Black)
                        && !board.PositionAttacked(ChessPosition.C1, Player.Black))
                    {
                        yield return ChessMoveInfo.Create(ChessPosition.E1, ChessPosition.C1);
                    }
                }
                else
                {
                    if ((board.CastleRights & CastleFlags.BlackShort) != 0
                        && board.PieceAt(ChessPosition.E8) == Piece.BKing
                        && board.PieceAt(ChessPosition.H8) == Piece.BRook
                        && board.PieceAt(ChessPosition.F8) == Piece.EMPTY
                        && board.PieceAt(ChessPosition.G8) == Piece.EMPTY
                        && !board.PositionAttacked(ChessPosition.E8, Player.White)
                        && !board.PositionAttacked(ChessPosition.F8, Player.White)
                        && !board.PositionAttacked(ChessPosition.G8, Player.White))
                    {
                        yield return ChessMoveInfo.Create(ChessPosition.E8, ChessPosition.G8);
                    }
                    if ((board.CastleRights & CastleFlags.BlackLong) != 0
                        && board.PieceAt(ChessPosition.E8) == Piece.BKing
                        && board.PieceAt(ChessPosition.A8) == Piece.BRook
                        && board.PieceAt(ChessPosition.B8) == Piece.EMPTY
                        && board.PieceAt(ChessPosition.C8) == Piece.EMPTY
                        && board.PieceAt(ChessPosition.D8) == Piece.EMPTY
                        && !board.PositionAttacked(ChessPosition.E8, Player.White)
                        && !board.PositionAttacked(ChessPosition.D8, Player.White)
                        && !board.PositionAttacked(ChessPosition.C8, Player.White))
                    {
                        yield return ChessMoveInfo.Create(ChessPosition.E8, ChessPosition.C8);
                    }

                }
            }

        }


        private static void AddDirection(ChessMoves retval, ChessBoard board, ChessPosition from, Direction dir, Player forwho, int maxdist, bool CapsOnly)
        {
            ChessPosition to = from.PositionInDirection(dir);
            int i = 1;
            while (to.IsInBounds() && i <= maxdist)
            {
                Piece targetpiece = board.PieceAt(to);
                if (targetpiece == Piece.EMPTY)
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
