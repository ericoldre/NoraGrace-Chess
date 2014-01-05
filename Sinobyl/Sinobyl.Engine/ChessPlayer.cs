using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{
    public enum ChessPlayerValue
    {
        None = -1,
        White = 0, Black = 1
    }

    public class ChessPlayerDictionary<T>
    {
        private T[] _values = new T[2];

        [System.Xml.Serialization.XmlIgnore()]
        public T this[ChessPlayer player]
        {
            get
            {
                return _values[(int)player];
            }
            set
            {
                _values[(int)player] = value;
            }
        }

        public T White { get { return this[ChessPlayer.White]; } set { this[ChessPlayer.White] = value; } }
        public T Black { get { return this[ChessPlayer.Black]; } set { this[ChessPlayer.Black] = value; } }

        public override bool Equals(object obj)
        {
            ChessPlayerDictionary<T> other = obj as ChessPlayerDictionary<T>;
            if (other == null) { return false; }
            if (!this.White.Equals(other.White)) { return false; }
            if (!this.Black.Equals(other.Black)) { return false; }
            return true;
        }
    }
    
    public struct ChessPlayer
    {
        public readonly ChessPlayerValue Value;
        public static readonly ChessPlayer None = new ChessPlayer(ChessPlayerValue.None);
        public static readonly ChessPlayer White = new ChessPlayer(ChessPlayerValue.White);
        public static readonly ChessPlayer Black = new ChessPlayer(ChessPlayerValue.Black);

        public ChessPlayer(ChessPlayerValue value)
        {
            Value = value;
        }

        #region operators

        public static implicit operator ChessPlayerValue(ChessPlayer player)
        {
            return player.Value;
        }
        public static implicit operator ChessPlayer(ChessPlayerValue value)
        {
            return new ChessPlayer(value);
        }

        public static explicit operator int(ChessPlayer Player)
        {
            return (int)Player.Value;
        }
        public static explicit operator ChessPlayer(int i)
        {
            return new ChessPlayer((ChessPlayerValue)i);
        }

        public static bool operator ==(ChessPlayer a, ChessPlayer b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(ChessPlayer a, ChessPlayer b)
        {
            return !(a == b);
        }

        #endregion

        public ChessPlayer PlayerOther()
        {
            //AssertPlayer(player);
            if (this == ChessPlayer.White) { return ChessPlayer.Black; }
            else { return ChessPlayer.White; }
        }
    }
}
