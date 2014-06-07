using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinobyl.Engine
{
    public static class StaticExchange
    {
        public static int CalculateScore(Move move, Board board)
        {
            System.Diagnostics.Debug.Assert(move != Move.EMPTY);
            //System.Diagnostics.Debug.Assert(ChessMove.GenMoves(board).Contains(move));

            int retval = 0;


            Piece mover = board.PieceAt(move.From());
            Piece taken = board.PieceAt(move.To());
            Player me = mover.PieceToPlayer();



            if (taken != Piece.EMPTY)
            {
                retval += taken.PieceValBasic();
                //do see
                var attacks = board.AttacksTo(move.To());
                attacks &= ~(move.From().ToBitboard());
                retval -= attackswap(board, attacks, me.PlayerOther(), move.To(), mover.PieceValBasic());
            }

            //int pieceSqVal = 0;
            //pieceSqVal -= eval._pcsqPiecePosStage[(int)mover, (int)move.From, (int)ChessGameStage.Opening];
            //pieceSqVal += eval._pcsqPiecePosStage[(int)mover, (int)move.To, (int)ChessGameStage.Opening];
            //if (me == ChessPlayer.Black) { pieceSqVal = -pieceSqVal; }
            //retval += pieceSqVal;


            return retval;
        }
        static int attackswap(Board board, Bitboard attacks, Player player, Position positionattacked, int pieceontargetval)
        {
            int nextAttackPieceVal = 0;
            Position nextAttackPos = 0;

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

        static bool attackpop(Board board, ref Bitboard attacks, Player player, Position positionattacked, out Position OutFrom, out int OutPieceVal)
        {

            OutFrom = Position.OUTOFBOUNDS;
            OutPieceVal = 0;

            Bitboard myAttacks = attacks & board[player];
            if ((myAttacks & board[PieceType.Pawn]) != 0)
            {
                OutFrom = (myAttacks & board[PieceType.Pawn]).NorthMostPosition();
                OutPieceVal = 100;
            }
            else if ((myAttacks & board[PieceType.Knight]) != 0)
            {
                OutFrom = (myAttacks & board[PieceType.Knight]).NorthMostPosition();
                OutPieceVal = 300;
            }
            else if ((myAttacks & board[PieceType.Bishop]) != 0)
            {
                OutFrom = (myAttacks & board[PieceType.Bishop]).NorthMostPosition();
                OutPieceVal = 300;
            }
            else if ((myAttacks & board[PieceType.Rook]) != 0)
            {
                OutFrom = (myAttacks & board[PieceType.Rook]).NorthMostPosition();
                OutPieceVal = 500;
            }
            else if ((myAttacks & board[PieceType.Queen]) != 0)
            {
                OutFrom = (myAttacks & board[PieceType.Queen]).NorthMostPosition();
                OutPieceVal = 900;
            }
            else if ((myAttacks & board[PieceType.King]) != 0)
            {
                OutFrom = (myAttacks & board[PieceType.King]).NorthMostPosition();
                OutPieceVal = 100000;
            }

            if (OutFrom == Position.OUTOFBOUNDS)
            {
                //i'm out of attacks to this position;
                return false;
            }

            Direction addAttackFrom = positionattacked.DirectionTo(OutFrom);
            if (!addAttackFrom.IsDirectionKnight())
            {
                Position AddPosition = 0;
                Piece AddPiece = board.PieceInDirection(OutFrom, addAttackFrom, ref AddPosition);
                if (addAttackFrom.IsDirectionRook() && AddPiece.PieceIsSliderRook())
                {
                    attacks |= AddPosition.ToBitboard();
                }
                else if (addAttackFrom.IsDirectionBishop() && AddPiece.PieceIsSliderBishop())
                {
                    attacks |= AddPosition.ToBitboard();
                }
            }
            attacks &= ~OutFrom.ToBitboard();
            return true;

        }

    }
}
