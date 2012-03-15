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
	public class MovegenTest
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

		#region PerftCalls
		public void PerftTest(string fen, int depth, int expectedLeaves, int expectedNodes)
		{

			ChessBoard board = new ChessBoard(fen);

			string fenStart = board.FEN.ToString();

			int moves_done = 0;
			int nodecount = -1;//start at -1 to skip root node
			int leafnodecount = 0;

			PerftSearch(board, depth, ref moves_done, ref nodecount, ref leafnodecount);

			string fenEnd = board.FEN.ToString();

			//Assert.AreEqual<string>(fenStart, fenEnd, string.Format("Start and End FEN do not match {0}  {1}", fenStart, fenEnd));
			Assert.AreEqual<int>(expectedLeaves, leafnodecount,string.Format("ExpectedLeaves D:{0} FEN:{1}",depth,fen));
			Assert.AreEqual<int>(expectedNodes, nodecount, string.Format("ExpectedNodes D:{0} FEN:{1}", depth, fen));
			
		}

		public void PerftSearch(ChessBoard board, int depth_remaining, ref int moves_done, ref int nodecount, ref int leafnodecount)
		{
			nodecount++;

			if (depth_remaining <= 0)
			{
				leafnodecount++;
				return;
			}

			List<ChessMove> moves = ChessMove.GenMoves(board, false);

			foreach (ChessMove move in moves)
			{
				board.MoveApply(move);

				if (!board.IsCheck(Chess.PlayerOther(board.WhosTurn)))
				{
					PerftSearch(board, depth_remaining - 1, ref moves_done, ref nodecount, ref leafnodecount);
				}

				board.MoveUndo();
				moves_done++;
			}
		}
		#endregion


		[TestMethod]
		public void PositionAttacks()
		{

			int iCount = 0;
            var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Sinobyl.Engine.Tests.pgnFiles.gm2600.pgn");
            var x = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames();
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

					foreach (ChessPosition pos in Chess.AllPositions)
					{
						List<ChessPosition> whiteattacks = board.AttacksTo(pos,ChessPlayer.White);
						List<ChessPosition> blackattacks = board.AttacksTo(pos, ChessPlayer.Black);
						bool whiteDoes = board.PositionAttacked(pos, ChessPlayer.White);
						bool blackDoes = board.PositionAttacked(pos, ChessPlayer.Black);

						Assert.AreEqual<bool>(whiteattacks.Count > 0, whiteDoes);
						Assert.AreEqual<bool>(blackattacks.Count > 0, blackDoes);

					}
				}

				if (iCount > 50) { break; }

			}


		}

		[TestMethod]
		public void MoveGenCaps()
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

                    var allmoves = ChessMove.GenMoves(board).OrderBy(m => m.From).ThenBy(m => m.To).ToArray();
                    var allmovesBB = ChessMove.GenMovesBitboards(board, false).OrderBy(m => m.From).ThenBy(m => m.To).ToArray();

					List<ChessMove> capmoves = ChessMove.GenMoves(board,true);
                    var capmovesBB = ChessMove.GenMovesBitboards(board, true);

                    //not in allmoves
                    //var notInAllMoves = allmoves.Where(m => !allmovesBB.Any(bbm => bbm.From == m.From && bbm.To == m.To && bbm.Promote == m.Promote)).ToArray();
                   // var notInBBMoves = allmovesBB.Where(m => !allmoves.Any(bbm => bbm.From == m.From && bbm.To == m.To && bbm.Promote == m.Promote)).ToArray();

                    Assert.AreEqual<int>(allmoves.Count(), allmovesBB.Count());
                    Assert.AreEqual<int>(capmoves.Count(), capmovesBB.Count());


					//make sure every cap more really is a cap
					foreach (ChessMove capmove in capmoves)
					{
						if (board.PieceAt(capmove.To) == ChessPiece.EMPTY)
						{
							if(board.EnPassant == capmove.To && (board.PieceAt(capmove.From)==ChessPiece.WPawn || board.PieceAt(capmove.From)==ChessPiece.BPawn))
							{
								continue;
							}
							Assert.Fail(string.Format("{0} is not a cap and found in capmoves from position {1}", capmove.ToString(), board.FEN.ToString()));
						}
					}
					//for all moves that are caps, make sure it's in capmove list
					foreach (ChessMove allmove in allmoves)
					{
						if (board.PieceAt(allmove.To) != ChessPiece.EMPTY)
						{
							bool foundCap = false;
							foreach (ChessMove capmove in capmoves)
							{
								if (capmove.ToString() == allmove.ToString())
								{
									foundCap = true;
									break;
								}
							}
							if (!foundCap)
							{
								Assert.Fail(string.Format("{0} not found in capmoves from position {1}",allmove.ToString(),board.FEN.ToString()));
							}
							
						}
					}

				}

				if (iCount > 100) { break; }

			}


		}

		[TestMethod]
		public void MoveGenPerft1()
		{

			string fen = Chess.FENStart;

			PerftTest(fen, 1, 20, 20);
			PerftTest(fen, 2, 400, 420);
			PerftTest(fen, 3, 8902, 9322);
			PerftTest(fen, 4, 197281, 206603);
			//PerftTest(fen, 5, 4865609, 5072212);
//			PerftTest(fen, 6, 119060324, 124132536);
		}

		[TestMethod]
		public void MoveGenPerft2()
		{

			string fen = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1";

			PerftTest(fen, 1, 48, 48);
			PerftTest(fen, 2, 2039, 2087);
			PerftTest(fen, 3, 97862, 99949);
			//PerftTest(fen, 4, 4085603, 4185552);
			//PerftTest(fen, 5, 193690690, 197876242);

		}

		[TestMethod]
		public void MoveGenPerft3()
		{

			string fen = "8/PPP4k/8/8/8/8/4Kppp/8 w - - 0 1";

			PerftTest(fen, 1, 18, 18);
			PerftTest(fen, 2, 290, 308);
			PerftTest(fen, 3, 5044, 5352);
			PerftTest(fen, 4, 89363, 94715);
			//PerftTest(fen, 5, 1745545, 1840260);
			//PerftTest(fen, 6, 34336777, 36177037);


		}

		[TestMethod]
		public void MoveGenPerft4()
		{

			string fen = "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1";

			PerftTest(fen, 1, 14, 14);
			PerftTest(fen, 2, 191, 205);
			PerftTest(fen, 3, 2812, 3017);
			PerftTest(fen, 4, 43238, 46255);
			//PerftTest(fen, 5, 674624, 720879);

		}

	}
}
