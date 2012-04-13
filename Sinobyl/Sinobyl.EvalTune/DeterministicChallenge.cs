using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinobyl.Engine;
using System.Threading.Tasks;

namespace Sinobyl.EvalTune
{
    public class DeterministicChallenge
    {
        public static ChessEvalSettings Challenge(ChessEvalSettings champion, ChessEvalSettings challenger, IEnumerable<ChessPGN> startingPositions)
        {
            int champWins = 0;
            int challengerWins = 0;
            int draws = 0;

            object winLock = new object();
            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = 5;
            Parallel.ForEach(startingPositions, options, pgn =>
            {
                Console.WriteLine("Starting Games");
                ChessEval championEval = new ChessEval(champion);
                ChessEval challengerEval = new ChessEval(challenger);
                ChessResultReason reason = ChessResultReason.Unknown;
                var result = Match(championEval, challengerEval, pgn, out reason);
                lock (winLock)
                {
                    switch (result)
                    {
                        case ChessResult.WhiteWins:
                            champWins++;
                            Console.WriteLine("Champ wins as white by {0}", reason.ToString());
                            break;
                        case ChessResult.BlackWins:
                            challengerWins++;
                            Console.WriteLine("Challenger wins as black by {0}", reason.ToString());
                            break;
                        default:
                            draws++;
                            Console.WriteLine("draw by {0}", reason.ToString());
                            break;
                    }
                }
                result = Match(challengerEval, championEval, pgn, out reason);
                lock (winLock)
                {
                    switch (result)
                    {
                        case ChessResult.WhiteWins:
                            challengerWins++;
                            Console.WriteLine("Challenger wins as white by {0}", reason.ToString());
                            break;
                        case ChessResult.BlackWins:
                            champWins++;
                            Console.WriteLine("Champ wins as black by {0}", reason.ToString());
                            break;
                        default:
                            draws++;
                            Console.WriteLine("draw by {0}", reason.ToString());
                            break;
                    }
                }

                Console.WriteLine("Ending Games");

            });

            Console.WriteLine("Champ:{0} Challenger:{1} Draws:{2}", champWins, challengerWins, draws);
            if (challengerWins > champWins)
            {
                return challenger;
            }
            else
            {
                return champion;
            }
        }

        public static ChessResult Match(ChessEval white, ChessEval black, ChessPGN startingPosition, out ChessResultReason reason)
        {
            ChessResult? retval = null;
            reason = ChessResultReason.Unknown;
            //ChessResult? adjudicatedResult = null;
           // int adjudicatedResultDuration = 0;
            ChessTrans whiteTrans = new ChessTrans();
            ChessTrans blackTrans = new ChessTrans();

            ChessMoves gameMoves = new ChessMoves();
            //setup init position
            ChessBoard board = new ChessBoard();
            foreach (var move in startingPosition.Moves)
            {
                board.MoveApply(move);
                gameMoves.Add(move);
            }

            while (retval == null)
            {
                ChessEval eval = board.WhosTurn == ChessPlayer.White ? white : black;
                ChessTrans trans = board.WhosTurn == ChessPlayer.White ? whiteTrans : blackTrans;

                ChessSearch.Args args = new ChessSearch.Args();
                args.GameStartPosition = new ChessFEN(startingPosition.StartingPosition);
                args.GameMoves = new ChessMoves(gameMoves.ToArray());
                args.TransTable = trans;
                args.Eval = eval;
                args.MaxNodes = 4000;

                ChessSearch search = new ChessSearch(args);
                var searchResult = search.Search();
                trans.AgeEntries(2);
                var bestMove = searchResult.PrincipleVariation[0];

                if (!bestMove.IsLegal(board))
                {
                    reason = ChessResultReason.Unknown;
                    retval = board.WhosTurn == ChessPlayer.White ? ChessResult.BlackWins : ChessResult.WhiteWins;
                    return retval.Value;
                }

                board.MoveApply(bestMove);
                gameMoves.Add(bestMove);

                retval = BoardResult(board, out reason);
            }
            return retval.Value;
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
