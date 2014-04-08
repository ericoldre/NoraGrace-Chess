﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{
    public enum ChessPieceType
    {
        Pawn = 1, Knight = 2, Bishop = 3, Rook = 4, Queen = 5, King = 6
    }

    public static class ChessPieceTypeInfo
    {


        public static readonly ChessPieceType[] AllPieceTypes = new ChessPieceType[]{
            ChessPieceType.Pawn, ChessPieceType.Knight, ChessPieceType.Bishop, ChessPieceType.Rook, ChessPieceType.Queen, ChessPieceType.King};


        public static ChessPiece ForPlayer(this ChessPieceType type, ChessPlayer player)
        {
            if (player == ChessPlayer.White)
            {
                switch (type)
                {
                    case ChessPieceType.Pawn:
                        return ChessPiece.WPawn;
                    case ChessPieceType.Knight:
                        return ChessPiece.WKnight;
                    case ChessPieceType.Bishop:
                        return ChessPiece.WBishop;
                    case ChessPieceType.Rook:
                        return ChessPiece.WRook;
                    case ChessPieceType.Queen:
                        return ChessPiece.WQueen;
                    case ChessPieceType.King:
                        return ChessPiece.WKing;
                }
            }
            else
            {
                switch (type)
                {
                    case ChessPieceType.Pawn:
                        return ChessPiece.BPawn;
                    case ChessPieceType.Knight:
                        return ChessPiece.BKnight;
                    case ChessPieceType.Bishop:
                        return ChessPiece.BBishop;
                    case ChessPieceType.Rook:
                        return ChessPiece.BRook;
                    case ChessPieceType.Queen:
                        return ChessPiece.BQueen;
                    case ChessPieceType.King:
                        return ChessPiece.BKing;
                }
            }
            throw new ArgumentException("invalid piece or player");
        }
    }
    public class ChessPieceTypeDictionary<T> where T:new()
    {
        [System.Xml.Serialization.XmlIgnore()]
        public T[] _values = new T[7];

        [System.Xml.Serialization.XmlIgnore()]
        public T this[ChessPieceType piecetype]
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

        public T Pawn { get { return this[ChessPieceType.Pawn]; } set { this[ChessPieceType.Pawn] = value; } }
        public T Knight { get { return this[ChessPieceType.Knight]; } set { this[ChessPieceType.Knight] = value; } }
        public T Bishop { get { return this[ChessPieceType.Bishop]; } set { this[ChessPieceType.Bishop] = value; } }
        public T Rook { get { return this[ChessPieceType.Rook]; } set { this[ChessPieceType.Rook] = value; } }
        public T Queen { get { return this[ChessPieceType.Queen]; } set { this[ChessPieceType.Queen] = value; } }
        public T King { get { return this[ChessPieceType.King]; } set { this[ChessPieceType.King] = value; } }


        public IEnumerable<KeyValuePair<ChessPiece, T>> PieceValues()
        {
            foreach (ChessPiece piece in ChessPieceInfo.AllPieces)
            {
                yield return new KeyValuePair<ChessPiece, T>(piece, this[piece.ToPieceType()]);
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
                foreach (var index in ChessPieceTypeInfo.AllPieceTypes)
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
