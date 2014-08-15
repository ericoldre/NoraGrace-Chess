using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoraGrace.Engine
{
    public enum Rank
    {
        Rank8 = 0, 
        Rank7 = 1, 
        Rank6 = 2, 
        Rank5 = 3, 
        Rank4 = 4, 
        Rank3 = 5, 
        Rank2 = 6, 
        Rank1 = 7,
        EMPTY = 8
    }

    public static class RankUtil
    {
        private static readonly string _rankdesclookup = "87654321";

        private static readonly Rank[] _allRanks = new Rank[] { Rank.Rank8, Rank.Rank7, Rank.Rank6, Rank.Rank5, Rank.Rank4, Rank.Rank3, Rank.Rank2, Rank.Rank1 };
        public static Rank[] AllRanks
        {
            get
            {
                return _allRanks;
            }
        }
        public static Rank Parse(char c)
        {
            int idx = _rankdesclookup.IndexOf(c);
            if (idx < 0) { throw new ArgumentException(c.ToString() + " is not a valid rank"); }
            return (Rank)idx;
        }
        
        public static string RankToString(this Rank rank)
        {
            //AssertRank(rank);
            return _rankdesclookup.Substring((int)rank, 1);
        }

        public static bool IsInBounds(this Rank rank)
        {
            return (int)rank >= 0 && (int)rank <= 7;
        }

        public static Position ToPosition(this Rank rank, File file)
        {
            //if (!IsValidFile(file)) { return ChessPosition.OUTOFBOUNDS; }
            //if (!IsValidRank(rank)) { return ChessPosition.OUTOFBOUNDS; }
            return (Position)((int)rank * 8) + (int)file;
        }

        public static Bitboard ToBitboard(this Rank rank)
        {
            switch (rank)
            {
                case Rank.Rank1:
                    return Bitboard.Rank1;
                case Rank.Rank2:
                    return Bitboard.Rank2;
                case Rank.Rank3:
                    return Bitboard.Rank3;
                case Rank.Rank4:
                    return Bitboard.Rank4;
                case Rank.Rank5:
                    return Bitboard.Rank5;
                case Rank.Rank6:
                    return Bitboard.Rank6;
                case Rank.Rank7:
                    return Bitboard.Rank7;
                case Rank.Rank8:
                    return Bitboard.Rank8;
                default:
                    return 0;
            }
        }

        public static Bitboard BitboardAllNorth(this Rank rank)
        {
            switch (rank)
            {
                case Rank.Rank1:
                    return Bitboard.Full;
                case Rank.Rank2:
                    return Bitboard.Rank2 | Bitboard.Rank3 | Bitboard.Rank4 | Bitboard.Rank5 | Bitboard.Rank6 | Bitboard.Rank7 | Bitboard.Rank8;
                case Rank.Rank3:
                    return Bitboard.Rank3 | Bitboard.Rank4 | Bitboard.Rank5 | Bitboard.Rank6 | Bitboard.Rank7 | Bitboard.Rank8;
                case Rank.Rank4:
                    return Bitboard.Rank4 | Bitboard.Rank5 | Bitboard.Rank6 | Bitboard.Rank7 | Bitboard.Rank8;
                case Rank.Rank5:
                    return Bitboard.Rank5 | Bitboard.Rank6 | Bitboard.Rank7 | Bitboard.Rank8;
                case Rank.Rank6:
                    return Bitboard.Rank6 | Bitboard.Rank7 | Bitboard.Rank8;
                case Rank.Rank7:
                    return Bitboard.Rank7 | Bitboard.Rank8;
                case Rank.Rank8:
                    return Bitboard.Rank8;
                default:
                    return 0;
            }
        }

        public static Bitboard BitboardAllSouth(this Rank rank)
        {
            switch (rank)
            {
                case Rank.Rank1:
                    return Bitboard.Rank1;
                case Rank.Rank2:
                    return Bitboard.Rank1 | Bitboard.Rank2;
                case Rank.Rank3:
                    return Bitboard.Rank1 | Bitboard.Rank2 | Bitboard.Rank3;
                case Rank.Rank4:
                    return Bitboard.Rank1 | Bitboard.Rank2 | Bitboard.Rank3 | Bitboard.Rank4;
                case Rank.Rank5:
                    return Bitboard.Rank1 | Bitboard.Rank2 | Bitboard.Rank3 | Bitboard.Rank4 | Bitboard.Rank5;
                case Rank.Rank6:
                    return Bitboard.Rank1 | Bitboard.Rank2 | Bitboard.Rank3 | Bitboard.Rank4 | Bitboard.Rank5 | Bitboard.Rank6;
                case Rank.Rank7:
                    return Bitboard.Rank1 | Bitboard.Rank2 | Bitboard.Rank3 | Bitboard.Rank4 | Bitboard.Rank5 | Bitboard.Rank6  | Bitboard.Rank7;
                case Rank.Rank8:
                    return Bitboard.Full;
                default:
                    return 0;
            }
        }

    }
}
