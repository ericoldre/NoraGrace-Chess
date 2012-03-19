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
    public class MovegenTest
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

        #region PerftCalls
        public void PerftTest(string fen, int depth, int expectedLeaves, int expectedNodes)
        {

            ChessBoard board = new ChessBoard(fen);

            string fenStart = board.FEN.ToString();

            int moves_done = 0;
            int nodecount = -1;//start at -1 to skip root node
            int leafnodecount = 0;

            PerftSearch(board, depth, ref moves_done, ref nodecount, ref leafnodecount);

            string fenEnd = board.FEN.ToString();

            //Assert.AreEqual<string>(fenStart, fenEnd, string.Format("Start and End FEN do not match {0}  {1}", fenStart, fenEnd));
            Assert.AreEqual<int>(expectedLeaves, leafnodecount, string.Format("ExpectedLeaves D:{0} FEN:{1}", depth, fen));
            Assert.AreEqual<int>(expectedNodes, nodecount, string.Format("ExpectedNodes D:{0} FEN:{1}", depth, fen));

        }

        public void PerftSearch(ChessBoard board, int depth_remaining, ref int moves_done, ref int nodecount, ref int leafnodecount)
        {
            nodecount++;

            if (depth_remaining <= 0)
            {
                leafnodecount++;
                return;
            }

            List<ChessMove> moves = ChessMove.GenMoves(board, false);
            //var oldBoard = new Murderhole.ChessBoard(board.FEN.ToString());
            //var oldMoves = Murderhole.ChessMove.GenMoves(oldBoard);
            //if (moves.Count != oldMoves.Count)
            //{
            //    var missing = oldMoves.Where(om => !moves.Any(nm => nm.ToString() == om.ToString())).ToArray();
            //    var extra = moves.Where(om => !oldMoves.Any(nm => nm.ToString() == om.ToString())).ToArray();
            //    leafnodecount += missing.Count();
            //    leafnodecount -= missing.Count();
            //    leafnodecount += extra.Count();
            //    leafnodecount -= extra.Count();
            //    var cc = ChessMove.GenMoves(board, false);
            //    leafnodecount += cc.Count();
            //    leafnodecount -= cc.Count();
            //    Assert.IsTrue(false);
            //}

            //var fen = board.FEN;
            
            foreach (ChessMove move in moves)
            {
                board.MoveApply(move);
                //VerifyBoardBitboards(board);
                if (!board.IsCheck(board.WhosTurn.PlayerOther()))
                {
                    PerftSearch(board, depth_remaining - 1, ref moves_done, ref nodecount, ref leafnodecount);
                }

                board.MoveUndo();
                //VerifyBoardBitboards(board);
                moves_done++;
            }
            //var movesAfter = ChessMove.GenMoves(board, false).ToList();
            //Assert.AreEqual<int>(moves.Count, movesAfter.Count);
        }
        #endregion

        public void VerifyBoardBitboards(ChessBoard board)
        {
            ChessBitboard[] expectedPieces = new ChessBitboard[12];
            var all = ChessBitboard.Empty;
            foreach (var pos in Chess.AllPositions)
            {
                if (board.PieceAt(pos) != ChessPiece.EMPTY)
                {
                    all |= pos.Bitboard();
                    expectedPieces[(int)board.PieceAt(pos)] |= pos.Bitboard();
                }
            }
            
            foreach (var piece in Chess.AllPieces)
            {
                Assert.AreEqual<ChessBitboard>(expectedPieces[(int)piece], board.PieceLocations(piece));
            }
            Assert.AreEqual<ChessBitboard>(all, board.PieceLocationsAll);
            Assert.AreEqual(Attacks.RotateVert(all), board.PieceLocationsAllVert);
            Assert.AreEqual(Attacks.RotateDiagA1H8(all), board.PieceLocationsAllA1H8);
            Assert.AreEqual(Attacks.RotateDiagH1A8(all), board.PieceLocationsAllH1A8);

        }

        [TestMethod]
        public void PositionAttacks()
        {

            int iCount = 0;
            var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Sinobyl.Engine.Tests.pgnFiles.gm2600.pgn");
            var x = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames();
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
                }

                if (iCount > 50) { break; }

            }


        }


        [TestMethod]
        public void MoveGenPerft1()
        {

            string fen = ChessFEN.FENStart;

            PerftTest(fen, 1, 20, 20);
            PerftTest(fen, 2, 400, 420);
            PerftTest(fen, 3, 8902, 9322);
            PerftTest(fen, 4, 197281, 206603);
            PerftTest(fen, 5, 4865609, 5072212);
            //PerftTest(fen, 6, 119060324, 124132536);
        }

        [TestMethod]
        public void MoveGenPerft2()
        {
        

            string fen = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1";

            PerftTest(fen, 1, 48, 48);
            PerftTest(fen, 2, 2039, 2087);
            PerftTest(fen, 3, 97862, 99949);
            PerftTest(fen, 4, 4085603, 4185552);
            PerftTest(fen, 5, 193690690, 197876242);

        }

        [TestMethod]
        public void MoveGenPerft3()
        {

            string fen = "8/PPP4k/8/8/8/8/4Kppp/8 w - - 0 1";

            PerftTest(fen, 1, 18, 18);
            PerftTest(fen, 2, 290, 308);
            PerftTest(fen, 3, 5044, 5352);
            PerftTest(fen, 4, 89363, 94715);
            PerftTest(fen, 5, 1745545, 1840260);
            PerftTest(fen, 6, 34336777, 36177037);


        }

        [TestMethod]
        public void MoveGenPerft4()
        {

            string fen = "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1";

            PerftTest(fen, 1, 14, 14);
            PerftTest(fen, 2, 191, 205);
            PerftTest(fen, 3, 2812, 3017);
            PerftTest(fen, 4, 43238, 46255);
            PerftTest(fen, 5, 674624, 720879);

        }

    }
}
