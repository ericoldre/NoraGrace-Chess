using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Data.Entity;
using System.Linq;
using System.Collections.Generic;
using NoraGrace.Sql;
using NoraGrace.Engine;

namespace NoraGrace.Web.Model.Tests
{
    [TestClass]
    public class GameInfoTests
    {

        public static void AssertValidGameInfo(GameInfo gameInfo)
        {
            Board board = new Board(gameInfo.StartingFEN);

            //first loop through all moves in history.
            for(int ply = 0; ply < gameInfo.MoveHistory.Length; ply++)
            {
                var plyInfo = gameInfo.MoveHistory[ply];
                Assert.IsTrue(PlyInfo.IsValid(plyInfo, board));
                var move = MoveUtil.Parse(board, plyInfo.Move.Description);
                board.MoveApply(move);
            }
            Assert.AreEqual<string>(board.FENCurrent.ToString(), gameInfo.FEN);

            //board is now is state of current game.
            Assert.IsTrue(gameInfo.LegalMoves.All(lm => MoveInfo.IsValid(lm, board))); //all moves are valid individually
            var sortedSpecifiedMoves = gameInfo.LegalMoves.Select(lm => MoveUtil.Parse(board, lm.Description)).OrderBy(m => m).ToList();
            var sortedLegalMoves = MoveUtil.GenMovesLegal(board).OrderBy(m => m).ToList();
            Assert.IsTrue(sortedLegalMoves.SequenceEqual(sortedSpecifiedMoves));

            Assert.IsTrue(gameInfo.Positions.All(p => PositionInfo.IsValid(p, board)));
            Assert.AreEqual<int>(64, gameInfo.Positions.Length);
            Assert.IsTrue(gameInfo.Positions.Select(p => p.Position).GroupBy(p => p).All(g => g.Count() == 1));//all positions are distinct.
            
        }
    }
}
