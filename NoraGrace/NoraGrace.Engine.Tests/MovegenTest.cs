﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoraGrace.Engine;
using System.IO;

namespace NoraGrace.Engine.Tests
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
        //public void PerftTest(string fen, int depth, int expectedLeaves, int expectedNodes)
        //{

        //    ChessBoard board = new ChessBoard(fen);

        //    string fenStart = board.FEN.ToString();

        //    int nodecount = -1;//start at -1 to skip root node
        //    int leafnodecount = 0;

        //    PerftSearch(board, depth, ref nodecount, ref leafnodecount);

        //    string fenEnd = board.FEN.ToString();

        //    //Assert.AreEqual<string>(fenStart, fenEnd, string.Format("Start and End FEN do not match {0}  {1}", fenStart, fenEnd));
        //    Assert.AreEqual<int>(expectedLeaves, leafnodecount, string.Format("ExpectedLeaves D:{0} FEN:{1}", depth, fen));
        //    Assert.AreEqual<int>(expectedNodes, nodecount, string.Format("ExpectedNodes D:{0} FEN:{1}", depth, fen));

        //}

        //public void PerftSearch(ChessBoard board, int depth_remaining, ref int nodecount, ref int leafnodecount)
        //{
        //    nodecount++;

        //    if (depth_remaining <= 0)
        //    {
        //        leafnodecount++;
        //        return;
        //    }

        //    List<ChessMove> moves = ChessMove.GenMoves(board, false).ToList();

        //    HashSet<ChessMove> legalMoves = new HashSet<ChessMove>();


        //    foreach (ChessMove move in moves)
        //    {
        //        board.MoveApply(move);
        //        //VerifyBoardBitboards(board);
        //        if (!board.IsCheck(board.WhosTurn.PlayerOther()))
        //        {
        //            legalMoves.Add(move);
        //            PerftSearch(board, depth_remaining - 1, ref nodecount, ref leafnodecount);
        //        }

        //        board.MoveUndo();
        //        //VerifyBoardBitboards(board);
        //    }

        //    List<ChessMove> generatedLegalMoves = ChessMove.GenMovesLegal(board).ToList();
        //    Assert.AreEqual<int>(legalMoves.Count, generatedLegalMoves.Count);
        //    foreach (var generatedLegalMove in generatedLegalMoves)
        //    {
        //        Assert.IsTrue(legalMoves.Contains(generatedLegalMove));
        //    }
        //    //var movesAfter = ChessMove.GenMoves(board, false).ToList();
        //    //Assert.AreEqual<int>(moves.Count, movesAfter.Count);
        //}

        public void PerftTest(string fen, int depth, int expectedLeaves, int expectedNodes)
        {

            Board board = new Board(fen);

            string fenStart = board.FENCurrent.ToString();

            int nodecount = -1;//start at -1 to skip root node
            int leafnodecount = 0;
            MovePicker[] moveBuffer = MovePicker.CreateStack();
            SearchData sdata = new SearchData(Evaluation.Evaluator.Default);

            PerftSearch(board, 0, sdata, depth, ref nodecount, ref leafnodecount);

            string fenEnd = board.FENCurrent.ToString();

            //Assert.AreEqual<string>(fenStart, fenEnd, string.Format("Start and End FEN do not match {0}  {1}", fenStart, fenEnd));
            Assert.AreEqual<int>(expectedLeaves, leafnodecount, string.Format("ExpectedLeaves D:{0} FEN:{1}", depth, fen));
            Assert.AreEqual<int>(expectedNodes, nodecount, string.Format("ExpectedNodes D:{0} FEN:{1}", depth, fen));

        }

        public void PerftSearch(Board board, int ply, SearchData sdata, int depth_remaining, ref int nodecount, ref int leafnodecount)
        {
            nodecount++;

            if (depth_remaining <= 0)
            {
                leafnodecount++;
                return;
            }

            var plyBuffer = sdata[ply].MoveGenerator;
            plyBuffer.Initialize(board, sdata[ply]);


            foreach (Move move in plyBuffer.SortedMoves())
            {
                Assert.IsTrue(move.IsPsuedoLegal(board));
                board.MoveApply(move);
                //VerifyBoardBitboards(board);
                if (!board.IsCheck(board.WhosTurn.PlayerOther()))
                {
                    //legalMoves.Add(move);
                    PerftSearch(board, ply + 1, sdata, depth_remaining - 1, ref nodecount, ref leafnodecount);
                }

                board.MoveUndo();
                //VerifyBoardBitboards(board);
            }

            //var movesAfter = ChessMove.GenMoves(board, false).ToList();
            //Assert.AreEqual<int>(moves.Count, movesAfter.Count);
        }
        #endregion

        public void VerifyBoardBitboards(Board board)
        {
            Bitboard[] expectedPieces = new Bitboard[12];
            var all = Bitboard.Empty;
            foreach (var pos in PositionUtil.AllPositions)
            {
                if (board.PieceAt(pos) != Piece.EMPTY)
                {
                    all |= pos.ToBitboard();
                    expectedPieces[(int)board.PieceAt(pos)] |= pos.ToBitboard();
                }
            }

            foreach (var piece in PieceUtil.AllPieces)
            {
                Assert.AreEqual<Bitboard>(expectedPieces[(int)piece], board[piece.PieceToPlayer(), piece.ToPieceType()]);
            }
            Assert.AreEqual<Bitboard>(all, board.PieceLocationsAll);
            //Assert.AreEqual(Attacks.RotateVert(all), board.PieceLocationsAllVert);
            //Assert.AreEqual(Attacks.RotateDiagA1H8(all), board.PieceLocationsAllA1H8);
            //Assert.AreEqual(Attacks.RotateDiagH1A8(all), board.PieceLocationsAllH1A8);

        }

        [TestMethod]
        public void MovePicker1()
        {
            FEN fen = new FEN("5rk1/pbr1q1pp/3pp3/2p2p2/1PP3n1/P2BPP2/1B2Q1PP/1R1R2K1 w - - 0 22 ");
            Board board = new Board(fen);
            MovePicker picker = new MovePicker(new MovePicker.MoveHistory(), new StaticExchange());

            picker.Initialize(board, new PlyData(), Move.EMPTY, false);

            AssertSameMove(MoveUtil.GenMoves(board), picker.SortedMoves());

        }

        [TestMethod]
        public void MovePicker2()
        {
            FEN fen = new FEN("5rk1/pbr1q1pp/3pp3/2p2p2/1PP3n1/P2BPP2/1B2Q1PP/1R1R2K1 w - - 0 22 ");
            Board board = new Board(fen);
            MovePicker picker = new MovePicker(new MovePicker.MoveHistory(), new StaticExchange());
            picker.RegisterCutoff(board, new ChessMoveData() { Move = MoveUtil.Parse(board, "a3a4") }, SearchDepth.PLY);
            picker.Initialize(board, new PlyData(), MoveUtil.Parse(board, "f3g4"), false);

            AssertSameMove(MoveUtil.GenMoves(board), picker.SortedMoves());

        }

        [TestMethod]
        public void MovePicker3()
        {
            FEN fen = new FEN("5rk1/pbr1q1pp/3pp3/2p2p2/1PP3n1/P2BPP2/1B2Q1PP/1R1R2K1 w - - 0 22 ");
            Board board = new Board(fen);
            MovePicker picker = new MovePicker(new MovePicker.MoveHistory(), new StaticExchange());
            picker.RegisterCutoff(board, new ChessMoveData() { Move = MoveUtil.Parse(board, "d3e4") }, SearchDepth.PLY);
            picker.Initialize(board, new PlyData(), MoveUtil.Parse(board, "f3g4"), false);

            AssertSameMove(MoveUtil.GenMoves(board), picker.SortedMoves());

        }

        [TestMethod]
        public void MovePicker4()
        {
            FEN fen = new FEN("5rk1/pbr1q1pp/3pp3/2p2p2/1PP3n1/P2BPP2/1B2Q1PP/1R1R2K1 w - - 0 22 ");
            Board board = new Board(fen);
            MovePicker picker = new MovePicker(new MovePicker.MoveHistory(), new StaticExchange());
            picker.RegisterCutoff(new Board("5rk1/pbr1q1pp/3pp3/2p5/1PP3n1/P2BPP2/1B2Q1PP/1R1R2K1 w - - 0 22"), new ChessMoveData() { Move = MoveUtil.Parse(board, "d3f5") }, SearchDepth.PLY);
            picker.Initialize(board, new PlyData(), MoveUtil.Parse(board, "f3g4"), false);

            AssertSameMove(MoveUtil.GenMoves(board), picker.SortedMoves().ToList());

        }

        public void AssertSameMove(IEnumerable<Move> list1, IEnumerable<Move> list2)
        {
            var sorted1 = list1.OrderBy(m => m.From()).ThenBy(m => m.To()).ThenBy(m => m.Promote()).ToArray();
            var sorted2 = list2.OrderBy(m => m.From()).ThenBy(m => m.To()).ThenBy(m => m.Promote()).ToArray();
            Assert.IsTrue(sorted1.SequenceEqual(sorted2));
        }

        [TestMethod]
        public void PositionAttacks()
        {

            int iCount = 0;
            var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Sinobyl.Engine.Tests.pgnFiles.gm2600.pgn");
            var x = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames();
            StreamReader reader = new StreamReader(stream);
            Evaluation.Evaluator eval = new Evaluation.Evaluator();

            while (!reader.EndOfStream)
            {
                iCount++;
                PGN pgn = PGN.NextGame(reader);
                if (pgn == null) { break; }

                Board board = new Board();

                foreach (Move move in pgn.Moves)
                {
                    board.MoveApply(move);
                }

                if (iCount > 50) { break; }

            }


        }


        [TestMethod]
        public void MoveGenPerft1()
        {

            string fen = FEN.FENStart;

            PerftTest(fen, 1, 20, 20);
            PerftTest(fen, 2, 400, 420);
            PerftTest(fen, 3, 8902, 9322);
            PerftTest(fen, 4, 197281, 206603);
            //PerftTest(fen, 5, 4865609, 5072212);
            //PerftTest(fen, 6, 119060324, 124132536);
        }

        [TestMethod]
        public void MoveGenPerft2()
        {


            string fen = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1";

            PerftTest(fen, 1, 48, 48);
            PerftTest(fen, 2, 2039, 2087);
            PerftTest(fen, 3, 97862, 99949);
            //PerftTest(fen, 4, 4085603, 4185552);
            //PerftTest(fen, 5, 193690690, 197876242);

        }

        [TestMethod]
        public void MoveGenPerft3()
        {

            string fen = "8/PPP4k/8/8/8/8/4Kppp/8 w - - 0 1";

            PerftTest(fen, 1, 18, 18);
            PerftTest(fen, 2, 290, 308);
            PerftTest(fen, 3, 5044, 5352);
            PerftTest(fen, 4, 89363, 94715);
            //PerftTest(fen, 5, 1745545, 1840260);
            // PerftTest(fen, 6, 34336777, 36177037);


        }

        [TestMethod]
        public void MoveGenPerft4()
        {

            string fen = "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1";

            PerftTest(fen, 1, 14, 14);
            PerftTest(fen, 2, 191, 205);
            PerftTest(fen, 3, 2812, 3017);
            PerftTest(fen, 4, 43238, 46255);
            //PerftTest(fen, 5, 674624, 720879);

        }

        [TestMethod]
        public void MoveBufferExcludeTest()
        {
            ChessMoveData[] array = new ChessMoveData[192];
            Move[] exclude = new Move[20];

            for (int i = 0; i < 10; i++) { array[i].Move = (Move)i; }

            exclude[0] = (Move)2;
            exclude[1] = (Move)3;
            exclude[2] = (Move)4;
            exclude[3] = (Move)49;
            exclude[4] = (Move)3;

            int newCount = MovePicker.ExcludeFrom(array, 0, 10, exclude, 5);

            Assert.AreEqual<int>(7, newCount);
            Assert.AreEqual<Move>((Move)0, array[0].Move);
            Assert.AreEqual<Move>((Move)1, array[1].Move);
            Assert.AreEqual<Move>((Move)5, array[2].Move);
            Assert.AreEqual<Move>((Move)6, array[3].Move);
            Assert.AreEqual<Move>((Move)7, array[4].Move);
            Assert.AreEqual<Move>((Move)8, array[5].Move);
            Assert.AreEqual<Move>((Move)9, array[6].Move);

        }

    }
}
