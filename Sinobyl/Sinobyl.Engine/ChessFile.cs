using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{
    public enum ChessFileValue
    {
        
        FileA = 0, FileB = 1, FileC = 2, FileD = 3, FileE = 4, FileF = 5, FileG = 6, FileH = 7,
        EMPTY = 8,
    }

    public struct ChessFile
    {
        private static readonly string _filedesclookup = "abcdefgh";

        public readonly ChessFileValue Value;

        public static readonly ChessFile FileA = new ChessFile(ChessFileValue.FileA);
        public static readonly ChessFile FileB = new ChessFile(ChessFileValue.FileB);
        public static readonly ChessFile FileC = new ChessFile(ChessFileValue.FileC);
        public static readonly ChessFile FileD = new ChessFile(ChessFileValue.FileD);
        public static readonly ChessFile FileE = new ChessFile(ChessFileValue.FileE);
        public static readonly ChessFile FileF = new ChessFile(ChessFileValue.FileF);
        public static readonly ChessFile FileG = new ChessFile(ChessFileValue.FileG);
        public static readonly ChessFile FileH = new ChessFile(ChessFileValue.FileH);
        public static readonly ChessFile EMPTY = new ChessFile(ChessFileValue.EMPTY);

        public ChessFile(ChessFileValue value)
        {
            Value = value;
        }

        #region operators

        public static implicit operator ChessFileValue(ChessFile piece)
        {
            return piece.Value;
        }
        public static implicit operator ChessFile(ChessFileValue value)
        {
            return new ChessFile(value);
        }

        public static explicit operator int(ChessFile file)
        {
            return (int)file.Value;
        }
        public static explicit operator ChessFile(int i)
        {
            return new ChessFile((ChessFileValue)i);
        }
        public static bool operator ==(ChessFile a, ChessFile b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(ChessFile a, ChessFile b)
        {
            return !(a == b);
        }

        public static bool operator >(ChessFile a, ChessFile b)
        {
            return a.Value > b.Value;
        }
        public static bool operator >=(ChessFile a, ChessFile b)
        {
            return a.Value >= b.Value;
        }
        public static bool operator <(ChessFile a, ChessFile b)
        {
            return a.Value < b.Value;
        }
        public static bool operator <=(ChessFile a, ChessFile b)
        {
            return a.Value <= b.Value;
        }


        #endregion


        public static ChessFile ParseAsFile(char c)
        {
            int idx = _filedesclookup.IndexOf(c.ToString().ToLower());
            if (idx < 0) { throw new ChessException(c.ToString() + " is not a valid file"); }
            return (ChessFile)idx;
        }

        public string FileToString()
        {
            //AssertFile(file);
            return _filedesclookup.Substring((int)this, 1);
        }

        public bool IsInBounds()
        {
            return (int)this >= 0 && (int)this <= 7;
        }

        public ChessPosition ToPosition(ChessRank rank)
        {
            return (ChessPosition)(((int)rank * 8) + (int)this);
        }

        public ChessBitboard Bitboard()
        {
            switch (this.Value)
            {
                case ChessFileValue.FileA:
                    return ChessBitboard.FileA;
                case ChessFileValue.FileB:
                    return ChessBitboard.FileB;
                case ChessFileValue.FileC:
                    return ChessBitboard.FileC;
                case ChessFileValue.FileD:
                    return ChessBitboard.FileD;
                case ChessFileValue.FileE:
                    return ChessBitboard.FileE;
                case ChessFileValue.FileF:
                    return ChessBitboard.FileF;
                case ChessFileValue.FileG:
                    return ChessBitboard.FileG;
                case ChessFileValue.FileH:
                    return ChessBitboard.FileH;
                default:
                    return 0;
            }
        }
    }
}
