using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sinobyl.Engine;
using System.IO;

namespace Sinobyl.Engine.Tests
{
	[TestClass]
	public class EvalTest
	{

		#region Common test methods
		private TestContext testContextInstance;
		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}


		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }
		//
		// Use TestInitialize to run code before running each test 
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		[TestMethod]
		public void PawnTest1()
		{
			ChessBoard board = new ChessBoard("7k/7p/2p5/2p1Pp2/3pP3/1P4P1/7P/5BK1 w - - 0 1 ");
			List<ChessPosition> passed = new List<ChessPosition>();
			List<ChessPosition> doubled = new List<ChessPosition>();
			List<ChessPosition> isolated = new List<ChessPosition>();

			int StartVal = 0;
			int EndVal = 0;

			ChessEval eval = new ChessEval();

			ChessEval.PawnInfo.EvalAllPawns(board.PieceList(ChessPiece.WPawn), board.PieceList(ChessPiece.BPawn), eval, ref StartVal, ref EndVal, passed, doubled, isolated);

			Assert.AreEqual<int>(2, passed.Count);
			Assert.AreEqual<int>(4, doubled.Count);
			Assert.AreEqual<int>(5, isolated.Count);


		}


		[TestMethod]
		public void EvalTest1()
		{
			int iCount = 0;
            var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Sinobyl.Engine.Tests.pgnFiles.gm2600.pgn");
			StreamReader reader = new StreamReader(stream);
			ChessEval eval = new ChessEval();

			while (!reader.EndOfStream)
			{
				iCount++;
				ChessPGN pgn = ChessPGN.NextGame(reader);
				if (pgn == null) { break; }

				ChessBoard board = new ChessBoard();

				foreach (ChessMove move in pgn.Moves)
				{
					board.MoveApply(move);

					ChessBoard boardRev = new ChessBoard(board.FEN.Reverse());

					int e1 = eval.Eval(board);
					int e2 = eval.Eval(boardRev);


					Assert.AreEqual<int>(e1, -e2);
					

				}

				if (iCount > 100) { break; }

			}
			

		}

		[TestMethod]
		public void SearchTest()
		{
			return;
			int iCount = 0;
            var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Sinobyl.Engine.Tests.pgnFiles.gm2600.pgn");
            StreamReader reader = new StreamReader(stream);
			ChessEval eval = new ChessEval();

			while (!reader.EndOfStream)
			{
				iCount++;
				ChessPGN pgn = ChessPGN.NextGame(reader);
				if (pgn == null) { break; }

				ChessBoard board = new ChessBoard();

				int MovesDone = 0;
				int TotalMoves = pgn.Moves.Count;

				foreach (ChessMove move in pgn.Moves)
				{
					board.MoveApply(move);

					ChessSearch.Args args = new ChessSearch.Args();
					args.GameStartPosition = board.FEN;
					args.MaxDepth = 4;

					ChessSearch search = new ChessSearch(args);
					MovesDone++;
					search.Search();

					if (MovesDone >= 20) { break; }

				}
				Assert.AreEqual<int>(MovesDone, TotalMoves);
				//only do one game
				break;

			}


		}
	}
}
