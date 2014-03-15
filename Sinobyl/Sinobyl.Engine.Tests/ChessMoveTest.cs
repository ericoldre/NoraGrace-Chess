using Sinobyl.Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Sinobyl.Engine.Tests
{
    
    
    /// <summary>
    ///This is a test class for ChessMoveTest and is intended
    ///to contain all ChessMoveTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ChessMoveTest
    {
        [TestMethod]
        public void ValidateFromToProm()
        {
            foreach (ChessPosition from in ChessPositionInfo.AllPositions)
            {
                foreach (ChessPosition to in ChessPositionInfo.AllPositions)
                {
                    foreach (ChessPiece prom in ChessPieceInfo.AllPieces.Concat(new ChessPiece[] {ChessPiece.EMPTY}))
                    {
                        ChessMove move = ChessMoveInfo.Create(from, to, prom);

                        var from2 = move.From();
                        var to2 = move.To();
                        var prom2 = move.Promote();

                        Assert.AreEqual<ChessPosition>(from, from2);
                        Assert.AreEqual<ChessPosition>(to, to2);
                        Assert.AreEqual<ChessPiece>(prom, prom2);
                        Assert.AreNotEqual<ChessMove>(move, ChessMove.NULL_MOVE);
                    }
                }    
            }
        }

        [TestMethod]
        public void ValidateFromTo()
        {
            foreach (ChessPosition from in ChessPositionInfo.AllPositions)
            {
                foreach (ChessPosition to in ChessPositionInfo.AllPositions)
                {

                    ChessMove move = ChessMoveInfo.Create(from, to, ChessPiece.EMPTY);

                    var from2 = move.From();
                    var to2 = move.To();
                    var prom2 = move.Promote();

                    Assert.AreEqual<ChessPosition>(from, from2);
                    Assert.AreEqual<ChessPosition>(to, to2);
                    Assert.AreEqual<ChessPiece>(ChessPiece.EMPTY, prom2);
                    Assert.AreNotEqual<ChessMove>(move, ChessMove.NULL_MOVE);
                }
            }
        }





    }
}
