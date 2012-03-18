using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{
    public enum ChessPosition
    {
        A8 = 56, B8 = 57, C8 = 58, D8 = 59, E8 = 60, F8 = 61, G8 = 62, H8 = 63,
        A7 = 48, B7 = 49, C7 = 50, D7 = 51, E7 = 52, F7 = 53, G7 = 54, H7 = 55,
        A6 = 40, B6 = 41, C6 = 42, D6 = 43, E6 = 44, F6 = 45, G6 = 46, H6 = 47,
        A5 = 32, B5 = 33, C5 = 34, D5 = 35, E5 = 36, F5 = 37, G5 = 38, H5 = 39,
        A4 = 24, B4 = 25, C4 = 26, D4 = 27, E4 = 28, F4 = 29, G4 = 30, H4 = 31,
        A3 = 16, B3 = 17, C3 = 18, D3 = 19, E3 = 20, F3 = 21, G3 = 22, H3 = 23,
        A2 = 8, B2 = 9, C2 = 10, D2 = 11, E2 = 12, F2 = 13, G2 = 14, H2 = 15,
        A1 = 0, B1 = 1, C1 = 2, D1 = 3, E1 = 4, F1 = 5, G1 = 6, H1 = 7,
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
                if (rankfrom > rankto) { return ChessDirection.DirS; }
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
            if (rankchange > 0)
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
                    rank += 1;
                    break;
                case ChessDirection.DirE:
                    file += 1;
                    break;
                case ChessDirection.DirS:
                    rank -= 1;
                    break;
                case ChessDirection.DirW:
                    file -= 1;
                    break;
                case ChessDirection.DirNE:
                    rank += 1; file += 1;
                    break;
                case ChessDirection.DirSE:
                    rank -= 1; file += 1;
                    break;
                case ChessDirection.DirSW:
                    rank -= 1; file -= 1;
                    break;
                case ChessDirection.DirNW:
                    rank += 1; file -= 1;
                    break;

                case ChessDirection.DirNNE:
                    rank += 2; file += 1;
                    break;
                case ChessDirection.DirEEN:
                    rank += 1; file += 2;
                    break;
                case ChessDirection.DirEES:
                    rank -= 1; file += 2;
                    break;
                case ChessDirection.DirSSE:
                    rank -= 2; file += 1;
                    break;

                case ChessDirection.DirSSW:
                    rank -= 2; file -= 1;
                    break;
                case ChessDirection.DirWWS:
                    rank -= 1; file -= 2;
                    break;
                case ChessDirection.DirWWN:
                    rank += 1; file -= 2;
                    break;
                case ChessDirection.DirNNW:
                    rank += 2; file -= 1;
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
