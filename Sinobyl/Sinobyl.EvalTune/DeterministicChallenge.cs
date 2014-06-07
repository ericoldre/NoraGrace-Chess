using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinobyl.Engine;
using System.Threading.Tasks;
using System.IO;
namespace Sinobyl.EvalTune
{
    public class DeterministicChallenge
    {
        public class ChessMatchProgress
        {
            public List<PGN> Results { get; set; }
            public PGN Game { get; set; }
            public TimeSpan GameTime { get; set; }
        }

        public static List<PGN> RunParallelRoundRobinMatch(IEnumerable<Func<DeterministicPlayer>> competitors, IEnumerable<PGN> startingPositions, TimeControlNodes timeControl, Action<ChessMatchProgress> onGameCompleted = null, bool isGauntlet = false)
        {

            List<PGN> retval = new List<PGN>();

            Func<DeterministicPlayer> gauntletPlayer = null;
            if (isGauntlet)
            {
                foreach (var player in competitors)
                {
                    gauntletPlayer = player;
                    break;
                }
            }

            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = 10;
#if DEBUG
            options.MaxDegreeOfParallelism = 1;
#endif
      
            Parallel.ForEach(startingPositions, options, startingPGN =>
            {
                foreach (var fWhitePlayer in competitors)
                {
                    foreach (var fBlackPlayer in competitors)
                    {
                        if (fWhitePlayer == fBlackPlayer) { continue; }
                        if (gauntletPlayer != null && gauntletPlayer != fWhitePlayer && gauntletPlayer != fBlackPlayer) { continue; }
                        DeterministicPlayer playerWhite = fWhitePlayer();
                        DeterministicPlayer playerBlack = fBlackPlayer();

                        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                        PGN game = Game(playerWhite, playerBlack, new FEN(startingPGN.StartingPosition), startingPGN.Moves, timeControl);
                        stopwatch.Stop();

                        lock (retval)
                        {
                            retval.Add(game);
                            if (onGameCompleted != null)
                            {
                                onGameCompleted(new ChessMatchProgress() { Results = retval, Game = game, GameTime = stopwatch.Elapsed });
                            }
                        }
                    }

                }
            });

            //Console.WriteLine("\nChamp:{0} Challenger:{1} Draws:{2}", champWins, challengerWins, draws);

            return retval;

        }


        public static PGN Game(DeterministicPlayer white, DeterministicPlayer black, FEN gameStartPosition, IEnumerable<Move> initalMoves, TimeControlNodes timeControl)
        {
            GameResult? gameResult = null;
            GameResultReason reason = GameResultReason.Unknown;


            List<Move> gameMoves = new List<Move>();
            //setup init position
            Board board = new Board(gameStartPosition);
            Dictionary<int, string> comments = new Dictionary<int, string>();
            foreach (var move in initalMoves)
            {
                board.MoveApply(move);
                gameMoves.Add(move);
            }

            int whiteClock = timeControl.InitialAmount;
            int blackClock = timeControl.InitialAmount;
            
            while (gameResult == null)
            {
                DeterministicPlayer player = board.WhosTurn == Player.White ? white : black;
                int playerClock = board.WhosTurn == Player.White ? whiteClock : blackClock;

                string moveComment = null;
                int nodesUsed = 0;

                var bestMove = player.Move(gameStartPosition, gameMoves.ToArray(), timeControl, playerClock, out moveComment, out nodesUsed);

                int playerClockNew = timeControl.CalcNewTimeLeft(playerClock, nodesUsed, board.FullMoveCount);
                if (board.WhosTurn == Player.White) { whiteClock = playerClockNew; } else { blackClock = playerClockNew; }

                if (bestMove == Move.EMPTY)
                {
                    gameResult = board.WhosTurn == Player.White ? GameResult.BlackWins : GameResult.WhiteWins;
                    reason = GameResultReason.Resign;
                    break;
                }

                gameMoves.Add(bestMove);

                if (bestMove.IsLegal(board))
                {
                    board.MoveApply(bestMove);
                    if (!string.IsNullOrWhiteSpace(moveComment))
                    {
                        comments.Add(board.HistoryCount - 1, moveComment);
                    }
                    gameResult = BoardResult(board, out reason);
                }
                else
                {
                    reason = GameResultReason.IllegalMove;
                    gameResult = board.WhosTurn == Player.White ? GameResult.BlackWins : GameResult.WhiteWins;
                }

            }

            PGN retval = new PGN(new ChessPGNHeaders(), gameMoves, gameResult, comments, reason);
            retval.Headers.Add("White", white.Name);
            retval.Headers.Add("Black", black.Name);
            return retval;
        }

        private static GameResult? BoardResult(Board _board, out GameResultReason _resultReason)
        {
            GameResult? _result = null;
            _resultReason = GameResultReason.Unknown;
            if (_board.IsMate())
            {
                _result = _board.WhosTurn == Player.White ? GameResult.BlackWins : GameResult.WhiteWins;
                _resultReason = GameResultReason.Checkmate;
            }
            else if (_board.IsDrawBy50MoveRule())
            {
                _result = GameResult.Draw;
                _resultReason = GameResultReason.FiftyMoveRule;
            }
            else if (_board.IsDrawByRepetition())
            {
                _result = GameResult.Draw;
                _resultReason = GameResultReason.Repetition;
            }
            else if (_board.IsDrawByStalemate())
            {
                _result = GameResult.Draw;
                _resultReason = GameResultReason.Stalemate;
            }
            return _result;
        }
    }
}
