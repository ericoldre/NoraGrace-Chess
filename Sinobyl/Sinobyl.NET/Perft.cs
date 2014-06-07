using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinobyl.Engine;
using Sinobyl.Engine.Evaluation;
namespace Sinobyl.NET
{
    public static class Perft
    {
        static PGN pgn = PGN.NextGame("1. e4 e5 2. Nf3 Nc6 3. Bb5 Nf6 4. O-O Nxe4 5. d4 Nd6 6. Bxc6 dxc6 7. dxe5 Nf5 8. Qxd8+ Kxd8 9. Nc3 Bd7 10. b3 h6 11. Bb2 Kc8 12. Rad1 b6 13. Ne2 c5 14. c4 Bc6 15. Nf4 Kb7 16. Nd5 Ne7 17. Rfe1 Rg8 18. Nf4 g5 19. Nh5 Rg6 20. Nf6 Bg7 21. Rd3 Bxf3 22. Rxf3 Bxf6 23. exf6 Nc6 24. Rd3 Rf8 25. Re4 Kc8 26. f4 gxf4 27. Rxf4 Re8 28. Bc3 Re2 29. Rf2 Re4 30. Rh3 a5 31. Rh5 a4 32. bxa4 Rxc4 33. Bd2 Rxa4 34. Rxh6 Rg8 35. Rh7 Rxa2 36. Rxf7 Ne5 37. Rg7 Rf8 38. h3 c4 39. Re7 Nd3 40. f7 Nxf2 41. Re8+ Kd7 42. Rxf8 Ke7 43. Rc8 Kxf7 44. Rxc7+ Ke6 45. Be3 Nd1 46. Bxb6 c3 47. h4 Ra6 48. Bd4 Ra4 49. Bxc3 Nxc3 50. Rxc3 Rxh4 51. Rf3 Rh5 52. Kf2 Rg5 53. Rf8 Ke5 1/2-1/2");
        static Evaluator eval = new Evaluator();
        //static ChessEvalOld evalOld = new ChessEvalOld();

        public static void NodesToDepth(int depth)
        {
            Board board = new Board();
            Evaluator eval = new Evaluator();
            List<Move> movesDone = new List<Move>();
            TranspositionTable transTable = new TranspositionTable();
            int totalNodes = 0;
            TimeSpan totalTime = new TimeSpan(0);
            //var timeManager = new TimeManagerBasic() { TimeControl = ChessTimeControl.Blitz(100, 100), ClockEnd = DateTime.Now.AddDays(100) };
            var timeManager = new TimeManagerAdvanced() { TimeControl = TimeControl.Blitz(100, 100), AmountOnClock = TimeSpan.FromDays(100) };
            foreach (Move move in pgn.Moves)
            {
                
                Search.Args args = new Search.Args();

                args.Eval = eval;
                args.GameStartPosition = new FEN(FEN.FENStart);
                args.GameMoves = movesDone;
                args.MaxDepth = depth;
                args.TransTable = transTable;
                args.TimeManager = timeManager;
                Program.ConsoleWriteline(board.FENCurrent.ToString());

                Search search = new Search(args);
                search.ProgressReported += (s, e) => 
                {
                    Program.ConsoleWriteline(string.Format("\t{1} {0}", e.Progress.PrincipleVariation.Descriptions(board, true), e.Progress.Depth));
                };
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var searchResults = search.Start();
                stopwatch.Stop();
                totalTime += stopwatch.Elapsed;


                totalNodes += searchResults.Nodes;

                board.MoveApply(move);
                movesDone.Add(move);

                //double nodesPerSecond = ((double)totalNodes / (double)totalTime.TotalMilliseconds) * 1000;
                Program.ConsoleWriteline(string.Format("Position {0} of {1}, {2} Nodes {3} Avg Nodes",
                    movesDone.Count,
                    pgn.Moves.Count,
                    searchResults.Nodes,
                    totalNodes / movesDone.Count));
            }

            Program.ConsoleWriteline(string.Format("Total Time:{0}, Nodes/Sec {1}", totalTime, (totalNodes / totalTime.TotalMilliseconds) * 1000));

        }

