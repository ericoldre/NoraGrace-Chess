using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinobyl.Engine
{
    public static partial class ChessMoveInfo
    {


        public class Comp : Comparer<ChessMove>
        {
            public readonly ChessBoard board;
            public readonly ChessMove tt_move;
            public readonly bool UseSEE;

            public Comp(ChessBoard a_board, ChessMove a_tt_move, bool a_see)
            {
                board = a_board;
                tt_move = a_tt_move;
                UseSEE = false;
            }

            public override int Compare(ChessMove x, ChessMove y)
            {
                if (x.Equals(tt_move) && y.Equals(tt_move))
                {
                    return 0;
                }
                if (x.Equals(tt_move))
                {
                    return -1;
                }
                if (y.Equals(tt_move))
                {
                    return 1;
                }
                //captures 1st
                if (board.PieceAt(x.To()) != ChessPiece.EMPTY && board.PieceAt(y.To()) == ChessPiece.EMPTY)
                {
                    return -1;
                }
                if (board.PieceAt(y.To()) != ChessPiece.EMPTY && board.PieceAt(x.To()) == ChessPiece.EMPTY)
                {
                    return 1;
                }

                if (!x.EstScore().HasValue)
                {
                    if (UseSEE)
                    {
                        //x.EstScore = CompEstScoreSEE(x, board);
                    }
                    else
                    {
                        //x.EstScore = CompEstScore(x, board);
                    }
                }
                if (!y.EstScore().HasValue)
                {
                    if (UseSEE)
                    {
                        //y.EstScore = CompEstScoreSEE(y, board);
                    }
                    else
                    {
                        //y.EstScore = CompEstScore(y, board);
                    }
                }
                if (x.EstScore() > y.EstScore()) { return -1; }
                if (x.EstScore() < y.EstScore()) { return 1; }
                return 0;
            }

            public static int CompEstScore(ChessMove move, ChessBoard board)
            {
                int retval = 0;

                ChessPiece mover = board.PieceAt(move.From());
                ChessPiece taken = board.PieceAt(move.To());
                ChessPlayer me = board.WhosTurn;

                retval -= eval._pcsqPiecePosStage[(int)mover, (int)move.From(), (int)ChessGameStage.Opening];
                retval += eval._pcsqPiecePosStage[(int)mover, (int)move.To(), (int)ChessGameStage.Opening];

                if (taken != ChessPiece.EMPTY)
                {
                    retval -= eval._matPieceStage[(int)taken, (int)ChessGameStage.Opening];
                }

                if (me == ChessPlayer.Black) { retval = -retval; }
                return retval;
            }

            public static int CompEstScoreSEE(ChessMove move, ChessBoard board)
            {

                int retval = 0;


                ChessPiece mover = board.PieceAt(move.From());
                ChessPiece taken = board.PieceAt(move.To());
                ChessPlayer me = mover.PieceToPlayer();

                int pieceSqVal = 0;
                pieceSqVal -= eval._pcsqPiecePosStage[(int)mover, (int)move.From(), (int)ChessGameStage.Opening];
                pieceSqVal += eval._pcsqPiecePosStage[(int)mover, (int)move.To(), (int)ChessGameStage.Opening];
                if (me == ChessPlayer.Black) { pieceSqVal = -pieceSqVal; }
                retval += pieceSqVal;

                if (taken != ChessPiece.EMPTY)
                {
                    retval += taken.PieceValBasic();
                    //do see
                    var attacks = board.AttacksTo(move.To());
                    attacks &= ~move.From().Bitboard();
                    retval -= attackswap(board, attacks, me.PlayerOther(), move.To(), mover.PieceValBasic());


                }

                return retval;



            }
            static int attackswap(ChessBoard board, ChessBitboard attacks, ChessPlayer player, ChessPosition positionattacked, int pieceontargetval)
            {
                int nextAttackPieceVal = 0;
                ChessPosition nextAttackPos = 0;

                bool HasAttack = attackpop(board, attacks, player, positionattacked, out nextAttackPos, out nextAttackPieceVal);
                if (!HasAttack) { return 0; }

                int moveval = pieceontargetval - attackswap(board, attacks, player.PlayerOther(), positionattacked, nextAttackPieceVal);

                if (moveval > 0)
                {
                    return moveval;
                }
                else
                {
                    return 0;
                }
            }

            static bool attackpop(ChessBoard board, ChessBitboard attacks, ChessPlayer player, ChessPosition positionattacked, out ChessPosition OutFrom, out int OutPieceVal)
            {


                ChessPosition mypawn = 0;
                ChessPosition myknight = 0;
                ChessPosition mybishop = 0;
                ChessPosition myrook = 0;
                ChessPosition myqueen = 0;
                ChessPosition myking = 0;


                if (player == ChessPlayer.White)
                {
                    foreach (ChessPosition attackPos in attacks.ToPositions())
                    {
                        ChessPiece attackPiece = board.PieceAt(attackPos);
                        switch (attackPiece)
                        {
                            case ChessPiece.WPawn:
                                mypawn = attackPos; break;
                            case ChessPiece.WKnight:
                                myknight = attackPos; break;
                            case ChessPiece.WBishop:
                                mybishop = attackPos; break;
                            case ChessPiece.WRook:
                                myrook = attackPos; break;
                            case ChessPiece.WQueen:
                                myqueen = attackPos; break;
                            case ChessPiece.WKing:
                                myking = attackPos; break;
                        }
                    }
                }
                else
                {
                    foreach (ChessPosition attackPos in attacks.ToPositions())
                    {
                        ChessPiece attackPiece = board.PieceAt(attackPos);
                        switch (attackPiece)
                        {
                            case ChessPiece.BPawn:
                                mypawn = attackPos; break;
                            case ChessPiece.BKnight:
                                myknight = attackPos; break;
                            case ChessPiece.BBishop:
                                mybishop = attackPos; break;
                            case ChessPiece.BRook:
                                myrook = attackPos; break;
                            case ChessPiece.BQueen:
                                myqueen = attackPos; break;
                            case ChessPiece.BKing:
                                myking = attackPos; break;
                        }
                    }
                }

                OutFrom = (ChessPosition)(int)-1;
                OutPieceVal = 0;

                if (mypawn != 0)
                {
                    OutFrom = mypawn;
                    OutPieceVal = 100;
                }
                else if (myknight != 0)
                {
                    OutFrom = myknight;
                    OutPieceVal = 300;
                }
                else if (mybishop != 0)
                {
                    OutFrom = mybishop;
                    OutPieceVal = 300;
                }
                else if (myrook != 0)
                {
                    OutFrom = myrook;
                    OutPieceVal = 500;
                }
                else if (myqueen != 0)
                {
                    OutFrom = myqueen;
                    OutPieceVal = 900;
                }
                else if (myking != 0)
                {
                    OutFrom = myking;
                    OutPieceVal = 100000;
                }

                if (OutFrom == 0)
                {
                    //i'm out of attacks to this position;
                    return false;
                }

                ChessDirection addAttackFrom = positionattacked.DirectionTo(OutFrom);
                if (!addAttackFrom.IsDirectionKnight())
                {
                    ChessPosition AddPosition = 0;
                    ChessPiece AddPiece = board.PieceInDirection(OutFrom, addAttackFrom, ref AddPosition);
                    if (addAttackFrom.IsDirectionRook() && AddPiece.PieceIsSliderRook())
                    {
                        attacks |= AddPosition.Bitboard();
                    }
                    else if (addAttackFrom.IsDirectionBishop() && AddPiece.PieceIsSliderBishop())
                    {
                        attacks |= AddPosition.Bitboard();
                    }
                }
                attacks &= ~OutFrom.Bitboard();
                return true;

            }

        }
        private static ChessEval eval = new ChessEval();
    }
}
