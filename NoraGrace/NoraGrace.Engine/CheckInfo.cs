using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoraGrace.Engine
{
    public class CheckInfo
    {
        public long Zobrist { get; private set; }
        public Player Player { get; private set; }

        public Position KingPosition { get; private set; }

        public Bitboard PawnDirect { get; private set; }
        public Bitboard KnightDirect { get; private set; }
        public Bitboard BishopDirect { get; private set; }
        public Bitboard RookDirect { get; private set; }
        public Bitboard DirectAll { get; private set; }
        public Bitboard PinnedOrDiscovered { get; private set; }
        public Bitboard Checkers { get; private set; }
        public bool IsCheck { get { return Checkers != Bitboard.Empty; } }

        public CheckInfo(Player player)
        {
            this.Player = player;
        }

        public bool IsInitialized(Board board)
        {
            return board.ZobristBoard == Zobrist;
        }

        public void Initialize(Board board)
        {
            if (board.ZobristBoard == Zobrist) { return; }

            this.Zobrist = board.ZobristBoard;

            CheckInfo retval = this;
            Position kingPos = board.KingPosition(Player);
            Bitboard all = board.PieceLocationsAll;
            Bitboard them = board[Player.PlayerOther()];

            retval.KingPosition = kingPos;

            retval.PawnDirect = Attacks.PawnAttacks(kingPos, Player);
            retval.KnightDirect = Attacks.KnightAttacks(kingPos);
            retval.BishopDirect = Attacks.BishopAttacks(kingPos, all);
            retval.RookDirect = Attacks.RookAttacks(kingPos, all);
            retval.DirectAll = retval.PawnDirect | retval.KnightDirect | retval.BishopDirect | retval.RookDirect;
            
            //find pieces currently checking king
            Bitboard checkers = Bitboard.Empty;
            checkers |= PawnDirect & them & board[PieceType.Pawn];
            checkers |= KnightDirect & them & board[PieceType.Knight];
            checkers |= BishopDirect & them & board.BishopSliders;
            checkers |= RookDirect & them & board.RookSliders;
            this.Checkers = checkers;


            retval.PinnedOrDiscovered = Bitboard.Empty;


            Bitboard theirRookSliders = them & board.RookSliders;
            Bitboard theirBishopSliders = them & board.BishopSliders;

            Bitboard xray = Attacks.RookAttacks(kingPos, theirRookSliders) & theirRookSliders;
            xray |= Attacks.BishopAttacks(kingPos, theirBishopSliders) & theirBishopSliders;

            while (xray != Bitboard.Empty)
            {
                Position xrayAttacker = BitboardUtil.PopFirst(ref xray);
                Bitboard between = xrayAttacker.Between(kingPos) & all;

                if (between != Bitboard.Empty)
                {
                    //there are piece(s) between slider and king
                    Position blocker = BitboardUtil.PopFirst(ref between);
                    if (between == Bitboard.Empty)
                    {
                        //only one piece inbetween xrayAttacker and king, it is pinned, or a discovered attack
                        retval.PinnedOrDiscovered |= blocker.ToBitboard();
                    }
                }
                else
                {
                    System.Diagnostics.Debug.Assert(checkers.Contains(xrayAttacker));
                }
                

            }
        }
    }
}