        #region PerftCalls
        public static void PerftSuite(int nodesPerPosition, bool doEval, bool doMoveSort)
        {
            //[White "Garry Kasparov"]
            //[Black "Vladimir Kramnik"]
            //var pgn = ChessPGN.NextGame("1. e4 e5 2. Nf3 Nc6 3. Bb5 Nf6 4. O-O Nxe4 5. d4 Nd6 6. Bxc6 dxc6 7. dxe5 Nf5 8. Qxd8+ Kxd8 9. Nc3 Bd7 10. b3 h6 11. Bb2 Kc8 12. Rad1 b6 13. Ne2 c5 14. c4 Bc6 15. Nf4 Kb7 16. Nd5 Ne7 17. Rfe1 Rg8 18. Nf4 g5 19. Nh5 Rg6 20. Nf6 Bg7 21. Rd3 Bxf3 22. Rxf3 Bxf6 23. exf6 Nc6 24. Rd3 Rf8 25. Re4 Kc8 26. f4 gxf4 27. Rxf4 Re8 28. Bc3 Re2 29. Rf2 Re4 30. Rh3 a5 31. Rh5 a4 32. bxa4 Rxc4 33. Bd2 Rxa4 34. Rxh6 Rg8 35. Rh7 Rxa2 36. Rxf7 Ne5 37. Rg7 Rf8 38. h3 c4 39. Re7 Nd3 40. f7 Nxf2 41. Re8+ Kd7 42. Rxf8 Ke7 43. Rc8 Kxf7 44. Rxc7+ Ke6 45. Be3 Nd1 46. Bxb6 c3 47. h4 Ra6 48. Bd4 Ra4 49. Bxc3 Nxc3 50. Rxc3 Rxh4 51. Rf3 Rh5 52. Kf2 Rg5 53. Rf8 Ke5 1/2-1/2");

            Board board = new Board();
            long totalNodes=0;
            int positionCount = 0;
            TimeSpan totalTime = new TimeSpan(0);

            foreach (Move move in pgn.Moves)
            {
                board.MoveApply(move);
                totalTime += PerftTest(board.FENCurrent.ToString(), nodesPerPosition, doEval, doMoveSort);
                totalNodes += nodesPerPosition;
                positionCount++;
                double nodesPerSecond = ((double)totalNodes / (double)totalTime.TotalMilliseconds) * 1000;
                Program.ConsoleWriteline(string.Format("Position {0} of {1}, {3}/Sec: {2}", positionCount, pgn.Moves.Count, (int)nodesPerSecond, doEval ? "Eval" : "Node"));
            }


        }
        private static TimeSpan PerftTest(string fen, int nodeCount, bool doEval, bool doMoveSort)
        {

            
            Board board = new Board(fen);

            int nodesDone = 0;//start at -1 to skip root node
            //Console.WriteLine("fen: " + fen);
            int depth;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            MovePicker.Stack buffer = new MovePicker.Stack();
            for (depth = 2; nodesDone < nodeCount; depth++)
            {
                PerftSearch(board,0,buffer, depth, nodeCount, ref nodesDone, doEval, doMoveSort);
                //Console.WriteLine(string.Format("depth:{0} nodes:{1} milliseconds:{2}", depth, nodesDone, sw.ElapsedMilliseconds));
            }
            sw.Stop();
            //Console.WriteLine("Average nodes/second: {0}", ((double)nodesDone / (double)sw.ElapsedMilliseconds) * 1000);
            //Console.WriteLine("");
            return sw.Elapsed;
        }

