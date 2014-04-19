using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinobyl.Engine
{
    public interface ITimeManager
    {
        event EventHandler<EventArgs> StopSearch;
        event EventHandler<TimeManagerRequestNodesEventArgs> RequestNodes;
        void StartSearch();
        
        void StartDepth(int depth);
        void EndDepth(int depth);

        void StartMove(ChessMove move);
        void EndMove(ChessMove move);

        void NewPV(ChessMove move);

        void NodeStart(int nodeCount);
    }

    public class TimeManagerRequestNodesEventArgs : EventArgs
    {
        public int NodeCount { get; set; }
    }

    public abstract class TimeManagerBase : ITimeManager
    {
        protected static log4net.ILog _log = log4net.LogManager.GetLogger(typeof(TimeManagerBase));
        public event EventHandler<EventArgs> StopSearch;
        public event EventHandler<TimeManagerRequestNodesEventArgs> RequestNodes;

        protected void RaiseStopSearch()
        {
            if (_log.IsDebugEnabled) { _log.Debug("StopSearch"); }
            var ev = this.StopSearch;
            if (ev != null) { ev(this, EventArgs.Empty); }
        }

        protected int GetNodeCount()
        {
            var ev = this.RequestNodes;
            var args = new TimeManagerRequestNodesEventArgs();
            if (ev != null)
            {
                ev(this, args);
            }
            return args.NodeCount;
        }

        public virtual void StartSearch()
        {
            if (_log.IsDebugEnabled) { _log.Debug("StopSearch"); }
        }

        public virtual void StartDepth(int depth)
        {
            if (_log.IsDebugEnabled) { _log.DebugFormat("StartDepth:{0}", depth); }
        }

        public virtual void EndDepth(int depth)
        {
            if (_log.IsDebugEnabled) { _log.DebugFormat("EndDepth:{0}", depth); }
        }

        public virtual void StartMove(ChessMove move)
        {
            if (_log.IsDebugEnabled) { _log.DebugFormat("StartMove:{0}", move.Description()); }
        }

        public virtual void EndMove(ChessMove move)
        {
            if (_log.IsDebugEnabled) { _log.DebugFormat("EndMove:{0}", move.Description()); }
        }

        public virtual void NewPV(ChessMove move)
        {
            if (_log.IsDebugEnabled) { _log.DebugFormat("NewPV:{0}", move.Description()); }
        }

        public virtual void NodeStart(int nodeCount)
        {
        }
    }

    /// <summary>
    /// Will never abort search;
    /// </summary>
    public class TimeManagerAnalyze : TimeManagerBase
    {

    }

    public class TimeManagerBasic: TimeManagerBase
    {

        public ChessTimeControl TimeControl { get; set; }
        public DateTime ClockEnd { get; set; }
        public DateTime StopAtTime { get; private set; }
        
        public TimeManagerBasic()
        {

        }       

        public override void StartSearch()
        {
            base.StartSearch();

            TimeSpan timeLeftAtStart = ClockEnd - DateTime.Now;

            TimeSpan extraPerMove = TimeSpan.FromSeconds(0);
            if (TimeControl.BonusEveryXMoves > 0)
            {
                extraPerMove = TimeSpan.FromMilliseconds(TimeControl.BonusAmount.TotalMilliseconds / TimeControl.BonusEveryXMoves);
            }
            
            TimeSpan timeToSpend = TimeSpan.FromMilliseconds(timeLeftAtStart.TotalMilliseconds / 30) + extraPerMove;
            StopAtTime = DateTime.Now + timeToSpend;
            if (_log.IsInfoEnabled) { _log.InfoFormat("timeToSpend:{0} StopAtTime:{1}", timeToSpend, StopAtTime); }
        }

        private void CheckStop()
        {
            if (DateTime.Now > StopAtTime)
            {
                base.RaiseStopSearch();
            }
        }

        public override void StartDepth(int depth)
        {
            base.StartDepth(depth);
            CheckStop();
        }

        public override void EndDepth(int depth)
        {
            base.EndDepth(depth);
            CheckStop();
        }

        public override void StartMove(ChessMove move)
        {
            base.StartMove(move);
            CheckStop();
        }

        public override void EndMove(ChessMove move)
        {
            base.EndMove(move);
            CheckStop();
        }

        public override void NewPV(ChessMove move)
        {
            base.NewPV(move);
            CheckStop();
        }

        public override void NodeStart(int nodeCount)
        {
            base.NodeStart(nodeCount);
            if ((nodeCount & 0x3FF) == 0)
            {
                CheckStop();
            }
        }
    }
}
