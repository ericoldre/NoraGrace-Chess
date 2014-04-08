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

    public sealed class ChessPlayerDictionary<T>
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

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;//randomly choosen prime
                foreach (var index in ChessPlayerInfo.AllPlayers)
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
    
    public static class ChessPlayerInfo
    {
        public static readonly ChessPlayer[] AllPlayers = new ChessPlayer[] { ChessPlayer.White, ChessPlayer.Black };

        public static ChessPlayer PlayerOther(this ChessPlayer player)
        {
            System.Diagnostics.Debug.Assert(player == ChessPlayer.White || player == ChessPlayer.Black);
            //AssertPlayer(player);
            return (ChessPlayer)((int)player ^ 1);
            if (player == ChessPlayer.White) { return ChessPlayer.Black; }
            else { return ChessPlayer.White; }
        }
    }
}
