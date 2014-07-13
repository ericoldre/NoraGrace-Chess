using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoraGrace.Engine.Evaluation;

namespace NoraGrace.Engine.Tests
{
    [TestClass]
    public class EvalPawnTest
    {

        //[TestMethod]
        //public void Test1()
        //{
        //    Assert.AreEqual<ChessBitboard>(ChessBitboard.B7, getCandidates(new ChessFEN("6k1/pp3ppp/8/8/4P3/P4P2/6PP/4K3 b K - 11 1")));
        //}

        [TestMethod]
        public void TestEasy()
        {
            Assert.AreEqual<Bitboard>(Bitboard.B5 | Bitboard.G5, getCandidates(new FEN("4k3/p7/8/PP3ppp/8/5P1P/8/4K3 b KQkq - 0 1")));
        }

        Bitboard getCandidates(FEN fen)
        {
            Board board = new Board(fen);
            var info = getPawnEval.Value.EvalAllPawns(board[Player.White, PieceType.Pawn], board[Player.Black, PieceType.Pawn], board.ZobristPawn);
            return info.Candidates;
        }

        Lazy<Evaluation.PawnEvaluator> getPawnEval = new Lazy<Evaluation.PawnEvaluator>(() => new Evaluation.PawnEvaluator(Evaluation.Settings.Default()));


        [TestMethod]
        public void TestUnstoppable()
        {
            
            Assert.IsTrue(GetUnstoppableEndScore("8/8/5P2/2k5/8/8/5K2/8 w - - 0 1") > 0);
            Assert.IsTrue(GetUnstoppableEndScore("8/8/5P2/2k5/8/8/5K2/8 b - - 0 1") == 0);

            // 
            Assert.IsTrue(GetUnstoppableEndScore("8/5k2/8/8/2K5/5p2/8/8 b - - 0 1") < 0);
            Assert.IsTrue(GetUnstoppableEndScore("8/5k2/8/8/2K5/5p2/8/8 w - - 0 1") == 0);

            //white could catch but black king guarding.
            Assert.IsTrue(GetUnstoppableEndScore("8/8/8/8/2K2p2/P7/6k1/8 w - - 0 1 ") < 0);
            
        }

        public int GetUnstoppableEndScore(string fen)
        {
            Board board = new Board(fen);
            var x = getPawnEval.Value.EvalAllPawns(board[Player.White, PieceType.Pawn], board[Player.Black, PieceType.Pawn], board.ZobristPawn);
            var s = Evaluation.PawnEvaluator.EvalUnstoppablePawns(board, x.PassedPawns, x.Candidates);
            return s.Endgame();
        }

    }
}
