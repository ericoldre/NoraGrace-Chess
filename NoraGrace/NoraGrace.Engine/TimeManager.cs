using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoraGrace.Engine
{
    public interface ITimeManager
    {
        event EventHandler<EventArgs> StopSearch;
        void StartSearch();
        void EndSearch();

        void StartDepth(int depth);
        void EndDepth(int depth);

        void StartMove(Move move);
        void EndMove(Move move);

        void NewPV(Move move);

        void NodeStart(int nodeCount);

        void FailingHigh();
        
    }

    public abstract class TimeManagerBase : ITimeManager
    {
        protected static log4net.ILog _log = log4net.LogManager.GetLogger(typeof(TimeManagerBase));
        public event EventHandler<EventArgs> StopSearch;

        private int _nodeCount;

        protected virtual bool IsFailingHigh { get; set; }

        protected void RaiseStopSearch()
        {
            if (_log.IsDebugEnabled) { _log.Debug("StopSearch"); }
            var ev = this.StopSearch;
            if (ev != null) { ev(this, EventArgs.Empty); }
        }

        protected int GetNodeCount()
        {
            return _nodeCount;
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

        public virtual void StartMove(Move move)
        {
            if (CheckStop()) { RaiseStopSearch(); }
            if (_log.IsDebugEnabled) { _log.DebugFormat("StartMove:{0}", move.Description()); }
        }

        public virtual void EndMove(Move move)
        {
            IsFailingHigh = false;
            if (CheckStop()) { RaiseStopSearch(); }
            if (_log.IsDebugEnabled) { _log.DebugFormat("EndMove:{0}", move.Description()); }
        }

        public virtual void NewPV(Move move)
        {
            if (CheckStop()) { RaiseStopSearch(); }
            if (_log.IsDebugEnabled) { _log.DebugFormat("NewPV:{0}", move.Description()); }
        }

        public virtual void NodeStart(int nodeCount)
        {
            _nodeCount = nodeCount;
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



    public abstract class TimeManagerGeneric<TUnit> : TimeManagerBase
    {
        //used as inputs
        public TimeControlGeneric<TUnit> TimeControl { get; set; }
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
            RatioBase = .0457;
            RatioComplexity = 0;//test show not yet really working .7;
            RatioHistory = 1;
            RatioFailHigh = 1.5; //should show good improvement
            RatioFloor = .333;
            RatioCeiling = 4;
            RatioOfTotalCeiling = .25;
        }

        public override void FailingHigh()
        {
            bool oldValue = this.IsFailingHigh;
            TUnit amountBefore = this.AmountToSpend;

            base.FailingHigh();

            bool newValue = this.IsFailingHigh;
            TUnit amountAfter = this.AmountToSpend;
            
        }

        

        
        public override void StartSearch()
        {
            base.StartSearch();

            TUnit perMoveBonus = TimeControl.MovesPerControl > 0 ?
                Multiply(TimeControl.BonusAmount, 1f / TimeControl.MovesPerControl) :
                Multiply(TimeControl.BonusAmount, 0);

            BaseAmount = Add(Multiply(AmountOnClock, RatioBase), perMoveBonus);
            AmountToSpend = BaseAmount;
            AmountToSpendCalc();
            
        }

        public virtual void AmountToSpendCalc()
        {
            var baseAmount = BaseAmount;

            var failhigh = baseAmount;
            if(IsFailingHigh)
            {
                failhigh = Multiply(baseAmount, RatioFailHigh);
            }

            var floor = Multiply(BaseAmount, RatioFloor);
            var ceiling = Multiply(BaseAmount, RatioCeiling);
            var ceiling2 = Multiply(this.AmountOnClock, this.RatioOfTotalCeiling);
            if(IsGreater(ceiling, ceiling2)){ceiling = ceiling2;}
            
            var bounded = failhigh;
            
            if (IsGreater(floor, bounded)) { bounded = floor; }

            
            if (IsGreater(bounded, ceiling)) { bounded = ceiling; }

            
            
            this.AmountToSpend = bounded;

            _log.InfoFormat("Base:{0} failH:{1} Bounded:{2}", baseAmount, failhigh, bounded);
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
                    base.IsFailingHigh = value;
                    AmountToSpendCalc();
                }
            }
        }

    }

    public class TimeManagerAdvanced : TimeManagerGeneric<TimeSpan>
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


    public class TimeManagerNodes : TimeManagerGeneric<int>
    {

        private int _spent = 0;
        //private DateTime _searchStartTime = DateTime.Now;

        public override int Add(int x, int y)
        {
            return x + y;
        }

        public override int Subtract(int x, int y)
        {
            return x - y;
        }

        public override int Multiply(int x, double multiplier)
        {
            return (int)Math.Round((double)x * multiplier);
        }

        public override bool IsGreater(int x, int y)
        {
            return x > y;
        }


        public override int AmountSpent()
        {
            return _spent;
        }

        public override void StartSearch()
        {
            base.StartSearch();
            _spent = 0;
        }

        public override void NodeStart(int nodeCount)
        {
            _spent = nodeCount;
            base.NodeStart(nodeCount);
        }

    }

}
