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
        void EndSearch();

        void StartDepth(int depth);
        void EndDepth(int depth);

        void StartMove(ChessMove move);
        void EndMove(ChessMove move);

        void NewPV(ChessMove move);

        void NodeStart(int nodeCount);

        void FailingHigh();
        
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

        protected virtual bool IsFailingHigh { get; set; }

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

        protected virtual bool CheckStop()
        {
            return false;
        }
        public virtual void StartSearch()
        {
            if (_log.IsDebugEnabled) { _log.Debug("StopSearch"); }
        }

        public virtual void EndSearch()
        {

        }

        public virtual void StartDepth(int depth)
        {
            if (CheckStop()) { RaiseStopSearch(); }
            if (_log.IsDebugEnabled) { _log.DebugFormat("StartDepth:{0}", depth); }
        }

        public virtual void EndDepth(int depth)
        {
            if (CheckStop()) { RaiseStopSearch(); }
            if (_log.IsDebugEnabled) { _log.DebugFormat("EndDepth:{0}", depth); }
        }

        public virtual void StartMove(ChessMove move)
        {
            if (CheckStop()) { RaiseStopSearch(); }
            if (_log.IsDebugEnabled) { _log.DebugFormat("StartMove:{0}", move.Description()); }
        }

        public virtual void EndMove(ChessMove move)
        {
            IsFailingHigh = false;
            if (CheckStop()) { RaiseStopSearch(); }
            if (_log.IsDebugEnabled) { _log.DebugFormat("EndMove:{0}", move.Description()); }
        }

        public virtual void NewPV(ChessMove move)
        {
            if (CheckStop()) { RaiseStopSearch(); }
            if (_log.IsDebugEnabled) { _log.DebugFormat("NewPV:{0}", move.Description()); }
        }

        public virtual void NodeStart(int nodeCount)
        {
            
        }

        public virtual void FailingHigh()
        {
            IsFailingHigh = true;
        }

    }

    /// <summary>
    /// Will never abort search;
    /// </summary>
    public class TimeManagerAnalyze : TimeManagerBase
    {

    }

    public class TimeManagerBasic : TimeManagerBase
    {

        public ChessTimeControl TimeControl { get; set; }
        public DateTime ClockEnd { get; set; }
        public DateTime StopAtTime { get; private set; }
        
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

        protected override bool CheckStop()
        {
            if (DateTime.Now > StopAtTime)
            {
                return true;
            }
            return false;
        }

        public override void NodeStart(int nodeCount)
        {
            base.NodeStart(nodeCount);
            if ((nodeCount & 0x3FF) == 0)
            {
                if (CheckStop()) { RaiseStopSearch(); }
            }
        }
    }

    public abstract class TimeManagerComplexity : TimeManagerBase
    {
        public event EventHandler<EventArgs> ComplexityUpdated;

        private int _currDepth;
        private int _startMoveNodes;
        private Dictionary<ChessMove, int> _moveNodeCounts = new Dictionary<ChessMove, int>();
        private readonly Dictionary<int, List<ChessMove>> _pvsByDepth = new Dictionary<int, List<ChessMove>>();


        
        private double _lastPct1;
        //private double _lastPct2;

        private List<double> _pct1History = new List<double>();
        //private List<double> _pct2History = new List<double>();

        private const double PCT1_TYPICAL = .316; //typical amount of nodes searched to refute 2nd best move compared to best move
        //private const double PCT2_TYPICAL = .250; //typical amount of nodes searched to refute 2nd & 3rd best move compared to best move

        private double _complexity = 1;
        public double Complexity 
        {
            get { return _complexity; }
            private set
            {
                if (value != _complexity)
                {
                    _complexity = value;
                    var ev = this.ComplexityUpdated;
                    if (ev != null) { ev(this, EventArgs.Empty); }
                }
            } 
        }

        public override void StartSearch()
        {
            base.StartSearch();
            _pvsByDepth.Clear();
        }




        
        public override void StartDepth(int depth)
        {
            _currDepth = depth;
            base.StartDepth(depth);
            _moveNodeCounts.Clear();
        }

        public override void EndDepth(int depth)
        {
            base.EndDepth(depth);

            if (_moveNodeCounts.Count == 1) { Complexity = 0; return; } //only one legal move, this is easy.

            int[] allCounts = _moveNodeCounts.Values.OrderByDescending((i) => i).ToArray();

            double highest = allCounts.First();
            double next1 = allCounts.Skip(1).Take(1).Average();
            double pct2nd = next1 / highest;

            if (pct2nd == 1) 
            {
                //we are just reading values from the trans table, these numbers are basically worse than garbage.
                Complexity = 1;
                return;
            }
            _lastPct1 = pct2nd;

            double pct1Complexity = pct2nd / PCT1_TYPICAL;
            double pvComplexity = PVComplexity();

            
            

            List<double> factors = new List<double>();
            factors.Add(pct1Complexity);
            factors.Add(pvComplexity);
            //factors.Add(1.00);
            for (int i = depth; i <= 6; i++)
            {
                factors.Add(1);
            }

            double totalComplexity = factors.Average();

            if (depth >= 4)
            {
                _log.InfoFormat(string.Format("total:{0:0.00} compNodes:{1:0.00} compPV:{2:0.00} ", totalComplexity, pct1Complexity, pvComplexity));
            }

            this.Complexity = totalComplexity;

        }

        public override void EndSearch()
        {
            base.EndSearch();
            _pct1History.Add(_lastPct1);

            _pct1History.Sort();

            if (_log.IsInfoEnabled)
            {
                int i5 = Math.Min((int)Math.Round(((double)_pct1History.Count * .05f)), _pct1History.Count - 1);
                int i25 = Math.Min((int)Math.Round(((double)_pct1History.Count * .25f)), _pct1History.Count - 1);
                int i75 = Math.Min((int)Math.Round(((double)_pct1History.Count * .75f)), _pct1History.Count - 1);
                int i95 = Math.Min((int)Math.Round(((double)_pct1History.Count * .95f)), _pct1History.Count - 1);
                
                _log.InfoFormat(string.Format("count: {0}, {1}, {2}, {3}, {4}", _pct1History.Count, i5, i25, i75, i95));
                _log.InfoFormat(string.Format("NodePct: avg:{0:0.00} 5pct:{1:0.00} 25:{2:0.00} 75:{3:0.00} 95:{4:0.00}", 
                    _pct1History.Average(), _pct1History[i5], _pct1History[i25], _pct1History[i75], _pct1History[i95]));

            }
        }

        public override void StartMove(ChessMove move)
        {
            base.StartMove(move);
            _startMoveNodes = GetNodeCount();
        }

        public override void EndMove(ChessMove move)
        {
            base.EndMove(move);
            int endMoveNodes = GetNodeCount();
            int moveNodes = endMoveNodes - _startMoveNodes;
            _moveNodeCounts.Add(move, moveNodes);

        }

        public override void NewPV(ChessMove move)
        {
            base.NewPV(move);
            if (!_pvsByDepth.ContainsKey(_currDepth)) { _pvsByDepth.Add(_currDepth, new List<ChessMove>()); }
            if (!_pvsByDepth[_currDepth].Contains(move))
            {
                _pvsByDepth[_currDepth].Add(move);
            }
        }

        private double PVComplexity()
        {
            double retval = 1;
            int consecutive1PV = PVCounts().TakeWhile(pvc => pvc < 2).Count();
            int otherPVsInLast4 = PVCounts().Take(2).Select(i => Math.Min(i - 1, 3)).Sum();

            if (consecutive1PV > 0)
            {
                retval *= Math.Pow(.85, Math.Min(consecutive1PV, 3));
            }
            if (otherPVsInLast4 > 0)
            {
                retval *= Math.Pow(1.2, otherPVsInLast4);
            }

            return retval;

        }
        private IEnumerable<int> PVCounts()
        {
            for (int i = _currDepth; _pvsByDepth.ContainsKey(i); i--)
            {
                yield return _pvsByDepth[i].Count;
            }
        }

    }


    public abstract class TimeManagerGeneric<TUnit> : TimeManagerComplexity
    {
        //used as inputs
        public ChessTimeControlGeneric<TUnit> TimeControl { get; set; }
        public TUnit AmountOnClock { get; set; }

        //
        public TUnit AmountToSpend { get; protected set; }
        public TUnit BaseAmount { get; protected set; }

        //configuration factors
        public double RatioBase { get; set; }
        public double RatioComplexity { get; set; }
        public double RatioHistory { get; set; }
        public double RatioFailHigh { get; set; }
        public double RatioFloor { get; set; }
        public double RatioCeiling { get; set; }
        public double RatioOfTotalCeiling { get; set; }

        //abstract math helpers
        public abstract TUnit Add(TUnit x, TUnit y);
        public abstract TUnit Subtract(TUnit x, TUnit y);
        public abstract TUnit Multiply(TUnit x, double multiplier);
        public abstract bool IsGreater(TUnit x, TUnit y);
        public abstract TUnit AmountSpent();

        public TimeManagerGeneric()
        {
            RatioBase = .033333;
            RatioComplexity = .7;
            RatioHistory = 1;
            RatioFailHigh = 1.4;
            RatioFloor = .333;
            RatioCeiling = 4;
            RatioOfTotalCeiling = .25;
            this.ComplexityUpdated += This_ComplexityUpdated;
        }

        void This_ComplexityUpdated(object sender, EventArgs e)
        {
            AmountToSpendCalc();
        }

        
        public override void StartSearch()
        {
            base.StartSearch();

            TUnit perMoveBonus = TimeControl.BonusEveryXMoves > 0 ?
                Multiply(TimeControl.BonusAmount, 1f / TimeControl.BonusEveryXMoves) :
                Multiply(TimeControl.BonusAmount, 0);

            BaseAmount = Add(Multiply(AmountOnClock, RatioBase), perMoveBonus);
            AmountToSpend = BaseAmount;
            AmountToSpendCalc();
            
        }

        public virtual void AmountToSpendCalc()
        {
            var baseAmount = BaseAmount;
            double c = Math.Pow(this.Complexity, RatioComplexity);

            double xx = Math.Pow(3.245, .5);
            double xxx = Math.Pow(0.245, .5);
            var complex = Multiply(baseAmount, Math.Pow(this.Complexity, RatioComplexity));


            var failhigh = complex;
            if(IsFailingHigh)
            {
                failhigh = Multiply(complex, RatioFailHigh);
            }

            var floor = Multiply(BaseAmount, RatioFloor);
            var ceiling = Multiply(BaseAmount, RatioCeiling);
            var ceiling2 = Multiply(this.AmountOnClock, this.RatioOfTotalCeiling);
            if(IsGreater(ceiling, ceiling2)){ceiling = ceiling2;}
            
            var bounded = failhigh;
            
            if (IsGreater(floor, bounded)) { bounded = floor; }

            
            if (IsGreater(bounded, ceiling)) { bounded = ceiling; }

            
            
            this.AmountToSpend = bounded;

            _log.InfoFormat("Base:{0} comp:{1} failH:{2} Bounded:{3} litComp:{4} pow{5}", baseAmount, complex, failhigh, bounded, this.Complexity, Math.Pow(this.Complexity, RatioComplexity));
        }

        private TUnit _spentByDepthStart;
        public override void StartDepth(int depth)
        {
            base.StartDepth(depth);

            _spentByDepthStart = AmountSpent();
        }

        public override void EndDepth(int depth)
        {
            base.EndDepth(depth);
            AmountToSpendCalc();
            TUnit spentByDepthEnd = AmountSpent();
            TUnit spentOnDepth = Subtract(spentByDepthEnd, _spentByDepthStart);
            TUnit projected = Add(spentByDepthEnd, spentOnDepth);
            if (IsGreater(projected, AmountToSpend))
            {
                RaiseStopSearch();
            }
        }

        public override void NodeStart(int nodeCount)
        {
            base.NodeStart(nodeCount);
            if (IsGreater(AmountSpent(), AmountToSpend))
            {
                RaiseStopSearch();
            }
        }
        protected override bool IsFailingHigh
        {
            set
            {
                if (value != IsFailingHigh)
                {
                    AmountToSpendCalc();
                    base.IsFailingHigh = value;
                }
            }
        }

    }

    public class TimeManagerNew : TimeManagerGeneric<TimeSpan>
    {

        private TimeSpan _timeSpent = TimeSpan.FromMilliseconds(0);
        private DateTime _searchStartTime = DateTime.Now;

        public override TimeSpan Add(TimeSpan x, TimeSpan y)
        {
            return x + y;
        }

        public override TimeSpan Subtract(TimeSpan x, TimeSpan y)
        {
            return x - y;
        }

        public override TimeSpan Multiply(TimeSpan x, double multiplier)
        {
            return TimeSpan.FromMilliseconds(x.TotalMilliseconds * multiplier);
        }

        public override bool IsGreater(TimeSpan x, TimeSpan y)
        {
            return x > y;
        }

        
        public override TimeSpan AmountSpent()
        {
            return _timeSpent;
        }

        public override void StartSearch()
        {
            base.StartSearch();
            _timeSpent = TimeSpan.FromMilliseconds(0);
            _searchStartTime = DateTime.Now;
        }

        public override void NodeStart(int nodeCount)
        {
            if ((nodeCount & 0x3FF) == 0)
            {
                _timeSpent = DateTime.Now - _searchStartTime;
                base.NodeStart(nodeCount);
            }

        }
        
    }

}
