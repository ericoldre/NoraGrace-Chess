﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinobyl.Engine;

namespace Sinobyl.CommandLine
{
    class Perft
    {
        static ChessPGN pgn = ChessPGN.NextGame("1. e4 e5 2. Nf3 Nc6 3. Bb5 Nf6 4. O-O Nxe4 5. d4 Nd6 6. Bxc6 dxc6 7. dxe5 Nf5 8. Qxd8+ Kxd8 9. Nc3 Bd7 10. b3 h6 11. Bb2 Kc8 12. Rad1 b6 13. Ne2 c5 14. c4 Bc6 15. Nf4 Kb7 16. Nd5 Ne7 17. Rfe1 Rg8 18. Nf4 g5 19. Nh5 Rg6 20. Nf6 Bg7 21. Rd3 Bxf3 22. Rxf3 Bxf6 23. exf6 Nc6 24. Rd3 Rf8 25. Re4 Kc8 26. f4 gxf4 27. Rxf4 Re8 28. Bc3 Re2 29. Rf2 Re4 30. Rh3 a5 31. Rh5 a4 32. bxa4 Rxc4 33. Bd2 Rxa4 34. Rxh6 Rg8 35. Rh7 Rxa2 36. Rxf7 Ne5 37. Rg7 Rf8 38. h3 c4 39. Re7 Nd3 40. f7 Nxf2 41. Re8+ Kd7 42. Rxf8 Ke7 43. Rc8 Kxf7 44. Rxc7+ Ke6 45. Be3 Nd1 46. Bxb6 c3 47. h4 Ra6 48. Bd4 Ra4 49. Bxc3 Nxc3 50. Rxc3 Rxh4 51. Rf3 Rh5 52. Kf2 Rg5 53. Rf8 Ke5 1/2-1/2");
        static IChessEval eval = new ChessEval();

        public static void NodesToDepth(int depth)
        {
            ChessBoard board = new ChessBoard();
            ChessEval eval = new ChessEval();
            ChessMoves movesDone = new ChessMoves();

            int totalNodes = 0;
            TimeSpan totalTime = new TimeSpan(0);
            foreach (ChessMove move in pgn.Moves)
            {
                
                ChessSearch.Args args = new ChessSearch.Args();

                args.Eval = eval;
                args.GameStartPosition = new ChessFEN(ChessFEN.FENStart);
                args.GameMoves = movesDone;
                args.MaxDepth = depth;
                args.TransTable = new ChessTrans();

                ChessSearch search = new ChessSearch(args);
                search.OnProgress += (s, e) => {Console.WriteLine("\t{0}", new ChessMoves(e.PrincipleVariation).ToString(board,true));};
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var searchResults = search.Search();
                stopwatch.Stop();
                totalTime += stopwatch.Elapsed;


                totalNodes += searchResults.Nodes;

                board.MoveApply(move);
                movesDone.Add(move);

                //double nodesPerSecond = ((double)totalNodes / (double)totalTime.TotalMilliseconds) * 1000;
                Console.WriteLine("Position {0} of {1}, {2} Nodes {3} Avg Nodes {4} Nodes/Sec",
                    movesDone.Count,
                    pgn.Moves.Count,
                    searchResults.Nodes,
                    totalNodes / movesDone.Count,
                    (totalNodes / totalTime.TotalMilliseconds) * 1000);
            }

        }

