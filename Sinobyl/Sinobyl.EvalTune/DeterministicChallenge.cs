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
        public static ChessMatchResults RunParallelRoundRobinMatch(IEnumerable<Func<DeterministicPlayer>> competitors, IEnumerable<ChessPGN> startingPositions, ChessTimeControlNodes timeControl, Action<ChessMatchProgress> onGameCompleted = null, bool isGauntlet = false)
        {

            ChessMatchResults retval = new ChessMatchResults();

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
                        ChessPGN game = Game(playerWhite, playerBlack, new ChessFEN(startingPGN.StartingPosition), startingPGN.Moves, timeControl);
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


        public static ChessPGN Game(DeterministicPlayer white, DeterministicPlayer black, ChessFEN gameStartPosition, IEnumerable<ChessMove> initalMoves, ChessTimeControlNodes timeControl)
        {
            ChessResult? gameResult = null;
            ChessResultReason reason = ChessResultReason.Unknown;

            
            ChessMoves gameMoves = new ChessMoves();
            //setup init position
            ChessBoard board = new ChessBoard(gameStartPosition);
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
                DeterministicPlayer player = board.WhosTurn == ChessPlayer.White ? white : black;
                int playerClock = board.WhosTurn == ChessPlayer.White ? whiteClock : blackClock;

                string moveComment = null;
                int nodesUsed = 0;

                var bestMove = player.Move(gameStartPosition, gameMoves.ToArray(), timeControl, playerClock, out moveComment, out nodesUsed);

                int playerClockNew = timeControl.CalcNewTimeLeft(playerClock, nodesUsed, board.FullMoveCount);
                if (board.WhosTurn == ChessPlayer.White) { whiteClock = playerClockNew; } else { blackClock = playerClockNew; }

                if (bestMove == ChessMove.EMPTY)
                {
                    gameResult = board.WhosTurn == ChessPlayer.White ? ChessResult.BlackWins : ChessResult.WhiteWins;
                    reason = ChessResultReason.Resign;
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
                    reason = ChessResultReason.IllegalMove;
                    gameResult = board.WhosTurn == ChessPlayer.White ? ChessResult.BlackWins : ChessResult.WhiteWins;
                }

            }

            ChessPGN retval = new ChessPGN(new ChessPGNHeaders(), gameMoves, gameResult, comments, reason);
            retval.Headers.Add("White", white.Name);
            retval.Headers.Add("Black", black.Name);
            return retval;
        }

        private static ChessResult? BoardResult(ChessBoard _board, out ChessResultReason _resultReason)
        {
            ChessResult? _result = null;
            _resultReason = ChessResultReason.Unknown;
            if (_board.IsMate())
            {
                _result = _board.WhosTurn == ChessPlayer.White ? ChessResult.BlackWins : ChessResult.WhiteWins;
                _resultReason = ChessResultReason.Checkmate;
            }
            else if (_board.IsDrawBy50MoveRule())
            {
                _result = ChessResult.Draw;
                _resultReason = ChessResultReason.FiftyMoveRule;
            }
            else if (_board.IsDrawByRepetition())
            {
                _result = ChessResult.Draw;
                _resultReason = ChessResultReason.Repetition;
            }
            else if (_board.IsDrawByStalemate())
            {
                _result = ChessResult.Draw;
                _resultReason = ChessResultReason.Stalemate;
            }
            return _result;
        }
    }
}
