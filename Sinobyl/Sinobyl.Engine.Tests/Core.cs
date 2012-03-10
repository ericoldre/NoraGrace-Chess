using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sinobyl.Engine;

namespace Sinobyl.Engine.Tests
{
	[TestClass]
	public class Core
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
		public void SerializationTests()
		{
			ChessGamePlayerPersonality pers = new ChessGamePlayerPersonality();
			pers.Name = "qwer";
			pers.MaxDepth = 64;
			string s = Chess.SerializeObject<ChessGamePlayerPersonality>(pers);
			ChessGamePlayerPersonality pers2 = Chess.DeserializeObject<ChessGamePlayerPersonality>(s);
			Assert.AreEqual<string>(pers.Name, pers2.Name);
			Assert.AreEqual<int>(pers.MaxDepth, pers2.MaxDepth);

			ChessTimeControl tc = ChessTimeControl.Blitz(6, 8);
			s = Chess.SerializeObject<ChessTimeControl>(tc);
			ChessTimeControl tc2 = Chess.DeserializeObject<ChessTimeControl>(s);
			Assert.AreEqual<TimeSpan>(tc.InitialTime, tc2.InitialTime);
			Assert.AreEqual<TimeSpan>(tc.BonusAmount, tc2.BonusAmount);
			Assert.AreEqual<int>(tc.BonusEveryXMoves, tc2.BonusEveryXMoves);


		}
		[TestMethod]
		public void LastTwoNull()
		{
			ChessBoard board = new ChessBoard();
			Assert.IsTrue(board.MovesSinceNull > 0);
			board.MoveNullApply();
			Assert.IsTrue(board.MovesSinceNull == 0);
			Assert.IsFalse(board.LastTwoMovesNull());
			board.MoveNullApply();

			Assert.IsTrue(board.LastTwoMovesNull());

			board.MoveNullUndo();
			Assert.IsTrue(board.MovesSinceNull == 0);
			Assert.IsFalse(board.LastTwoMovesNull());

			board.MoveNullUndo();
			Assert.IsTrue(board.MovesSinceNull > 0);
			Assert.IsFalse(board.LastTwoMovesNull());
		}

