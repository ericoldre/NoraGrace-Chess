using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{
    public enum ChessRank
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

    public static class ChessRankInfo
    {
        private static readonly string _rankdesclookup = "87654321";

        private static readonly ChessRank[] _allRanks = new ChessRank[] { ChessRank.Rank8, ChessRank.Rank7, ChessRank.Rank6, ChessRank.Rank5, ChessRank.Rank4, ChessRank.Rank3, ChessRank.Rank2, ChessRank.Rank1 };
        public static ChessRank[] AllRanks
        {
            get
            {
                return _allRanks;
            }
        }
        public static ChessRank Parse(char c)
        {
            int idx = _rankdesclookup.IndexOf(c);
            if (idx < 0) { throw new ArgumentException(c.ToString() + " is not a valid rank"); }
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

        public static ChessBitboard BitboardAllSouth(this ChessRank rank)
        {
            switch (rank)
            {
                case ChessRank.Rank1:
                    return ChessBitboard.Rank1;
                case ChessRank.Rank2:
                    return ChessBitboard.Rank1 | ChessBitboard.Rank2;
                case ChessRank.Rank3:
                    return ChessBitboard.Rank1 | ChessBitboard.Rank2 | ChessBitboard.Rank3;
                case ChessRank.Rank4:
                    return ChessBitboard.Rank1 | ChessBitboard.Rank2 | ChessBitboard.Rank3 | ChessBitboard.Rank4;
                case ChessRank.Rank5:
                    return ChessBitboard.Rank1 | ChessBitboard.Rank2 | ChessBitboard.Rank3 | ChessBitboard.Rank4 | ChessBitboard.Rank5;
                case ChessRank.Rank6:
                    return ChessBitboard.Rank1 | ChessBitboard.Rank2 | ChessBitboard.Rank3 | ChessBitboard.Rank4 | ChessBitboard.Rank5 | ChessBitboard.Rank6;
                case ChessRank.Rank7:
                    return ChessBitboard.Rank1 | ChessBitboard.Rank2 | ChessBitboard.Rank3 | ChessBitboard.Rank4 | ChessBitboard.Rank5 | ChessBitboard.Rank6  | ChessBitboard.Rank7;
                case ChessRank.Rank8:
                    return ChessBitboard.Full;
                default:
                    return 0;
            }
        }

    }
}
