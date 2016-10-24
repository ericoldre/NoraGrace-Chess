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

        public readonly System.Collections.ObjectModel.ObservableCollection<Sql.DbGame> GamesInMemory = new System.Collections.ObjectModel.ObservableCollection<DbGame>();
        public readonly System.Collections.ObjectModel.ObservableCollection<Sql.DbMove> MovesInMemory = new System.Collections.ObjectModel.ObservableCollection<DbMove>();

        public readonly Mock<DbSet<Sql.DbGame>> GamesMock;
        public readonly Mock<DbSet<Sql.DbMove>> MovesMock;
        
        public MoqChessDb()
        {
            GamesInMemory.CollectionChanged += GamesInMemory_CollectionChanged;

            this.GamesMock = CreateMockDbSet(GamesInMemory);
            this.MovesMock = CreateMockDbSet(MovesInMemory);

            this.SetupGet(c => c.Games).Returns(GamesMock.Object);
            this.SetupGet(c => c.Moves).Returns(MovesMock.Object);

            
        }

        private void GamesInMemory_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                var needGameIds = e.NewItems.OfType<Sql.DbGame>().Where(g => g.GameId == default(int)).ToArray();
                var nextId = 1;
                if (GamesInMemory.Count > 0) { nextId = GamesInMemory.Max(g => g.GameId) + 1; }
                foreach(var g in needGameIds)
                {
                    g.GameId = nextId++;
                }
            }
        }

        private static Mock<System.Data.Entity.DbSet<T>> CreateMockDbSet<T>(IList<T> list) where T : class
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
