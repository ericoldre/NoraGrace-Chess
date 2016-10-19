using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using NoraGrace.Engine;

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

        public GameInfo ApplyMove(int gameId, int moveNumber, Player player, string moveDescription)
        {
            var dbgame = _context.Games.Include(g => g.Moves).FirstOrDefault(g => g.GameId == gameId);
            if (dbgame.Result.HasValue) { throw new InvalidOperationException("game has already been completed"); }

            var board = Utils.DbGame2Board(dbgame);

            if(board.WhosTurn != player) { throw new ArgumentOutOfRangeException("player", string.Format("it is not that players turn")); }
            if(dbgame.Moves.Where(m=>m.Player==player).Count() != moveNumber - 1) { throw new ArgumentOutOfRangeException("moveNumber"); }
            Engine.Move move = Engine.MoveUtil.Parse(board, moveDescription);
            if (!Engine.MoveUtil.IsLegal(move, board)) { throw new ArgumentOutOfRangeException("move", string.Format("{0} is not a legal move from position {1}", move.Description(), board.FENCurrent)); }

            board.MoveApply(move);
            dbgame.Moves.Add(new Sql.Move() { GameId = gameId, MoveNumber = moveNumber, Player = player, Value = move });

            _context.SaveChanges();

            var result = GameInfo.CreateFromDb(dbgame);
            return result;
        }

        public static class Utils
        {
            public static Engine.Board DbGame2Board(Sql.Game dbGame)
            {
                Engine.Board board = new Engine.Board();

                foreach (var dbMove in dbGame.Moves)
                {
                    Engine.Move move = dbMove.Value;
                    if(!Engine.MoveUtil.IsLegal(move, board))
                    {
                        //TODO: better exception here.
                        throw new Exception("");
                    }
                    board.MoveApply(move);
                }
                return board;
            }
        }

    }
}
