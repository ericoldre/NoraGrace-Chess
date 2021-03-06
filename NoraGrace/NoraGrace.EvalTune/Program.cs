﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoraGrace.Engine;
using NoraGrace.Engine.Evaluation;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace NoraGrace.EvalTune
{
    class Program
    {
        static object writeLock = new object();
        static void Main(string[] args)
        {
            Random rand = new Random();

            //read in a series of openings.
            List<PGN> StartingPGNs = new List<PGN>();
            Console.WriteLine("Beginning parse of opening positions");
            using (System.IO.StreamReader reader = new System.IO.StreamReader(System.IO.File.OpenRead("OpeningPositions.pgn")))
            {
                StartingPGNs.AddRange(PGN.AllGames(reader).Take(10000));
            }
            Console.WriteLine("completed parse of opening positions");


            //ALTER THIS
            string paramName = "ExtendSEEPositiveOnly";

            //ALTER THIS
            double parameterValue = Settings.Default().PawnPassed8thRankScore;

            //ALTER THIS TO CHANGE THE PARAMETER TO TUNE THE SETTING YOU WANT.
            Func<double, double, string, DeterministicPlayer> fnCreatePlayer = (pval, otherval,pname) =>
            {
                string name = string.Format("{0}{1:f4}", pname, pval);
                Settings evalsettings = Settings.Default();

                //evalsettings.KingAttackFactor = pval;
               // evalsettings.KingAttackFactorQueenTropismBonus = pval / 2;
                //evalsettings.PawnCandidatePct = pval;
                //evalsettings.PawnPassed8thRankScore = (int)pval;
                //evalsettings.PawnPassedFarPct = pval + .1f;
                //if (pval > otherval)
                //{
                //    evalsettings.PawnPassedClosePct = .4f;
                //    evalsettings.PawnPassedFarPct = 1.2f;
                //}
                //else
                //{
                //    evalsettings.PawnPassedClosePct = .5f;
                //    evalsettings.PawnPassedFarPct = .5f;
                //}
                
                
                
                Evaluator eval = new Evaluator(evalsettings);

                TimeManagerNodes manager = new TimeManagerNodes();
                
                
                DeterministicPlayer player = new DeterministicPlayer(name, eval, manager);
                player.AlterSearchArgs = (searchArgs) =>
                {
                    searchArgs.ExtendSEEPositiveOnly = pval > otherval;
                    //manager.RatioComplexity = 0; // pval > otherval ? 1 : 0;
                   // manager.RatioFailHigh = pval > otherval ? 1.5 : 1;
                    //if (pval > otherval) { searchArgs.MaxDepth = 3; }
                };
                return player;
            };



            string tuneFileName = string.Format("{0}_TuneResults.txt", paramName);

            int totalWins = 0;
            int totalLosses = 0;
            int totalDraws = 0;

            //try and read previous value from tune log to start there.
            if (System.IO.File.Exists(tuneFileName))
            {
                using (var reader = System.IO.File.OpenText(tuneFileName))
                {
                    string line;
                    while((line = reader.ReadLine()) != null)
                    {
                        var split = line.Split(new char[] { '\t' });
                        double.TryParse(split[0], out parameterValue);
                        int.TryParse(split[1], out totalWins);
                        int.TryParse(split[2], out totalLosses);
                        int.TryParse(split[3], out totalDraws);
                    }
                }
            }

            

            while (true)
            {
                int nodesPerMove = 100000;// 15000;
                nodesPerMove = rand.Next(nodesPerMove, (int)((float)nodesPerMove * 1.1));

                TimeControlNodes timeControl = new TimeControlNodes() { InitialAmount = nodesPerMove * 20, MovesPerControl = 1, BonusAmount = nodesPerMove };
                
                //create list of starting positions
                int gamesPerMatch = 100;
                var startingPGNsForThisMatch = StartingPGNs.OrderBy(x => rand.Next()).Take(gamesPerMatch / 2).ToList();

                //create test param values;
                double deltaPct = 0.2f;
                double valHigh = parameterValue * (1f + deltaPct);
                double valLow = parameterValue * (1f - deltaPct);
                double delta = parameterValue - valLow;

                Func<DeterministicPlayer> fnHighPlayer = () => fnCreatePlayer(valHigh,valLow, paramName);
                Func<DeterministicPlayer> fnLowPlayer = () => fnCreatePlayer(valLow, valHigh, paramName);

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
                
                int matchWins = 0;
                int matchLosses = 0;
                int matchDraws = 0;

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
                            case GameResult.Draw:
                                Console.Write("-");
                                break;
                            case GameResult.WhiteWins:
                                Console.Write(p.Game.White == highPlayerName ? @"/" : @"\");
                                break;
                            case GameResult.BlackWins:
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
                    matchResults.ResultsForPlayer(compName, out matchWins, out matchLosses, out matchDraws);
                    float matchGames = (matchWins + matchLosses + matchDraws);
                    Console.WriteLine("Player:{4}, WinPct:{0} LossPct:{1} DrawPct:{2} GameCount:{3}",
                        ((float)matchWins / (float)matchGames).ToString("#0.##%"),
                        ((float)matchLosses / (float)matchGames).ToString("#0.##%"),
                        ((float)matchDraws / (float)matchGames).ToString("#0.##%"),
                        matchGames,
                        compName);
                }


                matchResults.ResultsForPlayer(highPlayerName, out matchWins, out matchLosses, out matchDraws);
                //var paramDelta = delta * 0.002f;
                var paramDelta = delta * 0.003f;
                var newParamValue = parameterValue + ((matchWins - matchLosses) * paramDelta);

                //report to param value.
                Console.WriteLine(string.Format("{0}\tFrom\t{1:f4}\tTo\t{2:f4}\tin\t{3}", paramName, parameterValue, newParamValue, timeSpent));
                

                totalWins += matchWins;
                totalLosses += matchLosses;
                totalDraws += matchDraws;
                int totalGames = totalWins + totalLosses + totalDraws;

                //report total results for 'high player'
                Console.WriteLine("HighPlayer WinPct:{0} LossPct:{1} DrawPct:{2} GameCount:{3}",
                        ((float)totalWins / (float)totalGames).ToString("#0.##%"),
                        ((float)totalLosses / (float)totalGames).ToString("#0.##%"),
                        ((float)totalDraws / (float)totalGames).ToString("#0.##%"),
                        totalGames);

                string newParamValueString = string.Format("{0:f8}\t{1}\t{2}\t{3}", newParamValue, totalWins, totalLosses, totalDraws);
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

        public static Settings ChampionSettings
        {
            get
            {
                lock (_lock)
                {
                    if (System.IO.File.Exists("ChampionSettings.xml"))
                    {
                        return Settings.Load(System.IO.File.OpenRead("ChampionSettings.xml"));
                    }
                    else
                    {
                        return Settings.Default();
                    }
                }
            }
            set
            {
                string xml = Settings.SerializeObject<Settings>(value);
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
