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
    public class FENTest
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
        public void FENTest1()
        {

            int iCount = 0;
            var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Sinobyl.Engine.Tests.pgnFiles.gm2600.pgn");
            StreamReader reader = new StreamReader(stream);
            while (!reader.EndOfStream)
            {
                iCount++;
                PGN pgn = PGN.NextGame(reader);
                if (pgn == null) { break; }

                Board board = new Board();

                foreach (Move move in pgn.Moves)
                {
                    board.MoveApply(move);
                    string sFenOrig = board.FENCurrent.ToString();

                    FEN fenFromBoard = new FEN(board);
                    FEN fenFromString = new FEN(sFenOrig);

                    Board board2 = new Board(fenFromString.ToString());

                    Assert.AreEqual(sFenOrig, fenFromBoard.ToString());
                    Assert.AreEqual(sFenOrig, fenFromString.ToString());
                    Assert.AreEqual(sFenOrig, board2.FENCurrent.ToString());

                    FEN fenReverse2 = fenFromBoard.Reverse().Reverse();

                    Assert.AreEqual(sFenOrig, fenReverse2.ToString());

                }

                if (iCount > 50) { break; }

            }
        }


    }
}
