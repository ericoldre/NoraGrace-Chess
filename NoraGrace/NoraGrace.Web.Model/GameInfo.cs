using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NoraGrace.Engine;

namespace NoraGrace.Web.Model
{
    public class GameInfo
    {

        #region properties

        public int GameId { get; private set; }
        public string StartingFEN { get; private set; }
        public string FEN { get; private set; }
        public string Result { get; private set; }
        public string ResultReason { get; private set; }
        public PlyInfo[] MoveHistory { get; private set; }
        public MoveInfo[] LegalMoves { get; private set; }
        public PositionInfo[] Positions { get; private set; }

        #endregion

        public static GameInfo CreateFromDb(Sql.Game game)
        {
            GameInfo retval = CreateFromData(game.GameId, game.Moves.Select(m => m.Value));
            return retval;
        }

        public static GameInfo CreateFromData(int gameId, IEnumerable<Engine.Move> moves)
        {
            Engine.Board board = new Engine.Board();
            List<PlyInfo> moveHist = new List<PlyInfo>();
            foreach(var move in moves)
            {
                moveHist.Add(PlyInfo.Create(board, move));
                board.MoveApply(move);
            }

            GameInfo retval = new GameInfo()
            {
                GameId = gameId,
                StartingFEN = Engine.FEN.FENStart,
                FEN = board.FENCurrent.ToString(),
                MoveHistory = moveHist.ToArray(),
                LegalMoves = MoveUtil.GenMovesLegal(board).Select(m => MoveInfo.Create(board, m)).ToArray(),
                Positions = PositionInfo.CreateSet(board)
            };

            return retval;
        }

    }

    public class PlyInfo
    {
        public int Ply { get; set; }
        public int MoveNumber { get; set; }
        public Engine.Player Player { get; set; }
        public MoveInfo Move { get; set; }

        public static PlyInfo Create(Engine.Board board, Engine.Move move)
        {
            return new PlyInfo()
            {
                Ply = board.FullMoveCount * 2 + board.WhosTurn == Engine.Player.Black ? 1 : 0,
                Player = board.WhosTurn,
                MoveNumber = board.FullMoveCount,
                Move = MoveInfo.Create(board, move)
            };
        }

        public static bool IsValid(PlyInfo plyInfo, Engine.Board board)
        {
            return plyInfo.Ply == (board.FullMoveCount * 2 + board.WhosTurn == Engine.Player.Black ? 1 : 0)
                && plyInfo.Player == board.WhosTurn
                && plyInfo.MoveNumber == board.FullMoveCount
                && MoveInfo.IsValid(plyInfo.Move, board);
        }
    }

    public class MoveInfo
    {
        public string Description { get; private set; }
        public string From { get; private set; }
        public string To { get; private set; }
        public string Promote { get; private set; }

        public static MoveInfo Create(Engine.Board board, Engine.Move move)
        {
            return new MoveInfo()
            {
                From = move.From().Name(),
                To = move.To().Name(),
                Promote = move.IsPromotion() ? move.Promote().ToPieceType().ToString() : null,
                Description = move.Description(board)
            };
        }

        public static bool IsValid(MoveInfo moveInfo, Engine.Board board)
        {
            var move = MoveUtil.Parse(board, moveInfo.Description);
            if (!move.IsLegal(board)) { return false; }
            return moveInfo.From == move.From().Name()
                && moveInfo.To == move.To().Name()
                && moveInfo.Promote == (move.IsPromotion() ? move.Promote().ToPieceType().ToString() : null);
        }
    }

    public class PositionInfo
    {
        public string Position { get; private set; }
        public string Piece { get; private set; }
        public string Color { get; private set; }

        public static PositionInfo Create(Board board, Engine.Position position)
        {
            return new PositionInfo()
            {
                Position = position.Name(),
                Piece = board.PieceAt(position) == Engine.Piece.EMPTY ? null : board.PieceAt(position).ToPieceType().ToString(),
                Color = board.PieceAt(position) == Engine.Piece.EMPTY ? null : board.PieceAt(position).PieceToPlayer().ToString()
            };
        }

        public static PositionInfo[] CreateSet(Board board)
        {
            return Engine.PositionUtil.AllPositions.Select(p => Create(board, p)).ToArray();
        }

        public static bool IsValid(PositionInfo info, Board board)
        {
            var position = PositionUtil.Parse(info.Position);
            return info.Position == position.Name()
                && info.Piece == (board.PieceAt(position) == Engine.Piece.EMPTY ? null : board.PieceAt(position).ToPieceType().ToString())
                && info.Color == (board.PieceAt(position) == Engine.Piece.EMPTY ? null : board.PieceAt(position).PieceToPlayer().ToString());
        }

        public static bool IsValid(PositionInfo[] infos, Board board)
        {
            return infos.All(inf => IsValid(inf, board)) //where each individual position is valid
                && infos.Select(inf => inf.Position).GroupBy(p => p).All(g => g.Count() == 1); //where all positions are distinct
        }
    }
}
