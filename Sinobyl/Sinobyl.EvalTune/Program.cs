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

            List<ChessPGN> StartingPGNs = new List<ChessPGN>();
            using (StreamReader reader = new StreamReader(File.OpenRead("OpeningPositions.pgn")))
            {
                StartingPGNs.AddRange(ChessPGN.AllGames(reader).Take(200));
            }

            
            
            Stack<IEvalSettingsMutator> mutatorStack = new Stack<IEvalSettingsMutator>();

            int nodes = 500;

            while (true)
            {

                var competitors = new List<Func<IChessGamePlayer>>();

                competitors.Add(() => new DeterministicPlayer("Default:N" + nodes.ToString(), new ChessEval(), nodes));
                int nodesPlayer2 = (int)((double)nodes * 1.2);
                competitors.Add(() => new DeterministicPlayer("Default:N" + nodesPlayer2.ToString(), new ChessEval(), nodesPlayer2));

                string eventName = "Challenge_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_N" + nodes.ToString();
                
                var pgnWriter = File.CreateText(eventName + ".pgn");
                
                int wins = 0;
                int losses = 0;
                int draws = 0;

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                var matchResults = ChessMatch.RunParallelRoundRobinMatch
                (
                    isGauntlet:true,
                    competitors: competitors,
                    startingPositions: StartingPGNs,
                    onGameCompleted: (p) => 
                    {
                        pgnWriter.Write(p.Game.ToString());
                        pgnWriter.Write("\n\n");
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
                pgnWriter.Dispose();

                File.WriteAllText(eventName + "_Summary.txt", matchResults.Summary());
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
                Console.WriteLine("");
                Console.WriteLine("");
                Console.WriteLine("");
                
                

                //increment amount of nodes.
                nodes = (int)((float)nodes * 1.5);
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
