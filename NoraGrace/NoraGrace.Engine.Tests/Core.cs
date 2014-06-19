using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoraGrace.Engine;

namespace NoraGrace.Engine.Tests
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
			Board board = new Board();
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
            foreach (Position pos in PositionInfo.AllPositions)
            {
                foreach (Direction dir in DirectionInfo.AllDirections)
                {
                    var correctResult = pos.PositionInDirection(dir);
                    if (correctResult.IsInBounds())
                    {
                        var result = pos.PositionInDirectionUnsafe(dir);
                        Assert.AreEqual<Position>(correctResult, result);
                    }
                }
            }
        }

		[TestMethod]
		public void DirectionFromToTest()
		{
			
			Position posCurr;
            foreach (Position posFrom in PositionInfo.AllPositions)
			{
                foreach (Position posTo in PositionInfo.AllPositions)
				{
					if(posFrom==posTo){continue;}
                    Direction dirFromTo = posFrom.DirectionTo(posTo);
					if (dirFromTo == 0) 
					{
                        foreach (Direction dirTry in DirectionInfo.AllDirectionsQueen)
						{
							posCurr = posFrom;
							while(posCurr.IsInBounds())
							{
								Assert.IsFalse(posCurr == posTo);
                                posCurr = posCurr.PositionInDirection(dirTry);
							}
						}
                        foreach (Direction dirTry in DirectionInfo.AllDirectionsKnight)
                        {
                            posCurr = posFrom.PositionInDirection(dirTry);
                            Assert.IsFalse(posCurr == posTo);
                        }
					}
                    else if (dirFromTo.IsDirectionKnight())
                    {
                        Assert.AreEqual<Position>(posTo, posFrom.PositionInDirection(dirFromTo));
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
			Assert.IsFalse(PositionInfo.PositionInDirection(Position.A1, Direction.DirW).IsInBounds());
            Assert.IsFalse(PositionInfo.PositionInDirection(Position.A1, Direction.DirS).IsInBounds());
            Assert.IsFalse(PositionInfo.PositionInDirection(Position.A8, Direction.DirW).IsInBounds());
            Assert.IsFalse(PositionInfo.PositionInDirection(Position.A8, Direction.DirN).IsInBounds());
            Assert.IsFalse(PositionInfo.PositionInDirection(Position.H1, Direction.DirE).IsInBounds());
            Assert.IsFalse(PositionInfo.PositionInDirection(Position.H1, Direction.DirS).IsInBounds());
            Assert.IsFalse(PositionInfo.PositionInDirection(Position.H8, Direction.DirE).IsInBounds());
            Assert.IsFalse(PositionInfo.PositionInDirection(Position.H8, Direction.DirN).IsInBounds());

            

            Assert.AreEqual<Position>(Position.D5, PositionInfo.PositionInDirection(Position.D4, Direction.DirN));
            Assert.AreEqual<Position>(Position.D3, PositionInfo.PositionInDirection(Position.D4, Direction.DirS));
            Assert.AreEqual<Position>(Position.C4, PositionInfo.PositionInDirection(Position.D4, Direction.DirW));
            Assert.AreEqual<Position>(Position.E4, PositionInfo.PositionInDirection(Position.D4, Direction.DirE));

            Assert.AreEqual<Position>(Position.E5, PositionInfo.PositionInDirection(Position.D4, Direction.DirNE));
            Assert.AreEqual<Position>(Position.E3, PositionInfo.PositionInDirection(Position.D4, Direction.DirSE));
            Assert.AreEqual<Position>(Position.C3, PositionInfo.PositionInDirection(Position.D4, Direction.DirSW));
            Assert.AreEqual<Position>(Position.C5, PositionInfo.PositionInDirection(Position.D4, Direction.DirNW));

            Assert.AreEqual<Position>(Position.F7, PositionInfo.PositionInDirection(Position.E5, Direction.DirNNE));
            Assert.AreEqual<Position>(Position.G6, PositionInfo.PositionInDirection(Position.E5, Direction.DirEEN));
            Assert.AreEqual<Position>(Position.G4, PositionInfo.PositionInDirection(Position.E5, Direction.DirEES));
            Assert.AreEqual<Position>(Position.F3, PositionInfo.PositionInDirection(Position.E5, Direction.DirSSE));
            Assert.AreEqual<Position>(Position.D3, PositionInfo.PositionInDirection(Position.E5, Direction.DirSSW));
            Assert.AreEqual<Position>(Position.C4, PositionInfo.PositionInDirection(Position.E5, Direction.DirWWS));
            Assert.AreEqual<Position>(Position.C6, PositionInfo.PositionInDirection(Position.E5, Direction.DirWWN));
            Assert.AreEqual<Position>(Position.D7, PositionInfo.PositionInDirection(Position.E5, Direction.DirNNW));

		}

        [TestMethod]
        public void BitboardBitCount()
        {

            foreach (Position pos in PositionInfo.AllPositions)
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

            foreach (Position pos in PositionInfo.AllPositions)
            {
                foreach (Direction dir in DirectionInfo.AllDirections)
                {
                    Position posEnd = PositionInfo.PositionInDirection(pos, dir);
                    Bitboard shifted = pos.ToBitboard().Shift(dir);
                    Assert.AreEqual<bool>(posEnd.IsInBounds(), shifted.ToPositions().Count() == 1);
                    Assert.AreEqual<Bitboard>(posEnd.ToBitboard(), shifted);
                }
            }

        }

        [TestMethod]
        public void BitboardLSBMSB()
        {
            foreach (var pos in PositionInfo.AllPositions)
            {
                var bb = pos.ToBitboard();
                var northMost = bb.NorthMostPosition();
                var southMost = bb.SouthMostPosition();

                Assert.AreEqual<Position>(pos, northMost);
                Assert.AreEqual<Position>(pos, southMost);

                var north = pos.PositionInDirection(Direction.DirN);
                if (north.IsInBounds())
                {
                    bb = pos.ToBitboard() | north.ToBitboard();
                    northMost = bb.NorthMostPosition();
                    southMost = bb.SouthMostPosition();

                    Assert.AreEqual<Position>(north, northMost);
                    Assert.AreEqual<Position>(pos, southMost);

                }

                var south = pos.PositionInDirection(Direction.DirS);
                if (south.IsInBounds())
                {
                    bb = pos.ToBitboard() | south.ToBitboard();
                    northMost = bb.NorthMostPosition();
                    southMost = bb.SouthMostPosition();

                    Assert.AreEqual<Position>(south, southMost);
                    Assert.AreEqual<Position>(pos, northMost);
                }

            }
        }

        [TestMethod]
        public void BitboardTests()
        {
            foreach (var pos in PositionInfo.AllPositions)
            {
                foreach (var dir in DirectionInfo.AllDirections)
                {
                    var posnew = PositionInfo.PositionInDirection(pos, dir);
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
            foreach (var pos1 in PositionInfo.AllPositions)
            {
                foreach (var pos2 in PositionInfo.AllPositions)
                {

                    //if we have 3 unique positions
                    if (pos1 != pos2)
                    {
                        Bitboard bb1 = pos1.ToBitboard();
                        Bitboard bb2 = pos2.ToBitboard();

                        Assert.AreNotEqual<Bitboard>(bb1, bb2);

                        var posArray = new Position[] { pos1, pos2 };
                        var bbAll = posArray.ToBitboard();

                        //verifiy created bitboard contains all three positions.
                        Assert.IsTrue(!(bbAll & bb1).Empty());
                        Assert.IsTrue(!(bbAll & bb2).Empty());

                        //verify bitboard does not contain any other positions
                        foreach (var posOther in PositionInfo.AllPositions.Where(posO => !posArray.Any(posE => posO == posE)))
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


			Assert.AreEqual<Rank>(Rank.Rank1, Position.A1.ToRank());
            Assert.AreEqual<Rank>(Rank.Rank2, Position.A2.ToRank());
            Assert.AreEqual<Rank>(Rank.Rank3, Position.A3.ToRank());
            Assert.AreEqual<Rank>(Rank.Rank4, Position.A4.ToRank());
            Assert.AreEqual<Rank>(Rank.Rank5, Position.A5.ToRank());
            Assert.AreEqual<Rank>(Rank.Rank6, Position.A6.ToRank());
            Assert.AreEqual<Rank>(Rank.Rank7, Position.A7.ToRank());
            Assert.AreEqual<Rank>(Rank.Rank8, Position.A8.ToRank());

            Assert.AreEqual<Rank>(Rank.Rank1, Position.H1.ToRank());
            Assert.AreEqual<Rank>(Rank.Rank2, Position.H2.ToRank());
            Assert.AreEqual<Rank>(Rank.Rank3, Position.H3.ToRank());
            Assert.AreEqual<Rank>(Rank.Rank4, Position.H4.ToRank());
            Assert.AreEqual<Rank>(Rank.Rank5, Position.H5.ToRank());
            Assert.AreEqual<Rank>(Rank.Rank6, Position.H6.ToRank());
            Assert.AreEqual<Rank>(Rank.Rank7, Position.H7.ToRank());
            Assert.AreEqual<Rank>(Rank.Rank8, Position.H8.ToRank());


			Assert.AreEqual<File>(File.FileA, Position.A1.ToFile());
            Assert.AreEqual<File>(File.FileB, Position.B1.ToFile());
            Assert.AreEqual<File>(File.FileC, Position.C1.ToFile());
            Assert.AreEqual<File>(File.FileD, Position.D1.ToFile());
            Assert.AreEqual<File>(File.FileE, Position.E1.ToFile());
            Assert.AreEqual<File>(File.FileF, Position.F1.ToFile());
            Assert.AreEqual<File>(File.FileG, Position.G1.ToFile());
            Assert.AreEqual<File>(File.FileH, Position.H1.ToFile());

            Assert.AreEqual<File>(File.FileA, Position.A8.ToFile());
            Assert.AreEqual<File>(File.FileB, Position.B8.ToFile());
            Assert.AreEqual<File>(File.FileC, Position.C8.ToFile());
            Assert.AreEqual<File>(File.FileD, Position.D8.ToFile());
            Assert.AreEqual<File>(File.FileE, Position.E8.ToFile());
            Assert.AreEqual<File>(File.FileF, Position.F8.ToFile());
            Assert.AreEqual<File>(File.FileG, Position.G8.ToFile());
            Assert.AreEqual<File>(File.FileH, Position.H8.ToFile());

			//FileRankToPos

			Assert.AreEqual<Position>(Position.A8, File.FileA.ToPosition(Rank.Rank8));
			Assert.AreEqual<Position>(Position.B7, File.FileB.ToPosition(Rank.Rank7));
			Assert.AreEqual<Position>(Position.C6, File.FileC.ToPosition(Rank.Rank6));
			Assert.AreEqual<Position>(Position.D5, File.FileD.ToPosition(Rank.Rank5));
			Assert.AreEqual<Position>(Position.E4, File.FileE.ToPosition(Rank.Rank4));
			Assert.AreEqual<Position>(Position.F3, File.FileF.ToPosition(Rank.Rank3));
			Assert.AreEqual<Position>(Position.G2, File.FileG.ToPosition(Rank.Rank2));
			Assert.AreEqual<Position>(Position.H1, File.FileH.ToPosition(Rank.Rank1));

		}
	}
}
