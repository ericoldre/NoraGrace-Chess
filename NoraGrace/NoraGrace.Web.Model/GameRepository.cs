using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace NoraGrace.Web.Model
{
    public class GameRepository
    {

        private readonly NoraGrace.Sql.ChessDb _context;

        public GameRepository(NoraGrace.Sql.ChessDb context)
        {
            _context = context;
        }

        public GameInfo Find(int gameId)
        {
            var dbresult = _context.Games.Include(g => g.Moves).FirstOrDefault(g => g.GameId == gameId);
            if (dbresult == null) { throw new ArgumentOutOfRangeException("gameId"); }
            var result = GameInfo.CreateFromDb(dbresult);
            return result;
        }

        public GameInfo Create(GameCreateOptions options)
        {
            Sql.Game game = new Sql.Game()
            {
                White = options.White,
                Black = options.Black
            };
            _context.Games.Add(game);
            _context.SaveChanges();
            var result = GameInfo.CreateFromDb(game);
            return result;
        }

        public GameInfo ApplyMove(int GameId, int Ply, int Move)
        {
            return null;
        }
    }
}
