using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinobyl.Engine;
using Sinobyl.Engine.Evaluation;

namespace Sinobyl.EvalTune
{
    public class DeterministicPlayer
    {
        public string Name { get; set; }
        public IChessEval Eval { get; set; }
        public TimeManagerNodes NodeManager { get; set; }
        public TranspositionTable TransTable { get; set; }
        public Func<Search.Progress, string> CommentFormatter { get; set; }
        public Action<Search.Args> AlterSearchArgs { get; set; }

        private int _resignRepCount = 0;

        public DeterministicPlayer(string name, IChessEval eval, TimeManagerNodes nodeManager)
        {
            Name = name;
            Eval = eval;
            NodeManager = nodeManager;
            TransTable = new TranspositionTable(10000);
        }


        public Move Move(FEN gameStartPosition, IEnumerable<Move> movesAlreadyPlayed, TimeControlNodes timeControls, int nodesOnClock, out string comment, out int nodesProcessed)
        {
            NodeManager.TimeControl = timeControls;
            NodeManager.AmountOnClock = nodesOnClock;

            Search.Args args = new Search.Args();
            args.GameStartPosition = gameStartPosition;
            args.GameMoves = new List<Move>(movesAlreadyPlayed);
            args.TransTable = TransTable;
            args.Eval = Eval;
            args.TimeManager = this.NodeManager;
            args.ContemptForDraw = 50;
            if (AlterSearchArgs != null) { AlterSearchArgs(args); }
            Search search = new Search(args);
            var searchResult = search.Start();

            if (CommentFormatter != null)
            {
                comment = CommentFormatter(searchResult);
            }
            else
            {
                comment = null;
            }

            TransTable.AgeEntries(4);

            Move bestMove = searchResult.PrincipleVariation[0];
            nodesProcessed = search.CountAIValSearch;

            return searchResult.PrincipleVariation[0];

        }

    }
}
