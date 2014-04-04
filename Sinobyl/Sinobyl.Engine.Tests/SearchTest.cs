using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sinobyl.Engine.Tests
{
    [TestClass]
    public class SearchTest
    {

        [TestMethod]
        public void TestAbortOnDraw()
        {

            ChessFEN fen = new ChessFEN("7k/4r3/8/2R5/5K2/8/8/8 w - - 97 100");

            //ChessBoard board99 = new ChessBoard(fen);

            //Assert.IsFalse(board99.IsDrawBy50MoveRule());

            //ChessMove moveKf3 = new ChessMove(ChessPosition.F4, ChessPosition.F3);
           // board99.MoveApply(moveKf3);

           // Assert.IsTrue(board99.IsDrawBy50MoveRule());

            ChessSearch search = new ChessSearch(new ChessSearch.Args() 
            { 
                GameStartPosition = fen ,
                TransTable = new ChessTrans(500),
                StopAtTime = DateTime.Now.AddDays(1)
            });

            int progressCount = 0;
            string output;
            search.ProgressReported += (s, e) =>
            {
                ChessBoard boardProgress = new ChessBoard(e.Progress.FEN);
                string pvstring = new ChessMoves(e.Progress.PrincipleVariation).ToString(boardProgress, true);
                output = string.Format("{0} {1} {2} {3} {4}", e.Progress.Depth, e.Progress.Score, Math.Round(e.Progress.Time.TotalMilliseconds / 10), e.Progress.Nodes, pvstring);
                Console.WriteLine(output);

                progressCount++;
                Assert.IsTrue(progressCount < 40);

            };

            search.Search();



        }


        void search_ProgressReported(object sender, SearchProgressEventArgs e)
        {
            throw new NotImplementedException();
        }
        //
    }
}
