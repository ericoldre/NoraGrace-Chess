using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinobyl.Engine;

namespace Sinobyl.EvalTune
{
    public class DeterministicPlayer
    {
        public string Name { get; set; }
        public IChessEval Eval { get; set; }
        public TimeManagerNodes NodeManager { get; set; }
        public ChessTrans TransTable { get; set; }
        public Func<ChessSearch.Progress, string> CommentFormatter { get; set; }
        public Action<ChessSearch.Args> AlterSearchArgs { get; set; }

        private int _resignRepCount = 0;

        public DeterministicPlayer(string name, IChessEval eval, TimeManagerNodes nodeManager)
        {
            Name = name;
            Eval = eval;
            NodeManager = nodeManager;
            TransTable = new ChessTrans(10000);
        }


        public ChessMove Move(ChessFEN gameStartPosition, IEnumerable<ChessMove> movesAlreadyPlayed, ChessTimeControlNodes timeControls, int nodesOnClock, out string comment, out int nodesProcessed)
        {
            NodeManager.TimeControl = timeControls;
            NodeManager.AmountOnClock = nodesOnClock;

            ChessSearch.Args args = new ChessSearch.Args();
            args.GameStartPosition = gameStartPosition;
            args.GameMoves = new ChessMoves(movesAlreadyPlayed.ToArray());
            args.TransTable = TransTable;
            args.Eval = Eval;
            args.TimeManager = this.NodeManager;
            args.ContemptForDraw = 0;
            if (AlterSearchArgs != null) { AlterSearchArgs(args); }
            ChessSearch search = new ChessSearch(args);
            var searchResult = search.Search();

            if (CommentFormatter != null)
            {
                comment = CommentFormatter(searchResult);
            }
            else
            {
                comment = null;
            }

            TransTable.AgeEntries(4);

            ChessMove bestMove = searchResult.PrincipleVariation[0];
            nodesProcessed = search.CountAIValSearch;

            return searchResult.PrincipleVariation[0];

        }

    }
}
