using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NoraGrace.Engine;
using NoraGrace.Engine.Evaluation;

namespace NoraGrace.EvalTune2
{

    class Stuff
    {
        public NoraGrace.Engine.Evaluation.Evaluator Evaluator { get; set; }
        public IList<MovePicker> MovePickerStack { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            
            //var pgns = PGN.AllGames(new System.IO.FileInfo("noise3.pgn")).Concat(PGN.AllGames(new System.IO.FileInfo("noise.pgn")).Take(20000));
            //BinaryPGN.ConvertToBinary(pgns, "noise.bin");
            //BinaryPGN.Randomize(new System.IO.FileInfo("noise.bin"), new System.IO.FileInfo("noiseRand.bin"));

            //Console.WriteLine("done");
            //return;


            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            Action<int> progCB = (i) =>
            {
                if (i % 3000 == 0) { Console.Write("."); }
                if (i == -1) { Console.WriteLine("*"); }
            };


            TunableParameterList parameters = new TunableParameterList();


            ////material
            parameters.AddRange(TunableParameter.MaterialParams());

            //mobility
            //parameters.AddRange(TunableParameter.MobilityParams());

            ////pawns
            //parameters.AddRange(TunableParameter.PawnParams());

            ////passed pawns
           // parameters.AddRange(TunableParameter.PassedPawnParams());

            ////king attacks
            //parameters.AddRange(TunableParameter.KingSafetyParams());

            //pcsq
            //parameters.AddRange(TunableParameterPcSq.SelectAll());


            Tune(parameters, "exchanges", progCB, true);
            //Tune(parameters, "AllButPcSq", progCB, true);

            //FindRook(progCB);
            //FindLowTanDiv(2, progCB);
            //FindLowPow(mcp, progCB);

            Console.WriteLine(sw.Elapsed);
            Console.ReadLine();
            return;


        }

        public static void Tune(TunableParameterList parameters, string testName, Action<int> progCallback, bool fullOptimize)
        {
            //save initial settings
            parameters.CreateSettings(parameters.CreateDefaultValues()).Save(string.Format("{0}.orig.xml", testName));

            double bestE = double.MaxValue;
            int iteration = 0;
            double[] initialValues = parameters.CreateDefaultValues();
            double[] increments = parameters.CreateIncrements();
            string[] names = parameters.CreateNames();

            Func<double[], double> fnScore = (testValues)=>
            {
                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();

                double e = Fitness.FindFitness(() => parameters.CreateEvaluator(testValues), progCallback, 60000);

                stopwatch.Stop();

                iteration++;

                Console.WriteLine("");
                Console.WriteLine(e);
                Console.WriteLine(stopwatch.Elapsed);

                if (e < bestE)
                {
                    bestE = e;
                    parameters.WriteToFile(string.Format("{0}.bestlog.txt", testName), testValues, e, iteration);
                    Console.WriteLine("newbest");
                    parameters.CreateSettings(testValues).Save(string.Format("{0}.best.xml", testName));
                }
                else
                {
                    //parameters.WriteToFile(string.Format("{0}.rejectlog.txt", testName), testValues, e, iteration);
                }
                Console.WriteLine("");
                return e;
            };

            if (fullOptimize)
            {
                Optimize.OptimizeWave(initialValues, increments, names, fnScore);
                Optimize.OptimizeNew(initialValues, increments, names, fnScore);
                //Optimize.OptimizeValues(initialValues, increments, fnScore);
            }
            else
            {
                Optimize.OptimizeEachIndividually(initialValues, increments, fnScore);
            }
            
            
        }

        


        //public static void FindLowK(Func<IEnumerable<BinaryPGN>> fnPGN, double tandiv)
        //{
        //    double lowE = double.MaxValue;
        //    double lowVal = 0;
        //    for (double k = .5; k < 3.0; k += .25)
        //    {
        //        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        //        stopwatch.Start();

        //        Func<Evaluator> fnEval = () =>
        //        {
        //            Engine.Evaluation.Settings settings = Engine.Evaluation.Settings.Default();
        //            //settings.MaterialValues.Pawn.Opening = pawnVal;
        //            return new Evaluator(settings);
        //        };
        //        double e = PgnListEParallel(fnPGN(), fnEval, k, tandiv);
        //        Console.WriteLine("");
        //        Console.WriteLine("k={1}, E={0}, time={2}", e, k, stopwatch.Elapsed);

