using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoraGrace.Engine
{
    public struct CheckInfo
    {
        public Position KingPosition { get; private set; }

        public Bitboard PawnDirect { get; private set; }
        public Bitboard KnightDirect { get; private set; }
        public Bitboard BishopDirect { get; private set; }
        public Bitboard RookDirect { get; private set; }
        public Bitboard DirectAll { get; private set; }
        public Bitboard PinnedOrDiscovered { get; private set; }
        

        public static CheckInfo Generate(Board board, Player player)
        {
            CheckInfo retval = new CheckInfo();
            Position kingPos = board.KingPosition(player);
            Bitboard all = board.PieceLocationsAll;
            Bitboard them = board[ player.PlayerOther()];

            retval.KingPosition = kingPos;

            retval.PawnDirect = Attacks.PawnAttacks(kingPos, player);
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
                Position xrayAttacker = BitboardInfo.PopFirst(ref xray);
                Bitboard xrayBlockers = xrayAttacker.Between(kingPos) & all;

                Position xrayBlocker1 = BitboardInfo.PopFirst(ref xrayBlockers);
                if (xrayBlockers == Bitboard.Empty)
                {
                    //only one piece inbetween xrayAttacker and king
                    retval.PinnedOrDiscovered |= xrayBlocker1.ToBitboard();
                }

            }
            return retval;
        }
    }
}
