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

    public abstract class TimeManagerComplexity : TimeManagerBase
    {

        private class ComplexityInfo
        {
            public double RefutePct { get; set; }

        }

        public double PctFloor { get; private set; }
        private double[] _pcts = new double[20];
        private ComplexityInfo _currentComplexity;
        private List<ComplexityInfo> _complexityHistory = new List<ComplexityInfo>();

        public double PctOfNormalToSpend { get; private set; }

        public TimeManagerComplexity()
        {

            PctFloor = .25;
            double growth = FindGrowth(PctFloor, _pcts.Length, 20);
            GetSequence(PctFloor, growth, _pcts.Length).ToArray().CopyTo(_pcts, 0);

            //Random rand = new Random(1);
            //while (_complexityHistory.Count < _pcts.Length)
            //{
            //    _complexityHistory.Add(new ComplexityInfo() { RefutePct = rand.NextDouble() * .5 });
            //}
        }

        private List<int> _moveNodes = new List<int>();
        private int _currentMoveStartNodes;
        public override void StartSearch(FEN fen)
        {
            //create a new complexity object and add it to the history of recent moves. ma
            _currentComplexity = new ComplexityInfo();
            while (_complexityHistory.Count >= _pcts.Length)
            {
                _complexityHistory.RemoveAt(0);
            }
            _complexityHistory.Add(_currentComplexity);
            System.Diagnostics.Debug.Assert(_complexityHistory.Count <= _pcts.Length);

            base.StartSearch(fen);
            
        }
        public override void StartDepth(int depth)
        {
            base.StartDepth(depth);
            _moveNodes.Clear();
        }
        public override void StartMove(Move move)
        {

            base.StartMove(move);
            _currentMoveStartNodes = GetNodeCount();
        }

        public override void EndMove(Move move)
        {
            base.EndMove(move);
            _moveNodes.Add(GetNodeCount() - _currentMoveStartNodes);
        }

        public override void EndDepth(int depth)
        {
            base.EndDepth(depth);

            if (_moveNodes.Count > 1)
            {
                _moveNodes.Sort();
                int top = _moveNodes[_moveNodes.Count - 1];
                int nextbest = _moveNodes[_moveNodes.Count - 2];
                if (top < 50000)
                {
                    //not enough to go on at all
                    _currentComplexity.RefutePct = 0;
                    PctOfNormalToSpend = 1;
                    return;
                }
                _currentComplexity.RefutePct = (double)nextbest / (double)top;
            }
            else
            {
                _currentComplexity.RefutePct = 0;
            }


            PctOfNormalToSpendCalc();

        }

        private void PctOfNormalToSpendCalc()
        {
            var orderedHistory = _complexityHistory.ToList().OrderBy(o => o.RefutePct).ToList();
            var idxCurrent = orderedHistory.IndexOf(_currentComplexity);
            if (orderedHistory.Count < _pcts.Length) { idxCurrent += (_pcts.Length - orderedHistory.Count) / 2; }

            idxCurrent = Math.Max(0, Math.Min(idxCurrent, _pcts.Length - 1));
            PctOfNormalToSpend = _pcts[idxCurrent];

            if (_log.IsDebugEnabled)
            {
                _log.DebugFormat("refutePct:{0} spendOfNorm:{1}", _currentComplexity.RefutePct, PctOfNormalToSpend);
            }
        }
        public override void EndSearch()
        {
            base.EndSearch();
        }

        #region initialize pct array
        private double FindGrowth(double floor, int count, int iterations)
        {
            System.Diagnostics.Debug.Assert(floor < 1);

            double minGrow = 1;
            double maxGrow = 2;

            while (Average(floor, minGrow, count) > 1) { minGrow *= .5; }
            while (Average(floor, maxGrow, count) < 1) { maxGrow *= 2; }

            for (int i = 0; i < iterations; i++)
            {
                double testGrow = (minGrow + maxGrow) / 2f;
                double testAvg = Average(floor, testGrow, count);
                if (testAvg < 1)
                {
                    minGrow = testGrow;
                }
                else
                {
                    maxGrow = testGrow;
                }
            }

            return (minGrow + maxGrow) / 2f; ;
        }
        
        private double Average(double floor, double growth, int count)
        {
            return GetSequence(floor, growth, count).Average();
        }
        private IEnumerable<double> GetSequence(double floor, double growth, int count)
        {
            for(int i = 0; i < count; i++)
            {
                yield return floor;
                floor = floor * growth;
            }
        }

        #endregion
    }


    public abstract class TimeManagerGeneric<TUnit> : TimeManagerComplexity
    {
        //used as inputs
        public TimeControlGeneric<TUnit> TimeControl { get; set; }
        public TUnit AmountOnClock { get; set; }
        public FEN FEN { get; private set; }
        //
        public TUnit AmountToSpend { get; protected set; }

        //configuration factors
        public double RatioBase { get; set; }

        public double RatioCeiling { get; set; }

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

            int movesLeft;


            if (this.TimeControl.MovesPerControl > 0)
            {
                movesLeft = (this.TimeControl.MovesPerControl - ((fen.fullmove - 1) % this.TimeControl.MovesPerControl));
            }
            else
            {
                movesLeft = 30;
            }

            RatioBase = 1f / (double)movesLeft;
            RatioCeiling = RatioBase * 4;


            RatioBase = Math.Min(.97, RatioBase);
            RatioCeiling = Math.Min(.97, RatioCeiling);


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
                var byPct = Multiply(Multiply(AmountOnClock, RatioBase), PctOfNormalToSpend);
                var byMax = Multiply(AmountOnClock, RatioCeiling);
                AmountToSpend = Min(byPct, byMax);
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
