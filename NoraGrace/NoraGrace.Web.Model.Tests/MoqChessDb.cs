using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NoraGrace.Sql;
using System.Data.Entity;

namespace NoraGrace.Web.Model.Tests
{
    public class MoqChessDb: Moq.Mock<NoraGrace.Sql.ChessDb>
    {

        public readonly List<Sql.Game> GamesInMemory = new List<Game>();
        public readonly List<Sql.Move> MovesInMemory = new List<Move>();
        public MoqChessDb()
        {
            this.Setup(c => c.Games).Returns(CreateMockDbSet(GamesInMemory.AsQueryable<Game>()).Object);
            this.Setup(c => c.Moves).Returns(CreateMockDbSet(MovesInMemory.AsQueryable<Move>()).Object);
        }

        private static Mock<System.Data.Entity.DbSet<T>> CreateMockDbSet<T>(IQueryable<T> queryable) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            return mockSet;
        }
    }
}
