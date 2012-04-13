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

    public class ChessPositionDictionary<T>
    {
        [System.Xml.Serialization.XmlIgnore()]
        private T[] _values = new T[64];

        public ChessPositionDictionary()
        {

        }

        public ChessPositionDictionary(params T[] initVals)
        {
            for (int i = 0; i < 63; i++)
            {
                _values[i] = initVals[i];
            }
        }

        [System.Xml.Serialization.XmlIgnore()]
        public T this[ChessPosition pos]
        {
            get
            {
                return _values[pos.GetIndex64()];
            }
            set
            {
                _values[pos.GetIndex64()] = value;
            }
        }

        public T A1 { get { return this[ChessPosition.A1]; } set { this[ChessPosition.A1] = value; } }
        public T A2 { get { return this[ChessPosition.A2]; } set { this[ChessPosition.A2] = value; } }
        public T A3 { get { return this[ChessPosition.A3]; } set { this[ChessPosition.A3] = value; } }
        public T A4 { get { return this[ChessPosition.A4]; } set { this[ChessPosition.A4] = value; } }
        public T A5 { get { return this[ChessPosition.A5]; } set { this[ChessPosition.A5] = value; } }
        public T A6 { get { return this[ChessPosition.A6]; } set { this[ChessPosition.A6] = value; } }
        public T A7 { get { return this[ChessPosition.A7]; } set { this[ChessPosition.A7] = value; } }
        public T A8 { get { return this[ChessPosition.A8]; } set { this[ChessPosition.A8] = value; } }

        public T B1 { get { return this[ChessPosition.B1]; } set { this[ChessPosition.B1] = value; } }
        public T B2 { get { return this[ChessPosition.B2]; } set { this[ChessPosition.B2] = value; } }
        public T B3 { get { return this[ChessPosition.B3]; } set { this[ChessPosition.B3] = value; } }
        public T B4 { get { return this[ChessPosition.B4]; } set { this[ChessPosition.B4] = value; } }
        public T B5 { get { return this[ChessPosition.B5]; } set { this[ChessPosition.B5] = value; } }
        public T B6 { get { return this[ChessPosition.B6]; } set { this[ChessPosition.B6] = value; } }
        public T B7 { get { return this[ChessPosition.B7]; } set { this[ChessPosition.B7] = value; } }
        public T B8 { get { return this[ChessPosition.B8]; } set { this[ChessPosition.B8] = value; } }

        public T C1 { get { return this[ChessPosition.C1]; } set { this[ChessPosition.C1] = value; } }
        public T C2 { get { return this[ChessPosition.C2]; } set { this[ChessPosition.C2] = value; } }
        public T C3 { get { return this[ChessPosition.C3]; } set { this[ChessPosition.C3] = value; } }
        public T C4 { get { return this[ChessPosition.C4]; } set { this[ChessPosition.C4] = value; } }
        public T C5 { get { return this[ChessPosition.C5]; } set { this[ChessPosition.C5] = value; } }
        public T C6 { get { return this[ChessPosition.C6]; } set { this[ChessPosition.C6] = value; } }
        public T C7 { get { return this[ChessPosition.C7]; } set { this[ChessPosition.C7] = value; } }
        public T C8 { get { return this[ChessPosition.C8]; } set { this[ChessPosition.C8] = value; } }

        public T D1 { get { return this[ChessPosition.D1]; } set { this[ChessPosition.D1] = value; } }
        public T D2 { get { return this[ChessPosition.D2]; } set { this[ChessPosition.D2] = value; } }
        public T D3 { get { return this[ChessPosition.D3]; } set { this[ChessPosition.D3] = value; } }
        public T D4 { get { return this[ChessPosition.D4]; } set { this[ChessPosition.D4] = value; } }
        public T D5 { get { return this[ChessPosition.D5]; } set { this[ChessPosition.D5] = value; } }
        public T D6 { get { return this[ChessPosition.D6]; } set { this[ChessPosition.D6] = value; } }
        public T D7 { get { return this[ChessPosition.D7]; } set { this[ChessPosition.D7] = value; } }
        public T D8 { get { return this[ChessPosition.D8]; } set { this[ChessPosition.D8] = value; } }

        public T E1 { get { return this[ChessPosition.E1]; } set { this[ChessPosition.E1] = value; } }
        public T E2 { get { return this[ChessPosition.E2]; } set { this[ChessPosition.E2] = value; } }
        public T E3 { get { return this[ChessPosition.E3]; } set { this[ChessPosition.E3] = value; } }
        public T E4 { get { return this[ChessPosition.E4]; } set { this[ChessPosition.E4] = value; } }
        public T E5 { get { return this[ChessPosition.E5]; } set { this[ChessPosition.E5] = value; } }
        public T E6 { get { return this[ChessPosition.E6]; } set { this[ChessPosition.E6] = value; } }
        public T E7 { get { return this[ChessPosition.E7]; } set { this[ChessPosition.E7] = value; } }
        public T E8 { get { return this[ChessPosition.E8]; } set { this[ChessPosition.E8] = value; } }

        public T F1 { get { return this[ChessPosition.F1]; } set { this[ChessPosition.F1] = value; } }
        public T F2 { get { return this[ChessPosition.F2]; } set { this[ChessPosition.F2] = value; } }
        public T F3 { get { return this[ChessPosition.F3]; } set { this[ChessPosition.F3] = value; } }
        public T F4 { get { return this[ChessPosition.F4]; } set { this[ChessPosition.F4] = value; } }
        public T F5 { get { return this[ChessPosition.F5]; } set { this[ChessPosition.F5] = value; } }
        public T F6 { get { return this[ChessPosition.F6]; } set { this[ChessPosition.F6] = value; } }
        public T F7 { get { return this[ChessPosition.F7]; } set { this[ChessPosition.F7] = value; } }
        public T F8 { get { return this[ChessPosition.F8]; } set { this[ChessPosition.F8] = value; } }

        public T G1 { get { return this[ChessPosition.G1]; } set { this[ChessPosition.G1] = value; } }
        public T G2 { get { return this[ChessPosition.G2]; } set { this[ChessPosition.G2] = value; } }
        public T G3 { get { return this[ChessPosition.G3]; } set { this[ChessPosition.G3] = value; } }
        public T G4 { get { return this[ChessPosition.G4]; } set { this[ChessPosition.G4] = value; } }
        public T G5 { get { return this[ChessPosition.G5]; } set { this[ChessPosition.G5] = value; } }
        public T G6 { get { return this[ChessPosition.G6]; } set { this[ChessPosition.G6] = value; } }
        public T G7 { get { return this[ChessPosition.G7]; } set { this[ChessPosition.G7] = value; } }
        public T G8 { get { return this[ChessPosition.G8]; } set { this[ChessPosition.G8] = value; } }

        public T H1 { get { return this[ChessPosition.H1]; } set { this[ChessPosition.H1] = value; } }
        public T H2 { get { return this[ChessPosition.H2]; } set { this[ChessPosition.H2] = value; } }
        public T H3 { get { return this[ChessPosition.H3]; } set { this[ChessPosition.H3] = value; } }
        public T H4 { get { return this[ChessPosition.H4]; } set { this[ChessPosition.H4] = value; } }
        public T H5 { get { return this[ChessPosition.H5]; } set { this[ChessPosition.H5] = value; } }
        public T H6 { get { return this[ChessPosition.H6]; } set { this[ChessPosition.H6] = value; } }
        public T H7 { get { return this[ChessPosition.H7]; } set { this[ChessPosition.H7] = value; } }
        public T H8 { get { return this[ChessPosition.H8]; } set { this[ChessPosition.H8] = value; } }

        public override bool Equals(object obj)
        {
            ChessPositionDictionary<T> other = obj as ChessPositionDictionary<T>;
            if (other == null) { return false; }

            foreach (ChessPosition pos in Chess.AllPositions)
            {
                if (!this[pos].Equals(other[pos]))
                {
                    return false;
                }
            }
            return true;
        }


    }
}
