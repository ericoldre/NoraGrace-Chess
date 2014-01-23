using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Sinobyl.Engine
{
    public class ChessSearchAsync
    {
        public event EventHandler<SearchProgressEventArgs> ProgressReported;
        public event EventHandler<SearchProgressEventArgs> Finished;
        private ChessSearch search;
        private BackgroundWorker bw;

        public ChessSearchAsync()
        {
            bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
        }

        void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ChessSearch.Progress bestAnswer = (ChessSearch.Progress)e.UserState;
            OnProgressReported(new SearchProgressEventArgs(bestAnswer));
        }

        protected virtual void OnProgressReported(SearchProgressEventArgs progress)
        {
            var eh = this.ProgressReported;
            if (eh != null) { eh(this, progress); }
        }

        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                ChessSearch.Progress finalAnswer = (ChessSearch.Progress)e.Result;
                OnFinished(new SearchProgressEventArgs(finalAnswer));
            }
            else
            {
                //search was cancelled and told to not return the best result. as in move takeback.
            }
        }

        protected virtual void OnFinished(SearchProgressEventArgs finalAnswer)
        {
            var eh = this.Finished;
            if (eh != null) { eh(this, finalAnswer); }
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            if (search != null)
            {
                search.ProgressReported -= search_OnProgress;
                search = null;
            }
            search = new ChessSearch((ChessSearch.Args)e.Argument);
            search.ProgressReported += search_OnProgress;
            ChessSearch.Progress retval = search.Search();
            e.Result = retval;
        }

        public void SearchAsync(ChessSearch.Args args)
        {
            bw.RunWorkerAsync(args);
        }

        void search_OnProgress(object sender, SearchProgressEventArgs e)
        {
            bw.ReportProgress(50, e.Progress);
        }

        public void Abort(bool raiseOnFinish)
        {
            ChessSearch o = search;
            if (o != null)
            {
                o.Abort(raiseOnFinish);
            }
        }

    }
}
