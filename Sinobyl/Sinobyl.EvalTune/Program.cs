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
                StartingPGNs.AddRange(ChessPGN.AllGames(reader).Take(5));
            }

            
            
            Stack<IEvalSettingsMutator> mutatorStack = new Stack<IEvalSettingsMutator>();

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

                Console.WriteLine("Trying Mutation: {0}", mutation.ToString());

                string ChallengeName = "Challenge_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");

                champion.Save(ChallengeName + "_Champion.xml");
                challenger.Save(ChallengeName + "_Challenger.xml");

                var pgnWriter = File.CreateText(ChallengeName + ".pgn");
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                var challengerWins = DeterministicChallenge.Challenge(
                    ()=>new ChessEval(champion), 
                    ()=>new ChessEval(challenger), 
                    StartingPGNs, 
                    "Mutation: " + mutation.ToString(), 
                    pgnWriter);

                stopwatch.Stop();
                pgnWriter.Dispose();

                if (challengerWins)
                {
                    Console.WriteLine("ChallengerWins in {0} seconds", stopwatch.ElapsedMilliseconds/1000);
                    foreach (var similiarMutation in mutation.SimilarMutators())
                    {
                        mutatorStack.Push(similiarMutation);
                    }
                    Program.ChampionSettings = challenger;
                }
                else
                {
                    Console.WriteLine("ChallengerFails in {0} seconds", stopwatch.ElapsedMilliseconds / 1000);
                }

                break;

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
