﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sinobyl.Engine;
using System.IO;

namespace Sinobyl.Engine.Tests
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
            var board = new ChessBoard(ChessFEN.FENStart);
            var boardReverse = new ChessBoard(new ChessFEN(ChessFEN.FENStart).Reverse());
            var eval = new ChessEval();

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
            var fen = new ChessFEN(sfen);
            var board = new ChessBoard(fen);
            var breverse = new ChessBoard(fen.Reverse());
            var eval = new ChessEval();
            var res1 = eval.EvalFor(board, board.WhosTurn);
            var res2 = eval.EvalFor(breverse, breverse.WhosTurn);

            
            Assert.AreEqual<int>(res1, res2);
            Assert.IsTrue(minOk <= res1 && res1 <= maxOk);
        }

        [TestMethod]
        public void PawnTest1()
        {
            ChessBoard board = new ChessBoard("7k/7p/2p5/2p1Pp2/3pP3/1P4P1/7P/5BK1 w - - 0 1 ");
            ChessBitboard passed;
            ChessBitboard doubled;
            ChessBitboard isolated;
            ChessBitboard unconnected;

            int StartVal = 0;
            int EndVal = 0;


            var pawnEval = new ChessEvalPawns(ChessEvalSettings.Default());

            var result = pawnEval.EvalAllPawns(board.PieceLocationsReally(ChessPiece.WPawn), board.PieceLocationsReally(ChessPiece.BPawn), board.ZobristPawn);

            Assert.AreEqual<int>(2, result.PassedPawns.ToPositions().Count());
            Assert.AreEqual<int>(2, result.Doubled.ToPositions().Count());
            Assert.AreEqual<int>(5, result.Isolated.ToPositions().Count());


        }

        [TestMethod]
        public void EvalTestPcSqRev()
        {
            ChessEvalSettings settings = new ChessEvalSettings();
            
            ChessEval eval = new ChessEval();

            foreach (var type in ChessPieceTypeInfo.AllPieceTypes)
            {
                foreach (var pos in ChessPositionInfo.AllPositions)
                {
                    ChessPosition revPos = pos.Reverse();
                    var whiteOpening = eval._pcsqPiecePosStage[(int)type.ForPlayer(ChessPlayer.White), (int)pos, (int)ChessGameStage.Opening];
                    var blackOpening = eval._pcsqPiecePosStage[(int)type.ForPlayer(ChessPlayer.Black), (int)revPos, (int)ChessGameStage.Opening];
                    Assert.AreEqual<int>(whiteOpening, -blackOpening);

                    var whiteEnd = eval._pcsqPiecePosStage[(int)type.ForPlayer(ChessPlayer.White), (int)pos, (int)ChessGameStage.Endgame];
                    var blackEnd = eval._pcsqPiecePosStage[(int)type.ForPlayer(ChessPlayer.Black), (int)revPos, (int)ChessGameStage.Endgame];
                    Assert.AreEqual<int>(whiteOpening, -blackOpening);

                }
            }

        }

        [TestMethod]
        public void EvalTest1()
        {
            int iCount = 0;
            var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Sinobyl.Engine.Tests.pgnFiles.gm2600.pgn");
            StreamReader reader = new StreamReader(stream);
            ChessEval eval = new ChessEval();

            while (!reader.EndOfStream)
            {
                iCount++;
                ChessPGN pgn = ChessPGN.NextGame(reader);
                if (pgn == null) { break; }

                ChessBoard board = new ChessBoard();

                foreach (ChessMove move in pgn.Moves)
                {
                    board.MoveApply(move);

                    var fen = board.FEN;
                    ChessBoard boardRev = new ChessBoard(fen.Reverse());
                    
                    ChessEvalInfo e1 = new ChessEvalInfo();
                    ChessEvalInfo e2 = new ChessEvalInfo();
                    eval.EvalDetail(board, e1);
                    eval.EvalDetail(boardRev, e2);

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
            ChessEval eval = new ChessEval();

            while (!reader.EndOfStream)
            {
                iCount++;
                ChessPGN pgn = ChessPGN.NextGame(reader);
                if (pgn == null) { break; }

                ChessBoard board = new ChessBoard();

                int MovesDone = 0;
                int TotalMoves = pgn.Moves.Count;

                foreach (ChessMove move in pgn.Moves)
                {
                    board.MoveApply(move);

                    ChessSearch.Args args = new ChessSearch.Args();
                    args.GameStartPosition = board.FEN;
                    args.MaxDepth = 4;

                    ChessSearch search = new ChessSearch(args);
                    MovesDone++;
                    search.Search();

                    if (MovesDone >= 20) { break; }

                }
                Assert.AreEqual<int>(MovesDone, TotalMoves);
                //only do one game
                break;

            }


        }
    }
}
