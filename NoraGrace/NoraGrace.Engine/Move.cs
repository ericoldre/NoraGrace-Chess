using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
namespace NoraGrace.Engine
{



    [System.Diagnostics.DebuggerDisplay(@"{NoraGrace.Engine.MoveUtil.DebugDescription(this),nq}")]
    public enum Move
    {
        EMPTY = 0
    }

    public static partial class MoveUtil
    {

        public enum NotationType
        {
            Coord,
            San,
            Detailed
        }

        public static Move Create(Position from, Position to, Piece piece, Piece captured)
        {
            System.Diagnostics.Debug.Assert((int)from >= 0 && (int)from <= 63);
            System.Diagnostics.Debug.Assert((int)to >= 0 && (int)to <= 63);
            System.Diagnostics.Debug.Assert((int)piece == ((int)piece & 0xF));
            System.Diagnostics.Debug.Assert((int)captured == ((int)captured & 0xF));
            System.Diagnostics.Debug.Assert(captured == Piece.EMPTY || (piece.PieceToPlayer() != captured.PieceToPlayer())); //if capture is opposite color
            System.Diagnostics.Debug.Assert(piece.ToPieceType() != PieceType.Pawn || !(Bitboard.Rank1 | Bitboard.Rank8).Contains(to)); //is not pawn to 8th

            return (Move)(
                (int)from
                | ((int)to << 6)
                | ((int)piece << 12)
                | ((int)captured << 16)
                );
        }

        public static Move Create(Position from, Position to, Piece piece, Piece captured, PieceType promoteType)
        {
            System.Diagnostics.Debug.Assert((int)from >= 0 && (int)from <= 63);
            System.Diagnostics.Debug.Assert((int)to >= 0 && (int)to <= 63);
            System.Diagnostics.Debug.Assert((int)piece == ((int)piece & 0xF));
            System.Diagnostics.Debug.Assert((int)captured == ((int)captured & 0xF));
            System.Diagnostics.Debug.Assert(captured == Piece.EMPTY || (piece.PieceToPlayer() != captured.PieceToPlayer())); //if capture is opposite color
            System.Diagnostics.Debug.Assert(promoteType == PieceType.Knight || promoteType == PieceType.Bishop || promoteType == PieceType.Rook || promoteType == PieceType.Queen);
            System.Diagnostics.Debug.Assert(piece.ToPieceType() == PieceType.Pawn);             //is pawn 
            System.Diagnostics.Debug.Assert((Bitboard.Rank1 | Bitboard.Rank8).Contains(to));    //to 8th
            System.Diagnostics.Debug.Assert((Bitboard.Rank2 | Bitboard.Rank7).Contains(from));  //from 7th


            return (Move)(
                (int)from
                | ((int)to << 6)
                | ((int)piece << 12)
                | ((int)captured << 16)
                | ((int)promoteType << 20)
                );
        }

        public static Position From(this Move move)
        {
            return (Position)((int)move & 0x3F);
        }

        public static Position To(this Move move)
        {
            return (Position)((int)move >> 6 & 0x3F);
        }

        public static Piece MovingPiece(this Move move)
        {
            return (Piece)((int)move >> 12 & 0xF);
        }

        public static PieceType MovingPieceType(this Move move)
        {
            return (PieceType)((int)move >> 12 & 0x7);
        }

        public static Player MovingPlayer(this Move move)
        {
            System.Diagnostics.Debug.Assert(move != Move.EMPTY);
            return (Player)((int)move >> 15 & 0x1); // shift of the moving piece to player bit.
        }

        public static bool IsCapture(this Move move)
        {
            return ((int)move >> 16 & 0xF) != 0;
        }

        public static Piece CapturedPiece(this Move move)
        {
            return (Piece)((int)move >> 16 & 0xF);
        }

        public static PieceType CapturedPieceType(this Move move)
        {
            return (PieceType)((int)move >> 16 & 0x7);
        }

        public static bool IsPromotion(this Move move)
        {
            return ((int)move >> 20 & 0x7) != 0;
        }

        public static Piece Promote(this Move move)
        {
            return move.PromoteType() == PieceType.EMPTY ? Piece.EMPTY : move.PromoteType().ForPlayer(move.MovingPlayer());
        }

        public static PieceType PromoteType(this Move move)
        {
            return (PieceType)((int)move >> 20 & 0x7);
        }

        public static bool IsEnPassant(this Move move)
        {
            return move.MovingPieceType() == PieceType.Pawn
                && !move.IsCapture()
                && move.From().ToFile() != move.To().ToFile();
        }

        public static bool IsCastle(this Move move)
        {
            return move.MovingPieceType() == PieceType.King && Math.Abs((int)move.From() - (int)move.To()) == 2;
        }

        public static bool IsPawnDoubleJump(this Move move)
        {
            return move.MovingPieceType() == PieceType.Pawn && Math.Abs((int)move.From() - (int)move.To()) == 16;
        }



        public static bool IsLegal(this Move move, Board board)
        {
            List<Move> legalmoves = new List<Move>(MoveUtil.GenMovesLegal(board));
            foreach (Move legalmove in legalmoves)
            {
                if (legalmove == move) { return true; }
            }
            return false;
        }

