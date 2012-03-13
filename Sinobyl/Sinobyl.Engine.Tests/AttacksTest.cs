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

        private static ChessBitboard bitboard_to_attacks_calc(ChessBitboard piecelocations, ChessDirection dir, ChessPosition position)
        {
            ChessBitboard retval = 0;
            for (; ; )
            {
                position = Chess.PositionInDirection(position, dir);
                if (!position.IsInBounds()) { break; }
                retval |= position.Bitboard();
                if (!(piecelocations & position.Bitboard()).Empty()) { break; }
            }
            return retval;
        }

        private static ChessBitboard RandomBitboard(int pctFill)
        {
            ChessBitboard retval = 0;
            foreach (var pos in Chess.AllPositions)
            {
                if (rand.Next(0, 99) < pctFill)
                {
                    retval |= pos.Bitboard();
                }
            }
            return retval;
        }
        
        [TestMethod]
        public void AttacksHoriz()
        {
            foreach (var pos in Chess.AllPositions)
            {
                for (int i = 0; i < 100; i++)
                {

                    var pieceLocations = RandomBitboard(15);
                    var rankLocations = pos.GetRank().Bitboard() & pieceLocations;
                    var results = Attacks.HorizAttacks(pos, pieceLocations);
                    var calcResults = bitboard_to_attacks_calc(pieceLocations, ChessDirection.DirE, pos) | bitboard_to_attacks_calc(pieceLocations, ChessDirection.DirW, pos);
                    Assert.AreEqual<ChessBitboard>(calcResults, results);
                }
            }
        }
        [TestMethod]
        public void AttacksVert()
        {

            foreach (var pos in Chess.AllPositions)
            {
                for (int i = 0; i < 100; i++)
                {
                    var pieceLocations = RandomBitboard(15);
                    var pieceLocationsVert = Attacks.RotateVert(pieceLocations);
                    var results = Attacks.VertAttacks(pos, pieceLocationsVert);
                    var calcResults = bitboard_to_attacks_calc(pieceLocations, ChessDirection.DirN, pos) | bitboard_to_attacks_calc(pieceLocations, ChessDirection.DirS, pos);
                    Assert.AreEqual<ChessBitboard>(calcResults, results);
                }
            }
        }

        [TestMethod]
        public void AttacksDiagA1H8()
        {
            foreach(var position in Chess.AllPositions)
            {
                var original = position.Bitboard();
                var rotated = Attacks.RotateDiagA1H8(original);
                var rotatedBack = Attacks.RotateDiagA1H8Reverse(rotated);
                Assert.AreEqual<ChessBitboard>(original, rotatedBack);
            }

            foreach (var pos in Chess.AllPositions)
            {
                for (int i = 0; i < 100; i++)
                {
                    var pieceLocations = RandomBitboard(15);
                    var pieceLocationsRotated = Attacks.RotateDiagA1H8(pieceLocations);
                    var results = Attacks.DiagA1H8Attacks(pos, pieceLocationsRotated);
                    var calcResults = bitboard_to_attacks_calc(pieceLocations, ChessDirection.DirNE, pos) | bitboard_to_attacks_calc(pieceLocations, ChessDirection.DirSW, pos);
                    Assert.AreEqual<ChessBitboard>(calcResults, results);
                }
            }
        }
        [TestMethod]
        public void AttacksDiagH1A8()
        {
            foreach (var position in Chess.AllPositions)
            {
                var original = position.Bitboard();
                var rotated = Attacks.RotateDiagH1A8(original);
                var rotatedBack = Attacks.RotateDiagH1A8Reverse(rotated);
                Assert.AreEqual<ChessBitboard>(original, rotatedBack);
            }

            foreach (var pos in Chess.AllPositions)
            {
                for (int i = 0; i < 100; i++)
                {
                    var pieceLocations = RandomBitboard(15);
                    var pieceLocationsRotated = Attacks.RotateDiagH1A8(pieceLocations);
                    var results = Attacks.DiagH1A8Attacks(pos, pieceLocationsRotated);
                    var calcResults = bitboard_to_attacks_calc(pieceLocations, ChessDirection.DirSE, pos) | bitboard_to_attacks_calc(pieceLocations, ChessDirection.DirNW, pos);
                    Assert.AreEqual<ChessBitboard>(calcResults, results);
                }
            }
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


                    ChessBitboard allpieces = 0;
                    foreach (ChessPiece pieceType in Chess.AllPieces)
                    {
                        Assert.AreEqual<ChessBitboard>(board.PieceList(pieceType).ToBitboard(), board.PieceLocations(pieceType));
                        allpieces |= board.PieceLocations(pieceType);
                    }
                    Assert.AreEqual<ChessBitboard>(allpieces, board.PieceLocationsAll);
                    Assert.AreEqual<ChessBitboard>(allpieces, Attacks.RotateVertReverse(board.PieceLocationsAllVert));
                    Assert.AreEqual<ChessBitboard>(allpieces, Attacks.RotateDiagA1H8Reverse(board.PieceLocationsAllA1H8));
                    Assert.AreEqual<ChessBitboard>(allpieces, Attacks.RotateDiagH1A8Reverse(board.PieceLocationsAllH1A8));
                    
                }

                while (board.HistoryCount > 0)
                {
                    board.MoveUndo();

                    ChessBitboard allpieces = 0;
                    foreach (ChessPiece pieceType in Chess.AllPieces)
                    {
                        Assert.AreEqual<ChessBitboard>(board.PieceList(pieceType).ToBitboard(), board.PieceLocations(pieceType));
                        allpieces |= board.PieceLocations(pieceType);
                    }
                    Assert.AreEqual<ChessBitboard>(allpieces, board.PieceLocationsAll);
                    Assert.AreEqual<ChessBitboard>(allpieces, Attacks.RotateVertReverse(board.PieceLocationsAllVert));
                    Assert.AreEqual<ChessBitboard>(allpieces, Attacks.RotateDiagA1H8Reverse(board.PieceLocationsAllA1H8));
                    Assert.AreEqual<ChessBitboard>(allpieces, Attacks.RotateDiagH1A8Reverse(board.PieceLocationsAllH1A8));

                }

                if (iCount > 100) { break; }

            }


        }

    } 
}
