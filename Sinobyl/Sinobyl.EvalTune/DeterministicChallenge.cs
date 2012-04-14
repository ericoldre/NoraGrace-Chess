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
        public static bool Challenge(ChessEvalSettings champion, ChessEvalSettings challenger, IEnumerable<ChessPGN> startingPositions, string EventName, StreamWriter pgnWriter)
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
                ChessEval championEval = new ChessEval(champion);
                ChessEval challengerEval = new ChessEval(challenger);

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var result = Match(championEval, challengerEval, new ChessFEN(pgn.StartingPosition), pgn.Moves);
                stopwatch.Stop();

                result.Headers.Add(new ChessPGNHeader("White", "Champion"));
                result.Headers.Add(new ChessPGNHeader("Black", "Challenger"));
                result.Headers.Add(new ChessPGNHeader("Event", EventName));
                result.Headers.Add(new ChessPGNHeader("Seconds", (stopwatch.ElapsedMilliseconds / 1000).ToString()));

                if (pgnWriter != null)
                {
                    string sPgn = result.ToString();
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
                            //Console.WriteLine("Champ wins as white by {0}", reason.ToString());
                            break;
                        case ChessResult.BlackWins:
                            challengerWins++;
                            //Console.WriteLine("Challenger wins as black by {0}", reason.ToString());
                            break;
                        default:
                            draws++;
                            //Console.WriteLine("draw by {0}", reason.ToString());
                            break;
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
                    string sPgn = result.ToString();
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
                            challengerWins++;
                            //Console.WriteLine("Challenger wins as white by {0}", reason.ToString());
                            break;
                        case ChessResult.BlackWins:
                            champWins++;
                            //Console.WriteLine("Champ wins as black by {0}", reason.ToString());
                            break;
                        default:
                            draws++;
                            //Console.WriteLine("draw by {0}", reason.ToString());
                            break;
                    }
                }

                //Console.WriteLine("Ending Games");

            });

            Console.WriteLine("Champ:{0} Challenger:{1} Draws:{2}", champWins, challengerWins, draws);

            return challengerWins > champWins;

        }

        public static ChessPGN Match(ChessEval white, ChessEval black, ChessFEN startingPosition, IEnumerable<ChessMove> initalMoves)
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
            foreach (var move in initalMoves)
            {
                board.MoveApply(move);
                gameMoves.Add(move);
            }

            while (gameResult == null)
            {
                ChessEval eval = board.WhosTurn == ChessPlayer.White ? white : black;
                ChessTrans trans = board.WhosTurn == ChessPlayer.White ? whiteTrans : blackTrans;

                ChessSearch.Args args = new ChessSearch.Args();
                args.GameStartPosition = startingPosition;
                args.GameMoves = new ChessMoves(gameMoves.ToArray());
                args.TransTable = trans;
                args.Eval = eval;
                args.MaxNodes = 10000;

                ChessSearch search = new ChessSearch(args);
                var searchResult = search.Search();
                trans.AgeEntries(2);
                var bestMove = searchResult.PrincipleVariation[0];
                gameMoves.Add(bestMove);

                if (bestMove.IsLegal(board))
                {
                    board.MoveApply(bestMove);    
                    gameResult = BoardResult(board, out reason);
                }
                else
                {
                    reason = ChessResultReason.IllegalMove;
                    gameResult = board.WhosTurn == ChessPlayer.White ? ChessResult.BlackWins : ChessResult.WhiteWins;
                }

            }

            ChessPGN retval = new ChessPGN(new ChessPGNHeaders(), gameMoves, gameResult, new List<ChessPGNComment>(), reason);
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
