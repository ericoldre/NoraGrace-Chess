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
        public const int MAX_ATTACK_COUNT = 3;
        public const int MAX_PIECE_COUNT = 8;

        private long _zobrist;
        private readonly Player _player;
        private readonly Bitboard[] _byPiece_Attacks = new Bitboard[MAX_PIECE_COUNT];
        private readonly Position[] _byPiece_Position = new Position[MAX_PIECE_COUNT];
        private readonly PieceType[] _byPiece_Type = new PieceType[MAX_PIECE_COUNT];

        private readonly int[] _byPiece_TypeIndex = new int[PieceTypeUtil.LookupArrayLength];
        private int _byPiece_Count;

        private readonly Bitboard[] _byPieceType = new Bitboard[PieceTypeUtil.LookupArrayLength];
        private readonly Bitboard[] _lessThan = new Bitboard[PieceTypeUtil.LookupArrayLength];
        private readonly Bitboard[] _counts = new Bitboard[MAX_ATTACK_COUNT + 1];
        


        public long Zobrist { get { return _zobrist; } }
        public Player Player { get { return _player; } }
              
        
        

        public AttackInfo(Player player)
        {
            _player = player;
        }

        public void Initialize(Board board)
        {
            //early exit if already set up for this position.
            if (_zobrist == board.ZobristBoard) { return; }

            //zero out the arrays.
            Array.Clear(_byPiece_Type, 0, MAX_PIECE_COUNT);
            Array.Clear(_byPiece_Position, 0, MAX_PIECE_COUNT);
            Array.Clear(_byPiece_Attacks, 0, MAX_PIECE_COUNT);
            Array.Clear(_byPiece_TypeIndex, 0, PieceTypeUtil.LookupArrayLength);

            Array.Clear(_counts, 0, MAX_ATTACK_COUNT + 1);
            Array.Clear(_lessThan, 0, PieceTypeUtil.LookupArrayLength);
            Array.Clear(_byPieceType, 0, PieceTypeUtil.LookupArrayLength);
            
            

            _zobrist = board.ZobristBoard;


            Bitboard bbPiece;
            Bitboard pattacks;

            int idx = 0;

            //knights
            bbPiece = board[_player] & board[PieceType.Knight];
            _byPiece_TypeIndex[(int)PieceType.Knight] = idx;
            while (bbPiece != Bitboard.Empty)
            {
                Position pos = BitboardUtil.PopFirst(ref bbPiece);
                pattacks = Attacks.KnightAttacks(pos);
                AddPieceTypeAttacks(PieceType.Knight, pattacks);
                AddPieceAttacks(idx, PieceType.Knight, pos, pattacks);
                idx++;
            }

            //bishops
            bbPiece = board[_player] & board[PieceType.Bishop];
            _byPiece_TypeIndex[(int)PieceType.Bishop] = idx;
            while (bbPiece != Bitboard.Empty)
            {
                Position pos = BitboardUtil.PopFirst(ref bbPiece);
                pattacks = Attacks.BishopAttacks(pos, board.PieceLocationsAll);
                AddPieceTypeAttacks(PieceType.Bishop, pattacks);
                AddPieceAttacks(idx, PieceType.Bishop, pos, pattacks);
                idx++;
            }

            //rooks
            bbPiece = board[_player] & board[PieceType.Rook];
            _byPiece_TypeIndex[(int)PieceType.Rook] = idx;
            while (bbPiece != Bitboard.Empty)
            {
                Position pos = BitboardUtil.PopFirst(ref bbPiece);
                pattacks = Attacks.RookAttacks(pos, board.PieceLocationsAll);
                AddPieceTypeAttacks(PieceType.Rook, pattacks);
                AddPieceAttacks(idx, PieceType.Rook, pos, pattacks);
                idx++;
            }

            //queen
            bbPiece = board[_player] & board[PieceType.Queen];
            _byPiece_TypeIndex[(int)PieceType.Queen] = idx;
            while (bbPiece != Bitboard.Empty)
            {
                Position pos = BitboardUtil.PopFirst(ref bbPiece);
                pattacks = Attacks.QueenAttacks(pos, board.PieceLocationsAll);
                AddPieceTypeAttacks(PieceType.Queen, pattacks);
                AddPieceAttacks(idx, PieceType.Queen, pos, pattacks);
                idx++;
            }
            //set last bypiece_index so works with queen
            _byPiece_TypeIndex[(int)PieceType.King] = idx;
            _byPiece_Count = idx;

            //add king attacks
            AddPieceTypeAttacks(PieceType.King, Attacks.KingAttacks(board.KingPosition(_player)));
            
            //add pawn attacks
            bbPiece = board[_player] & board[PieceType.Pawn];
            bbPiece = _player == Engine.Player.White ? bbPiece.ShiftDirN() : bbPiece.ShiftDirS();
            AddPieceTypeAttacks(PieceType.Pawn, bbPiece.ShiftDirE());
            AddPieceTypeAttacks(PieceType.Pawn, bbPiece.ShiftDirW());

            //setup less than attack arrays
            _lessThan[(int)PieceType.Pawn] = Bitboard.Empty;
            _lessThan[(int)PieceType.Knight] = _byPieceType[(int)PieceType.Pawn];
            _lessThan[(int)PieceType.Bishop] = _byPieceType[(int)PieceType.Pawn];
            _lessThan[(int)PieceType.Rook] = _byPieceType[(int)PieceType.Pawn] | _byPieceType[(int)PieceType.Knight] | _byPieceType[(int)PieceType.Bishop];
            _lessThan[(int)PieceType.Queen] = _lessThan[(int)PieceType.Rook] | _byPieceType[(int)PieceType.Rook];
            _lessThan[(int)PieceType.King] = _counts[1];

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddPieceAttacks(int currIndex, PieceType pieceType, Position position, Bitboard attacks)
        {
            //setup bypiece arrays
            _byPiece_Type[currIndex] = pieceType;
            _byPiece_Position[currIndex] = position;
            _byPiece_Attacks[currIndex] = attacks;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddPieceTypeAttacks(PieceType pieceType, Bitboard attacks)
        {
            System.Diagnostics.Debug.Assert(MAX_ATTACK_COUNT == 3);
            _byPieceType[(int)pieceType] |= attacks;
            _counts[3] |= _counts[2] & attacks;
            _counts[2] |= _counts[1] & attacks;
            _counts[1] |= attacks;
        }

        
        public void Verify(Board board)
        {

            for (int i = 0; i < _byPiece_Count; i++)
            {
                PieceType pt = _byPiece_Type[i];
                Position pos = _byPiece_Position[i];
                Bitboard attacks = _byPiece_Attacks[i];
                System.Diagnostics.Debug.Assert(board.PieceAt(pos).ToPieceType() == pt);
            }
        }

        public void PieceInfo(PieceType pieceType, int index, out Position position, out Bitboard attacks)
        {
            System.Diagnostics.Debug.Assert(pieceType >= PieceType.Knight && pieceType <= PieceType.Queen);
            index += _byPiece_TypeIndex[(int)pieceType];
            System.Diagnostics.Debug.Assert(_byPiece_Type[index] == pieceType);
            position = _byPiece_Position[index];
            attacks = _byPiece_Attacks[index];
        }

        public Bitboard SafeMovesFor(PieceType pieceType, AttackInfo hisAttacks)
        {
            System.Diagnostics.Debug.Assert(hisAttacks.Player != Player);
            System.Diagnostics.Debug.Assert(hisAttacks.Zobrist == Zobrist);
            return ~hisAttacks._byPieceType[(int)PieceType.Pawn];
        }
    }
}
