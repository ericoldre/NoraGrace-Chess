using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sinobyl.Engine.Tests
{
    [TestClass]
    public class AttacksTest
    {
        static Random rand = new Random(0);

        private static Bitboard bitboard_to_attacks_calc(Bitboard piecelocations, ChessDirection dir, ChessPosition position)
        {
            Bitboard retval = 0;
            for (; ; )
            {
                position = position.PositionInDirection(dir);
                if (!position.IsInBounds()) { break; }
                retval |= position.ToBitboard();
                if (!(piecelocations & position.ToBitboard()).Empty()) { break; }
            }
            return retval;
        }

        private static Bitboard RandomBitboard(int pctFill)
        {
            Bitboard retval = 0;
            foreach (var pos in ChessPositionInfo.AllPositions)
            {
                if (rand.Next(0, 99) < pctFill)
                {
                    retval |= pos.ToBitboard();
                }
            }
            return retval;
        }

        
        [TestMethod]
        public void VerifyBitboardsThroughMoves()
        {

            int iCount = 0;
            var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Sinobyl.Engine.Tests.pgnFiles.gm2600.pgn");
            System.IO.StreamReader reader = new System.IO.StreamReader(stream);
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


                    Bitboard allpieces = 0;
                    foreach (ChessPiece pieceType in ChessPieceInfo.AllPieces)
                    {
                        Assert.AreEqual<Bitboard>(board.PieceList(pieceType).ToBitboard(), board[pieceType]);
                        allpieces |= board[pieceType];
                    }
                    Assert.AreEqual<Bitboard>(allpieces, board.PieceLocationsAll);
                    //Assert.AreEqual<ChessBitboard>(allpieces, Attacks.RotateVertReverse(board.PieceLocationsAllVert));
                    //Assert.AreEqual<ChessBitboard>(allpieces, Attacks.RotateDiagA1H8Reverse(board.PieceLocationsAllA1H8));
                    //Assert.AreEqual<ChessBitboard>(allpieces, Attacks.RotateDiagH1A8Reverse(board.PieceLocationsAllH1A8));

                }

                while (board.HistoryCount > 0)
                {
                    board.MoveUndo();

                    Bitboard allpieces = 0;
                    foreach (ChessPiece pieceType in ChessPieceInfo.AllPieces)
                    {
                        Assert.AreEqual<Bitboard>(board.PieceList(pieceType).ToBitboard(), board[pieceType]);
                        allpieces |= board[pieceType];
                    }
                    Assert.AreEqual<Bitboard>(allpieces, board.PieceLocationsAll);
                    //Assert.AreEqual<ChessBitboard>(allpieces, Attacks.RotateVertReverse(board.PieceLocationsAllVert));
                    //Assert.AreEqual<ChessBitboard>(allpieces, Attacks.RotateDiagA1H8Reverse(board.PieceLocationsAllA1H8));
                    //Assert.AreEqual<ChessBitboard>(allpieces, Attacks.RotateDiagH1A8Reverse(board.PieceLocationsAllH1A8));

                }

                if (iCount > 100) { break; }

            }


        }

    }
}
