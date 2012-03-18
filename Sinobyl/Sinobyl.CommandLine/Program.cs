﻿using System;
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
		private static StreamWriter logger;
		public static string[] commandArgs;

		static bool KeepGoing = true;
		static void Main(string[] args)
		{
			commandArgs = args;

			Console.WriteLine("Murderhole II");
			string startDateTime = DateTime.Now.ToString("yyyyMMddHHmmss");
			logger = File.AppendText(string.Format("MurderholeII.{0}.log", startDateTime));

			foreach (string arg in args)
			{
				LogInfo("ARGUMENT", arg);
			}
			LogInfo("ENGSTRENGTH", Program.EngineStrength.ToString());
			winboard.EnginePersonality = Sinobyl.Engine.ChessGamePlayerPersonality.FromStrength(Program.EngineStrength);
			

			bwReadInput.DoWork += new DoWorkEventHandler(bwReadInput_DoWork);
			bwReadInput.ProgressChanged += new ProgressChangedEventHandler(bwReadInput_ProgressChanged);
			bwReadInput.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bwReadInput_RunWorkerCompleted);
			bwReadInput.WorkerReportsProgress = true;
			bwReadInput.RunWorkerAsync();

			while (KeepGoing)
			{
				System.Threading.Thread.Sleep(500);
			}
			logger.Close();
		}

		public static float EngineStrength
		{
			get
			{
				foreach (string arg in commandArgs)
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

		static void bwReadInput_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			//try
			//{
				winboard.ProcessCmd(e.UserState.ToString());
			//}
			//catch (Exception ex)
			//{
				//LogException(ex);
			//}
		}

		static void bwReadInput_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				while (true)
				{
					
					string input = Console.ReadLine();
					string[] split = input.Split(' ');

					logger.WriteLine("in\t" + input);
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
                        Perft.PerftSuite(int.Parse(split[1]));
                    }
                    else
                    {
                        bwReadInput.ReportProgress(0, input);
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
			logger.WriteLine("out\t" + output);
			logger.Flush();
			Console.WriteLine(output);
		}
		public static void LogInfo(string type, string message)
		{
			logger.WriteLine(type+"\t"+message);
			logger.Flush();
		}
		public static void LogException(Exception ex)
		{
			logger.WriteLine("Exception\t" + ex.Message + "---Source:" + ex.Source);
			while (ex.InnerException != null)
			{
				ex = ex.InnerException;
				logger.WriteLine("Exception\t" + ex.Message + "---Source:" + ex.Source);
			}
			logger.Flush();
			KeepGoing = false;
		}

        private static void movegenperf(int depth)
        {

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