		[TestMethod]
		public void DirectionFromToTest()
		{
			Chess.DirectionFromTo(ChessPosition.B8, ChessPosition.A8);

			ChessPosition posCurr;
			foreach (ChessPosition posFrom in Chess.AllPositions)
			{
				foreach (ChessPosition posTo in Chess.AllPositions)
				{
					if(posFrom==posTo){continue;}
					ChessDirection dirFromTo = Chess.DirectionFromTo(posFrom, posTo);
					if (dirFromTo == 0) 
					{
						foreach(ChessDirection dirTry in Chess.AllDirectionsQueen)
						{
							posCurr = posFrom;
							while(Chess.InBounds(posCurr))
							{
								Assert.IsFalse(posCurr == posTo);
								posCurr = Chess.PositionInDirection(posCurr,dirTry);
							}
						}
						continue; 
					}
					if (Chess.IsDirectionKnight(dirFromTo))
					{
						Assert.AreEqual<ChessPosition>(posTo, (ChessPosition)((int)posFrom + (int)dirFromTo));
					}
					bool FoundTo = false;
					posCurr = posFrom;
					while (Chess.InBounds(posCurr))
					{
						posCurr = Chess.PositionInDirection(posCurr, dirFromTo);
						if (posCurr == posTo) { FoundTo = true; }
					}
					if (!FoundTo)
					{
						Assert.IsTrue(FoundTo);
					}
					
				}
			}

		}
		[TestMethod]
		public void CharacterTests()
		{
			Assert.AreEqual<string>("1", Chess.RankToString(ChessRank.Rank1));
			Assert.AreEqual<string>("2", Chess.RankToString(ChessRank.Rank2));
			Assert.AreEqual<string>("3", Chess.RankToString(ChessRank.Rank3));
			Assert.AreEqual<string>("4", Chess.RankToString(ChessRank.Rank4));
			Assert.AreEqual<string>("5", Chess.RankToString(ChessRank.Rank5));
			Assert.AreEqual<string>("6", Chess.RankToString(ChessRank.Rank6));
			Assert.AreEqual<string>("7", Chess.RankToString(ChessRank.Rank7));
			Assert.AreEqual<string>("8", Chess.RankToString(ChessRank.Rank8));

			Assert.AreEqual<string>("a", Chess.FileToString(ChessFile.FileA));
			Assert.AreEqual<string>("b", Chess.FileToString(ChessFile.FileB));
			Assert.AreEqual<string>("c", Chess.FileToString(ChessFile.FileC));
			Assert.AreEqual<string>("d", Chess.FileToString(ChessFile.FileD));
			Assert.AreEqual<string>("e", Chess.FileToString(ChessFile.FileE));
			Assert.AreEqual<string>("f", Chess.FileToString(ChessFile.FileF));
			Assert.AreEqual<string>("g", Chess.FileToString(ChessFile.FileG));
			Assert.AreEqual<string>("h", Chess.FileToString(ChessFile.FileH));

			Assert.AreEqual<ChessRank>(ChessRank.Rank1, Chess.CharToRank('1'));
			Assert.AreEqual<ChessRank>(ChessRank.Rank2, Chess.CharToRank('2'));
			Assert.AreEqual<ChessRank>(ChessRank.Rank3, Chess.CharToRank('3'));
			Assert.AreEqual<ChessRank>(ChessRank.Rank4, Chess.CharToRank('4'));
			Assert.AreEqual<ChessRank>(ChessRank.Rank5, Chess.CharToRank('5'));
			Assert.AreEqual<ChessRank>(ChessRank.Rank6, Chess.CharToRank('6'));
			Assert.AreEqual<ChessRank>(ChessRank.Rank7, Chess.CharToRank('7'));
			Assert.AreEqual<ChessRank>(ChessRank.Rank8, Chess.CharToRank('8'));

			Assert.AreEqual<ChessFile>(ChessFile.FileA, Chess.CharToFile('a'));
			Assert.AreEqual<ChessFile>(ChessFile.FileB, Chess.CharToFile('b'));
			Assert.AreEqual<ChessFile>(ChessFile.FileC, Chess.CharToFile('c'));
			Assert.AreEqual<ChessFile>(ChessFile.FileD, Chess.CharToFile('d'));
			Assert.AreEqual<ChessFile>(ChessFile.FileE, Chess.CharToFile('e'));
			Assert.AreEqual<ChessFile>(ChessFile.FileF, Chess.CharToFile('f'));
			Assert.AreEqual<ChessFile>(ChessFile.FileG, Chess.CharToFile('g'));
			Assert.AreEqual<ChessFile>(ChessFile.FileH, Chess.CharToFile('h'));

		}

