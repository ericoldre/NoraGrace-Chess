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

        public CheckInfo(Player player)
        {
            this.Player = player;
        }

        public void Initialize(Board board)
        {
            if (board.ZobristBoard == Zobrist) { return; }

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

            retval.PinnedOrDiscovered = Bitboard.Empty;


            Bitboard theirRookSliders = them & board.RookSliders;
            Bitboard theirBishopSliders = them & board.BishopSliders;

            Bitboard xray = Attacks.RookAttacks(kingPos, theirRookSliders) & theirRookSliders;
            xray |= Attacks.BishopAttacks(kingPos, theirBishopSliders) & theirBishopSliders;

            while (xray != Bitboard.Empty)
            {
                Position xrayAttacker = BitboardUtil.PopFirst(ref xray);
                Bitboard xrayBlockers = xrayAttacker.Between(kingPos) & all;

                Position xrayBlocker1 = BitboardUtil.PopFirst(ref xrayBlockers);
                if (xrayBlockers == Bitboard.Empty)
                {
                    //only one piece inbetween xrayAttacker and king
                    retval.PinnedOrDiscovered |= xrayBlocker1.ToBitboard();
                }

            }
        }
    }
}
