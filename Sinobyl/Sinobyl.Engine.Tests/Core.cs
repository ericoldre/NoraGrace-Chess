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
                foreach (ChessDirection dir in ChessDirectionInfo.AllDirections)
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
                        foreach (ChessDirection dirTry in ChessDirectionInfo.AllDirectionsQueen)
						{
							posCurr = posFrom;
							while(posCurr.IsInBounds())
							{
								Assert.IsFalse(posCurr == posTo);
                                posCurr = posCurr.PositionInDirection(dirTry);
							}
						}
                        foreach (ChessDirection dirTry in ChessDirectionInfo.AllDirectionsKnight)
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
            Assert.AreEqual<string>("1", Rank.Rank1.RankToString());
            Assert.AreEqual<string>("2", Rank.Rank2.RankToString());
            Assert.AreEqual<string>("3", Rank.Rank3.RankToString());
            Assert.AreEqual<string>("4", Rank.Rank4.RankToString());
            Assert.AreEqual<string>("5", Rank.Rank5.RankToString());
            Assert.AreEqual<string>("6", Rank.Rank6.RankToString());
            Assert.AreEqual<string>("7", Rank.Rank7.RankToString());
            Assert.AreEqual<string>("8", Rank.Rank8.RankToString());

            Assert.AreEqual<string>("a", File.FileA.FileToString());
            Assert.AreEqual<string>("b", File.FileB.FileToString());
            Assert.AreEqual<string>("c", File.FileC.FileToString());
            Assert.AreEqual<string>("d", File.FileD.FileToString());
            Assert.AreEqual<string>("e", File.FileE.FileToString());
            Assert.AreEqual<string>("f", File.FileF.FileToString());
            Assert.AreEqual<string>("g", File.FileG.FileToString());
            Assert.AreEqual<string>("h", File.FileH.FileToString());

			Assert.AreEqual<Rank>(Rank.Rank1, RankInfo.Parse('1'));
			Assert.AreEqual<Rank>(Rank.Rank2, RankInfo.Parse('2'));
            Assert.AreEqual<Rank>(Rank.Rank3, RankInfo.Parse('3'));
            Assert.AreEqual<Rank>(Rank.Rank4, RankInfo.Parse('4'));
            Assert.AreEqual<Rank>(Rank.Rank5, RankInfo.Parse('5'));
            Assert.AreEqual<Rank>(Rank.Rank6, RankInfo.Parse('6'));
            Assert.AreEqual<Rank>(Rank.Rank7, RankInfo.Parse('7'));
            Assert.AreEqual<Rank>(Rank.Rank8, RankInfo.Parse('8'));

			Assert.AreEqual<File>(File.FileA, FileInfo.Parse('a'));
            Assert.AreEqual<File>(File.FileB, FileInfo.Parse('b'));
            Assert.AreEqual<File>(File.FileC, FileInfo.Parse('c'));
            Assert.AreEqual<File>(File.FileD, FileInfo.Parse('d'));
            Assert.AreEqual<File>(File.FileE, FileInfo.Parse('e'));
            Assert.AreEqual<File>(File.FileF, FileInfo.Parse('f'));
            Assert.AreEqual<File>(File.FileG, FileInfo.Parse('g'));
            Assert.AreEqual<File>(File.FileH, FileInfo.Parse('h'));

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
                Assert.AreEqual<int>(1, pos.ToBitboard().BitCount());
            }
            foreach (var rank in RankInfo.AllRanks)
            {
                Assert.AreEqual<int>(8, rank.ToBitboard().BitCount());
            }
            foreach (var file in FileInfo.AllFiles)
            {
                Assert.AreEqual<int>(8, file.ToBitboard().BitCount());
            }
        }

        [TestMethod]
        public void BitboardReverse()
        {
            Assert.AreEqual<Bitboard>(Bitboard.Rank8, Bitboard.Rank1.Reverse());
            Assert.AreEqual<Bitboard>(Bitboard.Rank7, Bitboard.Rank2.Reverse());
            Assert.AreEqual<Bitboard>(Bitboard.Rank6, Bitboard.Rank3.Reverse());
            Assert.AreEqual<Bitboard>(Bitboard.Rank5, Bitboard.Rank4.Reverse());
            Assert.AreEqual<Bitboard>(Bitboard.Rank4, Bitboard.Rank5.Reverse());
            Assert.AreEqual<Bitboard>(Bitboard.Rank3, Bitboard.Rank6.Reverse());
            Assert.AreEqual<Bitboard>(Bitboard.Rank2, Bitboard.Rank7.Reverse());
            Assert.AreEqual<Bitboard>(Bitboard.Rank1, Bitboard.Rank8.Reverse());

            Assert.AreEqual<Bitboard>(Bitboard.FileA, Bitboard.FileA.Reverse());
            Assert.AreEqual<Bitboard>(Bitboard.FileB, Bitboard.FileB.Reverse());
            Assert.AreEqual<Bitboard>(Bitboard.FileC, Bitboard.FileC.Reverse());
            Assert.AreEqual<Bitboard>(Bitboard.FileD, Bitboard.FileD.Reverse());
            Assert.AreEqual<Bitboard>(Bitboard.FileE, Bitboard.FileE.Reverse());
            Assert.AreEqual<Bitboard>(Bitboard.FileF, Bitboard.FileF.Reverse());
            Assert.AreEqual<Bitboard>(Bitboard.FileG, Bitboard.FileG.Reverse());
            Assert.AreEqual<Bitboard>(Bitboard.FileH, Bitboard.FileH.Reverse());

            Assert.AreEqual<Bitboard>(Bitboard.Full, Bitboard.Full.Reverse());
            Assert.AreEqual<Bitboard>(Bitboard.Empty, Bitboard.Empty.Reverse());
            

            
        }

        [TestMethod]
        public void BitboardShiftTests()
        {
            Assert.AreEqual<Bitboard>(~Bitboard.Rank1, Bitboard.Full.ShiftDirN());
            Assert.AreEqual<Bitboard>(~Bitboard.FileA, Bitboard.Full.ShiftDirE());
            Assert.AreEqual<Bitboard>(~Bitboard.Rank8, Bitboard.Full.ShiftDirS());
            Assert.AreEqual<Bitboard>(~Bitboard.FileH, Bitboard.Full.ShiftDirW());

            Assert.AreEqual<Bitboard>(~Bitboard.Rank1 & ~Bitboard.FileA, Bitboard.Full.ShiftDirNE());
            Assert.AreEqual<Bitboard>(~Bitboard.Rank8 & ~Bitboard.FileA, Bitboard.Full.ShiftDirSE());
            Assert.AreEqual<Bitboard>(~Bitboard.Rank8 & ~Bitboard.FileH, Bitboard.Full.ShiftDirSW());
            Assert.AreEqual<Bitboard>(~Bitboard.Rank1 & ~Bitboard.FileH, Bitboard.Full.ShiftDirNW());

            foreach (ChessPosition pos in ChessPositionInfo.AllPositions)
            {
                foreach (ChessDirection dir in ChessDirectionInfo.AllDirections)
                {
                    ChessPosition posEnd = ChessPositionInfo.PositionInDirection(pos, dir);
                    Bitboard shifted = pos.ToBitboard().Shift(dir);
                    Assert.AreEqual<bool>(posEnd.IsInBounds(), shifted.ToPositions().Count() == 1);
                    Assert.AreEqual<Bitboard>(posEnd.ToBitboard(), shifted);
                }
            }

        }

        [TestMethod]
        public void BitboardLSBMSB()
        {
            foreach (var pos in ChessPositionInfo.AllPositions)
            {
                var bb = pos.ToBitboard();
                var northMost = bb.NorthMostPosition();
                var southMost = bb.SouthMostPosition();

                Assert.AreEqual<ChessPosition>(pos, northMost);
                Assert.AreEqual<ChessPosition>(pos, southMost);

                var north = pos.PositionInDirection(ChessDirection.DirN);
                if (north.IsInBounds())
                {
                    bb = pos.ToBitboard() | north.ToBitboard();
                    northMost = bb.NorthMostPosition();
                    southMost = bb.SouthMostPosition();

                    Assert.AreEqual<ChessPosition>(north, northMost);
                    Assert.AreEqual<ChessPosition>(pos, southMost);

                }

                var south = pos.PositionInDirection(ChessDirection.DirS);
                if (south.IsInBounds())
                {
                    bb = pos.ToBitboard() | south.ToBitboard();
                    northMost = bb.NorthMostPosition();
                    southMost = bb.SouthMostPosition();

                    Assert.AreEqual<ChessPosition>(south, southMost);
                    Assert.AreEqual<ChessPosition>(pos, northMost);
                }

            }
        }

        [TestMethod]
        public void BitboardTests()
        {
            foreach (var pos in ChessPositionInfo.AllPositions)
            {
                foreach (var dir in ChessDirectionInfo.AllDirections)
                {
                    var posnew = ChessPositionInfo.PositionInDirection(pos, dir);
                    if (!posnew.IsInBounds())
                    {
                        Assert.IsTrue(posnew.ToBitboard().Empty());
                    }
                    else
                    {
                        Assert.IsTrue(posnew.ToBitboard().ToPositions().Count() == 1);
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
                        Bitboard bb1 = pos1.ToBitboard();
                        Bitboard bb2 = pos2.ToBitboard();

                        Assert.AreNotEqual<Bitboard>(bb1, bb2);

                        var posArray = new ChessPosition[] { pos1, pos2 };
                        var bbAll = posArray.ToBitboard();

                        //verifiy created bitboard contains all three positions.
                        Assert.IsTrue(!(bbAll & bb1).Empty());
                        Assert.IsTrue(!(bbAll & bb2).Empty());

                        //verify bitboard does not contain any other positions
                        foreach (var posOther in ChessPositionInfo.AllPositions.Where(posO => !posArray.Any(posE => posO == posE)))
                        {
                            var bbOther = posOther.ToBitboard();
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


			Assert.AreEqual<Rank>(Rank.Rank1, ChessPosition.A1.ToRank());
            Assert.AreEqual<Rank>(Rank.Rank2, ChessPosition.A2.ToRank());
            Assert.AreEqual<Rank>(Rank.Rank3, ChessPosition.A3.ToRank());
            Assert.AreEqual<Rank>(Rank.Rank4, ChessPosition.A4.ToRank());
            Assert.AreEqual<Rank>(Rank.Rank5, ChessPosition.A5.ToRank());
            Assert.AreEqual<Rank>(Rank.Rank6, ChessPosition.A6.ToRank());
            Assert.AreEqual<Rank>(Rank.Rank7, ChessPosition.A7.ToRank());
            Assert.AreEqual<Rank>(Rank.Rank8, ChessPosition.A8.ToRank());

            Assert.AreEqual<Rank>(Rank.Rank1, ChessPosition.H1.ToRank());
            Assert.AreEqual<Rank>(Rank.Rank2, ChessPosition.H2.ToRank());
            Assert.AreEqual<Rank>(Rank.Rank3, ChessPosition.H3.ToRank());
            Assert.AreEqual<Rank>(Rank.Rank4, ChessPosition.H4.ToRank());
            Assert.AreEqual<Rank>(Rank.Rank5, ChessPosition.H5.ToRank());
            Assert.AreEqual<Rank>(Rank.Rank6, ChessPosition.H6.ToRank());
            Assert.AreEqual<Rank>(Rank.Rank7, ChessPosition.H7.ToRank());
            Assert.AreEqual<Rank>(Rank.Rank8, ChessPosition.H8.ToRank());


			Assert.AreEqual<File>(File.FileA, ChessPosition.A1.ToFile());
            Assert.AreEqual<File>(File.FileB, ChessPosition.B1.ToFile());
            Assert.AreEqual<File>(File.FileC, ChessPosition.C1.ToFile());
            Assert.AreEqual<File>(File.FileD, ChessPosition.D1.ToFile());
            Assert.AreEqual<File>(File.FileE, ChessPosition.E1.ToFile());
            Assert.AreEqual<File>(File.FileF, ChessPosition.F1.ToFile());
            Assert.AreEqual<File>(File.FileG, ChessPosition.G1.ToFile());
            Assert.AreEqual<File>(File.FileH, ChessPosition.H1.ToFile());

            Assert.AreEqual<File>(File.FileA, ChessPosition.A8.ToFile());
            Assert.AreEqual<File>(File.FileB, ChessPosition.B8.ToFile());
            Assert.AreEqual<File>(File.FileC, ChessPosition.C8.ToFile());
            Assert.AreEqual<File>(File.FileD, ChessPosition.D8.ToFile());
            Assert.AreEqual<File>(File.FileE, ChessPosition.E8.ToFile());
            Assert.AreEqual<File>(File.FileF, ChessPosition.F8.ToFile());
            Assert.AreEqual<File>(File.FileG, ChessPosition.G8.ToFile());
            Assert.AreEqual<File>(File.FileH, ChessPosition.H8.ToFile());

			//FileRankToPos

			Assert.AreEqual<ChessPosition>(ChessPosition.A8, File.FileA.ToPosition(Rank.Rank8));
			Assert.AreEqual<ChessPosition>(ChessPosition.B7, File.FileB.ToPosition(Rank.Rank7));
			Assert.AreEqual<ChessPosition>(ChessPosition.C6, File.FileC.ToPosition(Rank.Rank6));
			Assert.AreEqual<ChessPosition>(ChessPosition.D5, File.FileD.ToPosition(Rank.Rank5));
			Assert.AreEqual<ChessPosition>(ChessPosition.E4, File.FileE.ToPosition(Rank.Rank4));
			Assert.AreEqual<ChessPosition>(ChessPosition.F3, File.FileF.ToPosition(Rank.Rank3));
			Assert.AreEqual<ChessPosition>(ChessPosition.G2, File.FileG.ToPosition(Rank.Rank2));
			Assert.AreEqual<ChessPosition>(ChessPosition.H1, File.FileH.ToPosition(Rank.Rank1));

		}
	}
}
