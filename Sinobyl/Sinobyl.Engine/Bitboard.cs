using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{
    [Flags]
    public enum Bitboard : ulong
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


    public static class BitboardInfo
    {


        private static readonly int[] _byteBitcount = new int[256] { 0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 4, 5, 5, 6, 5, 6, 6, 7, 5, 6, 6, 7, 6, 7, 7, 8 };

        public static int BitCount(this Bitboard bitboard)
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

        

        public static Position NorthMostPosition(this Bitboard bitboard)
        {
            System.Diagnostics.Debug.Assert(bitboard != Bitboard.Empty);
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
            return (Position)lsb;
        }

        //http://aggregate.org/MAGIC/
        public static Position SouthMostPosition(this Bitboard bitboard)
        {
            ulong x = (ulong)bitboard;

            x |= (x >> 1);
            x |= (x >> 2);
            x |= (x >> 4);
            x |= (x >> 8);
            x |= (x >> 16);
            x |= (x >> 32);
            return NorthMostPosition((Bitboard)(x & ~(x >> 1)));
        }



        public static Bitboard Reverse(this Bitboard bits)
        {
            return (Bitboard)(((ulong)(bits & Bitboard.Rank1) >> 56)
                | ((ulong)(bits & Bitboard.Rank2) >> 40)
                | ((ulong)(bits & Bitboard.Rank3) >> 24)
                | ((ulong)(bits & Bitboard.Rank4) >> 8)
                | ((ulong)(bits & Bitboard.Rank5) << 8)
                | ((ulong)(bits & Bitboard.Rank6) << 24)
                | ((ulong)(bits & Bitboard.Rank7) << 40)
                | ((ulong)(bits & Bitboard.Rank8) << 56));
        }

        public static Bitboard Flood(this Bitboard bits, Direction dir)
        {
            while (true)
            {
                var shift = bits.Shift(dir);
                if ((shift & bits) == shift) { break; }
                bits = shift | bits;
            }
            return bits;
        }

        public static Bitboard Shift(this Bitboard bits, Direction dir)
        {
            switch (dir)
            {
                case Direction.DirN:
                    return bits.ShiftDirN();
                case Direction.DirE:
                    return bits.ShiftDirE();
                case Direction.DirS:
                    return bits.ShiftDirS();
                case Direction.DirW:
                    return bits.ShiftDirW();
                case Direction.DirNE:
                    return bits.ShiftDirNE();
                case Direction.DirSE:
                    return bits.ShiftDirSE();
                case Direction.DirSW:
                    return bits.ShiftDirSW();
                case Direction.DirNW:
                    return bits.ShiftDirNW();
                case Direction.DirNNE:
                    return bits.ShiftDirNE().ShiftDirN();
                case Direction.DirEEN:
                    return bits.ShiftDirNE().ShiftDirE();
                case Direction.DirEES:
                    return bits.ShiftDirSE().ShiftDirE();
                case Direction.DirSSE:
                    return bits.ShiftDirSE().ShiftDirS();
                case Direction.DirSSW:
                    return bits.ShiftDirSW().ShiftDirS();
                case Direction.DirWWS:
                    return bits.ShiftDirSW().ShiftDirW();
                case Direction.DirWWN:
                    return bits.ShiftDirNW().ShiftDirW();
                case Direction.DirNNW:
                    return bits.ShiftDirNW().ShiftDirN();
                default:
                    throw new ArgumentOutOfRangeException("dir");
            }

        }

        public static Bitboard ShiftDirN(this Bitboard bits)
        {
            return (Bitboard)((ulong)(bits & ~Bitboard.Rank8) >> 8);
        }
        public static Bitboard ShiftDirE(this Bitboard bits)
        {
            return (Bitboard)((ulong)(bits & ~Bitboard.FileH) << 1);
        }
        public static Bitboard ShiftDirS(this Bitboard bits)
        {
            return (Bitboard)((ulong)(bits & ~Bitboard.Rank1) << 8);
        }
        public static Bitboard ShiftDirW(this Bitboard bits)
        {
            return (Bitboard)((ulong)(bits & ~Bitboard.FileA) >> 1);
        }
        public static Bitboard ShiftDirNE(this Bitboard bits)
        {
            return (Bitboard)((ulong)(bits & ~Bitboard.Rank8 & ~Bitboard.FileH) >> 7);
        }
        public static Bitboard ShiftDirSE(this Bitboard bits)
        {
            return (Bitboard)((ulong)(bits & ~Bitboard.Rank1 & ~Bitboard.FileH) << 9);
        }
        public static Bitboard ShiftDirSW(this Bitboard bits)
        {
            return (Bitboard)((ulong)(bits & ~Bitboard.Rank1 & ~Bitboard.FileA) << 7);
        }
        public static Bitboard ShiftDirNW(this Bitboard bits)
        {
            return (Bitboard)((ulong)(bits & ~Bitboard.Rank8 & ~Bitboard.FileA) >> 9);
        }

        public static bool Contains(this Bitboard bits, Position position)
        {
            System.Diagnostics.Debug.Assert(position.IsInBounds());
            return (bits & position.ToBitboard()) != Bitboard.Empty;
        }

        public static bool Contains(this Bitboard bits, Bitboard other)
        {
            return (bits & other) != Bitboard.Empty;
        }

        public static bool Empty(this Bitboard bitboard)
        {
            return bitboard == 0;
        }

        public static IEnumerable<Position> ToPositions(this Bitboard bitboard)
        {
            while (bitboard != 0)
            {
                Position first = bitboard.NorthMostPosition();
                yield return first;
                bitboard = bitboard & ~first.ToBitboard();
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

        public static Position PopFirst(ref Bitboard bitboard)
        {
            System.Diagnostics.Debug.Assert(bitboard != Bitboard.Empty);

            //self in-line the LSB function.
            Position first;
            if (((ulong)bitboard & 0xFFFFFFFF) != 0)
            {
                int number = (int)((ulong)bitboard & 0xFFFFFFFF);
                first = (Position)debrujinPositions[unchecked((uint)(number & -number) * 0x077CB531U) >> 27];
            }
            else
            {
                int number = (int)((ulong)bitboard >> 32);
                first = (Position)(debrujinPositions[unchecked((uint)(number & -number) * 0x077CB531U) >> 27] + 32);
            }

            bitboard = bitboard & ~first.ToBitboard();
            return first;
        }


    }
}
