using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoraGrace.Web.Model
{
    public class GameRepository
    {

        private readonly NoraGrace.Sql.ChessDb _context;

        public GameRepository(NoraGrace.Sql.ChessDb context)
        {
            _context = context;
        }

        public GameInfo Find(int GameId)
        {
            return null;
        }

        public GameInfo Create()
        {
            return null;
        }

        public GameInfo CreateMove(int GameId, int Ply, int Move)
        {
            return null;
        }
    }
}
