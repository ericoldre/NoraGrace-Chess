using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{
    public enum ChessRank
    {
        EMPTY = -1,
        Rank1 = 0, Rank2 = 1, Rank3 = 2, Rank4 = 3, Rank5 = 4, Rank6 = 5, Rank7 = 6, Rank8 = 7
    }

    public static class ExtensionsChessRank
    {
        private static readonly string _rankdesclookup = "12345678";

        public static ChessRank ParseAsRank(this char c)
        {
            int idx = _rankdesclookup.IndexOf(c);
            if (idx < 0) { throw new ChessException(c.ToString() + " is not a valid rank"); }
            return (ChessRank)idx;
        }

        public static string RankToString(this ChessRank rank)
        {
            //AssertRank(rank);
            return _rankdesclookup.Substring((int)rank, 1);
        }

        public static bool IsInBounds(this ChessRank rank)
        {
            return (int)rank >= 0 && (int)rank <= 7;
        }

        public static ChessPosition ToPosition(this ChessRank rank, ChessFile file)
        {
            //if (!IsValidFile(file)) { return ChessPosition.OUTOFBOUNDS; }
            //if (!IsValidRank(rank)) { return ChessPosition.OUTOFBOUNDS; }
            return (ChessPosition)((int)rank * 8) + (int)file;
        }

        public static ChessBitboard Bitboard(this ChessRank rank)
        {
            switch (rank)
            {
                case ChessRank.Rank1:
                    return ChessBitboard.Rank1;
                case ChessRank.Rank2:
                    return ChessBitboard.Rank2;
                case ChessRank.Rank3:
                    return ChessBitboard.Rank3;
                case ChessRank.Rank4:
                    return ChessBitboard.Rank4;
                case ChessRank.Rank5:
                    return ChessBitboard.Rank5;
                case ChessRank.Rank6:
                    return ChessBitboard.Rank6;
                case ChessRank.Rank7:
                    return ChessBitboard.Rank7;
                case ChessRank.Rank8:
                    return ChessBitboard.Rank8;
                default:
                    return 0;
            }
        }

        public static ChessBitboard BitboardAllNorth(this ChessRank rank)
        {
            switch (rank)
            {
                case ChessRank.Rank1:
                    return ChessBitboard.Full;
                case ChessRank.Rank2:
                    return ChessBitboard.Rank2 | ChessBitboard.Rank3 | ChessBitboard.Rank4 | ChessBitboard.Rank5 | ChessBitboard.Rank6 | ChessBitboard.Rank7 | ChessBitboard.Rank8;
                case ChessRank.Rank3:
                    return ChessBitboard.Rank3 | ChessBitboard.Rank4 | ChessBitboard.Rank5 | ChessBitboard.Rank6 | ChessBitboard.Rank7 | ChessBitboard.Rank8;
                case ChessRank.Rank4:
                    return ChessBitboard.Rank4 | ChessBitboard.Rank5 | ChessBitboard.Rank6 | ChessBitboard.Rank7 | ChessBitboard.Rank8;
                case ChessRank.Rank5:
                    return ChessBitboard.Rank5 | ChessBitboard.Rank6 | ChessBitboard.Rank7 | ChessBitboard.Rank8;
                case ChessRank.Rank6:
                    return ChessBitboard.Rank6 | ChessBitboard.Rank7 | ChessBitboard.Rank8;
                case ChessRank.Rank7:
                    return ChessBitboard.Rank7 | ChessBitboard.Rank8;
                case ChessRank.Rank8:
                    return ChessBitboard.Rank8;
                default:
                    return 0;
            }
        }

    }
}
