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

        private readonly List<SearchPlyData> _plyData = new List<SearchPlyData>();

        public SearchData(Evaluation.Evaluator evaluator)
        {
            Evaluator = evaluator;
            SEE = new StaticExchange();
            MoveHistory = new MovePicker.MoveHistory();

            _plyData = new List<SearchPlyData>();
            while (_plyData.Count <= Search.MAX_PLY + 1)
            {
                _plyData.Add(new SearchPlyData(this));
            }

        }

        public SearchPlyData this[int ply]
        {
            get 
            {
                return _plyData[ply]; 
            }
        }

    }

    public class PlyData
    {
        public AttackInfo AttacksWhite { get; private set; }
        public AttackInfo AttacksBlack { get; private set; }
        public CheckInfo ChecksWhite { get; private set; }
        public CheckInfo ChecksBlack { get; private set; }
        public Evaluation.EvalResults EvalResults { get; private set; }

        public PlyData()
        {
            EvalResults = new Evaluation.EvalResults();
            AttacksWhite = new AttackInfo(Player.White);
            AttacksBlack = new AttackInfo(Player.Black);
            ChecksWhite = new CheckInfo(Player.White);
            ChecksBlack = new CheckInfo(Player.Black);
        }

        public CheckInfo ChecksFor(Player player)
        {
            return player == Player.White ? ChecksWhite : ChecksBlack;
        }

        public CheckInfo ChecksFor(Player player, Board board)
        {
            var retval = player == Player.White ? ChecksWhite : ChecksBlack;
            retval.Initialize(board);
            return retval;
        }

        public AttackInfo AttacksFor(Player player)
        {
            return player == Player.White ? AttacksWhite : AttacksBlack;
        }

        public AttackInfo AttacksFor(Player player, Board board)
        {
            var retval = AttacksFor(player);
            retval.Initialize(board);
            return retval;
        }
        

    }

    public class SearchPlyData: PlyData
    {
        public MovePicker MoveGenerator { get; private set; }
        public SearchData SearchData { get; private set; }

        public SearchPlyData(SearchData searchData)
        {
            SearchData = searchData;
            MoveGenerator = new MovePicker(searchData.MoveHistory, searchData.SEE);
        }



    }
}