        public static bool IsLegalPsuedoMove(this Move move, Board board, AttackInfo opponentAttackInfo, CheckInfo myCheckInfo)
        {
            System.Diagnostics.Debug.Assert(opponentAttackInfo.IsInitialized(board));
            System.Diagnostics.Debug.Assert(myCheckInfo.IsInitialized(board));
            System.Diagnostics.Debug.Assert(move.MovingPlayer() == myCheckInfo.Player);
            System.Diagnostics.Debug.Assert(move.MovingPlayer().PlayerOther() == opponentAttackInfo.Player);


            if (myCheckInfo.IsCheck)
            {
                Bitboard checkers = myCheckInfo.Checkers;

                if (move.MovingPieceType() == PieceType.King)
                {
                    //in check, king evasions
                    if (opponentAttackInfo.All.Contains(move.To()))
                    {
                        return false;
                    }
                    else
                    {
                        Bitboard adjustedAttacks = Bitboard.Empty;
                        while (checkers != Bitboard.Empty)
                        {
                            //we could be moving to/from a sliding attack.
                            var checkerPos = BitboardUtil.PopFirst(ref checkers);
                            var checkerType = board.PieceAt(checkerPos).ToPieceType();
                            switch (checkerType)
                            {
                                case PieceType.Bishop:
                                    adjustedAttacks |= Attacks.BishopAttacks(checkerPos, board.PieceLocationsAll ^ move.From().ToBitboard());
                                    break;
                                case PieceType.Rook:
                                    adjustedAttacks |= Attacks.RookAttacks(checkerPos, board.PieceLocationsAll ^ move.From().ToBitboard());
                                    break;
                                case PieceType.Queen:
                                    adjustedAttacks |= Attacks.QueenAttacks(checkerPos, board.PieceLocationsAll ^ move.From().ToBitboard());
                                    break;
                            }
                        }
                        return !adjustedAttacks.Contains(move.To());
                    }
                }
                else
                {
                    //in check, non king move.

                    //find position of first checker
                    var checkerPos = BitboardUtil.PopFirst(ref checkers);


                    if (checkers != Bitboard.Empty) 
                    { 
                        //there are two attackers, no sense moving a non-king
                        return false; 
                    }
                    if (myCheckInfo.PinnedOrDiscovered.Contains(move.From())) 
                    {
                        // this piece is pinned, no sense moving it
                        return false; 
                    } 

                    //find squares between king and checker (or checker itself)
                    Bitboard targets = checkerPos.Between(board.KingPosition(board.WhosTurn)) | checkerPos.ToBitboard();


                    return targets.Contains(move.To()) || move.IsEnPassant();//if enpassant is available and we are in check, then must be enpassant check

                    
                }
            }
            else 
            {
                //not currently in check
                if(move.MovingPieceType() == PieceType.King)
                {
                    return !opponentAttackInfo.All.Contains(move.To());    
                }
                else if(myCheckInfo.PinnedOrDiscovered.Contains(move.From()))
                {
                    //pinned piece
                    var dirToKing = move.From().DirectionTo(board.KingPosition(move.MovingPlayer()));
                    var dirMoving = move.From().DirectionTo(move.To());
                    return dirMoving == dirToKing || dirMoving == dirToKing.Opposite();
                }
                else
                {
                    //normal piece, not pinned, not king
                    return true;
                }
                
            }

        }

