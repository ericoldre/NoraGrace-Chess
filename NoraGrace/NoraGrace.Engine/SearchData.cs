using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoraGrace.Engine
{
    public class SearchData
    {
        public Evaluation.Evaluator Evaluator { get; private set; }
        public StaticExchange SEE { get; private set; }
        public MovePicker.MoveHistory MoveHistory { get; private set; }
        public TranspositionTable TransTable { get; private set; }

        private readonly List<PlyData> _plyData = new List<PlyData>();

        public SearchData(Evaluation.Evaluator evaluator)
        {
            Evaluator = evaluator;
            SEE = new StaticExchange();
            MoveHistory = new MovePicker.MoveHistory();

            _plyData = new List<PlyData>();
            while (_plyData.Count <= Search.MAX_PLY + 1)
            {
                _plyData.Add(new PlyData(this));
            }

        }

        public PlyData this[int ply]
        {
            get 
            {
                return _plyData[ply]; 
            }
        }

    }

    public class PlyData
    {
        public MovePicker MoveGenerator { get; private set; }
        public Evaluation.EvalResults EvalResults { get; private set; }
        public SearchData SearchData { get; private set; }
        public AttackInfo AttacksWhite { get; private set; }
        public AttackInfo AttacksBlack { get; private set; }

        public PlyData(SearchData searchData)
        {
            SearchData = searchData;
            MoveGenerator = new MovePicker(searchData.MoveHistory, searchData.SEE);
            EvalResults = new Evaluation.EvalResults();
            AttacksWhite = new AttackInfo(Player.White);
            AttacksBlack = new AttackInfo(Player.Black);
        }

    }
}
