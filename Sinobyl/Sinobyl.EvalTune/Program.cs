using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinobyl.Engine;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Sinobyl.EvalTune
{
    class Program
    {
        static object writeLock = new object();
        static void Main(string[] args)
        {
            Random rand = new Random();

            //read in a series of openings.
            List<ChessPGN> StartingPGNs = new List<ChessPGN>();
            Console.WriteLine("Beginning parse of opening positions");
            using (System.IO.StreamReader reader = new System.IO.StreamReader(System.IO.File.OpenRead("OpeningPositions.pgn")))
            {
                StartingPGNs.AddRange(ChessPGN.AllGames(reader).Take(10000));
            }
            Console.WriteLine("completed parse of opening positions");



            string paramName = "RatioComplexity";

            //ALTER THIS TO CHANGE THE PARAMETER TO TUNE THE SETTING YOU WANT.
            Func<double, string, DeterministicPlayer> fnCreatePlayer = (pval,pname) =>
            {
                string name = string.Format("{0}{1:f4}", pname, pval);
                ChessEvalSettings evalsettings = ChessEvalSettings.Default();
                ChessEval eval = new ChessEval(evalsettings);
                TimeManagerNodes manager = new TimeManagerNodes();
                
                //manager.RatioBase = pval;
                manager.RatioComplexity = pval;

                DeterministicPlayer player = new DeterministicPlayer(name, eval, manager);

                return player;
            };


            double parameterValue = (new TimeManagerNodes()).RatioComplexity;

            string tuneFileName = string.Format("{0}_TuneResults.txt", paramName);

            //try and read previous value from tune log to start there.
            if (System.IO.File.Exists(tuneFileName))
            {
                using (var reader = System.IO.File.OpenText(tuneFileName))
                {
                    string line;
                    while((line = reader.ReadLine()) != null)
                    {
                        double.TryParse(line, out parameterValue);
                    }
                }
            }

            while (true)
            {
                int nodesPerMove = rand.Next(20000, 22000);

                ChessTimeControlNodes timeControl = new ChessTimeControlNodes() { InitialAmount = nodesPerMove * 20, BonusEveryXMoves = 1, BonusAmount = nodesPerMove };
                
                //create list of starting positions
                int gamesPerMatch = 100;
                var startingPGNsForThisMatch = StartingPGNs.OrderBy(x => rand.Next()).Take(gamesPerMatch / 2).ToList();

                //create test param values;
                double deltaPct = 0.25f;
                double valHigh = parameterValue * (1f + deltaPct);
                double valLow = parameterValue * (1f - deltaPct);
                double delta = parameterValue - valLow;

                Func<DeterministicPlayer> fnHighPlayer = () => fnCreatePlayer(valHigh, paramName);
                Func<DeterministicPlayer> fnLowPlayer = () => fnCreatePlayer(valLow, paramName);

                //ChessEvalSettings highSettings = ChessEvalSettings.Default();
                //ChessEvalSettings lowSettings = ChessEvalSettings.Default();

                //fnSetEvalScore(highSettings, valHigh);
                //fnSetEvalScore(lowSettings, valLow);

                string highPlayerName = fnHighPlayer().Name;
                string lowPlayerName = fnLowPlayer().Name;

                var competitors = new List<Func<DeterministicPlayer>>();
                competitors.Add(fnHighPlayer);
                competitors.Add(fnLowPlayer);

                string eventName = string.Format("Challenge_{3} {0} {1} vs {2}", paramName, valHigh, valLow, DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                
                //var pgnWriter = File.CreateText(eventName + ".pgn");
                
                int wins = 0;
                int losses = 0;
                int draws = 0;

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                var matchResults = DeterministicChallenge.RunParallelRoundRobinMatch
                (
                    isGauntlet:true,
                    competitors: competitors,
                    startingPositions: startingPGNsForThisMatch,
                    timeControl: timeControl,
                    onGameCompleted: (p) => 
                    {
                        //pgnWriter.Write(p.Game.ToString());
                       // pgnWriter.Write("\n\n");
                        switch (p.Game.Result)
                        {
                            case ChessResult.Draw:
                                Console.Write("-");
                                break;
                            case ChessResult.WhiteWins:
                                Console.Write(p.Game.White == highPlayerName ? @"/" : @"\");
                                break;
                            case ChessResult.BlackWins:
                                Console.Write(p.Game.White == highPlayerName ? @"\" : @"/");
                                break;
                        }
                        //if (p.Results.Count() % 200 == 0)
                        //{
                        //    Console.WriteLine();
                        //    foreach (var compName in competitors.Select(f => f().Name))
                        //    {
                        //        p.Results.ResultsForPlayer(compName, out wins, out losses, out draws);
                        //        float totalgames = (wins + losses + draws);
                        //        Console.WriteLine("Player:{4}, WinPct:{0} LossPct:{1} DrawPct:{2} GameCount:{3}",
                        //            ((float)wins / (float)totalgames).ToString("#0.##%"),
                        //            ((float)losses / (float)totalgames).ToString("#0.##%"),
                        //            ((float)draws / (float)totalgames).ToString("#0.##%"),
                        //            totalgames,
                        //            compName);
                        //    }
                        //    Console.WriteLine();
                        //}
                    }
                );

                stopwatch.Stop();
                //pgnWriter.Dispose();
                string timeSpent = stopwatch.Elapsed.ToString(@"hh\:mm\:ss");
                //record and print out results.
               // File.WriteAllText(eventName + "_Summary.txt", matchResults.Summary());
                Console.WriteLine();
                Console.WriteLine("Completed {1} node match in {0:c}", stopwatch.Elapsed, nodesPerMove);
                foreach (var compName in competitors.Select(f => f().Name))
                {
                    matchResults.ResultsForPlayer(compName, out wins, out losses, out draws);
                    float totalgames = (wins + losses + draws);
                    Console.WriteLine("Player:{4}, WinPct:{0} LossPct:{1} DrawPct:{2} GameCount:{3}",
                        ((float)wins / (float)totalgames).ToString("#0.##%"),
                        ((float)losses / (float)totalgames).ToString("#0.##%"),
                        ((float)draws / (float)totalgames).ToString("#0.##%"),
                        totalgames,
                        compName);
                }


                matchResults.ResultsForPlayer(highPlayerName, out wins, out losses, out draws);
                //var paramDelta = delta * 0.002f;
                var paramDelta = delta * 0.004f;
                var newParamValue = parameterValue + ((wins - losses) * paramDelta);
               
                string changeSummary = string.Format("{0}\tFrom\t{1:f4}\tTo\t{2:f4}\tin\t{3}", paramName, parameterValue, newParamValue, timeSpent);
                Console.WriteLine(changeSummary);

                string newParamValueString = string.Format("{0:f8}", newParamValue);
                System.IO.File.AppendAllLines(tuneFileName, new string[] { newParamValueString });

                Console.WriteLine("");
                Console.WriteLine("");
                Console.WriteLine("");

                parameterValue = newParamValue;

            }
            Console.WriteLine("done");
            Console.ReadLine();
        }

        private static object _lock = new object();

        public static ChessEvalSettings ChampionSettings
        {
            get
            {
                lock (_lock)
                {
                    if (System.IO.File.Exists("ChampionSettings.xml"))
                    {
                        return ChessEvalSettings.Load(System.IO.File.OpenRead("ChampionSettings.xml"));
                    }
                    else
                    {
                        return ChessEvalSettings.Default();
                    }
                }
            }
            set
            {
                string xml = ChessEvalSettings.SerializeObject<ChessEvalSettings>(value);
                lock (_lock)
                {
                    value.Save("ChampionSettings.xml");
                }
            }
        }


    }
    public static class Tools
    {
        private static readonly Type[] WriteTypes = new[] {
        typeof(string), typeof(DateTime), typeof(Enum), 
        typeof(decimal), typeof(Guid)};

        public static bool IsSimpleType(this Type type)
        {
            return type.IsPrimitive || WriteTypes.Contains(type);
        }
        public static XElement ToXml(this object input)
        {
            return input.ToXml(null);
        }
        public static XElement ToXml(this object input, string element)
        {
            if (input == null)
                return null;

            if (string.IsNullOrEmpty(element))
                element = "object";
            element = XmlConvert.EncodeName(element);
            var ret = new XElement(element);

            if (input != null)
            {
                var type = input.GetType();
                var props = type.GetProperties();

                var elements = from prop in props
                               let name = XmlConvert.EncodeName(prop.Name)
                               let val = prop.GetValue(input, null)
                               let value = prop.PropertyType.IsSimpleType()
                                    ? new XElement(name, val)
                                    : val.ToXml(name)
                               where value != null
                               select value;

                ret.Add(elements);
            }

            return ret;
        }
    }
}
