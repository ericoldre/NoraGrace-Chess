using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{
    public enum ChessPlayer
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
    
    public static class ExtensionsChessPlayer
    {
        public static ChessPlayer PlayerOther(this ChessPlayer player)
        {
            //AssertPlayer(player);
            if (player == ChessPlayer.White) { return ChessPlayer.Black; }
            else { return ChessPlayer.White; }
        }
    }
}
