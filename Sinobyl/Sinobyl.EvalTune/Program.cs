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
                while (StartingPGNs.Count <= 10)
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
                var winner = DeterministicChallenge.Challenge(champion, challenger, StartingPGNs);

                if (winner.Equals(challenger))
                {
                    Console.WriteLine("ChallengerWins");
                    foreach (var similiarMutation in mutation.SimilarMutators())
                    {
                        mutatorStack.Push(similiarMutation);
                    }
                    Program.ChampionSettings = winner;
                }
                else
                {
                    Console.WriteLine("ChallengerFails");
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
