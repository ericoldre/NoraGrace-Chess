using Sinobyl.Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Sinobyl.Engine.Tests
{
    
    
    /// <summary>
    ///This is a test class for ChessMoveTest and is intended
    ///to contain all ChessMoveTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ChessMoveTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
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

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for GetHashCode
        ///</summary>
        [TestMethod()]
        public void GetHashCodeTest()
        {
            System.Collections.Generic.Dictionary<int, ChessMove> dic = new System.Collections.Generic.Dictionary<int, ChessMove>();

            foreach (var to in PositionInfo.AllPositions)
            {
                foreach (var from in PositionInfo.AllPositions)
                {
                    foreach (var prom in PieceInfo.AllPieces)
                    {
                        if (prom.ToPieceType() == PieceType.Pawn || prom.ToPieceType() == PieceType.King) { continue; }
                        ChessMove move = ChessMoveInfo.Create(from, to, prom);
                        var hash = move.GetHashCode();
                        if (dic.ContainsKey(hash))
                        {
                            var otherMove = dic[hash];

                            var hash1 = move.GetHashCode();
                            var hash2 = otherMove.GetHashCode();
                            Assert.AreNotEqual<int>(hash1, hash2);
                        }
                        Assert.IsFalse(dic.ContainsKey(hash));
                        dic.Add(hash, move);
                    }
                }
            }

        }
    }
}
