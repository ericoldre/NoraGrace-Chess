using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinobyl.Engine
{

    public interface IChessGamePlayer
    {
        string Name { get; }
        ChessMove Move(FEN gameStartPosition, IEnumerable<ChessMove> movesAlreadyPlayed, TimeControl timeControls, TimeSpan timeLeft, out string Comment);
    }

    public class ChessMatchResults: List<PGN>
    {
        
    }
    public class ChessMatchProgress
    {
        public ChessMatchResults Results { get; set; }
        public PGN Game { get; set; }
        public TimeSpan GameTime { get; set; }
    }
    public class ChessMatch
    {
        

        public static ChessMatchResults RunParallelRoundRobinMatch(IEnumerable<Func<IChessGamePlayer>> competitors, IEnumerable<PGN> startingPositions, Action<ChessMatchProgress> onGameCompleted = null, bool isGauntlet = false)
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
            options.MaxDegreeOfParallelism = 12;
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
                        PGN game = Game(playerWhite, playerBlack, new FEN(startingPGN.StartingPosition), startingPGN.Moves, null);
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



        public static ChessMatchResults RunParallelMatch(Func<IChessGamePlayer> createPlayer1, Func<IChessGamePlayer> createPlayer2, IEnumerable<PGN> startingPositions, Action<ChessMatchProgress> onGameCompleted = null)
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
                    PGN game = Game(playerWhite, playerBlack, new FEN(startingPGN.StartingPosition), startingPGN.Moves, null);
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

        public static PGN Game(IChessGamePlayer white, IChessGamePlayer black, FEN gameStartPosition, IEnumerable<ChessMove> initalMoves, TimeControl timeControl)
        {
            GameResult? gameResult = null;
            GameResultReason reason = GameResultReason.Unknown;


            List<ChessMove> gameMoves = new List<ChessMove>();
            //setup init position
            Board board = new Board(gameStartPosition);
            Dictionary<int, string> comments = new Dictionary<int, string>();
            foreach (var move in initalMoves)
            {
                board.MoveApply(move);
                gameMoves.Add(move);
            }

            while (gameResult == null)
            {
                IChessGamePlayer player = board.WhosTurn == Player.White ? white : black;

                string moveComment = null;
                var bestMove = player.Move(gameStartPosition, gameMoves.ToArray(), timeControl, new TimeSpan(1,0,0), out moveComment);
                if (bestMove == ChessMove.EMPTY)
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
