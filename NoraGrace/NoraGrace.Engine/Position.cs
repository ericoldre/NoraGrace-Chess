using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoraGrace.Engine
{
    public enum Position
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

    public static class PositionInfo
    {

        public const int LookupArrayLength = 64;

        public static readonly Position[] AllPositions = new Position[]{
		    Position.A8,Position.B8,Position.C8,Position.D8,Position.E8,Position.F8,Position.G8,Position.H8,
            Position.A7,Position.B7,Position.C7,Position.D7,Position.E7,Position.F7,Position.G7,Position.H7,
		    Position.A6,Position.B6,Position.C6,Position.D6,Position.E6,Position.F6,Position.G6,Position.H6,
		    Position.A5,Position.B5,Position.C5,Position.D5,Position.E5,Position.F5,Position.G5,Position.H5,
		    Position.A4,Position.B4,Position.C4,Position.D4,Position.E4,Position.F4,Position.G4,Position.H4,
            Position.A3,Position.B3,Position.C3,Position.D3,Position.E3,Position.F3,Position.G3,Position.H3,
		    Position.A2,Position.B2,Position.C2,Position.D2,Position.E2,Position.F2,Position.G2,Position.H2,
            Position.A1,Position.B1,Position.C1,Position.D1,Position.E1,Position.F1,Position.G1,Position.H1,
		};


        public static Position Parse(string s)
        {
            if (s.Length != 2) { throw new ArgumentException(s + " is not a valid position"); }
            File file = FileInfo.Parse(s[0]);
            Rank rank = RankInfo.Parse(s[1]);
            return file.ToPosition(rank);
        }

        public static bool IsLight(this Position pos)
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
        public static bool IsInBounds(this Position pos)
        {
            return (int)pos >= 0 && (int)pos <= 63;
        }
        public static Rank ToRank(this Position pos)
        {
            //AssertPosition(pos);
            return (Rank)(((int)pos / 8));
        }
        public static File ToFile(this Position pos)
        {
            //AssertPosition(pos);
            return (File)((int)pos % 8);
        }
        public static string Name(this Position pos)
        {
            return pos.ToFile().FileToString() + pos.ToRank().RankToString();
        }


        public static Bitboard ToBitboard(this Position position)
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

        public static int DistanceTo(this Position thisPosition, Position otherPosition)
        {
            int rDiff = Math.Abs((int)thisPosition.ToRank() - (int)otherPosition.ToRank());
            int fDiff = Math.Abs((int)thisPosition.ToFile() - (int)otherPosition.ToFile());
            return rDiff > fDiff ? rDiff : fDiff;
        }

        public static int DistanceToNoDiag(this Position thisPosition, Position otherPosition)
        {
            int rDiff = Math.Abs((int)thisPosition.ToRank() - (int)otherPosition.ToRank());
            int fDiff = Math.Abs((int)thisPosition.ToFile() - (int)otherPosition.ToFile());
            return rDiff + fDiff;
        }

        public static int GetIndex64(this Position position)
        {
            return (int)position;
        }

        public static Bitboard ToBitboard(this IEnumerable<Position> positions)
        {
            Bitboard bitboard = 0;
            foreach (var position in positions)
            {
                bitboard |= position.ToBitboard();
            }
            return bitboard;
        }

        public static Bitboard Between(this Position from, Position to)
        {
            Direction dir = from.DirectionTo(to);
            Bitboard retval = Bitboard.Empty;
            if (dir != 0)
            {
                while (from != to)
                {
                    from = from.PositionInDirectionUnsafe(dir);
                    retval |= from.ToBitboard();
                }
            }
            return retval & ~to.ToBitboard();
        }

        public static Direction DirectionTo(this Position from, Position to)
        {

            Rank rankfrom = from.ToRank();
            File filefrom = from.ToFile();
            Rank rankto = to.ToRank();
            File fileto = to.ToFile();

            if (fileto == filefrom)
            {
                if (rankfrom < rankto) { return Direction.DirS; }
                return Direction.DirN;
            }
            else if (rankfrom == rankto)
            {
                if (filefrom > fileto) { return Direction.DirW; }
                return Direction.DirE;
            }
            int rankchange = rankto - rankfrom;
            int filechange = fileto - filefrom;
            int rankchangeabs = rankchange > 0 ? rankchange : -rankchange;
            int filechangeabs = filechange > 0 ? filechange : -filechange;
            if ((rankchangeabs == 1 && filechangeabs == 2) || (rankchangeabs == 2 && filechangeabs == 1))
            {
                //knight direction
                return (Direction)((int)rankchange * 8) + (int)filechange;
            }
            else if (rankchangeabs != filechangeabs)
            {
                return 0;
            }
            if (rankchange < 0)
            {
                if (filechange > 0) { return Direction.DirNE; }
                return Direction.DirNW;
            }
            else
            {
                if (filechange > 0) { return Direction.DirSE; }
                return Direction.DirSW;
            }

        }

        public static Position PositionInDirectionUnsafe(this Position pos, Direction dir)
        {
            return (Position)((int)pos + (int)dir);
        }
        public static Position PositionInDirection(this Position pos, Direction dir)
        {
            if (!pos.IsInBounds()) { return Position.OUTOFBOUNDS; }
            File file = pos.ToFile();
            Rank rank = pos.ToRank();
            switch (dir)
            {
                case Direction.DirN:
                    rank -= 1;
                    break;
                case Direction.DirE:
                    file += 1;
                    break;
                case Direction.DirS:
                    rank += 1;
                    break;
                case Direction.DirW:
                    file -= 1;
                    break;
                case Direction.DirNE:
                    rank -= 1; file += 1;
                    break;
                case Direction.DirSE:
                    rank += 1; file += 1;
                    break;
                case Direction.DirSW:
                    rank += 1; file -= 1;
                    break;
                case Direction.DirNW:
                    rank -= 1; file -= 1;
                    break;

                case Direction.DirNNE:
                    rank -= 2; file += 1;
                    break;
                case Direction.DirEEN:
                    rank -= 1; file += 2;
                    break;
                case Direction.DirEES:
                    rank += 1; file += 2;
                    break;
                case Direction.DirSSE:
                    rank += 2; file += 1;
                    break;

                case Direction.DirSSW:
                    rank += 2; file -= 1;
                    break;
                case Direction.DirWWS:
                    rank += 1; file -= 2;
                    break;
                case Direction.DirWWN:
                    rank -= 1; file -= 2;
                    break;
                case Direction.DirNNW:
                    rank -= 2; file -= 1;
                    break;
                default:
                    return (Position.OUTOFBOUNDS);
            }
            if (rank.IsInBounds() && file.IsInBounds())
            {
                return rank.ToPosition(file);
            }
            else
            {
                return (Position.OUTOFBOUNDS);
            }

        }
        public static Position Reverse(this Position pos)
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
        public T this[Position pos]
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

        public T A1 { get { return this[Position.A1]; } set { this[Position.A1] = value; } }
        public T A2 { get { return this[Position.A2]; } set { this[Position.A2] = value; } }
        public T A3 { get { return this[Position.A3]; } set { this[Position.A3] = value; } }
        public T A4 { get { return this[Position.A4]; } set { this[Position.A4] = value; } }
        public T A5 { get { return this[Position.A5]; } set { this[Position.A5] = value; } }
        public T A6 { get { return this[Position.A6]; } set { this[Position.A6] = value; } }
        public T A7 { get { return this[Position.A7]; } set { this[Position.A7] = value; } }
        public T A8 { get { return this[Position.A8]; } set { this[Position.A8] = value; } }

        public T B1 { get { return this[Position.B1]; } set { this[Position.B1] = value; } }
        public T B2 { get { return this[Position.B2]; } set { this[Position.B2] = value; } }
        public T B3 { get { return this[Position.B3]; } set { this[Position.B3] = value; } }
        public T B4 { get { return this[Position.B4]; } set { this[Position.B4] = value; } }
        public T B5 { get { return this[Position.B5]; } set { this[Position.B5] = value; } }
        public T B6 { get { return this[Position.B6]; } set { this[Position.B6] = value; } }
        public T B7 { get { return this[Position.B7]; } set { this[Position.B7] = value; } }
        public T B8 { get { return this[Position.B8]; } set { this[Position.B8] = value; } }

        public T C1 { get { return this[Position.C1]; } set { this[Position.C1] = value; } }
        public T C2 { get { return this[Position.C2]; } set { this[Position.C2] = value; } }
        public T C3 { get { return this[Position.C3]; } set { this[Position.C3] = value; } }
        public T C4 { get { return this[Position.C4]; } set { this[Position.C4] = value; } }
        public T C5 { get { return this[Position.C5]; } set { this[Position.C5] = value; } }
        public T C6 { get { return this[Position.C6]; } set { this[Position.C6] = value; } }
        public T C7 { get { return this[Position.C7]; } set { this[Position.C7] = value; } }
        public T C8 { get { return this[Position.C8]; } set { this[Position.C8] = value; } }

        public T D1 { get { return this[Position.D1]; } set { this[Position.D1] = value; } }
        public T D2 { get { return this[Position.D2]; } set { this[Position.D2] = value; } }
        public T D3 { get { return this[Position.D3]; } set { this[Position.D3] = value; } }
        public T D4 { get { return this[Position.D4]; } set { this[Position.D4] = value; } }
        public T D5 { get { return this[Position.D5]; } set { this[Position.D5] = value; } }
        public T D6 { get { return this[Position.D6]; } set { this[Position.D6] = value; } }
        public T D7 { get { return this[Position.D7]; } set { this[Position.D7] = value; } }
        public T D8 { get { return this[Position.D8]; } set { this[Position.D8] = value; } }

        public T E1 { get { return this[Position.E1]; } set { this[Position.E1] = value; } }
        public T E2 { get { return this[Position.E2]; } set { this[Position.E2] = value; } }
        public T E3 { get { return this[Position.E3]; } set { this[Position.E3] = value; } }
        public T E4 { get { return this[Position.E4]; } set { this[Position.E4] = value; } }
        public T E5 { get { return this[Position.E5]; } set { this[Position.E5] = value; } }
        public T E6 { get { return this[Position.E6]; } set { this[Position.E6] = value; } }
        public T E7 { get { return this[Position.E7]; } set { this[Position.E7] = value; } }
        public T E8 { get { return this[Position.E8]; } set { this[Position.E8] = value; } }

        public T F1 { get { return this[Position.F1]; } set { this[Position.F1] = value; } }
        public T F2 { get { return this[Position.F2]; } set { this[Position.F2] = value; } }
        public T F3 { get { return this[Position.F3]; } set { this[Position.F3] = value; } }
        public T F4 { get { return this[Position.F4]; } set { this[Position.F4] = value; } }
        public T F5 { get { return this[Position.F5]; } set { this[Position.F5] = value; } }
        public T F6 { get { return this[Position.F6]; } set { this[Position.F6] = value; } }
        public T F7 { get { return this[Position.F7]; } set { this[Position.F7] = value; } }
        public T F8 { get { return this[Position.F8]; } set { this[Position.F8] = value; } }

        public T G1 { get { return this[Position.G1]; } set { this[Position.G1] = value; } }
        public T G2 { get { return this[Position.G2]; } set { this[Position.G2] = value; } }
        public T G3 { get { return this[Position.G3]; } set { this[Position.G3] = value; } }
        public T G4 { get { return this[Position.G4]; } set { this[Position.G4] = value; } }
        public T G5 { get { return this[Position.G5]; } set { this[Position.G5] = value; } }
        public T G6 { get { return this[Position.G6]; } set { this[Position.G6] = value; } }
        public T G7 { get { return this[Position.G7]; } set { this[Position.G7] = value; } }
        public T G8 { get { return this[Position.G8]; } set { this[Position.G8] = value; } }

        public T H1 { get { return this[Position.H1]; } set { this[Position.H1] = value; } }
        public T H2 { get { return this[Position.H2]; } set { this[Position.H2] = value; } }
        public T H3 { get { return this[Position.H3]; } set { this[Position.H3] = value; } }
        public T H4 { get { return this[Position.H4]; } set { this[Position.H4] = value; } }
        public T H5 { get { return this[Position.H5]; } set { this[Position.H5] = value; } }
        public T H6 { get { return this[Position.H6]; } set { this[Position.H6] = value; } }
        public T H7 { get { return this[Position.H7]; } set { this[Position.H7] = value; } }
        public T H8 { get { return this[Position.H8]; } set { this[Position.H8] = value; } }

        public override bool Equals(object obj)
        {
            ChessPositionDictionary<T> other = obj as ChessPositionDictionary<T>;
            if (other == null) { return false; }

            foreach (Position pos in PositionInfo.AllPositions)
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
                foreach (Position pos in PositionInfo.AllPositions)
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
