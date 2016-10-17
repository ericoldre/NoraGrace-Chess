using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoraGrace.Web.Model
{
    public class GameInfo
    {
        public int GameId { get; private set; }
        protected GameInfo()
        {

        }

        public static GameInfo CreateFromDb(Sql.Game game)
        {
            GameInfo retval = new GameInfo();
            retval.GameId = game.GameId;
            return retval;
        }
    }

    public class PlyInfo
    {

    }

    public class MoveInfo
    {

    }
}
