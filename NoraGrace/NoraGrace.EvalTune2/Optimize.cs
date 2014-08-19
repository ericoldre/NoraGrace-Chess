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


    }

}
