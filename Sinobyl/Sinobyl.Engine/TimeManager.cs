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

        void UpdateProgress();
    }

    public class TimeManagerRequestNodesEventArgs : EventArgs
    {
        public int NodeCount { get; set; }
    }

    public class TimeManagerBasic: ITimeManager
    {
        private static log4net.ILog _log = log4net.LogManager.GetLogger(typeof(TimeManagerBasic));

        public event EventHandler<EventArgs> StopSearch;
        public event EventHandler<TimeManagerRequestNodesEventArgs> RequestNodes;

        private readonly ChessTimeControl _timeControl;
        private readonly TimeSpan _timeLeftAtStart;

        private DateTime _stopAtTime;

        public TimeManagerBasic(ChessTimeControl timeControl, TimeSpan timeLeft)
        {
            _timeControl = timeControl;
            _timeLeftAtStart = timeLeft;
        }

        

        public void StartSearch()
        {
            TimeSpan extraPerMove = TimeSpan.FromSeconds(0);
            if (_timeControl.BonusEveryXMoves > 0)
            {
                extraPerMove = TimeSpan.FromMilliseconds(_timeControl.BonusAmount.TotalMilliseconds / _timeControl.BonusEveryXMoves);
            }
            
            TimeSpan timeToSpend = TimeSpan.FromMilliseconds(_timeLeftAtStart.TotalMilliseconds / 30) + extraPerMove;
            _stopAtTime = DateTime.Now + timeToSpend;
        }

        private void CheckStop()
        {
            if (DateTime.Now > _stopAtTime)
            {
                var ev = this.StopSearch;
                if (ev != null) { ev(this, EventArgs.Empty); }
            }
        }
        public void StartDepth(int depth)
        {
            CheckStop();
        }

        public void EndDepth(int depth)
        {
            CheckStop();
        }

        public void StartMove(ChessMove move)
        {
            CheckStop();
        }

        public void EndMove(ChessMove move)
        {
            CheckStop();
        }

        public void NewPV(ChessMove move)
        {
            CheckStop();
        }

        public void UpdateProgress()
        {
            CheckStop();
        }
    }
}
