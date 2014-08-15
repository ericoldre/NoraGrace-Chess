using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoraGrace.Engine.Evaluation
{
    public enum GameStage
    {
        Opening = 0, Endgame = 1
    }

    public static class GameStageUtil
    {

        public static readonly GameStage[] AllGameStages = new GameStage[] { GameStage.Opening, GameStage.Endgame };

        public static GameStage Other(this GameStage stage)
        {
            if (stage == GameStage.Opening)
            {
                return GameStage.Endgame;
            }
            else
            {
                return GameStage.Opening;
            }
        }
    }

    public class ChessGameStageDictionary<T> where T:new()
    {
        [System.Xml.Serialization.XmlIgnore()]
        public T[] _values = new T[2];

        public T this[GameStage stage]
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

        public T Opening { get { return this[GameStage.Opening]; } set { this[GameStage.Opening] = value; } }
        public T Endgame { get { return this[GameStage.Endgame]; } set { this[GameStage.Endgame] = value; } }
    }
}