        //        if (e < lowE)
        //        {
        //            lowE = e;
        //            lowVal = k;
        //        }
        //        Console.WriteLine("lowval={0}, lowE={1}", lowVal, lowE);
        //    }
        //    Console.WriteLine("lowval={0}, lowE={1}", lowVal, lowE);
        //}


        public static void FindLowPow(Action<int> progressCb)
        {

            Func<double, double> fnScore = (pow) =>
            {
                Fitness.TanPow = pow;
                double retval = Fitness.FindFitness(Fitness.DefaultEvaluator, progressCb);
                Console.WriteLine(string.Format("pow={0} retval={1}", pow, retval));
                return retval;
            };

            var best = Optimize.OptimizeWithin(1, 10, .03, fnScore);

            Console.WriteLine(string.Format("best pow={0}", best));


        }


        public static void FindLowTanDiv(double k, Action<int> progressCb)
        {


            Func<double, double> fnScore = (tanDiv) =>
            {
                Fitness.TanDiv = tanDiv;
                double retval = Fitness.FindFitness(Fitness.DefaultEvaluator, progressCb);
                Console.WriteLine(string.Format("tanDiv={0} retval={1}", tanDiv, retval));
                return retval;
            };

            var best = Optimize.OptimizeWithin(1, 1000, 5, fnScore);

            Console.WriteLine(string.Format("best tanDiv={0}", best));


        }

        //public static void FindPawnVal(Func<IEnumerable<BinaryPGN>> fnPGN, double k, double mcp)
        //{
        //    double lowE = double.MaxValue;
        //    double lowVal = 0;
        //    for (int pawnVal = 30; pawnVal < 300; pawnVal += 20)
        //    {
        //        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        //        stopwatch.Start();

        //        Func<Evaluator> fnEval = () =>
        //        {
        //            Engine.Evaluation.Settings settings = new Settings();
        //            settings.MaterialValues.Pawn.Opening = pawnVal;
        //            return new Evaluator(settings);
        //        };
        //        double e = PgnListEParallel(fnPGN(), fnEval, k, mcp);
        //        Console.WriteLine("");
        //        Console.WriteLine("pawn={1}, E={0}, time={2}", e, pawnVal, stopwatch.Elapsed);

        //        if (e < lowE)
        //        {
        //            lowE = e;
        //            lowVal = pawnVal;
        //        }
        //        Console.WriteLine("lowval={0}, lowE={1}", lowVal, lowE);
        //    }
        //    Console.WriteLine("lowval={0}, lowE={1}", lowVal, lowE);
        //}

        //public static void FindRookVal(Func<IEnumerable<PGN>> fnPGN, double k, double mcp)
        //{
        //    double lowE = double.MaxValue;
        //    double lowVal = 0;
        //    for (int testVal = 250; testVal < 800; testVal += 50)
        //    {
        //        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        //        stopwatch.Start();

        //        Func<Evaluator> fnEval = () =>
        //        {
        //            Engine.Evaluation.Settings settings = Engine.Evaluation.Settings.Default();
        //            settings.MaterialValues.Rook.Endgame = testVal;
        //            return new Evaluator(settings);
        //        };
        //        double e = PgnListEParallel(fnPGN(), fnEval, k, mcp);
        //        Console.WriteLine("");
        //        Console.WriteLine("test={1}, E={0}, time={2}", e, testVal, stopwatch.Elapsed);

        //        if (e < lowE)
        //        {
        //            lowE = e;
        //            lowVal = testVal;
        //        }
        //        Console.WriteLine("lowval={0}, lowE={1}", lowVal, lowE);
        //    }
        //    Console.WriteLine("lowval={0}, lowE={1}", lowVal, lowE);
        //}





        public static double SigDiff(double qscore, GameResult gameResult, double k, double mcp)
        {
            double qSig = Sigmoid(qscore, k, mcp);
            double gScore = GameScore(gameResult);

            double sigDiff = Math.Abs(gScore - qSig);
            double siqSq = Math.Pow(sigDiff, 2);
            return siqSq;
        }




        public static double GameScore(GameResult results)
        {
            switch (results)
            {
                case GameResult.Draw:
                    return .5;
                case GameResult.WhiteWins:
                    return 1;
                case GameResult.BlackWins:
                    return 0;
                default:
                    throw new ArgumentException();
            }
        }

        /// <param name="s">score in centipawns</param>
        /// <param name="k">constant</param>
        /// <param name="mcp">what would be considered a "crushing" score.</param>
        /// <returns></returns>
        static double Sigmoid(double s, double k, double mcp)
        {
            return 1 / (1 + Math.Pow(10, (-k * s / mcp)));
        }



    }
}
