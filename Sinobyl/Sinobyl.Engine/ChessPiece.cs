using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{

    public enum ChessPieceValue
    {
        WPawn = 0, WKnight = 1, WBishop = 2, WRook = 3, WQueen = 4, WKing = 5,
        BPawn = 6, BKnight = 7, BBishop = 8, BRook = 9, BQueen = 10, BKing = 11,
        EMPTY = 12
    }



    public class ChessPieceDictionary<T> where T: new()
    {


        [System.Xml.Serialization.XmlIgnore()]
        private T[] _values = new T[12];

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
            foreach (ChessPiece piece in Chess.AllPieces)
            {
                yield return new KeyValuePair<ChessPiece, T>(piece, this[piece]);
            }
        }

        public override bool Equals(object obj)
        {
            ChessPieceDictionary<T> other = obj as ChessPieceDictionary<T>;
            if (other == null) { return false; }

            foreach (ChessPiece pos in Chess.AllPieces)
            {
                if (!this[pos].Equals(other[pos]))
                {
                    return false;
                }
            }
            return true;
        }
    }

    public struct ChessPiece
    {
        private static readonly string _piecedesclookup = "PNBRQKpnbrqk";

        public static readonly ChessPiece WPawn = new ChessPiece(ChessPieceValue.WPawn);
        public static readonly ChessPiece WKnight = new ChessPiece(ChessPieceValue.WKnight);
        public static readonly ChessPiece WBishop = new ChessPiece(ChessPieceValue.WBishop);
        public static readonly ChessPiece WRook = new ChessPiece(ChessPieceValue.WRook);
        public static readonly ChessPiece WQueen = new ChessPiece(ChessPieceValue.WQueen);
        public static readonly ChessPiece WKing = new ChessPiece(ChessPieceValue.WKing);
        public static readonly ChessPiece BPawn = new ChessPiece(ChessPieceValue.BPawn);
        public static readonly ChessPiece BKnight = new ChessPiece(ChessPieceValue.BKnight);
        public static readonly ChessPiece BBishop = new ChessPiece(ChessPieceValue.BBishop);
        public static readonly ChessPiece BRook = new ChessPiece(ChessPieceValue.BRook);
        public static readonly ChessPiece BQueen = new ChessPiece(ChessPieceValue.BQueen);
        public static readonly ChessPiece BKing = new ChessPiece(ChessPieceValue.BKing);
        public static readonly ChessPiece EMPTY = new ChessPiece(ChessPieceValue.EMPTY);


        public readonly ChessPieceValue Value;

        public ChessPiece(ChessPieceValue value)
        {
            Value = value;
        }

        #region operators

        public static implicit operator ChessPieceValue(ChessPiece piece)
        {
            return piece.Value;
        }
        public static implicit operator ChessPiece(ChessPieceValue value)
        {
            return new ChessPiece(value);
        }

        public static explicit operator int(ChessPiece piece)
        {
            return (int)piece.Value;
        }
        public static explicit operator ChessPiece(int i)
        {
            return new ChessPiece((ChessPieceValue)i);
        }
        public static bool operator ==(ChessPiece a, ChessPiece b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(ChessPiece a, ChessPiece b)
        {
            return !(a == b);
        }


        #endregion

        public static ChessPiece ParseAsPiece(char c)
        {
            int idx = _piecedesclookup.IndexOf(c);
            if (idx < 0) { throw new ChessException(c.ToString() + " is not a valid piece"); }
            return (ChessPiece)idx;
        }

        public static ChessPiece ParseAsPiece(char c, ChessPlayer player)
        {
            ChessPiece tmppiece = ParseAsPiece(c.ToString().ToUpper()[0]);
            if (player == ChessPlayer.White)
            {
                return tmppiece;
            }
            else
            {
                return (ChessPiece)((int)tmppiece + 6);
            }
        }

        public string PieceToString()
        {
            //AssertPiece(piece);
            return _piecedesclookup.Substring((int)this, 1);
        }


        public int PieceValBasic()
        {
            switch (this.Value)
            {
                case ChessPieceValue.WPawn:
                case ChessPieceValue.BPawn:
                    return 100;
                case ChessPieceValue.WKnight:
                case ChessPieceValue.BKnight:
                    return 300;
                case ChessPieceValue.WBishop:
                case ChessPieceValue.BBishop:
                    return 300;
                case ChessPieceValue.WRook:
                case ChessPieceValue.BRook:
                    return 500;
                case ChessPieceValue.WQueen:
                case ChessPieceValue.BQueen:
                    return 900;
                case ChessPieceValue.WKing:
                case ChessPieceValue.BKing:
                    return 10000;
                default:
                    return 0;
            }
        }

        public ChessPieceType ToPieceType()
        {
            switch (this.Value)
            {
                case ChessPieceValue.WPawn:
                case ChessPieceValue.BPawn:
                    return ChessPieceType.Pawn;
                case ChessPieceValue.WKnight:
                case ChessPieceValue.BKnight:
                    return ChessPieceType.Knight;
                case ChessPieceValue.WBishop:
                case ChessPieceValue.BBishop:
                    return ChessPieceType.Bishop;
                case ChessPieceValue.WRook:
                case ChessPieceValue.BRook:
                    return ChessPieceType.Rook;
                case ChessPieceValue.WQueen:
                case ChessPieceValue.BQueen:
                    return ChessPieceType.Queen;
                case ChessPieceValue.WKing:
                case ChessPieceValue.BKing:
                    return ChessPieceType.King;
                default:
                    throw new ArgumentException("invalid chess piece");
            }
        }

        public ChessPiece ToOppositePlayer()
        {
            switch (this.Value)
            {
                case ChessPieceValue.WPawn:
                    return ChessPiece.BPawn;
                case ChessPieceValue.WKnight:
                    return ChessPiece.BKnight;
                case ChessPieceValue.WBishop:
                    return ChessPiece.BBishop;
                case ChessPieceValue.WRook:
                    return ChessPiece.BRook;
                case ChessPieceValue.WQueen:
                    return ChessPiece.BQueen;
                case ChessPieceValue.WKing:
                    return ChessPiece.BKing;

                case ChessPieceValue.BPawn:
                    return ChessPiece.WPawn;
                case ChessPieceValue.BKnight:
                    return ChessPiece.WKnight;
                case ChessPieceValue.BBishop:
                    return ChessPiece.WBishop;
                case ChessPieceValue.BRook:
                    return ChessPiece.WRook;
                case ChessPieceValue.BQueen:
                    return ChessPiece.WQueen;
                case ChessPieceValue.BKing:
                    return ChessPiece.WKing;
                default:
                    return ChessPiece.EMPTY;
            }
        }
        public ChessPlayer PieceToPlayer()
        {
            if (this == ChessPiece.EMPTY) { return ChessPlayer.None; }
            //AssertPiece(piece);
            if ((int)this >= 0 && (int)this <= 5)
            {
                return ChessPlayer.White;
            }
            else
            {
                return ChessPlayer.Black;
            }
        }
        public bool PieceIsSliderRook()
        {
            switch (this.Value)
            {
                case ChessPieceValue.WRook:
                case ChessPieceValue.WQueen:
                case ChessPieceValue.BRook:
                case ChessPieceValue.BQueen:
                    return true;
                default:
                    return false;
            }
        }
        public bool PieceIsSliderBishop()
        {
            switch (this.Value)
            {
                case ChessPieceValue.WBishop:
                case ChessPieceValue.WQueen:
                case ChessPieceValue.BBishop:
                case ChessPieceValue.BQueen:
                    return true;
                default:
                    return false;
            }
        }

    }
}

