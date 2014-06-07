using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.ComponentModel;
namespace Sinobyl.Engine
{
    public abstract class ChessGamePlayer
    {
        public event EventHandler<MoveEventArgs> MovePlayed;
        public event EventHandler<SearchProgressEventArgs> Kibitz;
        public event EventHandler Resigned;

        public abstract void TurnStop();
        public abstract void YourTurn(ChessFEN initalPosition, ChessMoves prevMoves, TimeControl timeControl, TimeSpan timeLeft);
        public abstract string Name { get; }

        protected virtual void OnMovePlayed(ChessMove move)
        {
            var e = this.MovePlayed;
            if (e != null) { e(this, new MoveEventArgs(move)); }
        }

        protected virtual void OnResigned()
        {
            var e = this.Resigned;
            if (e != null) { e(this, new EventArgs()); }
        }

        protected virtual void OnKibitz(ChessSearch.Progress prog)
        {
            var eh = this.Kibitz;
            if (eh != null) { eh(this, new SearchProgressEventArgs(prog)); }
        }


        public void YourTurn(ChessBoard board, TimeControl timeControl, TimeSpan timeLeft)
        {
            //need to extrapolate previous moves and initial position from board
            ChessMoves moves = board.HistoryMoves;
            int moveCount = moves.Count;
            for (int i = 0; i < moveCount; i++)
            {
                board.MoveUndo();
            }
            ChessFEN initialPosition = board.FEN;
            foreach (ChessMove move in moves)
            {
                board.MoveApply(move);
            }
            YourTurn(initialPosition, moves, timeControl, timeLeft);
        }


    }

    public class ChessGamePlayerPersonality
    {
        public int MaxDepth { get; set; }
        public int NodesPerSecond { get; set; }
        public String Name { get; set; }
        public ChessSearch.BlunderChance Blunder { get; set; }

        public ChessGamePlayerPersonality()
        {
            MaxDepth = int.MaxValue;
            NodesPerSecond = int.MaxValue;
            Name = "Default personality";
            Blunder = new ChessSearch.BlunderChance();
        }

        public static ChessGamePlayerPersonality FromStrength(float a_strength)
        {
            if (a_strength < 0) { a_strength = 0; }
            if (a_strength > 1) { a_strength = 1; }
            ChessGamePlayerPersonality retval = new ChessGamePlayerPersonality();

            retval.NodesPerSecond = (int)(10000 * a_strength) + 500;
            retval.MaxDepth = (int)(10 * a_strength) + 2;
            retval.Blunder.BlunderSkipCount = 0;
            retval.Blunder.BlunderReduceEnd = .1;
            retval.Blunder.BlunderPct = .85 - (.75 * a_strength);
            retval.Blunder.BlunderDepth = .05 + (.10 * a_strength);

            if (a_strength == 1)
            {
                retval.NodesPerSecond = int.MaxValue;
                retval.MaxDepth = int.MaxValue;
                retval.Blunder.BlunderPct = 0;
                retval.Blunder.BlunderSkipCount = int.MaxValue;
                retval.Blunder.BlunderReduceEnd = 1;
            }

            return retval;

        }



    }
    public class ChessGamePlayerMurderhole : ChessGamePlayer, IDisposable
    {

        private readonly ChessSearchAsync search;
        private ChessGamePlayerPersonality _personality = ChessGamePlayerPersonality.FromStrength(1);
        private readonly TranspositionTable _transTable = new TranspositionTable();
        private BackgroundWorker BookBackgroundWorker;
        private ChessEval _eval = new ChessEval();
        private TimeManagerAdvanced _timeManager = new TimeManagerAdvanced();
        public TimeSpan DelaySearch { get; set; }

        public ChessGamePlayerMurderhole()
        {
            DelaySearch = new TimeSpan(0);
            search = new ChessSearchAsync();
            search.ProgressReported += search_OnProgress;
            search.Finished += search_OnFinish;
        }
        public ChessGamePlayerPersonality Personality
        {
            get
            {
                return _personality;
            }
            set
            {
                _personality = value;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                var bw = this.BookBackgroundWorker;
                if (bw != null)
                {
                    bw.Dispose();
                }
                search.Dispose();
                // Dispose any managed objects
                
            }
            // Now disposed of any unmanaged objects
        }


        public override string Name
        {
            get
            {
                return Personality.Name;
            }
        }

        void search_OnFinish(object sender, SearchProgressEventArgs e)
        {
            this.OnMovePlayed(e.Progress.PrincipleVariation[0]);
            //if (this.OnMove != null) { this.OnMove(this, progress.PrincipleVariation[0]); }
        }

        void search_OnProgress(object sender, SearchProgressEventArgs e)
        {
            this.OnKibitz(e.Progress);
        }

        public override void TurnStop()
        {
            search.Abort(false);
        }

        public override void YourTurn(ChessFEN initalPosition, ChessMoves prevMoves, TimeControl timeControl, TimeSpan timeLeft)
        {
            //ChessSearch a = new ChessSearch(game.FENCurrent);

            ChessOpening opening = new ChessOpening();//only here to debug static contructor

            ChessBoard currBoard = new ChessBoard(initalPosition, prevMoves);

            ChessFEN FenCurrent = currBoard.FEN;

            //check opening book
            ChessBookOpening book = new ChessBookOpening();
            ChessMove bookMove = book.FindMove(FenCurrent);
            if (bookMove != ChessMove.EMPTY)
            {
                if (BookBackgroundWorker == null)
                {
                    BookBackgroundWorker = new BackgroundWorker();
                    BookBackgroundWorker.DoWork += new DoWorkEventHandler(BookBackgroundWorker_DoWork);
                    BookBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BookBackgroundWorker_RunWorkerCompleted);
                    BookBackgroundWorker.WorkerSupportsCancellation = true;
                }
                BookBackgroundWorker.RunWorkerAsync(bookMove);
                return;
            }


            ChessSearch.Args args = new ChessSearch.Args();
            args.GameStartPosition = initalPosition;
            args.GameMoves = new ChessMoves(prevMoves);
            args.MaxDepth = _personality.MaxDepth;
            args.NodesPerSecond = _personality.NodesPerSecond;
            args.Blunder = _personality.Blunder;
            _timeManager.TimeControl = timeControl;
            _timeManager.AmountOnClock = timeLeft;
            args.TimeManager = _timeManager;
            args.TransTable = _transTable;
            args.Delay = this.DelaySearch;
            args.Eval = _eval;

            search.Abort(false);
            search.SearchAsync(args);

        }

        public void ForceMove()
        {
            search.Abort(true);
        }


        void BookBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ChessMove move = (ChessMove)e.Result;
            OnMovePlayed(move);
        }

        void BookBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = (BackgroundWorker)sender;

            DateTime endtime = DateTime.Now.Add(this.DelaySearch);
            while (DateTime.Now < endtime)
            {
                System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(50));
                if (bw.CancellationPending)
                {
                    return;
                }
            }
            ChessMove move = (ChessMove)e.Argument;
            e.Result = move;
        }




        void search_FoundMove(object sender, object move)
        {
            this.OnMovePlayed((ChessMove)move);
            //if (this.OnMove != null) { this.OnMove(this, move); }
        }
    }
}
