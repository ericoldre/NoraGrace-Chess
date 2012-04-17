using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinobyl.Engine;

namespace Sinobyl.EvalTune
{
    public class DeterministicPlayer: IChessGamePlayer
    {
        public string Name { get; set; }
        public IChessEval Eval { get; set; }
        public int MaxNodes { get; set; }
        public ChessTrans TransTable { get; set; }
        public Func<ChessSearch.Progress, string> CommentFormatter { get; set; }
        public DeterministicPlayer(string name, IChessEval eval, int maxNodes)
        {
            Name = name;
            Eval = eval;
            MaxNodes = maxNodes;
            TransTable = new ChessTrans();

        }
        #region IChessGamePlayer Members

        string IChessGamePlayer.Name
        {
            get { return Name; }
        }

        ChessMove IChessGamePlayer.Move(ChessFEN gameStartPosition, IEnumerable<ChessMove> movesAlreadyPlayed, ChessTimeControl timeControls, TimeSpan timeLeft, out string comment)
        {

            ChessSearch.Args args = new ChessSearch.Args();
            args.GameStartPosition = gameStartPosition;
            args.GameMoves = new ChessMoves(movesAlreadyPlayed.ToArray());
            args.TransTable = TransTable;
            args.Eval = Eval;
            args.MaxNodes = MaxNodes;

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

            return searchResult.PrincipleVariation[0];

        }

        #endregion
    }
}
