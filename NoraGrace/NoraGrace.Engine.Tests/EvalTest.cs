using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoraGrace.Engine;
using NoraGrace.Engine.Evaluation;
using System.IO;

namespace NoraGrace.Engine.Tests
{
    [TestClass]
    public class EvalTest
    {

        #region Common test methods
        private TestContext testContextInstance;
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }


        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void NeutralTest()
        {
            var board = new Board(FEN.FENStart);
            var boardReverse = new Board(new FEN(FEN.FENStart).Reverse());
            var eval = new Evaluator();

            int norm = eval.Eval(board);
            int rev = eval.Eval(boardReverse);

            Assert.AreEqual<int>(0, norm);
            Assert.AreEqual<int>(0, rev);


        }

        [TestMethod]
        public void TestProblematicPositions()
        {
            //TestPositionRange("7k/5p2/8/3K4/3p2p1/6Pp/1P1R1P1P/r7 b - - 0 38", -500, 500);

        }
        public void TestPositionRange(string sfen, int minOk, int maxOk)
        {
            var fen = new FEN(sfen);
            var board = new Board(fen);
            var breverse = new Board(fen.Reverse());
            var eval = new Evaluator();
            var res1 = eval.EvalFor(board, board.WhosTurn);
            var res2 = eval.EvalFor(breverse, breverse.WhosTurn);

            
            Assert.AreEqual<int>(res1, res2);
            Assert.IsTrue(minOk <= res1 && res1 <= maxOk);
        }

        [TestMethod]
        public void PawnTest1()
        {
            Board board = new Board("7k/7p/2p5/2p1Pp2/3pP3/1P4P1/7P/5BK1 w - - 0 1 ");
            Bitboard passed;
            Bitboard doubled;
            Bitboard isolated;
            Bitboard unconnected;

            int StartVal = 0;
            int EndVal = 0;


            var pawnEval = new PawnEvaluator(Settings.Default());

            var result = pawnEval.EvalAllPawns(board[Player.White, PieceType.Pawn], board[Player.Black, PieceType.Pawn], board.ZobristPawn);

            Assert.AreEqual<int>(2, result.PassedPawns.ToPositions().Count());
            Assert.AreEqual<int>(2, result.Doubled.ToPositions().Count());
            Assert.AreEqual<int>(5, result.Isolated.ToPositions().Count());


        }



        [TestMethod]
        public void EvalTest1()
        {
            int iCount = 0;
            var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Sinobyl.Engine.Tests.pgnFiles.gm2600.pgn");
            StreamReader reader = new StreamReader(stream);
            Evaluator eval = new Evaluator();

            while (!reader.EndOfStream)
            {
                iCount++;
                PGN pgn = PGN.NextGame(reader);
                if (pgn == null) { break; }

                Board board = new Board();

                foreach (Move move in pgn.Moves)
                {
                    board.MoveApply(move);

                    var fen = board.FENCurrent;
                    Board boardRev = new Board(fen.Reverse());
                    
                    EvalResults e1 = new EvalResults();
                    EvalResults e2 = new EvalResults();
                    eval.EvalLazy(board, e1, null, int.MinValue, int.MaxValue);
                    eval.EvalLazy(boardRev, e2, null, int.MinValue, int.MaxValue);

                    if (e1.Score != -e2.Score)
                    {
                        int redo = eval.Eval(board);
                        int redo2 = eval.Eval(boardRev);
                    }

                    Assert.AreEqual<int>(e1.Score, -e2.Score);


                }

                if (iCount > 100) { break; }

            }


        }

        //[TestMethod]
        //public void EvalPawnShelterStormTest()
        //{
        //    ChessEvalPawns evalPawns = new ChessEvalPawns(ChessEvalSettings.Default(), (uint)1);

            
        //    ChessFEN fen111 = new ChessFEN("8/2k5/8/8/8/8/5PPP/6K1 w - - 0 1 ");
        //    ChessFEN fen211 = new ChessFEN("8/2k5/8/8/8/5P2/6PP/6K1 w - - 0 1 ");
        //    ChessFEN fen311 = new ChessFEN("8/2k5/8/8/5P2/8/6PP/6K1 w - - 0 1 ");
        //    ChessFEN fen411 = new ChessFEN("8/2k5/8/5P2/8/8/6PP/6K1 w - - 0 1 ");
        //    ChessFEN fen121 = new ChessFEN("8/2k5/8/8/8/6P1/5P1P/6K1 w - - 0 1 ");
        //    ChessFEN fen131 = new ChessFEN("");
        //    ChessFEN fen211 = new ChessFEN("");
        //    ChessFEN fen211 = new ChessFEN("");
        //}

        [TestMethod]
        public void SearchTest()
        {
            return;
            int iCount = 0;
            var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Sinobyl.Engine.Tests.pgnFiles.gm2600.pgn");
            StreamReader reader = new StreamReader(stream);
            Evaluator eval = new Evaluator();

            while (!reader.EndOfStream)
            {
                iCount++;
                PGN pgn = PGN.NextGame(reader);
                if (pgn == null) { break; }

                Board board = new Board();

                int MovesDone = 0;
                int TotalMoves = pgn.Moves.Count;

                foreach (Move move in pgn.Moves)
                {
                    board.MoveApply(move);

                    Search.Args args = new Search.Args();
                    args.GameStartPosition = board.FENCurrent;
                    args.MaxDepth = 4;

                    Search search = new Search(args);
                    MovesDone++;
                    search.Start();

                    if (MovesDone >= 20) { break; }

                }
                Assert.AreEqual<int>(MovesDone, TotalMoves);
                //only do one game
                break;

            }


        }
    }
}
