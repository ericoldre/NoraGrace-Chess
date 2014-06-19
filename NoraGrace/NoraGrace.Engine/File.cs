using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoraGrace.Engine
{
    public enum File
    {
        
        FileA = 0, FileB = 1, FileC = 2, FileD = 3, FileE = 4, FileF = 5, FileG = 6, FileH = 7,
        EMPTY = 8,
    }

    public static class FileInfo
    {
        private static readonly string _filedesclookup = "abcdefgh";
        public static readonly File[] AllFiles = new File[] { File.FileA, File.FileB, File.FileC, File.FileD, File.FileE, File.FileF, File.FileG, File.FileH };

        public static File Parse(char c)
        {
            int idx = _filedesclookup.IndexOf(c.ToString().ToLower());
            if (idx < 0) { throw new ArgumentException(c.ToString() + " is not a valid file"); }
            return (File)idx;
        }

        public static string FileToString(this File file)
        {
            //AssertFile(file);
            return _filedesclookup.Substring((int)file, 1);
        }

        public static bool IsInBounds(this File file)
        {
            return (int)file >= 0 && (int)file <= 7;
        }

        public static Position ToPosition(this File file, Rank rank)
        {
            return (Position)((int)rank * 8) + (int)file;
        }
        public static Bitboard ToBitboard(this File file)
        {
            switch (file)
            {
                case File.FileA:
                    return Bitboard.FileA;
                case File.FileB:
                    return Bitboard.FileB;
                case File.FileC:
                    return Bitboard.FileC;
                case File.FileD:
                    return Bitboard.FileD;
                case File.FileE:
                    return Bitboard.FileE;
                case File.FileF:
                    return Bitboard.FileF;
                case File.FileG:
                    return Bitboard.FileG;
                case File.FileH:
                    return Bitboard.FileH;
                default:
                    throw new ArgumentOutOfRangeException("file");
            }
        }
    }
}
