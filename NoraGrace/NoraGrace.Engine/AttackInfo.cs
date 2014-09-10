using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace NoraGrace.Engine
{

    public class AttackInfo
    {
        public const int MAX_ATTACK_COUNT = 4;
        public const int MAX_PIECE_COUNT = 8;

        private long _zobrist;
        private readonly Player _player;

        private readonly Bitboard[] _byPieceType = new Bitboard[PieceTypeUtil.LookupArrayLength];
        private readonly Bitboard[] _lessThan = new Bitboard[PieceTypeUtil.LookupArrayLength];
        private readonly Bitboard[] _counts = new Bitboard[MAX_ATTACK_COUNT + 1];
        


        public long Zobrist { get { return _zobrist; } }
        public Player Player { get { return _player; } }
              
        
        

        public AttackInfo(Player player)
        {
            _player = player;
        }

        public bool IsInitialized(Board board)
        {
            return _zobrist == board.ZobristBoard;
        }

        public void Initialize(Board board)
        {
            //early exit if already set up for this position.
            if (_zobrist == board.ZobristBoard) { return; }

            //Array.Clear(_counts, 0, MAX_ATTACK_COUNT + 1);
            //Array.Clear(_lessThan, 0, PieceTypeUtil.LookupArrayLength);
            //Array.Clear(_byPieceType, 0, PieceTypeUtil.LookupArrayLength);

            Bitboard count1 = Bitboard.Empty;
            Bitboard count2 = Bitboard.Empty;
            Bitboard count3 = Bitboard.Empty;
            Bitboard count4 = Bitboard.Empty;

            Bitboard bbPieceAttacks;

            _zobrist = board.ZobristBoard;

            
            Bitboard allPieces = board.PieceLocationsAll;
            Bitboard myPieces = board[_player];
            Bitboard myBishopSliders = myPieces & board.BishopSliders;
            Bitboard myRookSliders = myPieces & board.RookSliders;

            Bitboard bbPiece;
            Bitboard pattacks;
            Position pos;
            

            //knights
            bbPieceAttacks = Bitboard.Empty;
            bbPiece = myPieces & board[PieceType.Knight];
            while (bbPiece != Bitboard.Empty)
            {
                pos = BitboardUtil.PopFirst(ref bbPiece);
                pattacks = Attacks.KnightAttacks(pos);
                bbPieceAttacks |= pattacks;
                count4 |= count3 & pattacks;
                count3 |= count2 & pattacks;
                count2 |= count1 & pattacks;
                count1 |= pattacks;
            }
            _byPieceType[(int)PieceType.Knight] = bbPieceAttacks;

            

            //bishops
            bbPieceAttacks = Bitboard.Empty;
            bbPiece = myPieces & board[PieceType.Bishop];
            while (bbPiece != Bitboard.Empty)
            {
                pos = BitboardUtil.PopFirst(ref bbPiece);
                pattacks = Attacks.BishopAttacks(pos, allPieces);
                bbPieceAttacks |= pattacks;
                pattacks = Attacks.BishopAttacks(pos, allPieces ^ myBishopSliders);
                count4 |= count3 & pattacks;
                count3 |= count2 & pattacks;
                count2 |= count1 & pattacks;
                count1 |= pattacks;
            }
            _byPieceType[(int)PieceType.Bishop] = bbPieceAttacks;

            //rooks
            bbPieceAttacks = Bitboard.Empty;
            bbPiece = myPieces & board[PieceType.Rook];
            while (bbPiece != Bitboard.Empty)
            {
                pos = BitboardUtil.PopFirst(ref bbPiece);
                pattacks = Attacks.RookAttacks(pos, allPieces);
                bbPieceAttacks |= pattacks;
                pattacks = Attacks.RookAttacks(pos, allPieces ^ myRookSliders);
                count4 |= count3 & pattacks;
                count3 |= count2 & pattacks;
                count2 |= count1 & pattacks;
                count1 |= pattacks;
            }
            _byPieceType[(int)PieceType.Rook] = bbPieceAttacks;

            //queen
            bbPieceAttacks = Bitboard.Empty;
            bbPiece = myPieces & board[PieceType.Queen];
            while (bbPiece != Bitboard.Empty)
            {
                pos = BitboardUtil.PopFirst(ref bbPiece);
                pattacks = Attacks.QueenAttacks(pos, allPieces);
                bbPieceAttacks |= pattacks;
                pattacks = Attacks.BishopAttacks(pos, allPieces ^ myBishopSliders);
                pattacks |= Attacks.RookAttacks(pos, allPieces ^ myRookSliders);
                count4 |= count3 & pattacks;
                count3 |= count2 & pattacks;
                count2 |= count1 & pattacks;
                count1 |= pattacks;
            }
            _byPieceType[(int)PieceType.Queen] = bbPieceAttacks;


            //add king attacks
            pattacks = Attacks.KingAttacks(board.KingPosition(_player));
            _byPieceType[(int)PieceType.King] = pattacks;
            count4 |= count3 & pattacks;
            count3 |= count2 & pattacks;
            count2 |= count1 & pattacks;
            count1 |= pattacks;

            //add pawn attacks
            bbPieceAttacks = Bitboard.Empty;
            bbPiece = myPieces & board[PieceType.Pawn];
            bbPiece = _player == Engine.Player.White ? bbPiece.ShiftDirN() : bbPiece.ShiftDirS();

            //pawn attacks west;
            pattacks = bbPiece.ShiftDirW();
            bbPieceAttacks |= pattacks;
            count4 |= count3 & pattacks;
            count3 |= count2 & pattacks;
            count2 |= count1 & pattacks;
            count1 |= pattacks;

            //pawn attacks east;
            pattacks = bbPiece.ShiftDirE();
            bbPieceAttacks |= pattacks;
            count4 |= count3 & pattacks;
            count3 |= count2 & pattacks;
            count2 |= count1 & pattacks;
            count1 |= pattacks;

            _byPieceType[(int)PieceType.Pawn] = bbPieceAttacks;

            //copy to counts array
            _counts[0] = ~count1;
            _counts[1] = count1;
            _counts[2] = count2;
            _counts[3] = count3;
            _counts[4] = count4;

            //setup less than attack arrays
            _lessThan[(int)PieceType.Pawn] = Bitboard.Empty;
            _lessThan[(int)PieceType.Knight] = _byPieceType[(int)PieceType.Pawn];
            _lessThan[(int)PieceType.Bishop] = _byPieceType[(int)PieceType.Pawn];
            _lessThan[(int)PieceType.Rook] = _byPieceType[(int)PieceType.Pawn] | _byPieceType[(int)PieceType.Knight] | _byPieceType[(int)PieceType.Bishop];
            _lessThan[(int)PieceType.Queen] = _lessThan[(int)PieceType.Rook] | _byPieceType[(int)PieceType.Rook];
            _lessThan[(int)PieceType.King] = _counts[1];

        }



        public Bitboard SafeMovesFor(PieceType pieceType, AttackInfo hisAttacks)
        {
            System.Diagnostics.Debug.Assert(hisAttacks.Player != Player);
            System.Diagnostics.Debug.Assert(hisAttacks.Zobrist == Zobrist);
            return ~hisAttacks._byPieceType[(int)PieceType.Pawn];
        }

        public int AttackCountTo(Position pos)
        {
            int retval = 0;
            Bitboard bb = pos.ToBitboard();
            retval += (int)((ulong)(_counts[1] & bb) >> (int)pos);
            retval += (int)((ulong)(_counts[2] & bb) >> (int)pos);
            retval += (int)((ulong)(_counts[3] & bb) >> (int)pos);
            retval += (int)((ulong)(_counts[4] & bb) >> (int)pos);
            return retval;
        }

        public Bitboard ByCount(int count)
        {
            return _counts[count];
        }

        public Bitboard All
        {
            get
            {
                return _counts[1];
            }
        }

        public Bitboard ByPieceType(PieceType pt)
        {
            return _byPieceType[(int)pt];
        }
        public Bitboard LessThan(PieceType pt)
        {
            return _lessThan[(int)pt];
        }

    
    }
}
