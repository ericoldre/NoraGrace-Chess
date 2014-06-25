using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NoraGrace.Engine;
namespace Sinobyl.Engine.Tests
{
    class gm2600
    {
        public FEN StartingPosition { get; private set; }
        public Move[] Moves { get; private set; }
        public FEN CurrentPosition { get; private set; }

    //    public static IEnumerable<gm2600> GetPositions()
    //    {

    //        var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Nora.Engine.Tests.pgnFiles.gm2600.pgn");
    //        System.IO.StreamReader reader = new System.IO.StreamReader(stream);
    //        Evaluation.Evaluator eval = new Evaluation.Evaluator();

    //        while (!reader.EndOfStream)
    //        {
    //            iCount++;
    //            PGN pgn = PGN.NextGame(reader);
    //            if (pgn == null) { break; }

    //            Board board = new Board();

    //            foreach (Move move in pgn.Moves)
    //            {
    //                board.MoveApply(move);


    //                Bitboard allpieces = 0;
    //                foreach (Piece pieceType in PieceInfo.AllPieces)
    //                {
    //                    Assert.AreEqual<Bitboard>(board.PieceList(pieceType).ToBitboard(), board[pieceType]);
    //                    allpieces |= board[pieceType];
    //                }
    //                Assert.AreEqual<Bitboard>(allpieces, board.PieceLocationsAll);
    //                //Assert.AreEqual<ChessBitboard>(allpieces, Attacks.RotateVertReverse(board.PieceLocationsAllVert));
    //                //Assert.AreEqual<ChessBitboard>(allpieces, Attacks.RotateDiagA1H8Reverse(board.PieceLocationsAllA1H8));
    //                //Assert.AreEqual<ChessBitboard>(allpieces, Attacks.RotateDiagH1A8Reverse(board.PieceLocationsAllH1A8));

    //            }
    //    }
    }
}
