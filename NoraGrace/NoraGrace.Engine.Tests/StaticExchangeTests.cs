using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoraGrace.Engine;

namespace Sinobyl.Engine.Tests
{
    [TestClass]
    public class StaticExchangeTests
    {

        [TestMethod]
        public void StaticExchangeTest1()
        {
            Board board = new Board("4k3/8/4p3/3n4/4P3/8/8/4K3 w - - 0 1 ");
            StaticExchange see = new StaticExchange();

            var move = MoveInfo.Create(Position.E4, Position.D5);

            int score = see.CalculateScore(board, move);
            Assert.IsTrue(score == 201);
        }

        [TestMethod]
        public void StaticExchangeTest2()
        {
            Board board = new Board("4k3/8/4p3/3n4/4P3/5B2/8/4K3 w - - 0 1");
            StaticExchange see = new StaticExchange();

            int moveSee = see.CalculateScore(board, MoveInfo.Create(Position.E4, Position.D5));
            Assert.IsTrue(moveSee >= 200 && moveSee <= 301);
        }

        //
        [TestMethod]
        public void StaticExchangeTest3()
        {
            Board board = new Board("4k3/3r4/4p3/3p4/4P3/5B2/8/4K3 w - - 0 1 ");
            StaticExchange see = new StaticExchange();
            var move = MoveInfo.Create(Position.E4, Position.D5);
            int moveSee = see.CalculateScore(board, move);
            Assert.IsTrue(moveSee == 0);
        }
    }
}
