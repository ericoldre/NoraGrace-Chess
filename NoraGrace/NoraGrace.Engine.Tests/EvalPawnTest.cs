using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    }
}
