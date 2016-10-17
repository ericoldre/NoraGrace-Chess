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
            var db = new MoqChessDb();
            var repo = new GameRepository(db.Object);
            
        }

        [TestMethod]
        public void CreateReturnsGameInfo()
        {
            var db = new MoqChessDb();
            var repo = new GameRepository(db.Object);

        }
    }
}
