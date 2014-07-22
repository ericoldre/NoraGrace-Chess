using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoraGrace.Engine
{
    public class StaticExchange
    {
        private static readonly int[] pieceVals = new int[PieceTypeInfo.LookupArrayLength];

        static StaticExchange()
        {
            pieceVals[0] = 0;
            pieceVals[(int)PieceType.Pawn] = 100;
            pieceVals[(int)PieceType.Knight] = 300;
            pieceVals[(int)PieceType.Bishop] = 300;
            pieceVals[(int)PieceType.Rook] = 500;
            pieceVals[(int)PieceType.Queen] = 900;
            pieceVals[(int)PieceType.King] = 10000;
        }

        private static int CalculateScoreOld(Move move, Board board)
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
                
            }

            //do see
            var attacks = board.AttacksTo(move.To());
            attacks &= ~(move.From().ToBitboard());
            retval -= attackswap(board, attacks, me.PlayerOther(), move.To(), mover.PieceValBasic());

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

        private readonly int[] array = new int[32];
        private readonly PieceType[] arrayAttacker = new PieceType[32];

        public int CalculateScore(Board board, Move move)
        {
            //return CalculateScoreOld(move, board);
            var moverPos = move.From();
            var targetPos = move.To();
            var moverType = board.PieceAt(moverPos).ToPieceType();
            var targetType = board.PieceAt(targetPos).ToPieceType();

            var moverBB = moverPos.ToBitboard();
            ////early cutoff.
            if (pieceVals[(int)targetType] > pieceVals[(int)moverType])
            {
                return (pieceVals[(int)targetType] - pieceVals[(int)moverType]) + 1;
            }

            var rookMask = Attacks.RookMask(targetPos);
            var bishopMask = Attacks.BishopMask(targetPos);



            Bitboard remainingPieces = board.PieceLocationsAll ^ moverPos.ToBitboard();

            Bitboard attackers = (Attacks.KnightAttacks(targetPos) & board[PieceType.Knight])
                | (Attacks.RookAttacks(targetPos, remainingPieces) & (board[PieceType.Queen] | board[PieceType.Rook]))
                | (Attacks.BishopAttacks(targetPos, remainingPieces) & (board[PieceType.Queen] | board[PieceType.Bishop]))
                | (Attacks.KingAttacks(targetPos) & board[PieceType.King])
                | (Attacks.PawnAttacks(targetPos, Player.Black) & board[Player.White, PieceType.Pawn])
                | (Attacks.PawnAttacks(targetPos, Player.White) & board[Player.Black, PieceType.Pawn]);
            
            attackers &= remainingPieces; //remove moving piece from attacker list.

            var side = board.WhosTurn.PlayerOther();
            var sideAttackers = attackers & board[side];

            if (sideAttackers == 0) 
            { 
                return pieceVals[(int)targetType]; 
            }

            array[0] = pieceVals[(int)targetType];
            arrayAttacker[0] = targetType;

            targetType = moverType;
            int index = 1;
            while (sideAttackers != 0)
            {

                for (moverType = PieceType.Pawn; moverType <= PieceType.King; moverType++)
                {
                    if ((board[moverType] & sideAttackers) != 0)
                    {
                        break;
                    }
                }

                moverPos = (board[moverType] & sideAttackers).NorthMostPosition();

                moverBB = moverPos.ToBitboard();

                remainingPieces ^= moverBB;

                if ((moverBB & rookMask) != 0)
                {
                    attackers |= Attacks.RookAttacks(targetPos, remainingPieces) & remainingPieces & (board.RookSliders);
                }
                else if ((moverBB & bishopMask) != 0)
                {
                    attackers |= Attacks.BishopAttacks(targetPos, remainingPieces) & remainingPieces & (board.BishopSliders);
                }

                attackers &= remainingPieces;

                arrayAttacker[index] = targetType;
                array[index] = (-array[index - 1]) + pieceVals[(int)targetType];
                index++;

                targetType = moverType;
                side = side.PlayerOther();
                sideAttackers = attackers & board[side];

                
            }

            while ((--index) > 0)
            {
                array[index - 1] = Math.Min(-array[index], array[index - 1]);
            }

            int retval = array[0];
            return retval;

        }


    }
}
