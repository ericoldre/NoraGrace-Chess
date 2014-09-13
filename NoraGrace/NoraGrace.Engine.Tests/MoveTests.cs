using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoraGrace.Engine;
using NoraGrace.Engine.Evaluation;
using System.IO;

namespace Sinobyl.Engine.Tests
{
    [TestClass]
    public class MoveTests
    {

        [TestMethod]
        public void Move_IsGoodCap_IsSafeQuiet_Tests()
        {
            StaticExchange staticExchange = new StaticExchange();
            MovePicker movePicker = new MovePicker(new MovePicker.MoveHistory(), staticExchange);
            PlyData plyData = new PlyData();

            foreach (var pgn in GetPgns().Take(500))
            {
                Board board = new Board(pgn.StartingPosition);

                foreach (var gameMove in pgn.Moves)
                {
                    board.MoveApply(gameMove);
                    movePicker.Initialize(board, plyData);

                    var myAttacks = plyData.AttacksFor(board.WhosTurn);
                    var hisAttacks = plyData.AttacksFor(board.WhosTurn.PlayerOther());

                    ChessMoveData moveData;
                    while((moveData = movePicker.NextMoveData()).Move != Move.EMPTY)
                    {
                        Move move = moveData.Move;

                        if (move.IsCapture() || move.IsEnPassant())
                        {
                            bool isGood = move.IsGoodCapture(board, staticExchange, myAttacks, hisAttacks);
                            int seeScore = staticExchange.CalculateScore(board, move);
                            if (isGood != (seeScore >= 0))
                            {
                                bool isGood2 = move.IsGoodCapture(board, staticExchange, myAttacks, hisAttacks);
                            }
                            Assert.IsTrue(isGood == (seeScore >= 0));
                        }
                        else
                        {
                            bool isSafe = move.IsSafeQuiet(board, staticExchange, myAttacks, hisAttacks);
                            int seeScore = staticExchange.CalculateScore(board, move);
                            if (isSafe != (seeScore >= 0))
                            {
                                bool isSafe2 = move.IsSafeQuiet(board, staticExchange, myAttacks, hisAttacks);
                            }
                            Assert.IsTrue(isSafe == (seeScore >= 0));
                        }

                    }
                }
            }
        }

        [TestMethod]
        public void Move_MVVLVATest()
        {
            var pn = MoveUtil.Create(Position.A3, Position.A4, Piece.WPawn, Piece.BKnight);
            var nn = MoveUtil.Create(Position.A3, Position.A4, Piece.WKnight, Piece.BKnight);
            var qn = MoveUtil.Create(Position.A3, Position.A4, Piece.WQueen, Piece.BKnight);
            var qp = MoveUtil.Create(Position.A3, Position.A4, Piece.WQueen, Piece.BPawn);

            var pnval = pn.MVVLVA();
            var nnval = nn.MVVLVA();
            var qnval = qn.MVVLVA();
            var qpval = qp.MVVLVA();

            Assert.IsTrue(pn.MVVLVA() > nn.MVVLVA());
            Assert.IsTrue(nn.MVVLVA() > qn.MVVLVA());
            Assert.IsTrue(qn.MVVLVA() > qp.MVVLVA());

        }

        [TestMethod]
        public void Move_IsLegalPsuedoMoveTest()
        {
            MovePicker mp = new MovePicker(new MovePicker.MoveHistory(), new StaticExchange());
            PlyData pd = new PlyData();
            
            foreach(var pgn in GetPgns().Take(500))
            {
                Board board = new Board(pgn.StartingPosition);
                List<Move> prevWhite = null;
                List<Move> prevBlack = null;

                foreach(var gameMove in pgn.Moves)
                {
                    board.MoveApply(gameMove);

                    var me = board.WhosTurn;
                    var him = me.PlayerOther();

                    mp.Initialize(board, pd, Move.EMPTY, false);
                    var myAttacks = pd.AttacksFor(me, board);
                    var hisAttacks = pd.AttacksFor(him, board);
                    var myChecks = pd.ChecksFor(me, board);
                    var hisChecks = pd.ChecksFor(him, board);

                    List<Move> psuedoMoves = mp.SortedMoves().ToList();

                    //IsPsuedoLegal test.
                    //look through all moves in previous ply, and make sure that 
                    List<Move> prevMoves = board.WhosTurn == Player.White ? prevWhite : prevBlack;
                    if (prevMoves != null && !board.IsCheck())
                    {
                        foreach(var prevMove in prevMoves)
                        {
                            bool isPsuedoLegal = prevMove.IsPsuedoLegal(board);
                            Assert.IsTrue(isPsuedoLegal == psuedoMoves.Contains(prevMove));
                        }
                    }
                    
                    foreach (var psuedoMove in mp.SortedMoves())
                    {
                        Assert.IsTrue(psuedoMove.IsPsuedoLegal(board));

                        var willBeLegal = psuedoMove.IsLegalPsuedoMove(board, hisAttacks, myChecks);
                        var willCheck = psuedoMove.CausesCheck(board, hisChecks);

                        board.MoveApply(psuedoMove);

                        var isCheck = board.IsCheck();
                        var isLegal = !board.IsCheck(me);

                        board.MoveUndo();


                        Assert.AreEqual(isLegal, willBeLegal);
                        if (isLegal)
                        {
                            Assert.AreEqual(isCheck, willCheck);
                        }
                    }

                    //set previous moves for is psuedolegal
                    if (board.WhosTurn == Player.White)
                    {
                        prevWhite = psuedoMoves;
                    }
                    else
                    {
                        prevBlack = psuedoMoves;
                    }

                }
            }
        }

        public static IEnumerable<PGN> GetPgns()
        {
            var names = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames();
            var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Sinobyl.Engine.Tests.pgnFiles.gm2600.pgn");
            System.IO.StreamReader reader = new System.IO.StreamReader(stream);

            while (!reader.EndOfStream)
            {
                PGN pgn = PGN.NextGame(reader);
                if (pgn == null) { break; }
                yield return pgn;
            }
        }



    }
}
