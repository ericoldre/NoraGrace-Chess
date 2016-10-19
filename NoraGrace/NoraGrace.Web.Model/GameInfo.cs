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

        public string FEN { get; private set; }
        public PlyInfo[] MoveHistory;
        public MoveInfo[] LegalMoves;

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
        public int Ply { get; set; }
        public int MoveNumber { get; set; }
        public Engine.Player Player { get; set; }
        public MoveInfo Move { get; set; }
    }

    public class MoveInfo
    {
        public string Description { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Promote { get; set; }
    }
}
