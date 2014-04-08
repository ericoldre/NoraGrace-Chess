using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{

    public enum ChessPiece
    {
        WPawn = 1, WKnight = 2, WBishop = 3, WRook = 4, WQueen = 5, WKing = 6,
        BPawn = 9, BKnight = 10, BBishop = 11, BRook = 12, BQueen = 13, BKing = 14,
        EMPTY = 0
    }

    
    public sealed class ChessPieceDictionary<T> where T: new()
    {


        [System.Xml.Serialization.XmlIgnore()]
        private T[] _values = new T[ChessPieceInfo.LookupArrayLength];

        [System.Xml.Serialization.XmlIgnore()]
        public T this[ChessPiece piece]
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

        public T WPawn { get { return this[ChessPiece.WPawn]; } set { this[ChessPiece.WPawn] = value; } }
        public T WKnight { get { return this[ChessPiece.WKnight]; } set { this[ChessPiece.WKnight] = value; } }
        public T WBishop { get { return this[ChessPiece.WBishop]; } set { this[ChessPiece.WBishop] = value; } }
        public T WRook { get { return this[ChessPiece.WRook]; } set { this[ChessPiece.WRook] = value; } }
        public T WQueen { get { return this[ChessPiece.WQueen]; } set { this[ChessPiece.WQueen] = value; } }
        public T WKing { get { return this[ChessPiece.WKing]; } set { this[ChessPiece.WKing] = value; } }

        public T BPawn { get { return this[ChessPiece.BPawn]; } set { this[ChessPiece.BPawn] = value; } }
        public T BKnight { get { return this[ChessPiece.BKnight]; } set { this[ChessPiece.BKnight] = value; } }
        public T BBishop { get { return this[ChessPiece.BBishop]; } set { this[ChessPiece.BBishop] = value; } }
        public T BRook { get { return this[ChessPiece.BRook]; } set { this[ChessPiece.BRook] = value; } }
        public T BQueen { get { return this[ChessPiece.BQueen]; } set { this[ChessPiece.BQueen] = value; } }
        public T BKing { get { return this[ChessPiece.BKing]; } set { this[ChessPiece.BKing] = value; } }

        

        public IEnumerable<KeyValuePair<ChessPiece, T>> PieceValues()
        {
            foreach (ChessPiece piece in ChessPieceInfo.AllPieces)
            {
                yield return new KeyValuePair<ChessPiece, T>(piece, this[piece]);
            }
        }

        public override bool Equals(object obj)
        {
            ChessPieceDictionary<T> other = obj as ChessPieceDictionary<T>;
            if (other == null) { return false; }

            foreach (ChessPiece pos in ChessPieceInfo.AllPieces)
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
                foreach (var index in ChessPieceInfo.AllPieces)
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

    public static class ChessPieceInfo
    {
        public const int LookupArrayLength = 15;

        //private static readonly string _piecedesclookup = " PNBRQK   pnbrqk";

        public static readonly ChessPiece[] AllPieces = new ChessPiece[]{
			ChessPiece.WPawn, ChessPiece.WKnight, ChessPiece.WBishop, ChessPiece.WRook, ChessPiece.WQueen, ChessPiece.WKing,
			ChessPiece.BPawn, ChessPiece.BKnight, ChessPiece.BBishop, ChessPiece.BRook, ChessPiece.BQueen, ChessPiece.BKing};

        public static ChessPiece Parse(char c)
        {
            switch (c)
            {
                case 'P':
                    return ChessPiece.WPawn;
                case 'N':
                    return ChessPiece.WKnight;
                case 'B':
                    return ChessPiece.WBishop;
                case 'R':
                    return ChessPiece.WRook;
                case 'Q':
                    return ChessPiece.WQueen;
                case 'K':
                    return ChessPiece.WKing;
                case 'p':
                    return ChessPiece.BPawn;
                case 'n':
                    return ChessPiece.BKnight;
                case 'b':
                    return ChessPiece.BBishop;
                case 'r':
                    return ChessPiece.BRook;
                case 'q':
                    return ChessPiece.BQueen;
                case 'k':
                    return ChessPiece.BKing;
                default:
                    throw new ArgumentException(c.ToString() + " is not a valid piece");
            }
        }

        public static ChessPiece ParseAsPiece(this char c, ChessPlayer player)
        {
            return Parse(c).ToPieceType().ForPlayer(player);
        }

        public static string PieceToString(this ChessPiece piece)
        {
            switch (piece)
            {
                case ChessPiece.WPawn:
                    return "P";
                case ChessPiece.WKnight:
                    return "N";
                case ChessPiece.WBishop:
                    return "B";
                case ChessPiece.WRook:
                    return "R";
                case ChessPiece.WQueen:
                    return "Q";
                case ChessPiece.WKing:
                    return "K";
                case ChessPiece.BPawn:
                    return "p";
                case ChessPiece.BKnight:
                    return "n";
                case ChessPiece.BBishop:
                    return "b";
                case ChessPiece.BRook:
                    return "r";
                case ChessPiece.BQueen:
                    return "q";
                case ChessPiece.BKing:
                    return "k";
                default:
                    throw new ArgumentException("not a valid piece");
            }
        }


        public static int PieceValBasic(this ChessPiece piece)
        {
            switch (piece)
            {
                case ChessPiece.WPawn:
                case ChessPiece.BPawn:
                    return 100;
                case ChessPiece.WKnight:
                case ChessPiece.BKnight:
                    return 300;
                case ChessPiece.WBishop:
                case ChessPiece.BBishop:
                    return 300;
                case ChessPiece.WRook:
                case ChessPiece.BRook:
                    return 500;
                case ChessPiece.WQueen:
                case ChessPiece.BQueen:
                    return 900;
                case ChessPiece.WKing:
                case ChessPiece.BKing:
                    return 10000;
                default:
                    return 0;
            }
        }

        public static ChessPieceType ToPieceType(this ChessPiece piece)
        {
            switch (piece)
            {
                case ChessPiece.WPawn:
                case ChessPiece.BPawn:
                    return ChessPieceType.Pawn;
                case ChessPiece.WKnight:
                case ChessPiece.BKnight:
                    return ChessPieceType.Knight;
                case ChessPiece.WBishop:
                case ChessPiece.BBishop:
                    return ChessPieceType.Bishop;
                case ChessPiece.WRook:
                case ChessPiece.BRook:
                    return ChessPieceType.Rook;
                case ChessPiece.WQueen:
                case ChessPiece.BQueen:
                    return ChessPieceType.Queen;
                case ChessPiece.WKing:
                case ChessPiece.BKing:
                    return ChessPieceType.King;
                default:
                    throw new ArgumentException("invalid chess piece");
            }
        }

        public static ChessPiece ToOppositePlayer(this ChessPiece piece)
        {
            switch (piece)
            {
                case ChessPiece.WPawn:
                    return ChessPiece.BPawn;
                case ChessPiece.WKnight:
                    return ChessPiece.BKnight;
                case ChessPiece.WBishop:
                    return ChessPiece.BBishop;
                case ChessPiece.WRook:
                    return ChessPiece.BRook;
                case ChessPiece.WQueen:
                    return ChessPiece.BQueen;
                case ChessPiece.WKing:
                    return ChessPiece.BKing;

                case ChessPiece.BPawn:
                    return ChessPiece.WPawn;
                case ChessPiece.BKnight:
                    return ChessPiece.WKnight;
                case ChessPiece.BBishop:
                    return ChessPiece.WBishop;
                case ChessPiece.BRook:
                    return ChessPiece.WRook;
                case ChessPiece.BQueen:
                    return ChessPiece.WQueen;
                case ChessPiece.BKing:
                    return ChessPiece.WKing;
                default:
                    return ChessPiece.EMPTY;
            }
        }
        public static ChessPlayer PieceToPlayer(this ChessPiece piece)
        {
            switch (piece)
            {
                case ChessPiece.WPawn:
                case ChessPiece.WKnight:
                case ChessPiece.WBishop:
                case ChessPiece.WRook:
                case ChessPiece.WQueen:
                case ChessPiece.WKing:
                    return ChessPlayer.White;
                case ChessPiece.BPawn:
                case ChessPiece.BKnight:
                case ChessPiece.BBishop:
                case ChessPiece.BRook:
                case ChessPiece.BQueen:
                case ChessPiece.BKing:
                    return ChessPlayer.Black;
                default:
                    return ChessPlayer.None;
            }
        }
        public static bool PieceIsSliderRook(this ChessPiece piece)
        {
            switch (piece)
            {
                case ChessPiece.WRook:
                case ChessPiece.WQueen:
                case ChessPiece.BRook:
                case ChessPiece.BQueen:
                    return true;
                default:
                    return false;
            }
        }
        public static bool PieceIsSliderBishop(this ChessPiece piece)
        {
            switch (piece)
            {
                case ChessPiece.WBishop:
                case ChessPiece.WQueen:
                case ChessPiece.BBishop:
                case ChessPiece.BQueen:
                    return true;
                default:
                    return false;
            }
        }

    }
}
