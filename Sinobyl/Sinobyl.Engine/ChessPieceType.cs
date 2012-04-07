using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{
    public enum ChessPieceType
    {
        Pawn = 0, Knight = 1, Bishop = 2, Rook = 3, Queen = 4, King = 5
    }

    public class ChessPieceTypeDictionary<T>
    {
        public T[] _values = new T[6];

        public T this[ChessPieceType piecetype]
        {
            get
            {
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
    }
}
