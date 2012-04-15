using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinobyl.Engine;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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
                while (StartingPGNs.Count < 2000)
                {
                    StartingPGNs.Add(ChessPGN.NextGame(reader));
                }
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


                var pgnWriter = File.CreateText("Challenge_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".pgn");
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                var challengerWins = DeterministicChallenge.Challenge(champion, challenger, StartingPGNs, "Mutation: " + mutation.ToString(), pgnWriter);

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
                        string xml = File.ReadAllText("ChampionSettings.xml");
                        var retval = Chess.DeserializeObject<ChessEvalSettings>(xml);
                        return retval;
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
                    if (File.Exists("ChampionSettings.xml"))
                    {
                        File.Delete("ChampionSettings.xml");
                    }
                    File.WriteAllText("ChampionSettings.xml",xml);
                }
            }
        }


    }
}