		[TestMethod]
		public void DirectionTests()
		{
			Assert.IsFalse(Chess.InBounds(Chess.PositionInDirection(ChessPosition.A1, ChessDirection.DirW)));
			Assert.IsFalse(Chess.InBounds(Chess.PositionInDirection(ChessPosition.A1, ChessDirection.DirS)));
			Assert.IsFalse(Chess.InBounds(Chess.PositionInDirection(ChessPosition.A8, ChessDirection.DirW)));
			Assert.IsFalse(Chess.InBounds(Chess.PositionInDirection(ChessPosition.A8, ChessDirection.DirN)));
			Assert.IsFalse(Chess.InBounds(Chess.PositionInDirection(ChessPosition.H1, ChessDirection.DirE)));
			Assert.IsFalse(Chess.InBounds(Chess.PositionInDirection(ChessPosition.H1, ChessDirection.DirS)));
			Assert.IsFalse(Chess.InBounds(Chess.PositionInDirection(ChessPosition.H8, ChessDirection.DirE)));
			Assert.IsFalse(Chess.InBounds(Chess.PositionInDirection(ChessPosition.H8, ChessDirection.DirN)));


			Assert.AreEqual<ChessPosition>(ChessPosition.D5, Chess.PositionInDirection(ChessPosition.D4, ChessDirection.DirN));
			Assert.AreEqual<ChessPosition>(ChessPosition.D3, Chess.PositionInDirection(ChessPosition.D4, ChessDirection.DirS));
			Assert.AreEqual<ChessPosition>(ChessPosition.C4, Chess.PositionInDirection(ChessPosition.D4, ChessDirection.DirW));
			Assert.AreEqual<ChessPosition>(ChessPosition.E4, Chess.PositionInDirection(ChessPosition.D4, ChessDirection.DirE));

			Assert.AreEqual<ChessPosition>(ChessPosition.E5, Chess.PositionInDirection(ChessPosition.D4, ChessDirection.DirNE));
			Assert.AreEqual<ChessPosition>(ChessPosition.E3, Chess.PositionInDirection(ChessPosition.D4, ChessDirection.DirSE));
			Assert.AreEqual<ChessPosition>(ChessPosition.C3, Chess.PositionInDirection(ChessPosition.D4, ChessDirection.DirSW));
			Assert.AreEqual<ChessPosition>(ChessPosition.C5, Chess.PositionInDirection(ChessPosition.D4, ChessDirection.DirNW));

			Assert.AreEqual<ChessPosition>(ChessPosition.F7, Chess.PositionInDirection(ChessPosition.E5, ChessDirection.DirNNE));
			Assert.AreEqual<ChessPosition>(ChessPosition.G6, Chess.PositionInDirection(ChessPosition.E5, ChessDirection.DirEEN));
			Assert.AreEqual<ChessPosition>(ChessPosition.G4, Chess.PositionInDirection(ChessPosition.E5, ChessDirection.DirEES));
			Assert.AreEqual<ChessPosition>(ChessPosition.F3, Chess.PositionInDirection(ChessPosition.E5, ChessDirection.DirSSE));
			Assert.AreEqual<ChessPosition>(ChessPosition.D3, Chess.PositionInDirection(ChessPosition.E5, ChessDirection.DirSSW));
			Assert.AreEqual<ChessPosition>(ChessPosition.C4, Chess.PositionInDirection(ChessPosition.E5, ChessDirection.DirWWS));
			Assert.AreEqual<ChessPosition>(ChessPosition.C6, Chess.PositionInDirection(ChessPosition.E5, ChessDirection.DirWWN));
			Assert.AreEqual<ChessPosition>(ChessPosition.D7, Chess.PositionInDirection(ChessPosition.E5, ChessDirection.DirNNW));

		}
		[TestMethod]
		public void FileAndRankTests()
		{


			Assert.AreEqual<ChessRank>(ChessRank.Rank1, Chess.PositionToRank(ChessPosition.A1));
			Assert.AreEqual<ChessRank>(ChessRank.Rank2, Chess.PositionToRank(ChessPosition.A2));
			Assert.AreEqual<ChessRank>(ChessRank.Rank3, Chess.PositionToRank(ChessPosition.A3));
			Assert.AreEqual<ChessRank>(ChessRank.Rank4, Chess.PositionToRank(ChessPosition.A4));
			Assert.AreEqual<ChessRank>(ChessRank.Rank5, Chess.PositionToRank(ChessPosition.A5));
			Assert.AreEqual<ChessRank>(ChessRank.Rank6, Chess.PositionToRank(ChessPosition.A6));
			Assert.AreEqual<ChessRank>(ChessRank.Rank7, Chess.PositionToRank(ChessPosition.A7));
			Assert.AreEqual<ChessRank>(ChessRank.Rank8, Chess.PositionToRank(ChessPosition.A8));

			Assert.AreEqual<ChessRank>(ChessRank.Rank1, Chess.PositionToRank(ChessPosition.H1));
			Assert.AreEqual<ChessRank>(ChessRank.Rank2, Chess.PositionToRank(ChessPosition.H2));
			Assert.AreEqual<ChessRank>(ChessRank.Rank3, Chess.PositionToRank(ChessPosition.H3));
			Assert.AreEqual<ChessRank>(ChessRank.Rank4, Chess.PositionToRank(ChessPosition.H4));
			Assert.AreEqual<ChessRank>(ChessRank.Rank5, Chess.PositionToRank(ChessPosition.H5));
			Assert.AreEqual<ChessRank>(ChessRank.Rank6, Chess.PositionToRank(ChessPosition.H6));
			Assert.AreEqual<ChessRank>(ChessRank.Rank7, Chess.PositionToRank(ChessPosition.H7));
			Assert.AreEqual<ChessRank>(ChessRank.Rank8, Chess.PositionToRank(ChessPosition.H8));


			Assert.AreEqual<ChessFile>(ChessFile.FileA, Chess.PositionToFile(ChessPosition.A1));
			Assert.AreEqual<ChessFile>(ChessFile.FileB, Chess.PositionToFile(ChessPosition.B1));
			Assert.AreEqual<ChessFile>(ChessFile.FileC, Chess.PositionToFile(ChessPosition.C1));
			Assert.AreEqual<ChessFile>(ChessFile.FileD, Chess.PositionToFile(ChessPosition.D1));
			Assert.AreEqual<ChessFile>(ChessFile.FileE, Chess.PositionToFile(ChessPosition.E1));
			Assert.AreEqual<ChessFile>(ChessFile.FileF, Chess.PositionToFile(ChessPosition.F1));
			Assert.AreEqual<ChessFile>(ChessFile.FileG, Chess.PositionToFile(ChessPosition.G1));
			Assert.AreEqual<ChessFile>(ChessFile.FileH, Chess.PositionToFile(ChessPosition.H1));

			Assert.AreEqual<ChessFile>(ChessFile.FileA, Chess.PositionToFile(ChessPosition.A8));
			Assert.AreEqual<ChessFile>(ChessFile.FileB, Chess.PositionToFile(ChessPosition.B7));
			Assert.AreEqual<ChessFile>(ChessFile.FileC, Chess.PositionToFile(ChessPosition.C6));
			Assert.AreEqual<ChessFile>(ChessFile.FileD, Chess.PositionToFile(ChessPosition.D5));
			Assert.AreEqual<ChessFile>(ChessFile.FileE, Chess.PositionToFile(ChessPosition.E4));
			Assert.AreEqual<ChessFile>(ChessFile.FileF, Chess.PositionToFile(ChessPosition.F3));
			Assert.AreEqual<ChessFile>(ChessFile.FileG, Chess.PositionToFile(ChessPosition.G2));
			Assert.AreEqual<ChessFile>(ChessFile.FileH, Chess.PositionToFile(ChessPosition.H8));

			//FileRankToPos

			Assert.AreEqual<ChessPosition>(ChessPosition.A8, Chess.FileRankToPos(ChessFile.FileA, ChessRank.Rank8));
			Assert.AreEqual<ChessPosition>(ChessPosition.B7, Chess.FileRankToPos(ChessFile.FileB, ChessRank.Rank7));
			Assert.AreEqual<ChessPosition>(ChessPosition.C6, Chess.FileRankToPos(ChessFile.FileC, ChessRank.Rank6));
			Assert.AreEqual<ChessPosition>(ChessPosition.D5, Chess.FileRankToPos(ChessFile.FileD, ChessRank.Rank5));
			Assert.AreEqual<ChessPosition>(ChessPosition.E4, Chess.FileRankToPos(ChessFile.FileE, ChessRank.Rank4));
			Assert.AreEqual<ChessPosition>(ChessPosition.F3, Chess.FileRankToPos(ChessFile.FileF, ChessRank.Rank3));
			Assert.AreEqual<ChessPosition>(ChessPosition.G2, Chess.FileRankToPos(ChessFile.FileG, ChessRank.Rank2));
			Assert.AreEqual<ChessPosition>(ChessPosition.H1, Chess.FileRankToPos(ChessFile.FileH, ChessRank.Rank1));

		}
	}
}
