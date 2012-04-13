using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{
    public enum ChessGameStage
    {
        Opening = 0, Endgame = 1
    }

    public class ChessGameStageDictionary<T>
    {
        [System.Xml.Serialization.XmlIgnore()]
        public T[] _values = new T[2];

        public T this[ChessGameStage stage]
        {
            get
            {
                return _values[(int)stage];
            }
            set
            {
                _values[(int)stage] = value;
            }
        }

        public T Opening { get { return this[ChessGameStage.Opening]; } set { this[ChessGameStage.Opening] = value; } }
        public T Endgame { get { return this[ChessGameStage.Endgame]; } set { this[ChessGameStage.Endgame] = value; } }
    }
}
