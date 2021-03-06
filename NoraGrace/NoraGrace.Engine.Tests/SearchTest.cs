﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NoraGrace.Engine.Tests
{
    [TestClass]
    public class SearchTest
    {

        [TestMethod]
        public void TestAbortOnDraw()
        {

            FEN fen = new FEN("7k/4r3/8/2R5/5K2/8/8/8 w - - 97 100");

            //ChessBoard board99 = new ChessBoard(fen);

            //Assert.IsFalse(board99.IsDrawBy50MoveRule());

            //ChessMove moveKf3 = ChessMoveInfo.Create(ChessPosition.F4, ChessPosition.F3);
           // board99.MoveApply(moveKf3);

           // Assert.IsTrue(board99.IsDrawBy50MoveRule());

            Search search = new Search(new Search.Args() 
            { 
                GameStartPosition = fen ,
                TransTable = new TranspositionTable(500),
            });

            int progressCount = 0;
            string output;
            search.ProgressReported += (s, e) =>
            {
                Board boardProgress = new Board(e.Progress.FEN);
                string pvstring = e.Progress.PrincipleVariation.Descriptions(boardProgress, true);
                output = string.Format("{0} {1} {2} {3} {4}", e.Progress.Depth, e.Progress.Score, Math.Round(e.Progress.Time.TotalMilliseconds / 10), e.Progress.Nodes, pvstring);
                Console.WriteLine(output);

                progressCount++;
                Assert.IsTrue(progressCount < 40);

            };

            search.Start();



        }

        [TestMethod]
        public void StaticExchangeTest()
        {
            FEN fen = new FEN("rnbqkb1r/ppp2ppp/4p3/3p1n2/4P1P1/1B4N1/PPPP1P1P/RNBQK2R w KQkq - 0 1 ");
            Board board = new Board(fen);

            AssertFirstGreaterSEE(board, "e4f5", "g3f5");
            AssertFirstGreaterSEE(board, "g4f5", "g3f5");
            AssertFirstGreaterSEE(board, "e4f5", "g3f5");
            AssertFirstGreaterSEE(board, "g3f5", "b3d5");


            board = new Board(fen.Reverse());
            AssertFirstGreaterSEE(board, "e5f4", "g6f4");
            AssertFirstGreaterSEE(board, "g5f4", "g6f4");
            AssertFirstGreaterSEE(board, "e5f4", "g6f4");
            AssertFirstGreaterSEE(board, "g6f4", "b6d4");

            //fen = new ChessFEN("r2qr1k1/pp3pp1/1bp1b2p/8/3P1P2/Q1PB1N2/P4PPP/1R3K1R w - - 3 19 ");
            //board = new ChessBoard(fen);  //"Qxd7 Qxf7+ Qxc6 Qxa5"
            ////AssertFirstGreaterSEE(board, "xxxx", "xxxx");
            ////AssertFirstGreaterSEE(board, "xxxx", "xxxx");
            ////AssertFirstGreaterSEE(board, "xxxx", "xxxx");
            ////AssertFirstGreaterSEE(board, "xxxx", "xxxx");
            ////AssertFirstGreaterSEE(board, "xxxx", "xxxx");


            //string caps = string.Join(" ", ChessMove.GenMoves(board, true).Select(m => m.ToString(board)).ToArray());

            //int i = caps.Length;
        }

        private void AssertFirstGreaterSEE(Board board, string move1, string move2)
        {
            Move m1 = MoveUtil.Parse(board, move1);
            Move m2 = MoveUtil.Parse(board, move2);
            StaticExchange se = new StaticExchange();

            int score1 = se.CalculateScore(board, m1);
            int score2 = se.CalculateScore(board, m2);
            Assert.IsTrue(score1 > score2);
        }
    }
}