        public static bool IsPsuedoLegal(this Move move, Board board)
        {
            if (move == Move.EMPTY) { return false; }

            var from = move.From();
            var to = move.To();
            
            if (move.MovingPiece() != board.PieceAt(from)) { return false; }
            if (move.CapturedPiece() != board.PieceAt(to)) { return false; }

            var piece = move.MovingPiece();

            if (piece == Piece.EMPTY) { return false; }
            var pieceType = piece.ToPieceType();
            var me = piece.PieceToPlayer();

            if (board.WhosTurn != me) { return false; }
            if (board[me].Contains(to)) { return false; }

            if (move.IsPromotion())
            {
                //these should be verified in move constructor, but put check here just in case.
                System.Diagnostics.Debug.Assert(pieceType == PieceType.Pawn);
                System.Diagnostics.Debug.Assert(to.ToRank() == me.MyRank(Rank.Rank8));
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
                    if (move.IsCastle())
                    {
                        if (me == Player.White
                            && from == Position.E1
                            && to == Position.G1
                            && board.PieceAt(Position.F1) == Piece.EMPTY
                            && board.PieceAt(Position.G1) == Piece.EMPTY
                            && board.PieceAt(Position.H1) == Piece.WRook
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
                            && board.PieceAt(Position.A1) == Piece.WRook
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
                            && board.PieceAt(Position.H8) == Piece.BRook
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
                            && board.PieceAt(Position.A8) == Piece.BRook
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
                    }
                    else if ((Attacks.KingAttacks(from) & targets).Contains(to))
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

                    if(move.IsEnPassant())
                    {
                        return to == board.EnPassant;
                    }
                    else if(move.IsCapture())
                    {
                        //already verified that attacking spot correct
                        return Attacks.PawnAttacks(from, me).Contains(to);
                    }
                    else if(move.IsPawnDoubleJump())
                    {
                        //already verified start and end spot validity.
                        return board.PieceAt(move.From().PositionInDirectionUnsafe(me.MyNorth())) == Piece.EMPTY;
                    }
                    else 
                    {
                        return true;
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
            PieceType pieceType = move.MovingPieceType();

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

        public static bool CausesCheck(this Move move, Board board, CheckInfo oppenentCheckInfo)
        {
            System.Diagnostics.Debug.Assert(oppenentCheckInfo.IsInitialized(board));
            System.Diagnostics.Debug.Assert(move.MovingPlayer().PlayerOther() == oppenentCheckInfo.Player);

            Position from = move.From();
            Position to = move.To();
            PieceType pieceType = move.MovingPieceType();

            if (move.IsPromotion())
            {
                PieceType pieceTypeAfter = move.IsPromotion() ? move.PromoteType() : pieceType; //in case of promotion
                Bitboard promoteAttacks = Bitboard.Empty;
                Bitboard allPiecesAfter = board.PieceLocationsAll ^ move.From().ToBitboard() ^ move.To().ToBitboard();
                switch (pieceTypeAfter)
                {
                    case PieceType.Knight:
                        promoteAttacks = Attacks.KnightAttacks(to);
                        break;
                    case PieceType.Bishop:
                        promoteAttacks = Attacks.BishopAttacks(to, allPiecesAfter);
                        break;
                    case PieceType.Rook:
                        promoteAttacks = Attacks.RookAttacks(to, allPiecesAfter);
                        break;
                    case PieceType.Queen:
                        promoteAttacks = Attacks.QueenAttacks(to, allPiecesAfter);
                        break;
                }
                if (promoteAttacks.Contains(board.KingPosition(board.WhosTurn.PlayerOther())))
                {
                    return true;
                }
            }
            if (oppenentCheckInfo.DirectAll.Contains(to))
            {
                bool direct = false;
                switch (pieceType)
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

            if (oppenentCheckInfo.PinnedOrDiscovered.Contains(from) || move.IsEnPassant())
            {
                Bitboard allAfterMove = (board.PieceLocationsAll & ~from.ToBitboard()) | to.ToBitboard();

                if (move.IsEnPassant())
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
            if (move.IsCastle())
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

        public static int MVVLVA(this Move move)
        {
            System.Diagnostics.Debug.Assert(move.IsCapture() || move.IsEnPassant());

            var pieceval = (int)move.MovingPieceType();
            var capval = move.IsEnPassant() ? (int)PieceType.Pawn : (int)move.CapturedPieceType();

            return (capval << 3) | (6 - pieceval);
        }

        public static bool IsSafeQuiet(this Move move, Board board, StaticExchange see, AttackInfo myAttacks, AttackInfo hisAttacks)
        {
            System.Diagnostics.Debug.Assert(myAttacks.IsInitialized(board));
            System.Diagnostics.Debug.Assert(hisAttacks.IsInitialized(board));
            System.Diagnostics.Debug.Assert(myAttacks.Player == move.MovingPlayer());
            System.Diagnostics.Debug.Assert(hisAttacks.Player == move.MovingPlayer().PlayerOther());

            System.Diagnostics.Debug.Assert(!move.IsCapture() && !move.IsEnPassant());

            var piece = move.MovingPieceType();
            var cap = move.IsEnPassant() ? PieceType.Pawn : move.CapturedPieceType();

            var to = move.To().ToBitboard();

            if (!hisAttacks.All.Contains(to)) { return true; }

            if (hisAttacks.LessThan(piece).Contains(to)) { return false; }

            if (myAttacks.ByCount(2).Contains(to) && !hisAttacks.ByCount(2).Contains(to)) { return true; }

            //or maybe SEE here
            return false;

        }

        public static bool IsGoodCapture(this Move move, Board board, StaticExchange see, AttackInfo myAttacks, AttackInfo hisAttacks)
        {
            System.Diagnostics.Debug.Assert(myAttacks.IsInitialized(board));
            System.Diagnostics.Debug.Assert(hisAttacks.IsInitialized(board));
            System.Diagnostics.Debug.Assert(myAttacks.Player == move.MovingPlayer());
            System.Diagnostics.Debug.Assert(hisAttacks.Player == move.MovingPlayer().PlayerOther());

            System.Diagnostics.Debug.Assert(move.IsCapture() || move.IsEnPassant());

            var piece = move.MovingPieceType();
            var cap = move.IsEnPassant() ? PieceType.Pawn : move.CapturedPieceType();

            if (piece.BasicVal() >= cap.BasicVal()) { return true; }

            var to = move.To().ToBitboard();

            if (!hisAttacks.All.Contains(to)) { return true; }

            if (hisAttacks.LessThan(piece).Contains(to)) { return false; }

            if (myAttacks.ByCount(2).Contains(to))
            {
                return see.CalculateScore(board, move) >= 0;
            }
            else
            {
                return false;
            }



        }
    }


}

