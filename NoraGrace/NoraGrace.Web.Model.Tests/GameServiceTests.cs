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
    public class GameServiceTests
    {

        private static log4net.ILog _log = log4net.LogManager.GetLogger(typeof(GameService));

        [TestMethod]
        public void FindReturnsGameInfo()
        {
            _log.Debug("");
            var game = new Sql.Game() { GameId = 64, White = "white", Black = "black", Result = Engine.GameResult.Draw, ResultReason = Engine.GameResultReason.Unknown };
            var db = new MoqChessDb();

            db.GamesInMemory.Add(game);

            var repo = new GameService(db.Object);
            var result = repo.Find(64);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(GameInfo));
            Assert.AreEqual(game.GameId, result.GameId);
            GameInfoTests.AssertValidGameInfo(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void FindThrowsOutOfRangeException()
        {
            var db = new MoqChessDb();
            var repo = new GameService(db.Object);
            var result = repo.Find(64);
        }

        [TestMethod]
        public void CreateInsertsToDB()
        {
            var db = new MoqChessDb();
            var repo = new GameService(db.Object);
            var result = repo.Create(new GameCreateOptions());
            Assert.AreEqual<int>(1, db.GamesInMemory.Count);
            db.GamesMock.Verify(g => g.Add(It.IsAny<Game>()), Times.Once());
            db.Verify(m => m.SaveChanges(), Times.Once());
            GameInfoTests.AssertValidGameInfo(result);
        }

        [TestMethod]
        public void CreateReturnsGameInfo()
        {
            var db = new MoqChessDb();
            var repo = new GameService(db.Object);
            var result = repo.Create(new GameCreateOptions());

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(GameInfo));
            GameInfoTests.AssertValidGameInfo(result);
            //Assert.AreEqual(0, result.);
        }

        [TestMethod]
        public void ApplyMoveAddsMove()
        {

            var db = new MoqChessDb();
            var game = new Sql.Game() { GameId = 64, White = "white", Black = "black" };
            db.GamesInMemory.Add(game);

            var repo = new GameService(db.Object);

            var result = repo.ApplyMove(64, 1, Player.White, "a4");

            Assert.AreEqual<int>(1, game.Moves.Count);
            db.Verify(m => m.SaveChanges(), Times.Once());
            GameInfoTests.AssertValidGameInfo(result);

        }


        //[TestMethod]
        //public void CanUseIncludeWithMocks()
        //{
        //    var child = new Sql.Move();
        //    var parent = new Sql.Game();
        //    parent.Moves.Add(child);

        //    var parents = new List<Game>
        //        {
        //            parent
        //        }.AsQueryable();

        //    var children = new List<Move>
        //        {
        //            child
        //        }.AsQueryable();

        //    var mockContext = new Mock<Sql.ChessDb>();

        //    var mockParentSet = CreateMockSet(parents);
        //    var mockChildSet = CreateMockSet(children);
        //    DbSet<Game> x = mockParentSet.Object;

        //    mockContext.SetupGet(mc => mc.Games).Returns(() => x);
        //    mockContext.SetupGet(mc => mc.Moves).Returns(mockChildSet.Object);

        //    mockContext.Object.Parents.Should().HaveCount(1);
        //    mockContext.Object.Children.Should().HaveCount(1);

        //    mockContext.Object.Parents.First().Children.FirstOrDefault().Should().NotBeNull();

        //    var query = mockContext.Object.Parents.Include(p => p.Children).Select(p => p);

        //    query.Should().NotBeNull().And.HaveCount(1);
        //    query.First().Children.Should().NotBeEmpty().And.HaveCount(1);
        //}


        //private static Mock<DbSet<T>> CreateMockSet<T>(IQueryable<T> childlessParents) where T : class
        //{
        //    var mockSet = new Mock<DbSet<T>>();

        //    mockSet.Setup(m => m.Provider).Returns(childlessParents.Provider);
        //    mockSet.Setup(m => m.Expression).Returns(childlessParents.Expression);
        //    mockSet.Setup(m => m.ElementType).Returns(childlessParents.ElementType);
        //    mockSet.Setup(m => m.GetEnumerator()).Returns(childlessParents.GetEnumerator());
        //    return mockSet;
        //}

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
