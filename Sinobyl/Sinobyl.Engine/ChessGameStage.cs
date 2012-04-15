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

    public static class ExtentionsChessGameStage
    {
        public static ChessGameStage Other(this ChessGameStage stage)
        {
            if (stage == ChessGameStage.Opening)
            {
                return ChessGameStage.Endgame;
            }
            else
            {
                return ChessGameStage.Opening;
            }
        }
    }

    public class ChessGameStageDictionary<T> where T:new()
    {
        [System.Xml.Serialization.XmlIgnore()]
        public T[] _values = new T[2];

        public T this[ChessGameStage stage]
        {
            get
            {
                if (_values[(int)stage] == null) { _values[(int)stage] = new T(); }
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
