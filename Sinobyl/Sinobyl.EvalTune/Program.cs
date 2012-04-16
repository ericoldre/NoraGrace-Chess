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
                StartingPGNs.AddRange(ChessPGN.AllGames(reader).Take(2000));
            }

            
            
            Stack<IEvalSettingsMutator> mutatorStack = new Stack<IEvalSettingsMutator>();

            int nodes = 400;

            while (true)
            {
                var champion = Program.ChampionSettings;
                if(mutatorStack.Count()==0)
                {
                    mutatorStack.Push(MutatorFactory.Create(rand));
                }
                var mutation = mutatorStack.Pop();
                var challenger = champion.CloneDeep();
                mutation.Mutate(challenger);
                champion = ChessEvalSettings.Default();
                challenger = ChessEvalSettings.Default();
                challenger.Weight.Mobility.Opening = 0;
                challenger.Weight.Mobility.Endgame = 0;

                //Console.WriteLine("Trying Mutation: {0}", mutation.ToString());

                string eventName = "Challenge_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_N" + nodes.ToString();

                champion.Save(eventName + "_Champion.xml");
                challenger.Save(eventName + "_Challenger.xml");

                var pgnWriter = File.CreateText(eventName + ".pgn");
                
                int wins = 0;
                int losses = 0;
                int draws = 0;

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                var matchResults = ChessMatch.RunParallelMatch
                (
                    createPlayer1: () => new DeterministicPlayer("Champion", new ChessEval(champion), nodes) { CommentFormatter = (r) => string.Format("Score:{2} Depth:{1} PV:{0}", new ChessMoves(r.PrincipleVariation).ToString(), r.Depth, r.Score) },
                    createPlayer2: () => new DeterministicPlayer("Challenger", new ChessEval(challenger), nodes) { CommentFormatter = (r) => string.Format("Score:{2} Depth:{1} PV:{0}", new ChessMoves(r.PrincipleVariation).ToString(), r.Depth, r.Score) },
                    startingPositions: StartingPGNs,
                    onGameCompleted: (p) => 
                    {
                        p.Game.Headers.Add(new ChessPGNHeader("Mutation", mutation.ToString()));
                        p.Game.Headers.Add(new ChessPGNHeader("GameTime", (p.GameTime.ToString("c"))));

                        pgnWriter.Write(p.Game.ToString());
                        pgnWriter.Write("\n\n");
                        switch (p.Game.Result)
                        {
                            case ChessResult.Draw:
                                Console.Write("-");
                                break;
                            case ChessResult.WhiteWins:
                                Console.Write(p.Player1IsWhite ? "1" : "0");
                                break;
                            case ChessResult.BlackWins:
                                Console.Write(p.Player1IsWhite ? "0" : "1");
                                break;
                        }
                        if (p.Results.Count() % 100 == 0)
                        {
                            p.Results.ResultsForPlayer("Champion", out wins, out losses, out draws);
                            float totalgames = (wins + losses + draws);
                            Console.Write("[Games:{3} Champion:{0} Challenger:{1} Draws:{2}]",
                                ((float)wins / (float)totalgames).ToString("#0.##%"),
                                ((float)losses / (float)totalgames).ToString("#0.##%"),
                                ((float)draws / (float)totalgames).ToString("#0.##%"),
                                totalgames);
                        }
                    }
                );

                stopwatch.Stop();
                pgnWriter.Dispose();

                File.WriteAllText(eventName + "_Summary.txt", matchResults.Summary());

                matchResults.ResultsForPlayer("Champion", out wins, out losses, out draws);

                Console.WriteLine("\nChamp:{0} Challenger:{1} Draws:{2} Time:{4}",
                    wins,
                    losses,
                    draws,
                    matchResults.Count,
                    stopwatch.Elapsed.ToString("c"));

                if (losses > wins)
                {
                    Console.WriteLine("ChallengerWins in {0:c}\n", stopwatch.Elapsed);
                    foreach (var similiarMutation in mutation.SimilarMutators())
                    {
                        mutatorStack.Push(similiarMutation);
                    }
                    Program.ChampionSettings = challenger;
                }
                else
                {
                    Console.WriteLine("ChallengerFails in {0:c}\n", stopwatch.Elapsed);
                }

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
                string xml = Chess.SerializeObject<ChessEvalSettings>(value);
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
