using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{
    public enum ChessPositionValue
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

    public struct ChessPosition
    {
        public static readonly ChessPosition A1 = new ChessPosition(ChessPositionValue.A1);
        public static readonly ChessPosition A2 = new ChessPosition(ChessPositionValue.A2);
        public static readonly ChessPosition A3 = new ChessPosition(ChessPositionValue.A3);
        public static readonly ChessPosition A4 = new ChessPosition(ChessPositionValue.A4);
        public static readonly ChessPosition A5 = new ChessPosition(ChessPositionValue.A5);
        public static readonly ChessPosition A6 = new ChessPosition(ChessPositionValue.A6);
        public static readonly ChessPosition A7 = new ChessPosition(ChessPositionValue.A7);
        public static readonly ChessPosition A8 = new ChessPosition(ChessPositionValue.A8);

        public static readonly ChessPosition B1 = new ChessPosition(ChessPositionValue.B1);
        public static readonly ChessPosition B2 = new ChessPosition(ChessPositionValue.B2);
        public static readonly ChessPosition B3 = new ChessPosition(ChessPositionValue.B3);
        public static readonly ChessPosition B4 = new ChessPosition(ChessPositionValue.B4);
        public static readonly ChessPosition B5 = new ChessPosition(ChessPositionValue.B5);
        public static readonly ChessPosition B6 = new ChessPosition(ChessPositionValue.B6);
        public static readonly ChessPosition B7 = new ChessPosition(ChessPositionValue.B7);
        public static readonly ChessPosition B8 = new ChessPosition(ChessPositionValue.B8);

        public static readonly ChessPosition C1 = new ChessPosition(ChessPositionValue.C1);
        public static readonly ChessPosition C2 = new ChessPosition(ChessPositionValue.C2);
        public static readonly ChessPosition C3 = new ChessPosition(ChessPositionValue.C3);
        public static readonly ChessPosition C4 = new ChessPosition(ChessPositionValue.C4);
        public static readonly ChessPosition C5 = new ChessPosition(ChessPositionValue.C5);
        public static readonly ChessPosition C6 = new ChessPosition(ChessPositionValue.C6);
        public static readonly ChessPosition C7 = new ChessPosition(ChessPositionValue.C7);
        public static readonly ChessPosition C8 = new ChessPosition(ChessPositionValue.C8);

        public static readonly ChessPosition D1 = new ChessPosition(ChessPositionValue.D1);
        public static readonly ChessPosition D2 = new ChessPosition(ChessPositionValue.D2);
        public static readonly ChessPosition D3 = new ChessPosition(ChessPositionValue.D3);
        public static readonly ChessPosition D4 = new ChessPosition(ChessPositionValue.D4);
        public static readonly ChessPosition D5 = new ChessPosition(ChessPositionValue.D5);
        public static readonly ChessPosition D6 = new ChessPosition(ChessPositionValue.D6);
        public static readonly ChessPosition D7 = new ChessPosition(ChessPositionValue.D7);
        public static readonly ChessPosition D8 = new ChessPosition(ChessPositionValue.D8);

        public static readonly ChessPosition E1 = new ChessPosition(ChessPositionValue.E1);
        public static readonly ChessPosition E2 = new ChessPosition(ChessPositionValue.E2);
        public static readonly ChessPosition E3 = new ChessPosition(ChessPositionValue.E3);
        public static readonly ChessPosition E4 = new ChessPosition(ChessPositionValue.E4);
        public static readonly ChessPosition E5 = new ChessPosition(ChessPositionValue.E5);
        public static readonly ChessPosition E6 = new ChessPosition(ChessPositionValue.E6);
        public static readonly ChessPosition E7 = new ChessPosition(ChessPositionValue.E7);
        public static readonly ChessPosition E8 = new ChessPosition(ChessPositionValue.E8);

        public static readonly ChessPosition F1 = new ChessPosition(ChessPositionValue.F1);
        public static readonly ChessPosition F2 = new ChessPosition(ChessPositionValue.F2);
        public static readonly ChessPosition F3 = new ChessPosition(ChessPositionValue.F3);
        public static readonly ChessPosition F4 = new ChessPosition(ChessPositionValue.F4);
        public static readonly ChessPosition F5 = new ChessPosition(ChessPositionValue.F5);
        public static readonly ChessPosition F6 = new ChessPosition(ChessPositionValue.F6);
        public static readonly ChessPosition F7 = new ChessPosition(ChessPositionValue.F7);
        public static readonly ChessPosition F8 = new ChessPosition(ChessPositionValue.F8);

        public static readonly ChessPosition G1 = new ChessPosition(ChessPositionValue.G1);
        public static readonly ChessPosition G2 = new ChessPosition(ChessPositionValue.G2);
        public static readonly ChessPosition G3 = new ChessPosition(ChessPositionValue.G3);
        public static readonly ChessPosition G4 = new ChessPosition(ChessPositionValue.G4);
        public static readonly ChessPosition G5 = new ChessPosition(ChessPositionValue.G5);
        public static readonly ChessPosition G6 = new ChessPosition(ChessPositionValue.G6);
        public static readonly ChessPosition G7 = new ChessPosition(ChessPositionValue.G7);
        public static readonly ChessPosition G8 = new ChessPosition(ChessPositionValue.G8);

        public static readonly ChessPosition H1 = new ChessPosition(ChessPositionValue.H1);
        public static readonly ChessPosition H2 = new ChessPosition(ChessPositionValue.H2);
        public static readonly ChessPosition H3 = new ChessPosition(ChessPositionValue.H3);
        public static readonly ChessPosition H4 = new ChessPosition(ChessPositionValue.H4);
        public static readonly ChessPosition H5 = new ChessPosition(ChessPositionValue.H5);
        public static readonly ChessPosition H6 = new ChessPosition(ChessPositionValue.H6);
        public static readonly ChessPosition H7 = new ChessPosition(ChessPositionValue.H7);
        public static readonly ChessPosition H8 = new ChessPosition(ChessPositionValue.H8);

        public static readonly ChessPosition OUTOFBOUNDS = new ChessPosition(ChessPositionValue.OUTOFBOUNDS);

        public readonly ChessPositionValue Value;

        public ChessPosition(ChessPositionValue value)
        {
            Value = value;
        }

        #region operators

        public static implicit operator ChessPositionValue(ChessPosition Position)
        {
            return Position.Value;
        }
        public static implicit operator ChessPosition(ChessPositionValue value)
        {
            return new ChessPosition(value);
        }

        public static explicit operator int(ChessPosition Position)
        {
            return (int)Position.Value;
        }

        public static explicit operator ChessPosition(int i)
        {
            return new ChessPosition((ChessPositionValue)i);
        }
        public static bool operator ==(ChessPosition a, ChessPosition b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(ChessPosition a, ChessPosition b)
        {
            return !(a == b);
        }


        #endregion


        public bool IsLight()
        {
            bool retval = (int)this % 2 == 1;
            switch (this.GetRank().Value)
            {
                case ChessRankValue.Rank2:
                case ChessRankValue.Rank4:
                case ChessRankValue.Rank6:
                case ChessRankValue.Rank8:
                    retval = !retval;
                    break;
            }
            return retval;
        }
        public bool IsInBounds()
        {
            return (int)this >= 0 && (int)this <= 63;
        }
        public ChessRank GetRank()
        {
            //AssertPosition(pos);
            return (ChessRank)(((int)this / 8));
        }

        public ChessFile GetFile()
        {
            //AssertPosition(pos);
            return (ChessFile)((int)this % 8);
        }
        public string PositionToString()
        {
            return this.GetFile().FileToString() + this.GetRank().RankToString();
        }

        
        public ChessBitboard Bitboard()
        {
            if (this.IsInBounds())
            {
                return (ChessBitboard)((ulong)1 << this.GetIndex64());
            }
            else
            {
                return (ChessBitboard.Empty);
            }
        }

        public int DistanceTo(ChessPosition otherPosition)
        {
            int rDiff = Math.Abs((int)this.GetRank() - (int)otherPosition.GetRank());
            int fDiff = Math.Abs((int)this.GetFile() - (int)otherPosition.GetFile());
            return rDiff > fDiff ? rDiff : fDiff;
        }

        public int GetIndex64()
        {
            return (int)this;
        }



        public ChessDirection DirectionTo(ChessPosition to)
        {

            ChessRank rankfrom = this.GetRank();
            ChessFile filefrom = this.GetFile();
            ChessRank rankto = to.GetRank();
            ChessFile fileto = to.GetFile();

            if (fileto == filefrom)
            {
                if (rankfrom.Value < rankto.Value) { return ChessDirection.DirS; }
                return ChessDirection.DirN;
            }
            else if (rankfrom == rankto)
            {
                if (filefrom > fileto) { return ChessDirection.DirW; }
                return ChessDirection.DirE;
            }
            int rankchange = rankto.Value - rankfrom.Value;
            int filechange = (int)fileto - (int)filefrom;
            int rankchangeabs = rankchange > 0 ? rankchange : -rankchange;
            int filechangeabs = filechange > 0 ? filechange : -filechange;
            if ((rankchangeabs == 1 && filechangeabs == 2) || (rankchangeabs == 2 && filechangeabs == 1))
            {
                //knight direction
                return (ChessDirectionValue)((int)rankchange * 8) + (int)filechange;
            }
            else if (rankchangeabs != filechangeabs)
            {
                return (ChessDirectionValue)0;
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

        public ChessPosition PositionInDirectionUnsafe(ChessDirection dir)
        {
            return (ChessPosition)((int)this + (int)dir);
        }
        public ChessPosition PositionInDirection(ChessDirection dir)
        {
            ChessFileValue file = this.GetFile();
            ChessRankValue rank = this.GetRank();
            switch (dir.Value)
            {
                case ChessDirectionValue.DirN:
                    rank -= 1;
                    break;
                case ChessDirectionValue.DirE:
                    file += 1;
                    break;
                case ChessDirectionValue.DirS:
                    rank += 1;
                    break;
                case ChessDirectionValue.DirW:
                    file -= 1;
                    break;
                case ChessDirectionValue.DirNE:
                    rank -= 1; file += 1;
                    break;
                case ChessDirectionValue.DirSE:
                    rank += 1; file += 1;
                    break;
                case ChessDirectionValue.DirSW:
                    rank += 1; file -= 1;
                    break;
                case ChessDirectionValue.DirNW:
                    rank -= 1; file -= 1;
                    break;

                case ChessDirectionValue.DirNNE:
                    rank -= 2; file += 1;
                    break;
                case ChessDirectionValue.DirEEN:
                    rank -= 1; file += 2;
                    break;
                case ChessDirectionValue.DirEES:
                    rank += 1; file += 2;
                    break;
                case ChessDirectionValue.DirSSE:
                    rank += 2; file += 1;
                    break;

                case ChessDirectionValue.DirSSW:
                    rank += 2; file -= 1;
                    break;
                case ChessDirectionValue.DirWWS:
                    rank += 1; file -= 2;
                    break;
                case ChessDirectionValue.DirWWN:
                    rank -= 1; file -= 2;
                    break;
                case ChessDirectionValue.DirNNW:
                    rank -= 2; file -= 1;
                    break;
                default:
                    return (ChessPosition.OUTOFBOUNDS);
            }
            ChessRank r = new ChessRank(rank);
            ChessFile f = new ChessFile(file);
            if (r.IsInBounds() && f.IsInBounds())
            {
                return r.ToPosition(f);
            }
            else
            {
                return (ChessPosition.OUTOFBOUNDS);
            }

        }
        public ChessPosition Reverse()
        {
            ChessRank r = this.GetRank();
            ChessFile f = this.GetFile();
            ChessRank newrank = ChessRank.EMPTY;
            switch (r.Value)
            {
                case ChessRankValue.Rank1:
                    newrank = ChessRank.Rank8;
                    break;
                case ChessRankValue.Rank2:
                    newrank = ChessRank.Rank7;
                    break;
                case ChessRankValue.Rank3:
                    newrank = ChessRank.Rank6;
                    break;
                case ChessRankValue.Rank4:
                    newrank = ChessRank.Rank5;
                    break;
                case ChessRankValue.Rank5:
                    newrank = ChessRank.Rank4;
                    break;
                case ChessRankValue.Rank6:
                    newrank = ChessRank.Rank3;
                    break;
                case ChessRankValue.Rank7:
                    newrank = ChessRank.Rank2;
                    break;
                case ChessRankValue.Rank8:
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
