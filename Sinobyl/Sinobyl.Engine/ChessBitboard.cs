using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{
    [Flags]
    public enum ChessBitboard : ulong
    {
        A8 = (ulong)1 << 0, B8 = (ulong)1 << 1, C8 = (ulong)1 << 2, D8 = (ulong)1 << 3, E8 = (ulong)1 << 4, F8 = (ulong)1 << 5, G8 = (ulong)1 << 6, H8 = (ulong)1 << 7,
        A7 = (ulong)1 << 8, B7 = (ulong)1 << 9, C7 = (ulong)1 << 10, D7 = (ulong)1 << 11, E7 = (ulong)1 << 12, F7 = (ulong)1 << 13, G7 = (ulong)1 << 14, H7 = (ulong)1 << 15,
        A6 = (ulong)1 << 16, B6 = (ulong)1 << 17, C6 = (ulong)1 << 18, D6 = (ulong)1 << 19, E6 = (ulong)1 << 20, F6 = (ulong)1 << 21, G6 = (ulong)1 << 22, H6 = (ulong)1 << 23,
        A5 = (ulong)1 << 24, B5 = (ulong)1 << 25, C5 = (ulong)1 << 26, D5 = (ulong)1 << 27, E5 = (ulong)1 << 28, F5 = (ulong)1 << 29, G5 = (ulong)1 << 30, H5 = (ulong)1 << 31,
        A4 = (ulong)1 << 32, B4 = (ulong)1 << 33, C4 = (ulong)1 << 34, D4 = (ulong)1 << 35, E4 = (ulong)1 << 36, F4 = (ulong)1 << 37, G4 = (ulong)1 << 38, H4 = (ulong)1 << 39,
        A3 = (ulong)1 << 40, B3 = (ulong)1 << 41, C3 = (ulong)1 << 42, D3 = (ulong)1 << 43, E3 = (ulong)1 << 44, F3 = (ulong)1 << 45, G3 = (ulong)1 << 46, H3 = (ulong)1 << 47,
        A2 = (ulong)1 << 48, B2 = (ulong)1 << 49, C2 = (ulong)1 << 50, D2 = (ulong)1 << 51, E2 = (ulong)1 << 52, F2 = (ulong)1 << 53, G2 = (ulong)1 << 54, H2 = (ulong)1 << 55,
        A1 = (ulong)1 << 56, B1 = (ulong)1 << 57, C1 = (ulong)1 << 58, D1 = (ulong)1 << 59, E1 = (ulong)1 << 60, F1 = (ulong)1 << 61, G1 = (ulong)1 << 62, H1 = (ulong)1 << 63,
        Rank1 = A1 | B1 | C1 | D1 | E1 | F1 | G1 | H1,
        Rank2 = A2 | B2 | C2 | D2 | E2 | F2 | G2 | H2,
        Rank3 = A3 | B3 | C3 | D3 | E3 | F3 | G3 | H3,
        Rank4 = A4 | B4 | C4 | D4 | E4 | F4 | G4 | H4,
        Rank5 = A5 | B5 | C5 | D5 | E5 | F5 | G5 | H5,
        Rank6 = A6 | B6 | C6 | D6 | E6 | F6 | G6 | H6,
        Rank7 = A7 | B7 | C7 | D7 | E7 | F7 | G7 | H7,
        Rank8 = A8 | B8 | C8 | D8 | E8 | F8 | G8 | H8,
        FileA = A1 | A2 | A3 | A4 | A5 | A6 | A7 | A8,
        FileB = B1 | B2 | B3 | B4 | B5 | B6 | B7 | B8,
        FileC = C1 | C2 | C3 | C4 | C5 | C6 | C7 | C8,
        FileD = D1 | D2 | D3 | D4 | D5 | D6 | D7 | D8,
        FileE = E1 | E2 | E3 | E4 | E5 | E6 | E7 | E8,
        FileF = F1 | F2 | F3 | F4 | F5 | F6 | F7 | F8,
        FileG = G1 | G2 | G3 | G4 | G5 | G6 | G7 | G8,
        FileH = H1 | H2 | H3 | H4 | H5 | H6 | H7 | H8,
        Empty = 0,
        Full = Rank1 | Rank2 | Rank3 | Rank4 | Rank5 | Rank6 | Rank7 | Rank8
    }


    public static class ChessBitboardInfo
    {


        private static readonly int[] _byteBitcount = new int[256] { 0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 4, 5, 5, 6, 5, 6, 6, 7, 5, 6, 6, 7, 6, 7, 7, 8 };

        public static int BitCount(this ChessBitboard bitboard)
        {
            ulong val = (ulong)bitboard;
            int retval = 0;
            while (val != 0)
            {
                retval += _byteBitcount[val & 255];
                val = val >> 8;
            }
            return retval;
            //return _byteBitcount[((ulong)bitboard & 255)]
            //    + _byteBitcount[(((ulong)bitboard >> 8) & 255)]
            //    + _byteBitcount[(((ulong)bitboard >> 16) & 255)]
            //    + _byteBitcount[(((ulong)bitboard >> 24) & 255)]
            //    + _byteBitcount[(((ulong)bitboard >> 32) & 255)]
            //    + _byteBitcount[(((ulong)bitboard >> 40) & 255)]
            //    + _byteBitcount[(((ulong)bitboard >> 48) & 255)]
            //    + _byteBitcount[(((ulong)bitboard >> 56) & 255)];
        }

        

        public static ChessPosition NorthMostPosition(this ChessBitboard bitboard)
        {
            System.Diagnostics.Debug.Assert(bitboard != ChessBitboard.Empty);
            int lsb;
            if (((ulong)bitboard & 0xFFFFFFFF) != 0)
            {
                ulong x = (ulong)bitboard & 0xFFFFFFFF;
                lsb = debrujinLSB((int)x);
            }
            else
            {
                ulong x = (ulong)bitboard >> 32;
                lsb = (debrujinLSB((int)x) + 32);
            }
            return (ChessPosition)lsb;
        }

        //http://aggregate.org/MAGIC/
        public static ChessPosition SouthMostPosition(this ChessBitboard bitboard)
        {
            ulong x = (ulong)bitboard;

            x |= (x >> 1);
            x |= (x >> 2);
            x |= (x >> 4);
            x |= (x >> 8);
            x |= (x >> 16);
            x |= (x >> 32);
            return NorthMostPosition((ChessBitboard)(x & ~(x >> 1)));
        }



        public static ChessBitboard Reverse(this ChessBitboard bits)
        {
            return (ChessBitboard)(((ulong)(bits & ChessBitboard.Rank1) >> 56)
                | ((ulong)(bits & ChessBitboard.Rank2) >> 40)
                | ((ulong)(bits & ChessBitboard.Rank3) >> 24)
                | ((ulong)(bits & ChessBitboard.Rank4) >> 8)
                | ((ulong)(bits & ChessBitboard.Rank5) << 8)
                | ((ulong)(bits & ChessBitboard.Rank6) << 24)
                | ((ulong)(bits & ChessBitboard.Rank7) << 40)
                | ((ulong)(bits & ChessBitboard.Rank8) << 56));
        }

        public static ChessBitboard Flood(this ChessBitboard bits, ChessDirection dir)
        {
            while (true)
            {
                var shift = bits.Shift(dir);
                if ((shift & bits) == shift) { break; }
                bits = shift | bits;
            }
            return bits;
        }

        public static ChessBitboard Shift(this ChessBitboard bits, ChessDirection dir)
        {
            switch (dir)
            {
                case ChessDirection.DirN:
                    return bits.ShiftDirN();
                case ChessDirection.DirE:
                    return bits.ShiftDirE();
                case ChessDirection.DirS:
                    return bits.ShiftDirS();
                case ChessDirection.DirW:
                    return bits.ShiftDirW();
                case ChessDirection.DirNE:
                    return bits.ShiftDirNE();
                case ChessDirection.DirSE:
                    return bits.ShiftDirSE();
                case ChessDirection.DirSW:
                    return bits.ShiftDirSW();
                case ChessDirection.DirNW:
                    return bits.ShiftDirNW();
                case ChessDirection.DirNNE:
                    return bits.ShiftDirNE().ShiftDirN();
                case ChessDirection.DirEEN:
                    return bits.ShiftDirNE().ShiftDirE();
                case ChessDirection.DirEES:
                    return bits.ShiftDirSE().ShiftDirE();
                case ChessDirection.DirSSE:
                    return bits.ShiftDirSE().ShiftDirS();
                case ChessDirection.DirSSW:
                    return bits.ShiftDirSW().ShiftDirS();
                case ChessDirection.DirWWS:
                    return bits.ShiftDirSW().ShiftDirW();
                case ChessDirection.DirWWN:
                    return bits.ShiftDirNW().ShiftDirW();
                case ChessDirection.DirNNW:
                    return bits.ShiftDirNW().ShiftDirN();
                default:
                    throw new ArgumentOutOfRangeException("dir");
            }

        }

        public static ChessBitboard ShiftDirN(this ChessBitboard bits)
        {
            return (ChessBitboard)((ulong)(bits & ~ChessBitboard.Rank8) >> 8);
        }
        public static ChessBitboard ShiftDirE(this ChessBitboard bits)
        {
            return (ChessBitboard)((ulong)(bits & ~ChessBitboard.FileH) << 1);
        }
        public static ChessBitboard ShiftDirS(this ChessBitboard bits)
        {
            return (ChessBitboard)((ulong)(bits & ~ChessBitboard.Rank1) << 8);
        }
        public static ChessBitboard ShiftDirW(this ChessBitboard bits)
        {
            return (ChessBitboard)((ulong)(bits & ~ChessBitboard.FileA) >> 1);
        }
        public static ChessBitboard ShiftDirNE(this ChessBitboard bits)
        {
            return (ChessBitboard)((ulong)(bits & ~ChessBitboard.Rank8 & ~ChessBitboard.FileH) >> 7);
        }
        public static ChessBitboard ShiftDirSE(this ChessBitboard bits)
        {
            return (ChessBitboard)((ulong)(bits & ~ChessBitboard.Rank1 & ~ChessBitboard.FileH) << 9);
        }
        public static ChessBitboard ShiftDirSW(this ChessBitboard bits)
        {
            return (ChessBitboard)((ulong)(bits & ~ChessBitboard.Rank1 & ~ChessBitboard.FileA) << 7);
        }
        public static ChessBitboard ShiftDirNW(this ChessBitboard bits)
        {
            return (ChessBitboard)((ulong)(bits & ~ChessBitboard.Rank8 & ~ChessBitboard.FileA) >> 9);
        }

        public static bool Contains(this ChessBitboard bits, ChessPosition position)
        {
            System.Diagnostics.Debug.Assert(position.IsInBounds());
            return (bits & position.Bitboard()) != ChessBitboard.Empty;
        }

        public static bool Contains(this ChessBitboard bits, ChessBitboard other)
        {
            return (bits & other) != ChessBitboard.Empty;
        }

        public static bool Empty(this ChessBitboard bitboard)
        {
            return bitboard == 0;
        }

        public static IEnumerable<ChessPosition> ToPositions(this ChessBitboard bitboard)
        {
            while (bitboard != 0)
            {
                ChessPosition first = bitboard.NorthMostPosition();
                yield return first;
                bitboard = bitboard & ~first.Bitboard();
            }
        }

        private static int[] debrujinPositions =
	    {
	        0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8,
	        31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9
	    };

        private static int debrujinLSB(int number)
        {
            return debrujinPositions[unchecked((uint)(number & -number) * 0x077CB531U) >> 27];
        }

        public static ChessPosition PopFirst(ref ChessBitboard bitboard)
        {
            System.Diagnostics.Debug.Assert(bitboard != ChessBitboard.Empty);

            //self in-line the LSB function.
            ChessPosition first;
            if (((ulong)bitboard & 0xFFFFFFFF) != 0)
            {
                int number = (int)((ulong)bitboard & 0xFFFFFFFF);
                first = (ChessPosition)debrujinPositions[unchecked((uint)(number & -number) * 0x077CB531U) >> 27];
            }
            else
            {
                int number = (int)((ulong)bitboard >> 32);
                first = (ChessPosition)(debrujinPositions[unchecked((uint)(number & -number) * 0x077CB531U) >> 27] + 32);
            }

            bitboard = bitboard & ~first.Bitboard();
            return first;
        }


    }
}
