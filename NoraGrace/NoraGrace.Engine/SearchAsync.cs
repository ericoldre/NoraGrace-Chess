using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace NoraGrace.Engine
{
    public class SearchAsync: IDisposable
    {
        public event EventHandler<SearchProgressEventArgs> ProgressReported;
        public event EventHandler<SearchProgressEventArgs> Finished;
        private Search search;
        private readonly BackgroundWorker bw = new BackgroundWorker();

        public SearchAsync()
        {
            bw.WorkerReportsProgress = true;
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
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
                // Dispose any managed objects
                bw.Dispose();
            }
            // Now disposed of any unmanaged objects
        }



        void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Search.Progress bestAnswer = (Search.Progress)e.UserState;
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
                Search.Progress finalAnswer = (Search.Progress)e.Result;
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
            search = new Search((Search.Args)e.Argument);
            search.ProgressReported += search_OnProgress;
            Search.Progress retval = search.Start();
            e.Result = retval;
        }

        public void Start(Search.Args args)
        {
            bw.RunWorkerAsync(args);
        }

        void search_OnProgress(object sender, SearchProgressEventArgs e)
        {
            bw.ReportProgress(50, e.Progress);
        }

        public void Abort(bool raiseOnFinish)
        {
            Search o = search;
            if (o != null)
            {
                o.Abort(raiseOnFinish);
            }
        }

    }
}
