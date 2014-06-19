using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NoraGrace.Engine.Tests
{
    [TestClass]
    public class ChessPieceTypeTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            foreach (Piece piece in PieceInfo.AllPieces)
            {
                var type = piece.ToPieceType();
                var player = piece.PieceToPlayer();
                var res = type.ForPlayer(player);

                Assert.AreEqual<Piece>(piece, res);
            }
        }
    }
}