        #region PerftCalls
        public static void PerftSuite(int nodesPerPosition, bool doEval, bool doMoveSort)
        {
            //[White "Garry Kasparov"]
            //[Black "Vladimir Kramnik"]
            //var pgn = ChessPGN.NextGame("1. e4 e5 2. Nf3 Nc6 3. Bb5 Nf6 4. O-O Nxe4 5. d4 Nd6 6. Bxc6 dxc6 7. dxe5 Nf5 8. Qxd8+ Kxd8 9. Nc3 Bd7 10. b3 h6 11. Bb2 Kc8 12. Rad1 b6 13. Ne2 c5 14. c4 Bc6 15. Nf4 Kb7 16. Nd5 Ne7 17. Rfe1 Rg8 18. Nf4 g5 19. Nh5 Rg6 20. Nf6 Bg7 21. Rd3 Bxf3 22. Rxf3 Bxf6 23. exf6 Nc6 24. Rd3 Rf8 25. Re4 Kc8 26. f4 gxf4 27. Rxf4 Re8 28. Bc3 Re2 29. Rf2 Re4 30. Rh3 a5 31. Rh5 a4 32. bxa4 Rxc4 33. Bd2 Rxa4 34. Rxh6 Rg8 35. Rh7 Rxa2 36. Rxf7 Ne5 37. Rg7 Rf8 38. h3 c4 39. Re7 Nd3 40. f7 Nxf2 41. Re8+ Kd7 42. Rxf8 Ke7 43. Rc8 Kxf7 44. Rxc7+ Ke6 45. Be3 Nd1 46. Bxb6 c3 47. h4 Ra6 48. Bd4 Ra4 49. Bxc3 Nxc3 50. Rxc3 Rxh4 51. Rf3 Rh5 52. Kf2 Rg5 53. Rf8 Ke5 1/2-1/2");

            ChessBoard board = new ChessBoard();
            long totalNodes=0;
            int positionCount = 0;
            TimeSpan totalTime = new TimeSpan(0);

            foreach (ChessMove move in pgn.Moves)
            {
                board.MoveApply(move);
                totalTime += PerftTest(board.FEN.ToString(), nodesPerPosition, doEval, doMoveSort);
                totalNodes += nodesPerPosition;
                positionCount++;
                double nodesPerSecond = ((double)totalNodes / (double)totalTime.TotalMilliseconds) * 1000;
                Console.WriteLine("Position {0} of {1}, {3}/Sec: {2}", positionCount, pgn.Moves.Count, (int)nodesPerSecond, doEval?"Eval":"Node");
            }


        }
        private static TimeSpan PerftTest(string fen, int nodeCount, bool doEval, bool doMoveSort)
        {

            
            ChessBoard board = new ChessBoard(fen);

            int nodesDone = 0;//start at -1 to skip root node
            //Console.WriteLine("fen: " + fen);
            int depth;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            for (depth = 2; nodesDone < nodeCount; depth++)
            {
                PerftSearch(board, depth, nodeCount, ref nodesDone, doEval, doMoveSort);
                //Console.WriteLine(string.Format("depth:{0} nodes:{1} milliseconds:{2}", depth, nodesDone, sw.ElapsedMilliseconds));
            }
            sw.Stop();
            //Console.WriteLine("Average nodes/second: {0}", ((double)nodesDone / (double)sw.ElapsedMilliseconds) * 1000);
            //Console.WriteLine("");
            return sw.Elapsed;
        }

        public static void PerftSearch(ChessBoard board, int depth_remaining, int nodeCount, ref int nodesDone, bool doEval, bool doMoveSort)
        {
            nodesDone++;
            if (doEval)
            {
                eval.EvalFor(board,ChessPlayer.White);
            }
            if (depth_remaining <= 0 || nodesDone >= nodeCount)
            {
                return;
            }

            List<ChessMove> moves = ChessMove.GenMoves(board);

            if (doMoveSort)
            {
                ChessMove.Comp moveOrderer = new ChessMove.Comp(board, new ChessMove(), true);
                moves.Sort(moveOrderer);
            }

            foreach (ChessMove move in moves)
            {
                board.MoveApply(move);

                if (!board.IsCheck(board.WhosTurn.PlayerOther()))
                {
                    PerftSearch(board, depth_remaining - 1, nodeCount, ref nodesDone, doEval, doMoveSort);
                }

                board.MoveUndo();
            }
        }
        #endregion
    }
}
