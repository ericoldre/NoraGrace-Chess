﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{
    [Flags]
    public enum ChessBitboard : ulong
    {
        A8 = (ulong)1 << 56, B8 = (ulong)1 << 57, C8 = (ulong)1 << 58, D8 = (ulong)1 << 59, E8 = (ulong)1 << 60, F8 = (ulong)1 << 61, G8 = (ulong)1 << 62, H8 = (ulong)1 << 63,
        A7 = (ulong)1 << 48, B7 = (ulong)1 << 49, C7 = (ulong)1 << 50, D7 = (ulong)1 << 51, E7 = (ulong)1 << 52, F7 = (ulong)1 << 53, G7 = (ulong)1 << 54, H7 = (ulong)1 << 55,
        A6 = (ulong)1 << 40, B6 = (ulong)1 << 41, C6 = (ulong)1 << 42, D6 = (ulong)1 << 43, E6 = (ulong)1 << 44, F6 = (ulong)1 << 45, G6 = (ulong)1 << 46, H6 = (ulong)1 << 47,
        A5 = (ulong)1 << 32, B5 = (ulong)1 << 33, C5 = (ulong)1 << 34, D5 = (ulong)1 << 35, E5 = (ulong)1 << 36, F5 = (ulong)1 << 37, G5 = (ulong)1 << 38, H5 = (ulong)1 << 39,
        A4 = (ulong)1 << 24, B4 = (ulong)1 << 25, C4 = (ulong)1 << 26, D4 = (ulong)1 << 27, E4 = (ulong)1 << 28, F4 = (ulong)1 << 29, G4 = (ulong)1 << 30, H4 = (ulong)1 << 31,
        A3 = (ulong)1 << 16, B3 = (ulong)1 << 17, C3 = (ulong)1 << 18, D3 = (ulong)1 << 19, E3 = (ulong)1 << 20, F3 = (ulong)1 << 21, G3 = (ulong)1 << 22, H3 = (ulong)1 << 23,
        A2 = (ulong)1 << 8, B2 = (ulong)1 << 9, C2 = (ulong)1 << 10, D2 = (ulong)1 << 11, E2 = (ulong)1 << 12, F2 = (ulong)1 << 13, G2 = (ulong)1 << 14, H2 = (ulong)1 << 15,
        A1 = (ulong)1 << 0, B1 = (ulong)1 << 1, C1 = (ulong)1 << 2, D1 = (ulong)1 << 3, E1 = (ulong)1 << 4, F1 = (ulong)1 << 5, G1 = (ulong)1 << 6, H1 = (ulong)1 << 7,
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


    public static class ExtensionsChessBitboard
    {
        private static int[] debrujinPositions =
	    {
	        0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8,
	        31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9
	    };

        private static int debrujinLSB(int number)
        {
            uint res = unchecked((uint)(number & -number) * 0x077CB531U) >> 27;
            return debrujinPositions[res];
        }

        public static ChessPosition FirstPosition(this ChessBitboard bitboard)
        {
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

        public static ChessBitboard Reverse(this ChessBitboard bits)
        {
            return (ChessBitboard)(((ulong)(bits & ChessBitboard.Rank8) >> 56)
                | ((ulong)(bits & ChessBitboard.Rank7) >> 40)
                | ((ulong)(bits & ChessBitboard.Rank6) >> 24)
                | ((ulong)(bits & ChessBitboard.Rank5) >> 8)
                | ((ulong)(bits & ChessBitboard.Rank4) << 8)
                | ((ulong)(bits & ChessBitboard.Rank3) << 24)
                | ((ulong)(bits & ChessBitboard.Rank2) << 40)
                | ((ulong)(bits & ChessBitboard.Rank1) << 56));
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
                    return ChessBitboard.Empty;
            }

        }

        public static ChessBitboard ShiftDirN(this ChessBitboard bits)
        {
            return (ChessBitboard)((ulong)(bits & ~ChessBitboard.Rank8) << 8);
        }
        public static ChessBitboard ShiftDirE(this ChessBitboard bits)
        {
            return (ChessBitboard)((ulong)(bits & ~ChessBitboard.FileH) << 1);
        }
        public static ChessBitboard ShiftDirS(this ChessBitboard bits)
        {
            return (ChessBitboard)((ulong)(bits & ~ChessBitboard.Rank1) >> 8);
        }
        public static ChessBitboard ShiftDirW(this ChessBitboard bits)
        {
            return (ChessBitboard)((ulong)(bits & ~ChessBitboard.FileA) >> 1);
        }
        public static ChessBitboard ShiftDirNE(this ChessBitboard bits)
        {
            return (ChessBitboard)((ulong)(bits & ~ChessBitboard.Rank8 & ~ChessBitboard.FileH) << 9);
        }
        public static ChessBitboard ShiftDirSE(this ChessBitboard bits)
        {
            return (ChessBitboard)((ulong)(bits & ~ChessBitboard.Rank1 & ~ChessBitboard.FileH) >> 7);
        }
        public static ChessBitboard ShiftDirSW(this ChessBitboard bits)
        {
            return (ChessBitboard)((ulong)(bits & ~ChessBitboard.Rank1 & ~ChessBitboard.FileA) >> 9);
        }
        public static ChessBitboard ShiftDirNW(this ChessBitboard bits)
        {
            return (ChessBitboard)((ulong)(bits & ~ChessBitboard.Rank8 & ~ChessBitboard.FileA) << 7);
        }



        public static bool Empty(this ChessBitboard bitboard)
        {
            return bitboard == 0;
        }

    }
}