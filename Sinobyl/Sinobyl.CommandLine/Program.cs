using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;

namespace Sinobyl.CommandLine
{
	public class Program
	{
		
		private static readonly BackgroundWorker bwReadInput = new BackgroundWorker();
		private static Winboard winboard = new Winboard();


        protected static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(Program));
        protected static readonly log4net.ILog _logInput = log4net.LogManager.GetLogger(typeof(Program).FullName + ".IO.Input");
        protected static readonly log4net.ILog _logOutput = log4net.LogManager.GetLogger(typeof(Program).FullName + ".IO.Output");
        
        

		static bool KeepGoing = true;
		static void Main(string[] args)
		{

            Console.WriteLine("Sinobyl");
			
			foreach (string arg in args)
			{
                if (_log.IsInfoEnabled) { _log.InfoFormat("ARGUMENT - {0}", arg); }
				//LogInfo("ARGUMENT", arg);
			}

            if (_log.IsInfoEnabled) { _log.InfoFormat("ENGSTRENGTH - {0}", Program.EngineStrength); }

			winboard.EnginePersonality = Sinobyl.Engine.ChessGamePlayerPersonality.FromStrength(Program.EngineStrength);
			

			bwReadInput.DoWork += new DoWorkEventHandler(bwReadInput_DoWork);
			bwReadInput.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bwReadInput_RunWorkerCompleted);
			bwReadInput.WorkerReportsProgress = true;
			bwReadInput.RunWorkerAsync();

			while (KeepGoing)
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
                KeepGoing = false;
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
			KeepGoing = false;
		}

		static void bwReadInput_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				while (true)
				{
					
					string input = Console.ReadLine();
					string[] split = input.Split(' ');
                    if (_logInput.IsInfoEnabled) { _logInput.Info(input); }
					if (input.ToLower() == "quit")
					{
						break;
					}
					else if (split[0] == "logtest")
					{
						logtest(split[1]);
					}
                    else if (split[0] == "perft")
                    {
                        Perft.PerftSuite(int.Parse(split[1]), false, false);
                    }
                    else if (split[0] == "evalperft")
                    {
                        Perft.PerftSuite(int.Parse(split[1]),true, false);
                    }
                    else if (split[0] == "evalsortperft")
                    {
                        Perft.PerftSuite(int.Parse(split[1]), true, true);
                    }
                    else if(split[0] == "nodestodepth")
                    {
                        Perft.NodesToDepth(int.Parse(split[1]));
                    }
                    else
                    {
                        winboard.ProcessCmd(input);
                        //bwReadInput.ReportProgress(0, input);
                    }
				}
			}
			catch (Exception ex)
			{
				LogException(ex);
			}
			
		}
		public static void ConsoleWriteline(string output)
		{
            if (_logOutput.IsInfoEnabled) { _logOutput.Info(output); }
            //LogInfo("OUT", output);
            Console.WriteLine(output);
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
							winboard.ProcessCmd(fullMsg); 
						}
						if (fullMsgCmd == "usermove") 
						{ 
							winboard.SimulateUsermove(fullMsg.Split(' ')[1]); 
						}
					}
					if (logType == "out")
					{
						if (fullMsgCmd == "move")
						{
							winboard.ProcessCmd("usermove "+fullMsg.Split(' ')[1]);
						}

					}
					winboard.GameDoneAnnounce();


				}
				Program.ConsoleWriteline("done processing logfile");

			}

		}

	}
}
