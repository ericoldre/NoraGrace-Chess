using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{
    public enum ChessRankValue
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

    public struct ChessRank
    {
        public static readonly ChessRank Rank1 = new ChessRank(ChessRankValue.Rank1);
        public static readonly ChessRank Rank2 = new ChessRank(ChessRankValue.Rank2);
        public static readonly ChessRank Rank3 = new ChessRank(ChessRankValue.Rank3);
        public static readonly ChessRank Rank4 = new ChessRank(ChessRankValue.Rank4);
        public static readonly ChessRank Rank5 = new ChessRank(ChessRankValue.Rank5);
        public static readonly ChessRank Rank6 = new ChessRank(ChessRankValue.Rank6);
        public static readonly ChessRank Rank7 = new ChessRank(ChessRankValue.Rank7);
        public static readonly ChessRank Rank8 = new ChessRank(ChessRankValue.Rank8);
        public static readonly ChessRank EMPTY = new ChessRank(ChessRankValue.EMPTY);

        private static readonly string _rankdesclookup = "87654321";
        public readonly ChessRankValue Value;

        public ChessRank(ChessRankValue value)
        {
            Value = value;
        }

        #region operators

        public static implicit operator ChessRankValue(ChessRank rank)
        {
            return rank.Value;
        }
        public static implicit operator ChessRank(ChessRankValue value)
        {
            return new ChessRank(value);
        }

        public static explicit operator int(ChessRank Rank)
        {
            return (int)Rank.Value;
        }
        public static explicit operator ChessRank(int i)
        {
            return new ChessRank((ChessRankValue)i);
        }
        public static bool operator ==(ChessRank a, ChessRank b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(ChessRank a, ChessRank b)
        {
            return !(a == b);
        }

        //public static bool operator >(ChessRank a, ChessRank b)
        //{
        //    return a.Value > b.Value;
        //}
        //public static bool operator >=(ChessRank a, ChessRank b)
        //{
        //    return a.Value >= b.Value;
        //}
        //public static bool operator <(ChessRank a, ChessRank b)
        //{
        //    return a.Value < b.Value;
        //}
        //public static bool operator <=(ChessRank a, ChessRank b)
        //{
        //    return a.Value <= b.Value;
        //}


        #endregion

        public static ChessRank ParseAsRank(char c)
        {
            int idx = _rankdesclookup.IndexOf(c);
            if (idx < 0) { throw new ChessException(c.ToString() + " is not a valid rank"); }
            return (ChessRank)idx;
        }

        public string RankToString()
        {
            //AssertRank(rank);
            return _rankdesclookup.Substring((int)this, 1);
        }

        public bool IsInBounds()
        {
            return (int)this >= 0 && (int)this <= 7;
        }

        public ChessPosition ToPosition(ChessFile file)
        {
            //if (!IsValidFile(file)) { return ChessPosition.OUTOFBOUNDS; }
            //if (!IsValidRank(rank)) { return ChessPosition.OUTOFBOUNDS; }
            return (ChessPosition)((int)this * 8) + (int)file;
        }

        public ChessBitboard Bitboard()
        {
            switch (this.Value)
            {
                case ChessRankValue.Rank1:
                    return ChessBitboard.Rank1;
                case ChessRankValue.Rank2:
                    return ChessBitboard.Rank2;
                case ChessRankValue.Rank3:
                    return ChessBitboard.Rank3;
                case ChessRankValue.Rank4:
                    return ChessBitboard.Rank4;
                case ChessRankValue.Rank5:
                    return ChessBitboard.Rank5;
                case ChessRankValue.Rank6:
                    return ChessBitboard.Rank6;
                case ChessRankValue.Rank7:
                    return ChessBitboard.Rank7;
                case ChessRankValue.Rank8:
                    return ChessBitboard.Rank8;
                default:
                    return 0;
            }
        }

        public ChessBitboard BitboardAllNorth()
        {
            switch (this.Value)
            {
                case ChessRankValue.Rank1:
                    return ChessBitboard.Full;
                case ChessRankValue.Rank2:
                    return ChessBitboard.Rank2 | ChessBitboard.Rank3 | ChessBitboard.Rank4 | ChessBitboard.Rank5 | ChessBitboard.Rank6 | ChessBitboard.Rank7 | ChessBitboard.Rank8;
                case ChessRankValue.Rank3:
                    return ChessBitboard.Rank3 | ChessBitboard.Rank4 | ChessBitboard.Rank5 | ChessBitboard.Rank6 | ChessBitboard.Rank7 | ChessBitboard.Rank8;
                case ChessRankValue.Rank4:
                    return ChessBitboard.Rank4 | ChessBitboard.Rank5 | ChessBitboard.Rank6 | ChessBitboard.Rank7 | ChessBitboard.Rank8;
                case ChessRankValue.Rank5:
                    return ChessBitboard.Rank5 | ChessBitboard.Rank6 | ChessBitboard.Rank7 | ChessBitboard.Rank8;
                case ChessRankValue.Rank6:
                    return ChessBitboard.Rank6 | ChessBitboard.Rank7 | ChessBitboard.Rank8;
                case ChessRankValue.Rank7:
                    return ChessBitboard.Rank7 | ChessBitboard.Rank8;
                case ChessRankValue.Rank8:
                    return ChessBitboard.Rank8;
                default:
                    return 0;
            }
        }

    }
}
