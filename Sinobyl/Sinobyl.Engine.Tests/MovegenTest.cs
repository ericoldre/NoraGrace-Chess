using System;
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
            MovePicker.Stack moveBuffer = new MovePicker.Stack();

            PerftSearch(board, 0, moveBuffer, depth, ref nodecount, ref leafnodecount);

            string fenEnd = board.FENCurrent.ToString();

            //Assert.AreEqual<string>(fenStart, fenEnd, string.Format("Start and End FEN do not match {0}  {1}", fenStart, fenEnd));
            Assert.AreEqual<int>(expectedLeaves, leafnodecount, string.Format("ExpectedLeaves D:{0} FEN:{1}", depth, fen));
            Assert.AreEqual<int>(expectedNodes, nodecount, string.Format("ExpectedNodes D:{0} FEN:{1}", depth, fen));

        }

        public void PerftSearch(Board board, int ply, MovePicker.Stack moveBuffer, int depth_remaining, ref int nodecount, ref int leafnodecount)
        {
            nodecount++;

            if (depth_remaining <= 0)
            {
                leafnodecount++;
                return;
            }

            var plyBuffer = moveBuffer[ply];
            plyBuffer.Initialize(board);


            foreach (ChessMove move in plyBuffer.SortedMoves())
            {
                Assert.IsTrue(move.IsPsuedoLegal(board));
                board.MoveApply(move);
                //VerifyBoardBitboards(board);
                if (!board.IsCheck(board.WhosTurn.PlayerOther()))
                {
                    //legalMoves.Add(move);
                    PerftSearch(board, ply + 1, moveBuffer, depth_remaining - 1, ref nodecount, ref leafnodecount);
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
            foreach (var pos in PositionInfo.AllPositions)
            {
                if (board.PieceAt(pos) != Piece.EMPTY)
                {
                    all |= pos.ToBitboard();
                    expectedPieces[(int)board.PieceAt(pos)] |= pos.ToBitboard();
                }
            }

            foreach (var piece in PieceInfo.AllPieces)
            {
                Assert.AreEqual<Bitboard>(expectedPieces[(int)piece], board[piece]);
            }
            Assert.AreEqual<Bitboard>(all, board.PieceLocationsAll);
            //Assert.AreEqual(Attacks.RotateVert(all), board.PieceLocationsAllVert);
            //Assert.AreEqual(Attacks.RotateDiagA1H8(all), board.PieceLocationsAllA1H8);
            //Assert.AreEqual(Attacks.RotateDiagH1A8(all), board.PieceLocationsAllH1A8);

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
                PGN pgn = PGN.NextGame(reader);
                if (pgn == null) { break; }

                Board board = new Board();

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
            ChessMove[] exclude = new ChessMove[20];

            for (int i = 0; i < 10; i++) { array[i].Move = (ChessMove)i; }

            exclude[0] = (ChessMove)2;
            exclude[1] = (ChessMove)3;
            exclude[2] = (ChessMove)4;
            exclude[3] = (ChessMove)49;
            exclude[4] = (ChessMove)3;

            int newCount = MovePicker.ExcludeFrom(array, 0, 10, exclude, 5);

            Assert.AreEqual<int>(7, newCount);
            Assert.AreEqual<ChessMove>((ChessMove)0, array[0].Move);
            Assert.AreEqual<ChessMove>((ChessMove)1, array[1].Move);
            Assert.AreEqual<ChessMove>((ChessMove)5, array[2].Move);
            Assert.AreEqual<ChessMove>((ChessMove)6, array[3].Move);
            Assert.AreEqual<ChessMove>((ChessMove)7, array[4].Move);
            Assert.AreEqual<ChessMove>((ChessMove)8, array[5].Move);
            Assert.AreEqual<ChessMove>((ChessMove)9, array[6].Move);

        }

    }
}
