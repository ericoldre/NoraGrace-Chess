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
        void StartSearch(FEN fen);
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
        protected static log4net.ILog _log = log4net.LogManager.GetLogger(typeof(TimeManager));
        public event EventHandler<EventArgs> StopSearch;

        private int _nodeCount;

        protected virtual bool IsFailingHigh { get; set; }

        protected void RaiseStopSearch()
        {
            var ev = this.StopSearch;
            if (ev != null) { ev(this, EventArgs.Empty); }
        }

        protected int GetNodeCount()
        {
            return _nodeCount;
        }

        public virtual void StartSearch(FEN fen)
        {

        }

        public virtual void EndSearch()
        {

        }

        public virtual void StartDepth(int depth)
        {

        }

        public virtual void EndDepth(int depth)
        {

        }

        public virtual void StartMove(Move move)
        {

        }

        public virtual void EndMove(Move move)
        {
            IsFailingHigh = false;
        }

        public virtual void NewPV(Move move)
        {

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
        public FEN FEN { get; private set; }
        //
        public TUnit AmountToSpend { get; protected set; }

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

        public TUnit Max(TUnit v1, TUnit v2)
        {
            if (IsGreater(v1, v2)) { return v1; }
            else { return v2; }
        }

        public TUnit Min(TUnit v1, TUnit v2)
        {
            if (IsGreater(v1, v2)) { return v2; }
            else { return v1; }
        }

        public TimeManagerGeneric()
        {
            RatioBase = .0457;

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

        public override void StartSearch(FEN fen)
        {
            base.StartSearch(fen);

            this.FEN = fen;

            int movesLeft = 30;

            TUnit perMoveBonus = TimeControl.MovesPerControl > 0 ?
                Multiply(TimeControl.BonusAmount, 1f / TimeControl.MovesPerControl) :
                Multiply(TimeControl.BonusAmount, 0);

            if (this.TimeControl.MovesPerControl > 0)
            {
                movesLeft = (this.TimeControl.MovesPerControl - ((fen.fullmove - 1) % this.TimeControl.MovesPerControl));
            }
            else
            {

            }

            RatioBase = 1f / (double)movesLeft;
            RatioCeiling = RatioBase * 4;
            RatioFloor = RatioBase / 4;


            RatioBase = Math.Min(.97, RatioBase);
            RatioCeiling = Math.Min(.97, RatioBase);


            AmountToSpendCalc();

            if (_log.IsInfoEnabled)
            {
                _log.InfoFormat("move:{0} movesleft:{1} clock:{2} spendbase:{3}", fen.fullmove, movesLeft, this.AmountOnClock, AmountToSpend);
            }

        }

        public virtual void AmountToSpendCalc()
        {
            if(IsFailingHigh)
            {
                AmountToSpend = Multiply(AmountOnClock, RatioCeiling);
            }
            else
            {
                AmountToSpend = Multiply(AmountOnClock, RatioBase);
            }

            //_log.InfoFormat("Base:{0} failH:{1} Bounded:{2}", baseAmount, failhigh, bounded);
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

        public override void EndSearch()
        {
            base.EndSearch();
            if (_log.IsDebugEnabled)
            {
                _log.DebugFormat("Done:{0}", AmountSpent());
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

    public class TimeManager : TimeManagerGeneric<TimeSpan>
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

        public override void StartSearch(FEN fen)
        {
            base.StartSearch(fen);
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

        public override void StartSearch(FEN fen)
        {
            base.StartSearch(fen);
            _spent = 0;
        }

        public override void NodeStart(int nodeCount)
        {
            _spent = nodeCount;
            base.NodeStart(nodeCount);
        }

    }

}
