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

        public int? ResignLimit { get; set; }
        public int? ResignReps { get; set; }

        private int _resignRepCount = 0;

        public DeterministicPlayer(string name, IChessEval eval, int maxNodes)
        {
            Name = name;
            Eval = eval;
            MaxNodes = maxNodes;
            TransTable = new ChessTrans(10000);
            ResignLimit = null;
            ResignReps = null;

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
            args.ContemptForDraw = 70;

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
            if (this.ResignLimit.HasValue && searchResult.Score < -this.ResignLimit)
            {
                this._resignRepCount++;
            }
            else
            {
                this._resignRepCount = 0;
            }
            if (this.ResignReps.HasValue && this._resignRepCount > this.ResignReps)
            {
                return ChessMove.EMPTY;
            }
            return searchResult.PrincipleVariation[0];

        }

        #endregion
    }
}