        public static void PerftSearch(Board board, int ply, MovePicker.Stack buffer, int depth_remaining, int nodeCount, ref int nodesDone, bool doEval, bool doMoveSort)
        {
            nodesDone++;
            if (doEval)
            {
                
                var newEval = eval.Eval(board);
                //int i = newEval.Score;
                //if (i == int.MaxValue) { nodesDone++; } //this is just to make sure compiler doesn't optimize away call to .Score, which might be expensive but we want to measure

            }
            if (depth_remaining <= 0 || nodesDone >= nodeCount)
            {
                return;
            }

            
            var moveBuffer = buffer[ply];
            moveBuffer.Initialize(board);

            //System.Diagnostics.Debug.Assert(moves.Count == moveBuffer.MoveCount);
            

            if (doMoveSort)
            {
                //moveBuffer.Sort(board, true, ChessMove.EMPTY);
                //var movelist = moves.ToList();
                //ChessMove.Comp moveOrderer = new ChessMove.Comp(board, ChessMove.EMPTY, true);
                //movelist.Sort(moveOrderer);
                //moves = movelist;
            }

            
            ChessMoveData moveData;
            while ((moveData = moveBuffer.NextMoveData()).Move != Move.EMPTY)
            {

                Move move = moveData.Move;
                //System.Diagnostics.Debug.Assert(moves.Contains(move));

                board.MoveApply(move);

                if (!board.IsCheck(board.WhosTurn.PlayerOther()))
                {
                    PerftSearch(board, ply + 1, buffer, depth_remaining - 1, nodeCount, ref nodesDone, doEval, doMoveSort);
                }

                board.MoveUndo();
            }
        }
        #endregion

        public static void AnnotatePGNEval(string fileNameIn, string fileNameOut)
        {
            try
            {
                
            
                if (!System.IO.File.Exists(fileNameIn))
                {
                    Program.ConsoleWriteline("file does not exist");
                }
                int count = 0;
                using (System.IO.StreamReader reader = new System.IO.StreamReader(fileNameIn))
                {

                    using (var writer = new System.IO.StreamWriter(fileNameOut, false))
                    {
                        foreach (var pgn in PGN.AllGames(reader))
                        {
                            AnnotatePGNWithEval(pgn);
                            pgn.Write(writer);
                            count++;
                            //writer.Write(pgn.ToString());
                        }
                    }
                }
                Program.ConsoleWriteline("annotated " + count.ToString() + " games");
            }
            catch (Exception ex)
            {

            }
        }

        private static Evaluator _annotateEval;
        public static void AnnotatePGNWithEval(PGN pgn)
        {
            try
            {
                if (_annotateEval == null) { _annotateEval = new Evaluator(); }
                Board board = new Board(pgn.StartingPosition);

                foreach (var move in pgn.Moves)
                {
                    board.MoveApply(move);
                    EvalResults eval = new EvalResults();
                    var evalScore = _annotateEval.EvalLazy(board, eval, null, int.MinValue, int.MaxValue);

                    //string evalComment = string.Format("white:{0} mat:{2} watt:{7} batt:{8} pcsq:{3} mob:{4} pawns:{5} pass:{6} start:{1:F2}", 
                    string evalComment = string.Format("watt:{7} batt:{8} passed:{9} candid:{10}", 
                        eval.Score, 
                        eval.StageStartWeight, 
                        eval.Material, 
                        eval.PcSqPhased, 
                        eval.MobilityPhased, 
                        eval.PawnsPhased, 
                        eval.PawnsPassedPhased, 
                        eval.Attacks[0].KingAttackerScore,
                        eval.Attacks[1].KingAttackerScore,
                        eval.PassedPawns,
                        eval.CandidatePawns);

                    if (pgn.Comments.ContainsKey(board.HistoryCount - 1))
                    {
                        evalComment = pgn.Comments[board.HistoryCount - 1] + " " + evalComment;
                        pgn.Comments.Remove(board.HistoryCount - 1);
                    }
                    pgn.Comments.Add(board.HistoryCount - 1, evalComment);

                }
            }
            catch (Exception ex)
            {
                Program.LogException(ex);
                Program.ConsoleWriteline(ex.Message);
            }
            

        }
    }
}
