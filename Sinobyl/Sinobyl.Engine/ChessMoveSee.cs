using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinobyl.Engine
{
    public static class ChessMoveSEE
    {
        public static int CompEstScoreSEE(ChessMove move, ChessBoard board)
        {
            System.Diagnostics.Debug.Assert(move != ChessMove.EMPTY);
            //System.Diagnostics.Debug.Assert(ChessMove.GenMoves(board).Contains(move));

            int retval = 0;


            ChessPiece mover = board.PieceAt(move.From());
            ChessPiece taken = board.PieceAt(move.To());
            ChessPlayer me = mover.PieceToPlayer();



            if (taken != ChessPiece.EMPTY)
            {
                retval += taken.PieceValBasic();
                //do see
                var attacks = board.AttacksTo(move.To());
                attacks &= ~(move.From().Bitboard());
                retval -= attackswap(board, attacks, me.PlayerOther(), move.To(), mover.PieceValBasic());
            }

            //int pieceSqVal = 0;
            //pieceSqVal -= eval._pcsqPiecePosStage[(int)mover, (int)move.From, (int)ChessGameStage.Opening];
            //pieceSqVal += eval._pcsqPiecePosStage[(int)mover, (int)move.To, (int)ChessGameStage.Opening];
            //if (me == ChessPlayer.Black) { pieceSqVal = -pieceSqVal; }
            //retval += pieceSqVal;


            return retval;
        }
        static int attackswap(ChessBoard board, ChessBitboard attacks, ChessPlayer player, ChessPosition positionattacked, int pieceontargetval)
        {
            int nextAttackPieceVal = 0;
            ChessPosition nextAttackPos = 0;

            bool HasAttack = attackpop(board, ref attacks, player, positionattacked, out nextAttackPos, out nextAttackPieceVal);
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

        static bool attackpop(ChessBoard board, ref ChessBitboard attacks, ChessPlayer player, ChessPosition positionattacked, out ChessPosition OutFrom, out int OutPieceVal)
        {

            OutFrom = ChessPosition.OUTOFBOUNDS;
            OutPieceVal = 0;

            ChessBitboard myAttacks = attacks & board[player];
            if ((myAttacks & board[ChessPieceType.Pawn]) != 0)
            {
                OutFrom = (myAttacks & board[ChessPieceType.Pawn]).NorthMostPosition();
                OutPieceVal = 100;
            }
            else if ((myAttacks & board[ChessPieceType.Knight]) != 0)
            {
                OutFrom = (myAttacks & board[ChessPieceType.Knight]).NorthMostPosition();
                OutPieceVal = 300;
            }
            else if ((myAttacks & board[ChessPieceType.Bishop]) != 0)
            {
                OutFrom = (myAttacks & board[ChessPieceType.Bishop]).NorthMostPosition();
                OutPieceVal = 300;
            }
            else if ((myAttacks & board[ChessPieceType.Rook]) != 0)
            {
                OutFrom = (myAttacks & board[ChessPieceType.Rook]).NorthMostPosition();
                OutPieceVal = 500;
            }
            else if ((myAttacks & board[ChessPieceType.Queen]) != 0)
            {
                OutFrom = (myAttacks & board[ChessPieceType.Queen]).NorthMostPosition();
                OutPieceVal = 900;
            }
            else if ((myAttacks & board[ChessPieceType.King]) != 0)
            {
                OutFrom = (myAttacks & board[ChessPieceType.King]).NorthMostPosition();
                OutPieceVal = 100000;
            }

            if (OutFrom == ChessPosition.OUTOFBOUNDS)
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
}
