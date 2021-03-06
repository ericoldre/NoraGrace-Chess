﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoraGrace.Engine
{
    public enum PieceType
    {
        EMPTY = 0, Pawn = 1, Knight = 2, Bishop = 3, Rook = 4, Queen = 5, King = 6
    }

    public static class PieceTypeUtil
    {
        public const int LookupArrayLength = 7;

        public static readonly PieceType[] AllPieceTypes = new PieceType[]{
            PieceType.Pawn, PieceType.Knight, PieceType.Bishop, PieceType.Rook, PieceType.Queen, PieceType.King};

        public static int BasicVal(this PieceType type)
        {
            switch (type)
            {
                case PieceType.EMPTY:
                    return 0;
                case PieceType.Pawn:
                    return 100;
                case PieceType.Knight:
                case PieceType.Bishop:
                    return 300;
                case PieceType.Rook:
                    return 500;
                case PieceType.Queen:
                    return 900;
                case PieceType.King:
                    return 10000;
                default:
                    return 0;
            }
        }

        public static int MaximumMoves(this PieceType type)
        {
            switch (type)
            {
                case PieceType.EMPTY:
                    return 0;
                case PieceType.Pawn:
                    return 4;
                case PieceType.Knight:
                    return 8;
                case PieceType.Bishop:
                    return 13;
                case PieceType.Rook:
                    return 14;
                case PieceType.Queen:
                    return 27;
                case PieceType.King:
                    return 8;
                default:
                    return 0;
            }
        }

        public static bool IsSliderHorizontal(this PieceType pt)
        {
            return pt == PieceType.Rook || pt == PieceType.Queen;
        }

        public static bool IsSliderDiag(this PieceType pt)
        {
            return pt == PieceType.Bishop || pt == PieceType.Queen;
        }

        public static Piece ForPlayer(this PieceType type, Player player)
        {
            System.Diagnostics.Debug.Assert(player == Player.White || player == Player.Black);
            System.Diagnostics.Debug.Assert(
                type == PieceType.Pawn
                || type == PieceType.Knight
                || type == PieceType.Bishop
                || type == PieceType.Rook
                || type == PieceType.Queen
                || type == PieceType.King);

            return (Piece)((int)type | ((int)player << 3));
            //if (player == ChessPlayer.White)
            //{
            //    switch (type)
            //    {
            //        case ChessPieceType.Pawn:
            //            return ChessPiece.WPawn;
            //        case ChessPieceType.Knight:
            //            return ChessPiece.WKnight;
            //        case ChessPieceType.Bishop:
            //            return ChessPiece.WBishop;
            //        case ChessPieceType.Rook:
            //            return ChessPiece.WRook;
            //        case ChessPieceType.Queen:
            //            return ChessPiece.WQueen;
            //        case ChessPieceType.King:
            //            return ChessPiece.WKing;
            //    }
            //}
            //else
            //{
            //    switch (type)
            //    {
            //        case ChessPieceType.Pawn:
            //            return ChessPiece.BPawn;
            //        case ChessPieceType.Knight:
            //            return ChessPiece.BKnight;
            //        case ChessPieceType.Bishop:
            //            return ChessPiece.BBishop;
            //        case ChessPieceType.Rook:
            //            return ChessPiece.BRook;
            //        case ChessPieceType.Queen:
            //            return ChessPiece.BQueen;
            //        case ChessPieceType.King:
            //            return ChessPiece.BKing;
            //    }
            //}
            //throw new ArgumentException("invalid piece or player");
        }
    }
    public class ChessPieceTypeDictionary<T> where T:new()
    {
        [System.Xml.Serialization.XmlIgnore()]
        public T[] _values = new T[7];

        [System.Xml.Serialization.XmlIgnore()]
        public T this[PieceType piecetype]
        {
            get
            {
                if (_values[(int)piecetype] == null) { _values[(int)piecetype] = new T(); }
                return _values[(int)piecetype];
            }
            set
            {
                _values[(int)piecetype] = value;
            }
        }

        public T Pawn { get { return this[PieceType.Pawn]; } set { this[PieceType.Pawn] = value; } }
        public T Knight { get { return this[PieceType.Knight]; } set { this[PieceType.Knight] = value; } }
        public T Bishop { get { return this[PieceType.Bishop]; } set { this[PieceType.Bishop] = value; } }
        public T Rook { get { return this[PieceType.Rook]; } set { this[PieceType.Rook] = value; } }
        public T Queen { get { return this[PieceType.Queen]; } set { this[PieceType.Queen] = value; } }
        public T King { get { return this[PieceType.King]; } set { this[PieceType.King] = value; } }


        public IEnumerable<KeyValuePair<Piece, T>> PieceValues()
        {
            foreach (Piece piece in PieceUtil.AllPieces)
            {
                yield return new KeyValuePair<Piece, T>(piece, this[piece.ToPieceType()]);
            }
        }

        public override bool Equals(object obj)
        {
            ChessPieceTypeDictionary<T> other = obj as ChessPieceTypeDictionary<T>;
            if (other == null) { return false; }

            if (!this.Pawn.Equals(other.Pawn)) { return false; }
            if (!this.Knight.Equals(other.Knight)) { return false; }
            if (!this.Bishop.Equals(other.Bishop)) { return false; }
            if (!this.Rook.Equals(other.Rook)) { return false; }
            if (!this.Queen.Equals(other.Queen)) { return false; }
            if (!this.King.Equals(other.King)) { return false; }
            return true;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;//randomly choosen prime
                foreach (var index in PieceTypeUtil.AllPieceTypes)
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
}
