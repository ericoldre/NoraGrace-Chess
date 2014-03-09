using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace Sinobyl.Engine
{

    public interface IChessGamePlayer
    {
        string Name { get; }
        ChessMove Move(ChessFEN gameStartPosition, IEnumerable<ChessMove> movesAlreadyPlayed, ChessTimeControl timeControls, TimeSpan timeLeft, out string Comment);
    }

    public class ChessMatchResults: List<ChessPGN>
    {
        
    }
    public class ChessMatchProgress
    {
        public ChessMatchResults Results { get; set; }
        public ChessPGN Game { get; set; }
        public TimeSpan GameTime { get; set; }
    }
    public class ChessMatch
    {
        

        public static ChessMatchResults RunParallelRoundRobinMatch(IEnumerable<Func<IChessGamePlayer>> competitors, IEnumerable<ChessPGN> startingPositions, Action<ChessMatchProgress> onGameCompleted = null, bool isGauntlet = false)
        {
            ChessMatchResults retval = new ChessMatchResults();

            Func<IChessGamePlayer> gauntletPlayer = null;
            if (isGauntlet)
            {
                foreach (var player in competitors)
                {
                    gauntletPlayer = player;
                    break;
                }
            }

            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = 6;
            Parallel.ForEach(startingPositions, options, startingPGN =>
            {
                foreach (var fWhitePlayer in competitors)
                {
                    foreach (var fBlackPlayer in competitors)
                    {
                        if (fWhitePlayer == fBlackPlayer) { continue; }
                        if (gauntletPlayer != null && gauntletPlayer != fWhitePlayer && gauntletPlayer != fBlackPlayer) { continue; }
                        IChessGamePlayer playerWhite = fWhitePlayer();
                        IChessGamePlayer playerBlack = fBlackPlayer();

                        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                        ChessPGN game = Game(playerWhite, playerBlack, new ChessFEN(startingPGN.StartingPosition), startingPGN.Moves, null);
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



        public static ChessMatchResults RunParallelMatch(Func<IChessGamePlayer> createPlayer1, Func<IChessGamePlayer> createPlayer2, IEnumerable<ChessPGN> startingPositions, Action<ChessMatchProgress> onGameCompleted = null)
        {
            ChessMatchResults retval = new ChessMatchResults();


            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = 6;
            Parallel.ForEach(startingPositions, options, startingPGN =>
            {


                foreach(bool player1IsWhite in new bool[]{true,false})
                {
                    IChessGamePlayer player1 = createPlayer1();
                    IChessGamePlayer player2 = createPlayer2();

                    IChessGamePlayer playerWhite = player1IsWhite?player1:player2;
                    IChessGamePlayer playerBlack = player1IsWhite?player2:player1;

                    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                    ChessPGN game = Game(playerWhite, playerBlack, new ChessFEN(startingPGN.StartingPosition), startingPGN.Moves, null);
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
                

            });

            //Console.WriteLine("\nChamp:{0} Challenger:{1} Draws:{2}", champWins, challengerWins, draws);

            return retval;

        }

        public static ChessPGN Game(IChessGamePlayer white, IChessGamePlayer black, ChessFEN gameStartPosition, IEnumerable<ChessMove> initalMoves, ChessTimeControl timeControl)
        {
            ChessResult? gameResult = null;
            ChessResultReason reason = ChessResultReason.Unknown;


            ChessMoves gameMoves = new ChessMoves();
            //setup init position
            ChessBoard board = new ChessBoard(gameStartPosition);
            List<ChessPGNComment> comments = new List<ChessPGNComment>();
            foreach (var move in initalMoves)
            {
                board.MoveApply(move);
                gameMoves.Add(move);
            }

            while (gameResult == null)
            {
                IChessGamePlayer player = board.WhosTurn == ChessPlayer.White ? white : black;

                string moveComment = null;
                var bestMove = player.Move(gameStartPosition, gameMoves.ToArray(), timeControl, new TimeSpan(1,0,0), out moveComment);
                gameMoves.Add(bestMove);

                if (bestMove.IsLegal(board))
                {
                    board.MoveApply(bestMove);
                    if (!string.IsNullOrWhiteSpace(moveComment))
                    {
                        comments.Add(new ChessPGNComment(board.HistoryCount, moveComment));
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
