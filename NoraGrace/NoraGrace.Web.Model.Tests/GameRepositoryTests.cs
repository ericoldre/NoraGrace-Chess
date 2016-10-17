using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NoraGrace.Web.Model.Tests
{
    [TestClass]
    public class GameRepositoryTests
    {
        [TestMethod]
        public void FindReturnsGameInfo()
        {

            var game = new Sql.Game() { GameId = 64, White = "white", Black = "black", Result = Engine.GameResult.Draw, ResultReason = Engine.GameResultReason.Unknown };
            var db = new MoqChessDb();

            db.GamesInMemory.Add(game);

            var repo = new GameRepository(db.Object);
            var result = repo.Find(64);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(GameInfo));
            Assert.AreEqual(game.GameId, result.GameId);
            
        }

        //[TestMethod]
        //public void FindReturnsOutOfRange()
        //{
        //    var db = new MoqChessDb();
        //    var repo = new GameRepository(db.Object);

        //}


        //[TestMethod]
        //public void CreateReturnsGameInfo()
        //{
        //    var db = new MoqChessDb();
        //    var repo = new GameRepository(db.Object);

        //}
    }
}
