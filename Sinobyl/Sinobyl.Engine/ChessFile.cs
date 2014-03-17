using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{
    public enum ChessFile
    {
        
        FileA = 0, FileB = 1, FileC = 2, FileD = 3, FileE = 4, FileF = 5, FileG = 6, FileH = 7,
        EMPTY = 8,
    }

    public static class ChessFileInfo
    {
        private static readonly string _filedesclookup = "abcdefgh";
        public static readonly ChessFile[] AllFiles = new ChessFile[] { ChessFile.FileA, ChessFile.FileB, ChessFile.FileC, ChessFile.FileD, ChessFile.FileE, ChessFile.FileF, ChessFile.FileG, ChessFile.FileH };

        public static ChessFile Parse(char c)
        {
            int idx = _filedesclookup.IndexOf(c.ToString().ToLower());
            if (idx < 0) { throw new ArgumentException(c.ToString() + " is not a valid file"); }
            return (ChessFile)idx;
        }

        public static string FileToString(this ChessFile file)
        {
            //AssertFile(file);
            return _filedesclookup.Substring((int)file, 1);
        }

        public static bool IsInBounds(this ChessFile file)
        {
            return (int)file >= 0 && (int)file <= 7;
        }

        public static ChessPosition ToPosition(this ChessFile file, ChessRank rank)
        {
            return (ChessPosition)((int)rank * 8) + (int)file;
        }
        public static ChessBitboard Bitboard(this ChessFile file)
        {
            switch (file)
            {
                case ChessFile.FileA:
                    return ChessBitboard.FileA;
                case ChessFile.FileB:
                    return ChessBitboard.FileB;
                case ChessFile.FileC:
                    return ChessBitboard.FileC;
                case ChessFile.FileD:
                    return ChessBitboard.FileD;
                case ChessFile.FileE:
                    return ChessBitboard.FileE;
                case ChessFile.FileF:
                    return ChessBitboard.FileF;
                case ChessFile.FileG:
                    return ChessBitboard.FileG;
                case ChessFile.FileH:
                    return ChessBitboard.FileH;
                default:
                    throw new ArgumentOutOfRangeException("file");
            }
        }
    }
}
