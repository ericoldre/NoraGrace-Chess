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

        public readonly Mock<DbSet<Sql.Game>> GamesMock;
        public readonly Mock<DbSet<Sql.Move>> MovesMock;
        
        public MoqChessDb()
        {
            this.GamesMock = CreateMockDbSet(GamesInMemory);
            this.MovesMock = CreateMockDbSet(MovesInMemory);

            this.SetupGet(c => c.Games).Returns(GamesMock.Object);
            this.SetupGet(c => c.Moves).Returns(MovesMock.Object);

            
        }

        private static Mock<System.Data.Entity.DbSet<T>> CreateMockDbSet<T>(List<T> list) where T : class
        {
            IQueryable<T> queryable = list.AsQueryable<T>();
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            
            mockSet.Setup(s => s.Include(It.IsAny<string>())).Returns(mockSet.Object);

            mockSet.Setup(d => d.Add(It.IsAny<T>())).Callback<T>((s) => list.Add(s));
            return mockSet;
        }
    }
}
