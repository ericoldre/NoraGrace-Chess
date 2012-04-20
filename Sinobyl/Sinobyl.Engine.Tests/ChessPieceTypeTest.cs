using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sinobyl.Engine.Tests
{
    [TestClass]
    public class ChessPieceTypeTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            foreach (ChessPiece piece in Chess.AllPieces)
            {
                var type = piece.ToPieceType();
                var player = piece.PieceToPlayer();
                var res = type.ForPlayer(player);

                Assert.AreEqual<ChessPiece>(piece, res);
            }
        }
    }
}
