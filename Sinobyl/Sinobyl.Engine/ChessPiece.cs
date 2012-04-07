using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{

    public enum ChessPiece
    {
        EMPTY = -1,
        WPawn = 0, WKnight = 1, WBishop = 2, WRook = 3, WQueen = 4, WKing = 5,
        BPawn = 6, BKnight = 7, BBishop = 8, BRook = 9, BQueen = 10, BKing = 11,
        OOB = 12
    }

    public class ChessPieceDictionary<T>
    {
        public T[] _values = new T[12];

        public T this[ChessPiece piece]
        {
            get
            {
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

    public static class ExtensionsChessPiece
    {
        private static readonly string _piecedesclookup = "PNBRQKpnbrqk";

        public static ChessPiece ParseAsPiece(this char c)
        {
            int idx = _piecedesclookup.IndexOf(c);
            if (idx < 0) { throw new ChessException(c.ToString() + " is not a valid piece"); }
            return (ChessPiece)idx;
        }

        public static ChessPiece ParseAsPiece(this char c, ChessPlayer player)
        {
            ChessPiece tmppiece = c.ToString().ToUpper()[0].ParseAsPiece();
            if (player == ChessPlayer.White)
            {
                return tmppiece;
            }
            else
            {
                return (ChessPiece)((int)tmppiece + 6);
            }
        }

        public static string PieceToString(this ChessPiece piece)
        {
            //AssertPiece(piece);
            return _piecedesclookup.Substring((int)piece, 1);
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
            if (piece == ChessPiece.EMPTY) { return ChessPlayer.None; }
            //AssertPiece(piece);
            if ((int)piece >= 0 && (int)piece <= 5)
            {
                return ChessPlayer.White;
            }
            else
            {
                return ChessPlayer.Black;
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
