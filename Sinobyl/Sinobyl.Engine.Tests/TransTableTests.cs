using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sinobyl.Engine;

namespace Sinobyl.Engine.Tests
{
    [TestClass]
    public class TransTableTests
    {
        [TestMethod]
        public void TransTable_GetAddress()
        {
            ChessTrans trans2 = new ChessTrans(2);
            
            Assert.AreEqual<int>(0, trans2.GetAddress(0));
            Assert.AreEqual<int>(1, trans2.GetAddress(1));
            Assert.AreEqual<int>(0, trans2.GetAddress(2));
            Assert.AreEqual<int>(1, trans2.GetAddress(3));
            Assert.AreEqual<int>(0, trans2.GetAddress(4));
            Assert.AreEqual<int>(1, trans2.GetAddress(5));
            Assert.AreEqual<int>(1, trans2.GetAddress(-1));
            Assert.AreEqual<int>(0, trans2.GetAddress(-2));
            Assert.AreEqual<int>(1, trans2.GetAddress(-3));
            Assert.AreEqual<int>(0, trans2.GetAddress(-4));
            Assert.AreEqual<int>(1, trans2.GetAddress(-5));


            ChessTrans trans3 = new ChessTrans(3);

            Assert.AreEqual<int>(0, trans3.GetAddress(0));
            Assert.AreEqual<int>(1, trans3.GetAddress(1));
            Assert.AreEqual<int>(2, trans3.GetAddress(2));
            Assert.AreEqual<int>(0, trans3.GetAddress(3));
            Assert.AreEqual<int>(1, trans3.GetAddress(4));
            Assert.AreEqual<int>(2, trans3.GetAddress(5));
            Assert.AreEqual<int>(1, trans3.GetAddress(-1));
            Assert.AreEqual<int>(2, trans3.GetAddress(-2));
            Assert.AreEqual<int>(0, trans3.GetAddress(-3));
            Assert.AreEqual<int>(1, trans3.GetAddress(-4));
            Assert.AreEqual<int>(2, trans3.GetAddress(-5));


        }

        [TestMethod]
        public void TransCutoff1()
        {
            ChessTrans trans = new ChessTrans(1);

            ChessMove storeMove = RandomMove();
            int storeScore = RandScore();
            ChessMove readMove;
            int readScore = 0;


            trans.Store(0, 2, ChessTrans.EntryType.Exactly, storeScore, storeMove);
            var doCutoff = trans.QueryCutoff(0, 2, -1000, 1000, out readMove, out readScore);
            Assert.IsTrue(doCutoff == true);
            Assert.AreEqual<int>(storeScore, readScore);
            Assert.AreEqual<ChessMove>(readMove, storeMove);




        }

        [TestMethod]
        public void CheckAllStored()
        {

            ChessTrans trans = new ChessTrans(100);

            var moves = Enumerable.Range(0, 500).Select(i => RandomMove()).ToArray();

            //store deep entries
            for (int i = 0; i < 100; i++)
            {
                trans.Store(i, 10, ChessTrans.EntryType.Worthless, 0, moves[i]);
            }

            //store shallow entries
            for (int i = 100; i < 200; i++)
            {
                trans.Store(i, 3, ChessTrans.EntryType.Worthless, 0, moves[i]);
            }

            for (int i = 0; i < 200; i++)
            {
                ChessMove bestmove;
                int score = 0;
                trans.QueryCutoff(i, 11, 0, 0, out bestmove, out score);
                Assert.AreEqual<ChessMove>(moves[i], bestmove);
            }

            //store even more shallow entries
            for (int i = 200; i < 300; i++)
            {
                trans.Store(i, 1, ChessTrans.EntryType.Worthless, 0, moves[i]);
            }

            //assert deepest entries still there
            for (int i = 0; i < 100; i++)
            {
                ChessMove bestmove;
                int score = 0;
                trans.QueryCutoff(i, 11, 0, 0, out bestmove, out score);
                Assert.AreEqual<ChessMove>(moves[i], bestmove);
            }

            //assert deepest entries still there
            for (int i = 200; i < 300; i++)
            {
                ChessMove bestmove;
                int score = 0;
                trans.QueryCutoff(i, 11, 0, 0, out bestmove, out score);
                Assert.AreEqual<ChessMove>(moves[i], bestmove);
            }
        }

        public void AssertNotIncorrect(ChessTrans trans, long storedZob, int storedDepth, int storedScore, ChessMove storedMove)
        {
            //if entry replaced nothing we can check;
            if (trans.GetEntry(storedZob) == null) { return; }

            ChessMove readMove;
            int readScore = 0;
            bool doCutoff;

            //check too deep
            doCutoff = trans.QueryCutoff(storedZob, storedDepth + 1, int.MinValue, int.MaxValue, out readMove, out readScore);
            Assert.AreEqual<ChessMove>(storedMove, readMove);
            Assert.IsFalse(doCutoff);

            //
            doCutoff = trans.QueryCutoff(storedZob, storedDepth, int.MinValue, int.MaxValue, out readMove, out readScore);
            Assert.AreEqual<ChessMove>(storedMove, readMove);
            Assert.IsFalse(doCutoff);
 
        }

        

        public Random random = new Random(6);
        public ChessMove RandomMove()
        {
            ChessPosition f = ChessPositionInfo.AllPositions[random.Next(ChessPositionInfo.AllPositions.Length)];
            ChessPosition t = ChessPositionInfo.AllPositions[random.Next(ChessPositionInfo.AllPositions.Length)];
            return new ChessMove(f, t);
        }

        public ChessTrans.EntryType RandEntryType()
        {
            return (ChessTrans.EntryType)random.Next(0, 5);
        }

        private Int64 Rand64()
        {
            byte[] bytes = new byte[8];
            random.NextBytes(bytes);
            Int64 retval = 0;
            for (int i = 0; i <= 7; i++)
            {
                //Int64 ibyte = (Int64)bytes[i]&256;
                Int64 ibyte = (Int64)bytes[i];
                retval |= ibyte << (i * 8);
            }
            return retval;
        }

        private int RandScore()
        {
            return random.Next(-1000, 1000);
        }


    }
}
