using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Sinobyl.CommandLine
{
	public class Program
	{
		
		private static readonly BackgroundWorker _bwReadInput = new BackgroundWorker();
		private static readonly Winboard _winboard = new Winboard();


        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(Program));
        private static readonly log4net.ILog _logInput = log4net.LogManager.GetLogger(typeof(Program).FullName + ".IO.Input");
        private static readonly log4net.ILog _logOutput = log4net.LogManager.GetLogger(typeof(Program).FullName + ".IO.Output");
        
		private static bool _keepGoing = true;
		
        static void Main(string[] args)
		{
            
            Console.WriteLine("Sinobyl");
			
			foreach (string arg in args)
			{
                if (_log.IsInfoEnabled) { _log.InfoFormat("ARGUMENT - {0}", arg); }
				//LogInfo("ARGUMENT", arg);
			}

            if (_log.IsInfoEnabled) { _log.InfoFormat("ENGSTRENGTH - {0}", Program.EngineStrength); }

			_winboard.EnginePersonality = Sinobyl.Engine.ChessGamePlayerPersonality.FromStrength(Program.EngineStrength);
			

			_bwReadInput.DoWork += new DoWorkEventHandler(bwReadInput_DoWork);
			_bwReadInput.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bwReadInput_RunWorkerCompleted);
			_bwReadInput.WorkerReportsProgress = true;
			_bwReadInput.RunWorkerAsync();

            
			while (_keepGoing)
			{
				System.Threading.Thread.Sleep(500);
			}
		}



        public static void LogException(Exception ex)
        {
            try
            {
                _log.Fatal("FATAL EXCEPTION");
                while (ex != null)
                {
                    _log.FatalFormat("EXCEPTION:{0}\tSOURCE:{1}\n", ex.Message, ex.Source);
                    foreach (string st in ex.StackTrace.Split('\n'))
                    {
                        _log.Fatal(st);
                    }

                    ex = ex.InnerException;
                    if (ex != null) { _log.Fatal("INNER EXCEPTION"); }
                }
            }
            finally
            {
                _keepGoing = false;
            }
        }

		public static float EngineStrength
		{
			get
			{
                foreach (string arg in Environment.GetCommandLineArgs())
				{
					string[] splits = arg.Split('=');
					if (splits.GetUpperBound(0) >= 1)
					{
						if (splits[0].ToLower() == "st")
						{
							float val;
							if(float.TryParse(splits[1],out val))
							{
								return val / 100;
							}
						}
					}
				}
                
				return 1;
			}
		}

		static void bwReadInput_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			_keepGoing = false;
		}

		static void bwReadInput_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				while (true)
				{
					
					string input = Console.ReadLine();
                    if (_logInput.IsInfoEnabled) { _logInput.Info(input); }
                    bool shouldContinue = ProcessInput(input);
                    if (!shouldContinue)
                    {
                        break;
                    }
				}
			}
			catch (Exception ex)
			{
				LogException(ex);
			}
			
		}

        public static bool ProcessInput(string input)
        {
            string[] split = input.Split(' ');
            string primaryCommand = split[0].ToLowerInvariant();
            switch (primaryCommand)
            {
                case "quit":
                    return false;
                case "logtest":
                    logtest(split[1]);
                    break;
                case "perft":
                    Perft.PerftSuite(int.Parse(split[1]), false, false);
                    break;
                case "evalperft":
                    Perft.PerftSuite(int.Parse(split[1]), true, false);
                    break;
                case "evalsortperft":
                    Perft.PerftSuite(int.Parse(split[1]), true, true);
                    break;
                case "nodestodepth":
                    Perft.NodesToDepth(int.Parse(split[1]));
                    PrintSearchCutoffStats();
                    break;
                case "annotateeval":
                    Perft.AnnotatePGNEval(split[1], split[2]);
                    break;
                case "counts":
                    ConsoleWriteline(string.Format(" nodes:{0,10}\n evals:{1,10}\n pawns:{2,10}\n mater:{3,10}",
                        Sinobyl.Engine.ChessSearch.CountTotalAINodes,
                        Sinobyl.Engine.ChessEval.TotalEvalCount, 
                        Sinobyl.Engine.ChessEvalPawns.TotalEvalPawnCount,
                        Sinobyl.Engine.ChessEvalMaterial.TotalEvalMaterialCount));
                    break;
                default:
                    _winboard.ProcessCmd(input);
                    break;
            }
            return true;

        }

        
		public static void ConsoleWriteline(string output)
		{
            if (_logOutput.IsInfoEnabled) { _logOutput.Info(output); }
            //LogInfo("OUT", output);
            Console.WriteLine(output);
		}

        public static void PrintSearchCutoffStats()
        {
            for (int depth = 1; depth < 50; depth++)
            {
                var stats = Sinobyl.Engine.CutoffStats.AtDepth[depth];
                if (stats.TotalNodes == 0 && depth > 0) { break; }
                //ConsoleWriteline(string.Format("d:{0,9} t:{1,6} alpha:{2,9} pv:{3,9} beta:{4,9}", depth, stats.TotalNodes, stats.FailLows, stats.PVNodes, stats.TotalCutoffs));
                ConsoleWriteline(string.Format("d:{0,-9} a:{1:F2} pv:{2:F2} b:{3:F2} 1st:{4:F5} avg:{5:F5}", 
                    depth, 
                    stats.FailLowPct,
                    stats.PVPct,
                    stats.CutoffPct, 
                    stats.CutoffPctFirst,
                    stats.CutoffAvg));
            }
        }

		private static void logtest(string logFile)
		{
			using (System.IO.StreamReader reader = new System.IO.StreamReader(logFile))
			{
				
				while (!reader.EndOfStream)
				{
					string line = reader.ReadLine();
					string logType = line.Split('\t')[0];
					string fullMsg = line.Split('\t')[1];
					string fullMsgCmd = fullMsg.Split(' ')[0];


					if (logType == "in")
					{
						if (fullMsgCmd == "new") 
						{ 
							_winboard.ProcessCmd(fullMsg); 
						}
						if (fullMsgCmd == "usermove") 
						{ 
							_winboard.SimulateUsermove(fullMsg.Split(' ')[1]); 
						}
					}
					if (logType == "out")
					{
						if (fullMsgCmd == "move")
						{
							_winboard.ProcessCmd("usermove "+fullMsg.Split(' ')[1]);
						}

					}
					_winboard.GameDoneAnnounce();


				}
				Program.ConsoleWriteline("done processing logfile");

			}

		}

	}
}
