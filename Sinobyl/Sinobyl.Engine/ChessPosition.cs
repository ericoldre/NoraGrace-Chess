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

    public static class ChessPositionInfo
    {

        public const int LookupArrayLength = 64;

        public static readonly ChessPosition[] AllPositions = new ChessPosition[]{
		    ChessPosition.A8,ChessPosition.B8,ChessPosition.C8,ChessPosition.D8,ChessPosition.E8,ChessPosition.F8,ChessPosition.G8,ChessPosition.H8,
            ChessPosition.A7,ChessPosition.B7,ChessPosition.C7,ChessPosition.D7,ChessPosition.E7,ChessPosition.F7,ChessPosition.G7,ChessPosition.H7,
		    ChessPosition.A6,ChessPosition.B6,ChessPosition.C6,ChessPosition.D6,ChessPosition.E6,ChessPosition.F6,ChessPosition.G6,ChessPosition.H6,
		    ChessPosition.A5,ChessPosition.B5,ChessPosition.C5,ChessPosition.D5,ChessPosition.E5,ChessPosition.F5,ChessPosition.G5,ChessPosition.H5,
		    ChessPosition.A4,ChessPosition.B4,ChessPosition.C4,ChessPosition.D4,ChessPosition.E4,ChessPosition.F4,ChessPosition.G4,ChessPosition.H4,
            ChessPosition.A3,ChessPosition.B3,ChessPosition.C3,ChessPosition.D3,ChessPosition.E3,ChessPosition.F3,ChessPosition.G3,ChessPosition.H3,
		    ChessPosition.A2,ChessPosition.B2,ChessPosition.C2,ChessPosition.D2,ChessPosition.E2,ChessPosition.F2,ChessPosition.G2,ChessPosition.H2,
            ChessPosition.A1,ChessPosition.B1,ChessPosition.C1,ChessPosition.D1,ChessPosition.E1,ChessPosition.F1,ChessPosition.G1,ChessPosition.H1,
		};


        public static ChessPosition Parse(string s)
        {
            if (s.Length != 2) { throw new ArgumentException(s + " is not a valid position"); }
            File file = FileInfo.Parse(s[0]);
            Rank rank = RankInfo.Parse(s[1]);
            return file.ToPosition(rank);
        }

        public static bool IsLight(this ChessPosition pos)
        {
            bool retval = (int)pos % 2 == 1;
            switch (pos.ToRank())
            {
                case Rank.Rank2:
                case Rank.Rank4:
                case Rank.Rank6:
                case Rank.Rank8:
                    retval = !retval;
                    break;
            }
            return retval;
        }
        public static bool IsInBounds(this ChessPosition pos)
        {
            return (int)pos >= 0 && (int)pos <= 63;
        }
        public static Rank ToRank(this ChessPosition pos)
        {
            //AssertPosition(pos);
            return (Rank)(((int)pos / 8));
        }
        public static File ToFile(this ChessPosition pos)
        {
            //AssertPosition(pos);
            return (File)((int)pos % 8);
        }
        public static string PositionToString(this ChessPosition pos)
        {
            return pos.ToFile().FileToString() + pos.ToRank().RankToString();
        }


        public static Bitboard ToBitboard(this ChessPosition position)
        {
            if (position.IsInBounds())
            {
                return (Bitboard)((ulong)1 << position.GetIndex64());
            }
            else
            {
                return (Bitboard.Empty);
            }
        }

        public static int DistanceTo(this ChessPosition thisPosition, ChessPosition otherPosition)
        {
            int rDiff = Math.Abs((int)thisPosition.ToRank() - (int)otherPosition.ToRank());
            int fDiff = Math.Abs((int)thisPosition.ToFile() - (int)otherPosition.ToFile());
            return rDiff > fDiff ? rDiff : fDiff;
        }

        public static int DistanceToNoDiag(this ChessPosition thisPosition, ChessPosition otherPosition)
        {
            int rDiff = Math.Abs((int)thisPosition.ToRank() - (int)otherPosition.ToRank());
            int fDiff = Math.Abs((int)thisPosition.ToFile() - (int)otherPosition.ToFile());
            return rDiff + fDiff;
        }

        public static int GetIndex64(this ChessPosition position)
        {
            return (int)position;
        }

        public static Bitboard ToBitboard(this IEnumerable<ChessPosition> positions)
        {
            Bitboard bitboard = 0;
            foreach (var position in positions)
            {
                bitboard |= position.ToBitboard();
            }
            return bitboard;
        }

        public static Bitboard Between(this ChessPosition from, ChessPosition to)
        {
            ChessDirection dir = from.DirectionTo(to);
            Bitboard retval = Bitboard.Empty;
            if (dir != 0)
            {
                while (from != to)
                {
                    from = from.PositionInDirectionUnsafe(dir);
                    retval |= from.ToBitboard();
                }
            }
            return retval;
        }

        public static ChessDirection DirectionTo(this ChessPosition from, ChessPosition to)
        {

            Rank rankfrom = from.ToRank();
            File filefrom = from.ToFile();
            Rank rankto = to.ToRank();
            File fileto = to.ToFile();

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
            if (!pos.IsInBounds()) { return ChessPosition.OUTOFBOUNDS; }
            File file = pos.ToFile();
            Rank rank = pos.ToRank();
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
            Rank r = pos.ToRank();
            File f = pos.ToFile();
            Rank newrank = Rank.EMPTY;
            switch (r)
            {
                case Rank.Rank1:
                    newrank = Rank.Rank8;
                    break;
                case Rank.Rank2:
                    newrank = Rank.Rank7;
                    break;
                case Rank.Rank3:
                    newrank = Rank.Rank6;
                    break;
                case Rank.Rank4:
                    newrank = Rank.Rank5;
                    break;
                case Rank.Rank5:
                    newrank = Rank.Rank4;
                    break;
                case Rank.Rank6:
                    newrank = Rank.Rank3;
                    break;
                case Rank.Rank7:
                    newrank = Rank.Rank2;
                    break;
                case Rank.Rank8:
                    newrank = Rank.Rank1;
                    break;
            }
            return f.ToPosition(newrank);
        }

    }

    public sealed class ChessPositionDictionary<T>
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
                return _values[(int)pos];
            }
            set
            {
                _values[(int)pos] = value;
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

            foreach (ChessPosition pos in ChessPositionInfo.AllPositions)
            {
                if (!this[pos].Equals(other[pos]))
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;//randomly choosen prime
                foreach (ChessPosition pos in ChessPositionInfo.AllPositions)
                {
                    T field = this[pos];
                    int fieldHash = 6823; //randomly choosen prime
                    if (field != null)
                    {
                        fieldHash = field.GetHashCode();
                    }
                    hash = (hash * 23) + fieldHash;
                }
                return hash;
            }
        }


    }
}
