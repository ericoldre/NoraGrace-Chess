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
        public void PositionInDirectionUnsafeTest()
        {
            foreach (ChessPosition pos in ChessPositionInfo.AllPositions)
            {
                foreach (ChessDirection dir in Chess.AllDirections)
                {
                    var correctResult = pos.PositionInDirection(dir);
                    if (correctResult.IsInBounds())
                    {
                        var result = pos.PositionInDirectionUnsafe(dir);
                        Assert.AreEqual<ChessPosition>(correctResult, result);
                    }
                }
            }
        }

		[TestMethod]
		public void DirectionFromToTest()
		{
			
			ChessPosition posCurr;
            foreach (ChessPosition posFrom in ChessPositionInfo.AllPositions)
			{
                foreach (ChessPosition posTo in ChessPositionInfo.AllPositions)
				{
					if(posFrom==posTo){continue;}
                    ChessDirection dirFromTo = posFrom.DirectionTo(posTo);
					if (dirFromTo == 0) 
					{
						foreach(ChessDirection dirTry in Chess.AllDirectionsQueen)
						{
							posCurr = posFrom;
							while(posCurr.IsInBounds())
							{
								Assert.IsFalse(posCurr == posTo);
                                posCurr = posCurr.PositionInDirection(dirTry);
							}
						}
                        foreach (ChessDirection dirTry in Chess.AllDirectionsKnight)
                        {
                            posCurr = posFrom.PositionInDirection(dirTry);
                            Assert.IsFalse(posCurr == posTo);
                        }
					}
                    else if (dirFromTo.IsDirectionKnight())
                    {
                        Assert.AreEqual<ChessPosition>(posTo, posFrom.PositionInDirection(dirFromTo));
                    }
                    else
                    {
                        bool FoundTo = false;
                        posCurr = posFrom;
                        while (posCurr.IsInBounds())
                        {
                            posCurr = posCurr.PositionInDirection(dirFromTo);
                            if (posCurr == posTo) { FoundTo = true; }
                        }
                        Assert.IsTrue(FoundTo);
                    }
				}
			}

		}
		[TestMethod]
		public void CharacterTests()
		{
            Assert.AreEqual<string>("1", ChessRank.Rank1.RankToString());
            Assert.AreEqual<string>("2", ChessRank.Rank2.RankToString());
            Assert.AreEqual<string>("3", ChessRank.Rank3.RankToString());
            Assert.AreEqual<string>("4", ChessRank.Rank4.RankToString());
            Assert.AreEqual<string>("5", ChessRank.Rank5.RankToString());
            Assert.AreEqual<string>("6", ChessRank.Rank6.RankToString());
            Assert.AreEqual<string>("7", ChessRank.Rank7.RankToString());
            Assert.AreEqual<string>("8", ChessRank.Rank8.RankToString());

            Assert.AreEqual<string>("a", ChessFile.FileA.FileToString());
            Assert.AreEqual<string>("b", ChessFile.FileB.FileToString());
            Assert.AreEqual<string>("c", ChessFile.FileC.FileToString());
            Assert.AreEqual<string>("d", ChessFile.FileD.FileToString());
            Assert.AreEqual<string>("e", ChessFile.FileE.FileToString());
            Assert.AreEqual<string>("f", ChessFile.FileF.FileToString());
            Assert.AreEqual<string>("g", ChessFile.FileG.FileToString());
            Assert.AreEqual<string>("h", ChessFile.FileH.FileToString());

			Assert.AreEqual<ChessRank>(ChessRank.Rank1, ChessRankInfo.Parse('1'));
			Assert.AreEqual<ChessRank>(ChessRank.Rank2, ChessRankInfo.Parse('2'));
            Assert.AreEqual<ChessRank>(ChessRank.Rank3, ChessRankInfo.Parse('3'));
            Assert.AreEqual<ChessRank>(ChessRank.Rank4, ChessRankInfo.Parse('4'));
            Assert.AreEqual<ChessRank>(ChessRank.Rank5, ChessRankInfo.Parse('5'));
            Assert.AreEqual<ChessRank>(ChessRank.Rank6, ChessRankInfo.Parse('6'));
            Assert.AreEqual<ChessRank>(ChessRank.Rank7, ChessRankInfo.Parse('7'));
            Assert.AreEqual<ChessRank>(ChessRank.Rank8, ChessRankInfo.Parse('8'));

			Assert.AreEqual<ChessFile>(ChessFile.FileA, ChessFileInfo.Parse('a'));
            Assert.AreEqual<ChessFile>(ChessFile.FileB, ChessFileInfo.Parse('b'));
            Assert.AreEqual<ChessFile>(ChessFile.FileC, ChessFileInfo.Parse('c'));
            Assert.AreEqual<ChessFile>(ChessFile.FileD, ChessFileInfo.Parse('d'));
            Assert.AreEqual<ChessFile>(ChessFile.FileE, ChessFileInfo.Parse('e'));
            Assert.AreEqual<ChessFile>(ChessFile.FileF, ChessFileInfo.Parse('f'));
            Assert.AreEqual<ChessFile>(ChessFile.FileG, ChessFileInfo.Parse('g'));
            Assert.AreEqual<ChessFile>(ChessFile.FileH, ChessFileInfo.Parse('h'));

		}

		[TestMethod]
		public void DirectionTests()
		{
			Assert.IsFalse(ChessPositionInfo.PositionInDirection(ChessPosition.A1, ChessDirection.DirW).IsInBounds());
            Assert.IsFalse(ChessPositionInfo.PositionInDirection(ChessPosition.A1, ChessDirection.DirS).IsInBounds());
            Assert.IsFalse(ChessPositionInfo.PositionInDirection(ChessPosition.A8, ChessDirection.DirW).IsInBounds());
            Assert.IsFalse(ChessPositionInfo.PositionInDirection(ChessPosition.A8, ChessDirection.DirN).IsInBounds());
            Assert.IsFalse(ChessPositionInfo.PositionInDirection(ChessPosition.H1, ChessDirection.DirE).IsInBounds());
            Assert.IsFalse(ChessPositionInfo.PositionInDirection(ChessPosition.H1, ChessDirection.DirS).IsInBounds());
            Assert.IsFalse(ChessPositionInfo.PositionInDirection(ChessPosition.H8, ChessDirection.DirE).IsInBounds());
            Assert.IsFalse(ChessPositionInfo.PositionInDirection(ChessPosition.H8, ChessDirection.DirN).IsInBounds());

            

            Assert.AreEqual<ChessPosition>(ChessPosition.D5, ChessPositionInfo.PositionInDirection(ChessPosition.D4, ChessDirection.DirN));
            Assert.AreEqual<ChessPosition>(ChessPosition.D3, ChessPositionInfo.PositionInDirection(ChessPosition.D4, ChessDirection.DirS));
            Assert.AreEqual<ChessPosition>(ChessPosition.C4, ChessPositionInfo.PositionInDirection(ChessPosition.D4, ChessDirection.DirW));
            Assert.AreEqual<ChessPosition>(ChessPosition.E4, ChessPositionInfo.PositionInDirection(ChessPosition.D4, ChessDirection.DirE));

            Assert.AreEqual<ChessPosition>(ChessPosition.E5, ChessPositionInfo.PositionInDirection(ChessPosition.D4, ChessDirection.DirNE));
            Assert.AreEqual<ChessPosition>(ChessPosition.E3, ChessPositionInfo.PositionInDirection(ChessPosition.D4, ChessDirection.DirSE));
            Assert.AreEqual<ChessPosition>(ChessPosition.C3, ChessPositionInfo.PositionInDirection(ChessPosition.D4, ChessDirection.DirSW));
            Assert.AreEqual<ChessPosition>(ChessPosition.C5, ChessPositionInfo.PositionInDirection(ChessPosition.D4, ChessDirection.DirNW));

            Assert.AreEqual<ChessPosition>(ChessPosition.F7, ChessPositionInfo.PositionInDirection(ChessPosition.E5, ChessDirection.DirNNE));
            Assert.AreEqual<ChessPosition>(ChessPosition.G6, ChessPositionInfo.PositionInDirection(ChessPosition.E5, ChessDirection.DirEEN));
            Assert.AreEqual<ChessPosition>(ChessPosition.G4, ChessPositionInfo.PositionInDirection(ChessPosition.E5, ChessDirection.DirEES));
            Assert.AreEqual<ChessPosition>(ChessPosition.F3, ChessPositionInfo.PositionInDirection(ChessPosition.E5, ChessDirection.DirSSE));
            Assert.AreEqual<ChessPosition>(ChessPosition.D3, ChessPositionInfo.PositionInDirection(ChessPosition.E5, ChessDirection.DirSSW));
            Assert.AreEqual<ChessPosition>(ChessPosition.C4, ChessPositionInfo.PositionInDirection(ChessPosition.E5, ChessDirection.DirWWS));
            Assert.AreEqual<ChessPosition>(ChessPosition.C6, ChessPositionInfo.PositionInDirection(ChessPosition.E5, ChessDirection.DirWWN));
            Assert.AreEqual<ChessPosition>(ChessPosition.D7, ChessPositionInfo.PositionInDirection(ChessPosition.E5, ChessDirection.DirNNW));

		}

        [TestMethod]
        public void BitboardBitCount()
        {

            foreach (ChessPosition pos in ChessPositionInfo.AllPositions)
            {
                Assert.AreEqual<int>(1, pos.Bitboard().BitCount());
            }
            foreach (var rank in ChessRankInfo.AllRanks)
            {
                Assert.AreEqual<int>(8, rank.Bitboard().BitCount());
            }
            foreach (var file in ChessFileInfo.AllFiles)
            {
                Assert.AreEqual<int>(8, file.Bitboard().BitCount());
            }
        }

        [TestMethod]
        public void BitboardReverse()
        {
            Assert.AreEqual<ChessBitboard>(ChessBitboard.Rank8, ChessBitboard.Rank1.Reverse());
            Assert.AreEqual<ChessBitboard>(ChessBitboard.Rank7, ChessBitboard.Rank2.Reverse());
            Assert.AreEqual<ChessBitboard>(ChessBitboard.Rank6, ChessBitboard.Rank3.Reverse());
            Assert.AreEqual<ChessBitboard>(ChessBitboard.Rank5, ChessBitboard.Rank4.Reverse());
            Assert.AreEqual<ChessBitboard>(ChessBitboard.Rank4, ChessBitboard.Rank5.Reverse());
            Assert.AreEqual<ChessBitboard>(ChessBitboard.Rank3, ChessBitboard.Rank6.Reverse());
            Assert.AreEqual<ChessBitboard>(ChessBitboard.Rank2, ChessBitboard.Rank7.Reverse());
            Assert.AreEqual<ChessBitboard>(ChessBitboard.Rank1, ChessBitboard.Rank8.Reverse());

            Assert.AreEqual<ChessBitboard>(ChessBitboard.FileA, ChessBitboard.FileA.Reverse());
            Assert.AreEqual<ChessBitboard>(ChessBitboard.FileB, ChessBitboard.FileB.Reverse());
            Assert.AreEqual<ChessBitboard>(ChessBitboard.FileC, ChessBitboard.FileC.Reverse());
            Assert.AreEqual<ChessBitboard>(ChessBitboard.FileD, ChessBitboard.FileD.Reverse());
            Assert.AreEqual<ChessBitboard>(ChessBitboard.FileE, ChessBitboard.FileE.Reverse());
            Assert.AreEqual<ChessBitboard>(ChessBitboard.FileF, ChessBitboard.FileF.Reverse());
            Assert.AreEqual<ChessBitboard>(ChessBitboard.FileG, ChessBitboard.FileG.Reverse());
            Assert.AreEqual<ChessBitboard>(ChessBitboard.FileH, ChessBitboard.FileH.Reverse());

            Assert.AreEqual<ChessBitboard>(ChessBitboard.Full, ChessBitboard.Full.Reverse());
            Assert.AreEqual<ChessBitboard>(ChessBitboard.Empty, ChessBitboard.Empty.Reverse());
            

            
        }

        [TestMethod]
        public void BitboardShiftTests()
        {
            Assert.AreEqual<ChessBitboard>(~ChessBitboard.Rank1, ChessBitboard.Full.ShiftDirN());
            Assert.AreEqual<ChessBitboard>(~ChessBitboard.FileA, ChessBitboard.Full.ShiftDirE());
            Assert.AreEqual<ChessBitboard>(~ChessBitboard.Rank8, ChessBitboard.Full.ShiftDirS());
            Assert.AreEqual<ChessBitboard>(~ChessBitboard.FileH, ChessBitboard.Full.ShiftDirW());

            Assert.AreEqual<ChessBitboard>(~ChessBitboard.Rank1 & ~ChessBitboard.FileA, ChessBitboard.Full.ShiftDirNE());
            Assert.AreEqual<ChessBitboard>(~ChessBitboard.Rank8 & ~ChessBitboard.FileA, ChessBitboard.Full.ShiftDirSE());
            Assert.AreEqual<ChessBitboard>(~ChessBitboard.Rank8 & ~ChessBitboard.FileH, ChessBitboard.Full.ShiftDirSW());
            Assert.AreEqual<ChessBitboard>(~ChessBitboard.Rank1 & ~ChessBitboard.FileH, ChessBitboard.Full.ShiftDirNW());

            foreach (ChessPosition pos in ChessPositionInfo.AllPositions)
            {
                foreach (ChessDirection dir in Chess.AllDirections)
                {
                    ChessPosition posEnd = ChessPositionInfo.PositionInDirection(pos, dir);
                    ChessBitboard shifted = pos.Bitboard().Shift(dir);
                    Assert.AreEqual<bool>(posEnd.IsInBounds(), shifted.ToPositions().Count() == 1);
                    Assert.AreEqual<ChessBitboard>(posEnd.Bitboard(), shifted);
                }
            }

        }

        [TestMethod]
        public void BitboardTests()
        {
            foreach (var pos in ChessPositionInfo.AllPositions)
            {
                foreach (var dir in Chess.AllDirections)
                {
                    var posnew = ChessPositionInfo.PositionInDirection(pos, dir);
                    if (!posnew.IsInBounds())
                    {
                        Assert.IsTrue(posnew.Bitboard().Empty());
                    }
                    else
                    {
                        Assert.IsTrue(posnew.Bitboard().ToPositions().Count() == 1);
                    }
                }
            }
            foreach (var pos1 in ChessPositionInfo.AllPositions)
            {
                foreach (var pos2 in ChessPositionInfo.AllPositions)
                {

                    //if we have 3 unique positions
                    if (pos1 != pos2)
                    {
                        ChessBitboard bb1 = pos1.Bitboard();
                        ChessBitboard bb2 = pos2.Bitboard();

                        Assert.AreNotEqual<ChessBitboard>(bb1, bb2);

                        var posArray = new ChessPosition[] { pos1, pos2 };
                        var bbAll = posArray.ToBitboard();

                        //verifiy created bitboard contains all three positions.
                        Assert.IsTrue(!(bbAll & bb1).Empty());
                        Assert.IsTrue(!(bbAll & bb2).Empty());

                        //verify bitboard does not contain any other positions
                        foreach (var posOther in ChessPositionInfo.AllPositions.Where(posO => !posArray.Any(posE => posO == posE)))
                        {
                            var bbOther = posOther.Bitboard();
                            Assert.IsTrue((bbOther & bbAll).Empty());    
                        }

                        //verify that bitboard when positions are enumerated is returns all 3 positions.
                        var posFromBitboards = bbAll.ToPositions().ToArray();
                        var bbRemade = posFromBitboards.ToBitboard();
                        Assert.IsTrue(bbRemade == bbAll);
                    }
                    
                }
            }
        }
		[TestMethod]
		public void FileAndRankTests()
		{


			Assert.AreEqual<ChessRank>(ChessRank.Rank1, ChessPosition.A1.GetRank());
            Assert.AreEqual<ChessRank>(ChessRank.Rank2, ChessPosition.A2.GetRank());
            Assert.AreEqual<ChessRank>(ChessRank.Rank3, ChessPosition.A3.GetRank());
            Assert.AreEqual<ChessRank>(ChessRank.Rank4, ChessPosition.A4.GetRank());
            Assert.AreEqual<ChessRank>(ChessRank.Rank5, ChessPosition.A5.GetRank());
            Assert.AreEqual<ChessRank>(ChessRank.Rank6, ChessPosition.A6.GetRank());
            Assert.AreEqual<ChessRank>(ChessRank.Rank7, ChessPosition.A7.GetRank());
            Assert.AreEqual<ChessRank>(ChessRank.Rank8, ChessPosition.A8.GetRank());

            Assert.AreEqual<ChessRank>(ChessRank.Rank1, ChessPosition.H1.GetRank());
            Assert.AreEqual<ChessRank>(ChessRank.Rank2, ChessPosition.H2.GetRank());
            Assert.AreEqual<ChessRank>(ChessRank.Rank3, ChessPosition.H3.GetRank());
            Assert.AreEqual<ChessRank>(ChessRank.Rank4, ChessPosition.H4.GetRank());
            Assert.AreEqual<ChessRank>(ChessRank.Rank5, ChessPosition.H5.GetRank());
            Assert.AreEqual<ChessRank>(ChessRank.Rank6, ChessPosition.H6.GetRank());
            Assert.AreEqual<ChessRank>(ChessRank.Rank7, ChessPosition.H7.GetRank());
            Assert.AreEqual<ChessRank>(ChessRank.Rank8, ChessPosition.H8.GetRank());


			Assert.AreEqual<ChessFile>(ChessFile.FileA, ChessPosition.A1.GetFile());
            Assert.AreEqual<ChessFile>(ChessFile.FileB, ChessPosition.B1.GetFile());
            Assert.AreEqual<ChessFile>(ChessFile.FileC, ChessPosition.C1.GetFile());
            Assert.AreEqual<ChessFile>(ChessFile.FileD, ChessPosition.D1.GetFile());
            Assert.AreEqual<ChessFile>(ChessFile.FileE, ChessPosition.E1.GetFile());
            Assert.AreEqual<ChessFile>(ChessFile.FileF, ChessPosition.F1.GetFile());
            Assert.AreEqual<ChessFile>(ChessFile.FileG, ChessPosition.G1.GetFile());
            Assert.AreEqual<ChessFile>(ChessFile.FileH, ChessPosition.H1.GetFile());

            Assert.AreEqual<ChessFile>(ChessFile.FileA, ChessPosition.A8.GetFile());
            Assert.AreEqual<ChessFile>(ChessFile.FileB, ChessPosition.B8.GetFile());
            Assert.AreEqual<ChessFile>(ChessFile.FileC, ChessPosition.C8.GetFile());
            Assert.AreEqual<ChessFile>(ChessFile.FileD, ChessPosition.D8.GetFile());
            Assert.AreEqual<ChessFile>(ChessFile.FileE, ChessPosition.E8.GetFile());
            Assert.AreEqual<ChessFile>(ChessFile.FileF, ChessPosition.F8.GetFile());
            Assert.AreEqual<ChessFile>(ChessFile.FileG, ChessPosition.G8.GetFile());
            Assert.AreEqual<ChessFile>(ChessFile.FileH, ChessPosition.H8.GetFile());

			//FileRankToPos

			Assert.AreEqual<ChessPosition>(ChessPosition.A8, ChessFile.FileA.ToPosition(ChessRank.Rank8));
			Assert.AreEqual<ChessPosition>(ChessPosition.B7, ChessFile.FileB.ToPosition(ChessRank.Rank7));
			Assert.AreEqual<ChessPosition>(ChessPosition.C6, ChessFile.FileC.ToPosition(ChessRank.Rank6));
			Assert.AreEqual<ChessPosition>(ChessPosition.D5, ChessFile.FileD.ToPosition(ChessRank.Rank5));
			Assert.AreEqual<ChessPosition>(ChessPosition.E4, ChessFile.FileE.ToPosition(ChessRank.Rank4));
			Assert.AreEqual<ChessPosition>(ChessPosition.F3, ChessFile.FileF.ToPosition(ChessRank.Rank3));
			Assert.AreEqual<ChessPosition>(ChessPosition.G2, ChessFile.FileG.ToPosition(ChessRank.Rank2));
			Assert.AreEqual<ChessPosition>(ChessPosition.H1, ChessFile.FileH.ToPosition(ChessRank.Rank1));

		}
	}
}
