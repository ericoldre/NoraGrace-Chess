using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{
    public enum ChessPosition
    {
        A8 = 0, B8 = 1, C8 = 2, D8 = 3, E8 = 4, F8 = 5, G8 = 6, H8 = 7,
        A7 = 8, B7 = 9, C7 = 10, D7 = 11, E7 = 12, F7 = 13, G7 = 14, H7 = 15,
        A6 = 16, B6 = 17, C6 = 18, D6 = 19, E6 = 20, F6 = 21, G6 = 22, H6 = 23,
        A5 = 24, B5 = 25, C5 = 26, D5 = 27, E5 = 28, F5 = 29, G5 = 30, H5 = 31,
        A4 = 32, B4 = 33, C4 = 34, D4 = 35, E4 = 36, F4 = 37, G4 = 38, H4 = 39,
        A3 = 40, B3 = 41, C3 = 42, D3 = 43, E3 = 44, F3 = 45, G3 = 46, H3 = 47,
        A2 = 48, B2 = 49, C2 = 50, D2 = 51, E2 = 52, F2 = 53, G2 = 54, H2 = 55,
        A1 = 56, B1 = 57, C1 = 58, D1 = 59, E1 = 60, F1 = 61, G1 = 62, H1 = 63,
        OUTOFBOUNDS = 64
    }

    public static class ExtensionsChessPosition
    {
        public static bool IsInBounds(this ChessPosition pos)
        {
            return (int)pos >= 0 && (int)pos <= 63;
        }
        public static ChessRank GetRank(this ChessPosition pos)
        {
            //AssertPosition(pos);
            return (ChessRank)(((int)pos / 8));
        }
        public static ChessFile GetFile(this ChessPosition pos)
        {
            //AssertPosition(pos);
            return (ChessFile)((int)pos % 8);
        }
        public static string PositionToString(this ChessPosition pos)
        {
            return pos.GetFile().FileToString() + pos.GetRank().RankToString();
        }

        public static IEnumerable<ChessPosition> ToPositions(this ChessBitboard bitboard)
        {
            while (bitboard != 0)
            {
                ChessPosition first = bitboard.FirstPosition();
                yield return first;
                bitboard = bitboard & ~first.Bitboard();
            }
        }
        public static ChessBitboard Bitboard(this ChessPosition position)
        {
            if (position.IsInBounds())
            {
                return (ChessBitboard)((ulong)1 << position.GetIndex64());
            }
            else
            {
                return (ChessBitboard.Empty);
            }
        }

        public static int GetIndex64(this ChessPosition position)
        {
            return (int)position;
        }

        public static ChessBitboard ToBitboard(this IEnumerable<ChessPosition> positions)
        {
            ChessBitboard bitboard = 0;
            foreach (var position in positions)
            {
                bitboard |= position.Bitboard();
            }
            return bitboard;
        }

        public static ChessDirection DirectionTo(this ChessPosition from, ChessPosition to)
        {

            ChessRank rankfrom = from.GetRank();
            ChessFile filefrom = from.GetFile();
            ChessRank rankto = to.GetRank();
            ChessFile fileto = to.GetFile();

            if (fileto == filefrom)
            {
                if (rankfrom < rankto) { return ChessDirection.DirS; }
                return ChessDirection.DirN;
            }
            else if (rankfrom == rankto)
            {
                if (filefrom > fileto) { return ChessDirection.DirW; }
                return ChessDirection.DirE;
            }
            int rankchange = rankto - rankfrom;
            int filechange = fileto - filefrom;
            int rankchangeabs = rankchange > 0 ? rankchange : -rankchange;
            int filechangeabs = filechange > 0 ? filechange : -filechange;
            if ((rankchangeabs == 1 && filechangeabs == 2) || (rankchangeabs == 2 && filechangeabs == 1))
            {
                //knight direction
                return (ChessDirection)((int)rankchange * 8) + (int)filechange;
            }
            else if (rankchangeabs != filechangeabs)
            {
                return 0;
            }
            if (rankchange < 0)
            {
                if (filechange > 0) { return ChessDirection.DirNE; }
                return ChessDirection.DirNW;
            }
            else
            {
                if (filechange > 0) { return ChessDirection.DirSE; }
                return ChessDirection.DirSW;
            }

        }

        public static ChessPosition PositionInDirectionUnsafe(this ChessPosition pos, ChessDirection dir)
        {
            return (ChessPosition)((int)pos + (int)dir);
        }
        public static ChessPosition PositionInDirection(this ChessPosition pos, ChessDirection dir)
        {
            ChessFile file = pos.GetFile();
            ChessRank rank = pos.GetRank();
            switch (dir)
            {
                case ChessDirection.DirN:
                    rank -= 1;
                    break;
                case ChessDirection.DirE:
                    file += 1;
                    break;
                case ChessDirection.DirS:
                    rank += 1;
                    break;
                case ChessDirection.DirW:
                    file -= 1;
                    break;
                case ChessDirection.DirNE:
                    rank -= 1; file += 1;
                    break;
                case ChessDirection.DirSE:
                    rank += 1; file += 1;
                    break;
                case ChessDirection.DirSW:
                    rank += 1; file -= 1;
                    break;
                case ChessDirection.DirNW:
                    rank -= 1; file -= 1;
                    break;

                case ChessDirection.DirNNE:
                    rank -= 2; file += 1;
                    break;
                case ChessDirection.DirEEN:
                    rank -= 1; file += 2;
                    break;
                case ChessDirection.DirEES:
                    rank += 1; file += 2;
                    break;
                case ChessDirection.DirSSE:
                    rank += 2; file += 1;
                    break;

                case ChessDirection.DirSSW:
                    rank += 2; file -= 1;
                    break;
                case ChessDirection.DirWWS:
                    rank += 1; file -= 2;
                    break;
                case ChessDirection.DirWWN:
                    rank -= 1; file -= 2;
                    break;
                case ChessDirection.DirNNW:
                    rank -= 2; file -= 1;
                    break;
                default:
                    return (ChessPosition.OUTOFBOUNDS);
            }
            if (rank.IsInBounds() && file.IsInBounds())
            {
                return rank.ToPosition(file);
            }
            else
            {
                return (ChessPosition.OUTOFBOUNDS);
            }

        }
        public static ChessPosition Reverse(this ChessPosition pos)
        {
            ChessRank r = pos.GetRank();
            ChessFile f = pos.GetFile();
            ChessRank newrank = ChessRank.EMPTY;
            switch (r)
            {
                case ChessRank.Rank1:
                    newrank = ChessRank.Rank8;
                    break;
                case ChessRank.Rank2:
                    newrank = ChessRank.Rank7;
                    break;
                case ChessRank.Rank3:
                    newrank = ChessRank.Rank6;
                    break;
                case ChessRank.Rank4:
                    newrank = ChessRank.Rank5;
                    break;
                case ChessRank.Rank5:
                    newrank = ChessRank.Rank4;
                    break;
                case ChessRank.Rank6:
                    newrank = ChessRank.Rank3;
                    break;
                case ChessRank.Rank7:
                    newrank = ChessRank.Rank2;
                    break;
                case ChessRank.Rank8:
                    newrank = ChessRank.Rank1;
                    break;
            }
            return f.ToPosition(newrank);
        }

    }
}
