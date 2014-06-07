using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{

    public enum Piece
    {
        WPawn = 1, WKnight = 2, WBishop = 3, WRook = 4, WQueen = 5, WKing = 6,
        BPawn = 9, BKnight = 10, BBishop = 11, BRook = 12, BQueen = 13, BKing = 14,
        EMPTY = 0
    }

    
    public sealed class PieceDictionary<T> where T: new()
    {


        [System.Xml.Serialization.XmlIgnore()]
        private T[] _values = new T[PieceInfo.LookupArrayLength];

        [System.Xml.Serialization.XmlIgnore()]
        public T this[Piece piece]
        {
            get
            {
                if (_values[(int)piece] == null) { _values[(int)piece] = new T(); }
                return _values[(int)piece];
            }
            set
            {
                _values[(int)piece] = value;
            }
        }

        public T WPawn { get { return this[Piece.WPawn]; } set { this[Piece.WPawn] = value; } }
        public T WKnight { get { return this[Piece.WKnight]; } set { this[Piece.WKnight] = value; } }
        public T WBishop { get { return this[Piece.WBishop]; } set { this[Piece.WBishop] = value; } }
        public T WRook { get { return this[Piece.WRook]; } set { this[Piece.WRook] = value; } }
        public T WQueen { get { return this[Piece.WQueen]; } set { this[Piece.WQueen] = value; } }
        public T WKing { get { return this[Piece.WKing]; } set { this[Piece.WKing] = value; } }

        public T BPawn { get { return this[Piece.BPawn]; } set { this[Piece.BPawn] = value; } }
        public T BKnight { get { return this[Piece.BKnight]; } set { this[Piece.BKnight] = value; } }
        public T BBishop { get { return this[Piece.BBishop]; } set { this[Piece.BBishop] = value; } }
        public T BRook { get { return this[Piece.BRook]; } set { this[Piece.BRook] = value; } }
        public T BQueen { get { return this[Piece.BQueen]; } set { this[Piece.BQueen] = value; } }
        public T BKing { get { return this[Piece.BKing]; } set { this[Piece.BKing] = value; } }

        

        public IEnumerable<KeyValuePair<Piece, T>> PieceValues()
        {
            foreach (Piece piece in PieceInfo.AllPieces)
            {
                yield return new KeyValuePair<Piece, T>(piece, this[piece]);
            }
        }

        public override bool Equals(object obj)
        {
            PieceDictionary<T> other = obj as PieceDictionary<T>;
            if (other == null) { return false; }

            foreach (Piece pos in PieceInfo.AllPieces)
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
                foreach (var index in PieceInfo.AllPieces)
                {
                    T field = this[index];
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

    public static class PieceInfo
    {
        public const int LookupArrayLength = 15;

        //private static readonly string _piecedesclookup = " PNBRQK   pnbrqk";

        public static readonly Piece[] AllPieces = new Piece[]{
			Piece.WPawn, Piece.WKnight, Piece.WBishop, Piece.WRook, Piece.WQueen, Piece.WKing,
			Piece.BPawn, Piece.BKnight, Piece.BBishop, Piece.BRook, Piece.BQueen, Piece.BKing};

        public static Piece Parse(char c)
        {
            switch (c)
            {
                case 'P':
                    return Piece.WPawn;
                case 'N':
                    return Piece.WKnight;
                case 'B':
                    return Piece.WBishop;
                case 'R':
                    return Piece.WRook;
                case 'Q':
                    return Piece.WQueen;
                case 'K':
                    return Piece.WKing;
                case 'p':
                    return Piece.BPawn;
                case 'n':
                    return Piece.BKnight;
                case 'b':
                    return Piece.BBishop;
                case 'r':
                    return Piece.BRook;
                case 'q':
                    return Piece.BQueen;
                case 'k':
                    return Piece.BKing;
                default:
                    throw new ArgumentException(c.ToString() + " is not a valid piece");
            }
        }

        public static Piece ParseAsPiece(this char c, Player player)
        {
            return Parse(c).ToPieceType().ForPlayer(player);
        }

        public static string PieceToString(this Piece piece)
        {
            switch (piece)
            {
                case Piece.WPawn:
                    return "P";
                case Piece.WKnight:
                    return "N";
                case Piece.WBishop:
                    return "B";
                case Piece.WRook:
                    return "R";
                case Piece.WQueen:
                    return "Q";
                case Piece.WKing:
                    return "K";
                case Piece.BPawn:
                    return "p";
                case Piece.BKnight:
                    return "n";
                case Piece.BBishop:
                    return "b";
                case Piece.BRook:
                    return "r";
                case Piece.BQueen:
                    return "q";
                case Piece.BKing:
                    return "k";
                default:
                    throw new ArgumentException("not a valid piece");
            }
        }


        public static int PieceValBasic(this Piece piece)
        {
            switch (piece)
            {
                case Piece.WPawn:
                case Piece.BPawn:
                    return 100;
                case Piece.WKnight:
                case Piece.BKnight:
                    return 300;
                case Piece.WBishop:
                case Piece.BBishop:
                    return 300;
                case Piece.WRook:
                case Piece.BRook:
                    return 500;
                case Piece.WQueen:
                case Piece.BQueen:
                    return 900;
                case Piece.WKing:
                case Piece.BKing:
                    return 10000;
                default:
                    return 0;
            }
        }

        public static PieceType ToPieceType(this Piece piece)
        {
            return (PieceType)((int)piece & 7);
        }

        public static Piece ToOppositePlayer(this Piece piece)
        {
            if (piece == Piece.EMPTY) { return Piece.EMPTY; }

            return (Piece)((int)piece ^ 8);
        }
        public static Player PieceToPlayer(this Piece piece)
        {
            System.Diagnostics.Debug.Assert(piece != Piece.EMPTY);
            return (Player)((int)piece >> 3);

        }
        public static bool PieceIsSliderRook(this Piece piece)
        {
            switch (piece)
            {
                case Piece.WRook:
                case Piece.WQueen:
                case Piece.BRook:
                case Piece.BQueen:
                    return true;
                default:
                    return false;
            }
        }
        public static bool PieceIsSliderBishop(this Piece piece)
        {
            switch (piece)
            {
                case Piece.WBishop:
                case Piece.WQueen:
                case Piece.BBishop:
                case Piece.BQueen:
                    return true;
                default:
                    return false;
            }
        }

    }
}
