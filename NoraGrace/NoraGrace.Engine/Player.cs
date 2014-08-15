using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoraGrace.Engine
{
    public enum Player
    {
        None = -1,
        White = 0, Black = 1
    }

    public sealed class PlayerDictionary<T>
    {
        private T[] _values = new T[2];

        [System.Xml.Serialization.XmlIgnore()]
        public T this[Player player]
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

        public T White { get { return this[Player.White]; } set { this[Player.White] = value; } }
        public T Black { get { return this[Player.Black]; } set { this[Player.Black] = value; } }

        public override bool Equals(object obj)
        {
            PlayerDictionary<T> other = obj as PlayerDictionary<T>;
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
                foreach (var index in PlayerUtil.AllPlayers)
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
    
    public static class PlayerUtil
    {
        public static readonly Player[] AllPlayers = new Player[] { Player.White, Player.Black };

        public static Player PlayerOther(this Player player)
        {
            System.Diagnostics.Debug.Assert(player == Player.White || player == Player.Black);
            return (Player)((int)player ^ 1);
        }

        public static Direction MyNorth(this Player player)
        {
            System.Diagnostics.Debug.Assert(player == Player.White || player == Player.Black);
            return player == Player.White ? Direction.DirN : Direction.DirS;
        }

        public static readonly Rank[][] _myRanks = new Rank[][]
        {
            new Rank[] {Rank.Rank8, Rank.Rank7, Rank.Rank6, Rank.Rank5, Rank.Rank4, Rank.Rank3, Rank.Rank2, Rank.Rank1},
            new Rank[] {Rank.Rank1, Rank.Rank2, Rank.Rank3, Rank.Rank4, Rank.Rank5, Rank.Rank6, Rank.Rank7, Rank.Rank8}
        };

        public static Rank MyRank(this Player player, Rank rank)
        {
            return _myRanks[(int)player][(int)rank];
        }

        public static Rank MyRank2(this Player player)
        {
            System.Diagnostics.Debug.Assert(player == Player.White || player == Player.Black);
            return player == Player.White ? Rank.Rank2 : Rank.Rank7;
        }

        public static Rank MyRank8(this Player player)
        {
            System.Diagnostics.Debug.Assert(player == Player.White || player == Player.Black);
            return player == Player.White ? Rank.Rank8 : Rank.Rank1;
        }
    }
}
