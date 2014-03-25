using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinobyl.Engine;
using System.IO;
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
            using (StreamReader reader = new StreamReader(File.OpenRead("OpeningPositions.pgn")))
            {
                StartingPGNs.AddRange(ChessPGN.AllGames(reader).Take(100000));
            }
            Console.WriteLine("completed parse of opening positions");
            
            
            //Stack<IEvalSettingsMutator> mutatorStack = new Stack<IEvalSettingsMutator>();

            Func<ChessEvalSettings, double> fnGetParamVal = (s) => (double)s.PawnPassed8thRankScore;
            Action<ChessEvalSettings, double> fnSetEvalScore = (s, v) => { s.PawnPassed8thRankScore = (int)Math.Round(v); };
            string paramName = "PawnPassed8thRankScore";


            double parameterValue = fnGetParamVal(ChessEvalSettings.Default());

            while (true)
            {
                int nodes = rand.Next(6000, 8000);
                int delta = rand.Next(30, 40);
                
                //create list of starting positions
                int gamesPerMatch = 100;
                var startingPGNsForThisMatch = StartingPGNs.OrderBy(x => rand.Next()).Take(gamesPerMatch / 2).ToList();

                

                //create settings
                int valHigh = (int)Math.Round(parameterValue) + delta;
                int valLow = (int)Math.Round(parameterValue) - delta;


                ChessEvalSettings highSettings = ChessEvalSettings.Default();
                ChessEvalSettings lowSettings = ChessEvalSettings.Default();

                fnSetEvalScore(highSettings, valHigh);
                fnSetEvalScore(lowSettings, valLow);

                string highPlayerName = string.Format("{0}{1}", paramName, valHigh, nodes);
                string lowPlayerName = string.Format("{0}{1}", paramName, valLow, nodes);

                var competitors = new List<Func<IChessGamePlayer>>();
                competitors.Add(() => new DeterministicPlayer(highPlayerName, new ChessEval(highSettings), nodes));
                competitors.Add(() => new DeterministicPlayer(lowPlayerName, new ChessEval(lowSettings), nodes));

                string eventName = string.Format("Challenge_{3} {0} {1} vs {2}", paramName, valHigh, valLow, DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                
                //var pgnWriter = File.CreateText(eventName + ".pgn");
                
                int wins = 0;
                int losses = 0;
                int draws = 0;

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                var matchResults = ChessMatch.RunParallelRoundRobinMatch
                (
                    isGauntlet:true,
                    competitors: competitors,
                    startingPositions: startingPGNsForThisMatch,
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
                                Console.Write("1");
                                break;
                            case ChessResult.BlackWins:
                                Console.Write("0");
                                break;
                        }
                        if (p.Results.Count() % 200 == 0)
                        {
                            Console.WriteLine();
                            foreach (var compName in competitors.Select(f => f().Name))
                            {
                                p.Results.ResultsForPlayer(compName, out wins, out losses, out draws);
                                float totalgames = (wins + losses + draws);
                                Console.WriteLine("Player:{4}, WinPct:{0} LossPct:{1} DrawPct:{2} GameCount:{3}",
                                    ((float)wins / (float)totalgames).ToString("#0.##%"),
                                    ((float)losses / (float)totalgames).ToString("#0.##%"),
                                    ((float)draws / (float)totalgames).ToString("#0.##%"),
                                    totalgames,
                                    compName);
                            }
                            Console.WriteLine();
                        }
                    }
                );

                stopwatch.Stop();
                //pgnWriter.Dispose();
                string timeSpent = stopwatch.Elapsed.ToString(@"hh\:mm\:ss");
                //record and print out results.
               // File.WriteAllText(eventName + "_Summary.txt", matchResults.Summary());
                Console.WriteLine();
                Console.WriteLine("Completed {1} node match in {0:c}", stopwatch.Elapsed, nodes);
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
                var paramDelta = delta * 0.005f;
                var newParamValue = parameterValue + ((wins - losses) * paramDelta);
               
                string changeSummary = string.Format("{0}\tFrom\t{1:f2}\tTo\t{2:f2}\tin\t{3}", paramName, parameterValue, newParamValue, timeSpent);
                Console.WriteLine(changeSummary);
                File.AppendAllLines(string.Format("{0}_TuneResults.txt", paramName), new string[] { changeSummary });

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
                    if (File.Exists("ChampionSettings.xml"))
                    {
                        return ChessEvalSettings.Load(File.OpenRead("ChampionSettings.xml"));
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
