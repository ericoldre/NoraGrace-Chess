using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoraGrace.EvalTune2
{
    public class Optimize
    {





        public static void OptimizeValues(double[] values, double[] increments, Func<double[], double> fnScore)
        {
            OptimizeEachIndividually(values, increments, fnScore);
            OptimizeLowerValues(values, increments, 0, fnScore);
        }

        public static void OptimizeEachIndividually(double[] values, double[] increments, Func<double[], double> fnScore)
        {
            for (int i = 0; i < values.Length; i++)
            {
                OptimizeSingleValue(values, increments, i, fnScore);
            }
        }

        public static double OptimizeWithin(double min, double max, double within, Func<double, double> fnScore)
        {
            System.Diagnostics.Debug.Assert(max > min);

            double middle = (min + max) / 2;
            double inc = (max - min) / 10;
            if (max - min < within) { return middle; }
            
            double[] children = new double[] { middle };
            double[] incr = new double[] { inc };
            Func<double[], double> fnScoreChild = (a) => fnScore(a[0]);
            OptimizeSingleValue(children, incr, 0, fnScoreChild);
            var est = children[0];

            return OptimizeWithin(est - inc, est + inc, within, fnScore);
        }

        /// <summary>
        /// varies values[index] until best score found
        /// </summary>
        public static void OptimizeSingleValue(double[] values, double[] increments, int index, Func<double[], double> fnScore)
        {

            double bestScore = fnScore(values);
            double bestValue = values[index];

            double indexInitialValue = values[index];

            foreach (int incrementSign in new int[] { -1, 1 })
            {
                double currentValue = values[index];
                bool skipOtherSign = false;
                while (true)
                {
                    //find value to test;
                    currentValue += (increments[index] * incrementSign);

                    values[index] = currentValue;

                    var score = fnScore(values);

                    if (score < bestScore)
                    {
                        bestScore = score;
                        skipOtherSign = true;
                        bestValue = currentValue;
                    }
                    else
                    {
                        break;
                    }
                }
                if (skipOtherSign)
                {
                    break;
                }
            }
            values[index] = bestValue;
        }

        public static double OptimizeLowerValues(double[] values, double[] increments, int index, Func<double[], double> fnScore)
        {



            //if no params to vary then return 
            if (index >= values.Length)
            {
                return fnScore(values);
            }

            //create copy of initial values passed in.
            double[] initialVals = values.Clone() as double[];

            double[] children = values.Clone() as double[];
            double bestScore = OptimizeLowerValues(children, increments, index + 1, fnScore);
            double[] bestParams = children.Clone() as double[];

            double indexInitialValue = values[index];

            foreach (int incrementSign in new int[] { -1, 1 })
            {
                double indexCurrentValue = indexInitialValue;
                bool skipOtherSign = false;
                while (true)
                {
                    //find value to test;
                    indexCurrentValue += (increments[index] * incrementSign);
                    children = bestParams.Clone() as double[];

                    children[index] = indexCurrentValue;

                    var score = OptimizeLowerValues(children, increments, index + 1, fnScore);


                    if (score < bestScore)
                    {
                        bestScore = score;
                        skipOtherSign = true;
                        bestParams = children.Clone() as double[];
                    }
                    else
                    {
                        break;
                    }

                }
                if (skipOtherSign)
                {
                    break;
                }
            }


            Array.Copy(bestParams, values, values.Length);
            return bestScore;
        }

        private static int GetValuesHash(double[] values)
        {
            int hc = values.Length;
            for (int i = 0; i < values.Length; ++i)
            {
                hc = unchecked(hc * 17 + values[i].GetHashCode());
            }
            return hc;
        }

        public static void OptimizeNew(double[] values, double[] increments, string[] names, Func<double[], double> fnScore)
        {
            System.Diagnostics.Debug.Assert(values.Length == increments.Length && values.Length == names.Length);
            double bestScore = fnScore(values);
            double[] testValues = new double[values.Length];
            double[] initial = new double[values.Length];
            Array.Copy(values, initial, values.Length);

            ParameterHistory.List pHistory = new ParameterHistory.List(values, increments, names);
            int consecutiveFailures = 0;
            int iterations = 0;
            int improvements = 0;
            var testParamHistories = pHistory.SelectSubset();

            Dictionary<int, double> scoreHistory = new Dictionary<int, double>();
            while (true)
            {
                //init test values to best;
                Array.Copy(values, testValues, values.Length);

                //if last test failed, choose new parameters to vary
                if (consecutiveFailures > 0)
                {
                    testParamHistories = pHistory.SelectSubset();
                }

                Console.WriteLine("i:{0} imp:{1} conFail:{2}", iterations, improvements, consecutiveFailures);
                foreach (var tph in testParamHistories)
                {

                    double startVal = testValues[tph.Index];
                    double diff = tph.Increment * tph.CurrentSign * tph.MultiplierRandom;
                    testValues[tph.Index] += diff;
                    Console.WriteLine(tph.Name + " " + startVal.ToString() + " " + diff.ToString());
                }
                double score = double.MaxValue;
                int phash = GetValuesHash(testValues);
                if(scoreHistory.ContainsKey(phash))
                {
                    score = scoreHistory[phash];
                }
                else
                {
                    score = fnScore(testValues);
                }
                
                bool newBest = score < bestScore;

                foreach (var tph in testParamHistories)
                {
                    tph.RecordHistory(tph.CurrentSign, newBest);
                }

                if (newBest)
                {
                    bestScore = score;
                    Array.Copy(testValues, values, values.Length);
                    consecutiveFailures = 0;
                    improvements++;
                }
                else
                {
                    consecutiveFailures++;
                }
                iterations++;

                if (consecutiveFailures > 100)
                {
                    break;
                }
            }

        }

    }

    

    public class ParameterHistory
    {
        private readonly int _index;
        private readonly double _increment;
        private readonly double _initValue;
        private readonly string _name;
        private readonly ParameterSignHistory _upHist = new ParameterSignHistory(1);
        private readonly ParameterSignHistory _downHist = new ParameterSignHistory(-1);

        public ParameterHistory(int index, double initValue, double increment, string name)
        {
            _index = index;
            _initValue = initValue;
            _increment = increment;
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }



        public void RecordHistory(int sign, bool success)
        {
            switch (sign)
            {
                case 1:
                    _upHist.RecordHist(success);
                    break;
                case -1:
                    _downHist.RecordHist(success);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public int Index
        {
            get { return _index; }
        }

        public double Increment
        {
            get { return _increment; }
        }

        public double InitValue
        {
            get { return _initValue; }
        }

        public int CurrentSign
        {
            get { return GoodHist.Sign; }
        }

        private ParameterSignHistory GoodHist
        {
            get { return _upHist.Score > _downHist.Score ? _upHist : _downHist; }
        }

        private ParameterSignHistory BadHist
        {
            get { return GoodHist == _upHist ? _upHist : _downHist; }
        }

        private readonly Random rand = new Random();
        public double MultiplierRandom
        {
            get
            {
                double retval = 1;
                while (rand.Next(3) == 0)
                {
                    retval += 1;
                }
                return retval;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} Score:{1}", _name, Score);
        }
        public double Score
        {
            get 
            {
                return GoodHist.Score + (BadHist.Score / 3);
            }//Upstats.Score + DownStats.Score/3
        }

        public class List : List<ParameterHistory>
        {

            public List(double[] values, double[] increments, string[] names)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    this.Add(new ParameterHistory(i, values[i], increments[i], names[i]));
                }
            }

            private readonly Random rand = new Random();
            public List<ParameterHistory> SelectSubset()
            {
                int totalcount = 1 + rand.Next((int)Math.Sqrt(this.Count) + 2);
                totalcount = Math.Min(this.Count, totalcount);
                totalcount = Math.Max(1, totalcount);

                int randCount = rand.Next((int)Math.Sqrt(totalcount));

                HashSet<ParameterHistory> indexes = new HashSet<ParameterHistory>();
                while (indexes.Count < randCount)
                {
                    var o = this[rand.Next(this.Count)];
                    if (!indexes.Contains(o)) { indexes.Add(o); }
                }
                var retval = this.OrderByDescending(o => o.Score).Where(o => !indexes.Contains(o)).Take(totalcount - randCount).ToList();
                return retval;
            }
            
        }
    }

    public class ParameterSignHistory
    {
        public const int RECENT_LENGTH = 10;

        private readonly int _sign;
        private Queue<bool> _recent = new Queue<bool>();
        private int _attempts_total = 0;
        private int _attempts_success = 0;
        private bool _last_attempt;
        

        public ParameterSignHistory(int sign)
        {
            _sign = sign;
        }

        public int Sign { get { return _sign; } }

        public void RecordHist(bool success)
        {
            _last_attempt = success;
            _attempts_total++;
            _attempts_success += success ? 1 : 0;
            _recent.Enqueue(success);
            while (_recent.Count > RECENT_LENGTH)
            {
                _recent.Dequeue();
            }
        }

        public double Score
        {
            get
            {
                return (PctSuccessLifetime + PctSuccessLifetime + PctSuccessLast + PctSuccessLast) / 4;
            }
        }

        public double PctSuccessLifetime
        {
            get
            {
                var total = _attempts_total;
                var success = _attempts_success;
                if (total < RECENT_LENGTH)
                {
                    int diff = RECENT_LENGTH - total;
                    total += diff;
                    success += diff / 2;
                }
                return (double)success / (double)total;
            }
        }

        public double PctSuccessRecent
        {
            get
            {
                var total = _recent.Count;
                var success = _recent.Where(s => s).Count();
                if (total < RECENT_LENGTH)
                {
                    int diff = RECENT_LENGTH - total;
                    total += diff;
                    success += diff / 2;
                }
                return (double)success / (double)total;
            }
        }


        public double PctSuccessLast
        {
            get
            {
                return _last_attempt ? 1 : 0;
            }
        }
    }
}
