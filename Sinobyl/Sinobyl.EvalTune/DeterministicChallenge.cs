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
        public static bool Challenge(Func<IChessEval> champion, Func<IChessEval> challenger, IEnumerable<ChessPGN> startingPositions, string EventName, StreamWriter pgnWriter)
        {
            int champWins = 0;
            int challengerWins = 0;
            int draws = 0;

            object winLock = new object();
            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = 6;
            Parallel.ForEach(startingPositions, options, pgn =>
            {
                //Console.WriteLine("Starting Games");
                IChessEval championEval = champion();
                IChessEval challengerEval = challenger();

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var result = Match(championEval, challengerEval, new ChessFEN(pgn.StartingPosition), pgn.Moves);
                stopwatch.Stop();

                result.Headers.Add(new ChessPGNHeader("White", "Champion"));
                result.Headers.Add(new ChessPGNHeader("Black", "Challenger"));
                result.Headers.Add(new ChessPGNHeader("Event", EventName));
                result.Headers.Add(new ChessPGNHeader("Seconds", (stopwatch.ElapsedMilliseconds / 1000).ToString()));

                if (pgnWriter != null)
                {
                    string sPgn = result.ToString(90);
                    lock (pgnWriter)
                    {
                        pgnWriter.Write(sPgn);
                        pgnWriter.WriteLine();
                        pgnWriter.WriteLine();
                    }
                }

                lock (winLock)
                {
                    switch (result.Result)
                    {
                        case ChessResult.WhiteWins:
                            champWins++;
                            Console.Write("0");
                            //Console.WriteLine("Champ wins as white by {0}", reason.ToString());
                            break;
                        case ChessResult.BlackWins:
                            challengerWins++;
                            Console.Write("1");
                            //Console.WriteLine("Challenger wins as black by {0}", reason.ToString());
                            break;
                        default:
                            Console.Write("-");
                            draws++;
                            //Console.WriteLine("draw by {0}", reason.ToString());
                            break;
                    }

                    if ((champWins + challengerWins + draws) % 100 == 0)
                    {
                        float totalgames = (champWins + challengerWins + draws);
                        float champPct = ((float)champWins) / totalgames;
                        float challengerPct = ((float)challengerWins) / totalgames;
                        float drawPct = ((float)draws) / totalgames;
                        var champPctString = champPct.ToString("#0.##%", System.Globalization.CultureInfo.InvariantCulture);
                        var challengerPctString = challengerPct.ToString("#0.##%", System.Globalization.CultureInfo.InvariantCulture);
                        var drawPctString = drawPct.ToString("#0.##%", System.Globalization.CultureInfo.InvariantCulture);
                        //.ToString("#0.##%", CultureInfo.InvariantCulture)
                        Console.Write("[Games:{3} Champ:{0} Challenger:{1} Draws:{2}]", champPctString, challengerPctString, drawPctString, totalgames);
                    }
                }

                stopwatch = System.Diagnostics.Stopwatch.StartNew();
                result = Match(challengerEval, championEval, new ChessFEN(pgn.StartingPosition), pgn.Moves);
                stopwatch.Stop();

                result.Headers.Add(new ChessPGNHeader("White", "Challenger"));
                result.Headers.Add(new ChessPGNHeader("Black", "Champion"));
                result.Headers.Add(new ChessPGNHeader("Event", EventName));
                result.Headers.Add(new ChessPGNHeader("Seconds", (stopwatch.ElapsedMilliseconds / 1000).ToString()));

                if (pgnWriter != null)
                {
                    string sPgn = result.ToString(90);
                    lock (pgnWriter)
                    {
                        pgnWriter.Write(sPgn);
                        pgnWriter.WriteLine();
                        pgnWriter.WriteLine();
                    }
                }

                lock (winLock)
                {
                    switch (result.Result)
                    {
                        case ChessResult.WhiteWins:
                            Console.Write("1");
                            challengerWins++;
                            //Console.WriteLine("Challenger wins as white by {0}", reason.ToString());
                            break;
                        case ChessResult.BlackWins:
                            Console.Write("0");
                            champWins++;
                            //Console.WriteLine("Champ wins as black by {0}", reason.ToString());
                            break;
                        default:
                            Console.Write("-");
                            draws++;
                            //Console.WriteLine("draw by {0}", reason.ToString());
                            break;
                    }
                    if ((champWins + challengerWins + draws) % 100 == 0)
                    {
                        float totalgames = (champWins + challengerWins + draws);
                        float champPct = ((float)champWins) / totalgames;
                        float challengerPct = ((float)challengerWins) / totalgames;
                        float drawPct = ((float)draws) / totalgames;
                        var champPctString = champPct.ToString("#0.##%", System.Globalization.CultureInfo.InvariantCulture);
                        var challengerPctString = challengerPct.ToString("#0.##%", System.Globalization.CultureInfo.InvariantCulture);
                        var drawPctString = drawPct.ToString("#0.##%", System.Globalization.CultureInfo.InvariantCulture);
                        //.ToString("#0.##%", CultureInfo.InvariantCulture)
                        Console.Write("[Games:{3} Champ:{0} Challenger:{1} Draws:{2}]", champPctString, challengerPctString, drawPctString, totalgames);
                    }
                }

                //Console.WriteLine("Ending Games");

            });

            Console.WriteLine("\nChamp:{0} Challenger:{1} Draws:{2}", champWins, challengerWins, draws);

            return challengerWins > champWins;

        }

        public static ChessPGN Match(IChessEval white, IChessEval black, ChessFEN startingPosition, IEnumerable<ChessMove> initalMoves)
        {
            ChessResult? gameResult = null;
            ChessResultReason reason = ChessResultReason.Unknown;
            //ChessResult? adjudicatedResult = null;
           // int adjudicatedResultDuration = 0;
            ChessTrans whiteTrans = new ChessTrans();
            ChessTrans blackTrans = new ChessTrans();

            ChessMoves gameMoves = new ChessMoves();
            //setup init position
            ChessBoard board = new ChessBoard(startingPosition);
            List<ChessPGNComment> comments = new List<ChessPGNComment>();
            foreach (var move in initalMoves)
            {
                board.MoveApply(move);
                gameMoves.Add(move);
            }

            while (gameResult == null)
            {
                IChessEval eval = board.WhosTurn == ChessPlayer.White ? white : black;
                ChessTrans trans = board.WhosTurn == ChessPlayer.White ? whiteTrans : blackTrans;

                ChessSearch.Args args = new ChessSearch.Args();
                args.GameStartPosition = startingPosition;
                args.GameMoves = new ChessMoves(gameMoves.ToArray());
                args.TransTable = trans;
                args.Eval = eval;
                args.MaxNodes = 1000;

                ChessSearch search = new ChessSearch(args);
                var searchResult = search.Search();
                trans.AgeEntries(2);
                var bestMove = searchResult.PrincipleVariation[0];
                gameMoves.Add(bestMove);

                if (bestMove.IsLegal(board))
                {
                    string comment = string.Format("Score:{2} Depth:{1} PV:{0}", new ChessMoves(searchResult.PrincipleVariation).ToString(), searchResult.Depth, searchResult.Score);

                    board.MoveApply(bestMove);

                    comments.Add(new ChessPGNComment(board.HistoryCount, comment));
                    gameResult = BoardResult(board, out reason);
                }
                else
                {
                    reason = ChessResultReason.IllegalMove;
                    gameResult = board.WhosTurn == ChessPlayer.White ? ChessResult.BlackWins : ChessResult.WhiteWins;
                }

            }

            ChessPGN retval = new ChessPGN(new ChessPGNHeaders(), gameMoves, gameResult, comments, reason);
            return retval;
        }

        public static ChessResult? BoardResult(ChessBoard _board, out ChessResultReason _resultReason)
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
