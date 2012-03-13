using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{
    public static class Attacks
    {
        private static readonly ChessBitboard[,] _attacks_from_horiz_lu = new ChessBitboard[65, 64];
        private static readonly ChessBitboard[,] _attacks_from_vert_lu = new ChessBitboard[65, 64];
        private static readonly ChessBitboard[,] _attacks_from_diaga8_lu = new ChessBitboard[65, 64];
        private static readonly ChessBitboard[,] _attacks_from_diagh8_lu = new ChessBitboard[65, 64];
        private static readonly ChessBitboard[] _attacks_from_knight_lu = new ChessBitboard[65];
        private static readonly ChessBitboard[] _attacks_from_king_lu = new ChessBitboard[65];
        private static readonly ChessBitboard[] _attacks_from_wpawn_lu = new ChessBitboard[65];
        private static readonly ChessBitboard[] _attacks_from_bpawn_lu = new ChessBitboard[65];

        public static int[] _attacks_from_horiz_offset = {
	        1, 1, 1, 1, 1, 1, 1, 1,
	        9, 9, 9, 9, 9, 9, 9, 9,
	        17,17,17,17,17,17,17,17,
	        25,25,25,25,25,25,25,25,
	        33,33,33,33,33,33,33,33,
	        41,41,41,41,41,41,41,41,
	        49,49,49,49,49,49,49,49,
	        57,57,57,57,57,57,57,57
        };

        public static int[] _attacks_from_vert_offset = {
	        1, 9, 17, 25, 33, 41, 49, 57,
	        1, 9, 17, 25, 33, 41, 49, 57,
	        1, 9, 17, 25, 33, 41, 49, 57,
	        1, 9, 17, 25, 33, 41, 49, 57,
	        1, 9, 17, 25, 33, 41, 49, 57,
	        1, 9, 17, 25, 33, 41, 49, 57,
	        1, 9, 17, 25, 33, 41, 49, 57,
	        1, 9, 17, 25, 33, 41, 49, 57
        };

        public static int[] _attacks_from_diaga8_offset = {
	         1,  2,  4,  7, 11, 16, 22, 29,
	         2,  4,  7, 11, 16, 22, 29, 37,
	         4,  7, 11, 16, 22, 29, 37, 44,
	         7, 11, 16, 22, 29, 37, 44, 50,
	        11, 16, 22, 29, 37, 44, 50, 55,
	        16, 22, 29, 37, 44, 50, 55, 59,
	        22, 29, 37, 44, 50, 55, 59, 62,
	        29, 37, 44, 50, 55, 59, 62, 64
        };

        public static int[] _attacks_from_diagh8_offset = {
	        29, 22, 16, 11,  7,  4,  2,  1,
	        37, 29, 22, 16, 11,  7,  4,  2,
	        44, 37, 29, 22, 16, 11,  7,  4,
	        50, 44, 37, 29, 22, 16, 11,  7,
	        55, 50, 44, 37, 29, 22, 16, 11,
	        59, 55, 50, 44, 37, 29, 22, 16,
	        62, 59, 55, 50, 44, 37, 29, 22,
	        64, 62, 59, 55, 50, 44, 37, 29
        };

        public static int[] _position_translate_vert = {
	        0, 8,  16, 24, 32, 40, 48, 56,
	        1, 9,  17, 25, 33, 41, 49, 57,
	        2, 10, 18, 26, 34, 42, 50, 58,
	        3, 11, 19, 27, 35, 43, 51, 59,
	        4, 12, 20, 28, 36, 44, 52, 60,
	        5, 13, 21, 29, 37, 45, 53, 61,
	        6, 14, 22, 30, 38, 46, 54, 62,
	        7, 15, 23, 31, 39, 47, 55, 63,
	        64};

        public static int[] _position_translate_diaga8 =
        {
	        0,   1,  3,  6, 10, 15, 21, 28, 
	        2,   4,  7, 11, 16, 22, 29, 36,
	        5,   8, 12, 17, 23, 30, 37, 43, 
	        9,  13, 18, 24, 31, 38, 44, 49, 
	        14, 19, 25, 32, 39, 45, 50, 54, 
	        20, 26, 33, 40, 46, 51, 55, 58,
	        27, 34, 41, 47, 52, 56, 59, 61,
	        35, 42, 48, 53, 57, 60, 62, 63,
	        64
        };

        public static int[] _position_translate_diagh8 = {
	        28, 21, 15, 10,  6,  3,  1,  0, 
	        36, 29, 22, 16, 11,  7,  4,  2,
	        43, 37, 30, 23, 17, 12,  8,  5, 
	        49, 44, 38, 31, 24, 18, 13,  9, 
	        54, 50, 45, 39, 32, 25, 19, 14, 
	        58, 55, 51, 46, 40, 33, 26, 20,
	        61, 59, 56, 52, 47, 41, 34, 27,
	        63, 62, 60, 57, 53, 48, 42, 35,
	    64};

        [Flags]
        public enum ChessBitboardRotatedVert : ulong {}

        [Flags]
        public enum ChessBitboardRotatedA1H8 : ulong { }

        [Flags]
        public enum ChessBitboardRotatedH1A8 : ulong { }


        static Attacks()
        {
            
            //horizontal attacks
            foreach(var sq in Chess.AllPositions)
            {
                for (ulong pattern = 0; pattern <= 63; pattern++)
                {
                    int sqIdx = sq.GetIndex64();
                    int offset = _attacks_from_horiz_offset[sqIdx];
                    var piece_locations = (ChessBitboard)(pattern << offset);
                    var tmpattacks = bitboard_to_attacks_calc(piece_locations, ChessDirection.DirE, sq);
                    tmpattacks = tmpattacks | bitboard_to_attacks_calc(piece_locations, ChessDirection.DirW, sq);
                    _attacks_from_horiz_lu[sq.GetIndex64(), (int)pattern] = tmpattacks;
                }
            }

            //vertical attacks
            foreach (var sq in Chess.AllPositions)
            {
                for (ulong pattern = 0; pattern <= 63; pattern++)
                {
                    int sqIdx = sq.GetIndex64();
                    int offset = _attacks_from_vert_offset[sqIdx];
                    var rotatedBoard = (ChessBitboardRotatedVert)(pattern << offset);
                    var restoredBoard = RotateVertReverse(rotatedBoard);

                    var tmpattacks = bitboard_to_attacks_calc(restoredBoard, ChessDirection.DirN, sq);
                    tmpattacks = tmpattacks | bitboard_to_attacks_calc(restoredBoard, ChessDirection.DirS, sq);
                    _attacks_from_vert_lu[sq.GetIndex64(), (int)pattern] = tmpattacks;
                }
            }
            //Diag a1h8
            foreach (var sq in Chess.AllPositions)
            {
                for (ulong pattern = 0; pattern <= 63; pattern++)
                {
                    int sqIdx = sq.GetIndex64();
                    int offset = _attacks_from_diagh8_offset[sqIdx];
                    var rotatedPieceLocations = (ChessBitboardRotatedA1H8)(pattern << offset);
                    var restoredPieceLocations = RotateDiagA1H8Reverse(rotatedPieceLocations);

                    var tmpattacks = bitboard_to_attacks_calc(restoredPieceLocations, ChessDirection.DirNE, sq);
                    tmpattacks = tmpattacks | bitboard_to_attacks_calc(restoredPieceLocations, ChessDirection.DirSW, sq);
                    _attacks_from_diagh8_lu[sqIdx, (int)pattern] = tmpattacks;
                }
            }

            //diag h1a8
            foreach (var sq in Chess.AllPositions)
            {
                for (ulong pattern = 0; pattern <= 63; pattern++)
                {
                    int sqIdx = sq.GetIndex64();
                    int offset = _attacks_from_diaga8_offset[sqIdx];
                    var rotatedPieceLocations = (ChessBitboardRotatedH1A8)(pattern << offset);
                    var restoredPieceLocations = RotateDiagH1A8Reverse(rotatedPieceLocations);

                    var tmpattacks = bitboard_to_attacks_calc(restoredPieceLocations, ChessDirection.DirSE, sq);
                    tmpattacks = tmpattacks | bitboard_to_attacks_calc(restoredPieceLocations, ChessDirection.DirNW, sq);
                    _attacks_from_diaga8_lu[sqIdx, (int)pattern] = tmpattacks;
                }
            }

            //knight attacks
            foreach (var sq in Chess.AllPositions)
            {
                ChessBitboard board = 0;
                foreach (var dir in Chess.AllDirectionsKnight)
                {
                    board |= Chess.PositionInDirection(sq, dir).Bitboard();
                }
                _attacks_from_knight_lu[sq.GetIndex64()] = board;
            }
            
            //king attacks
            foreach (var sq in Chess.AllPositions)
            {
                ChessBitboard board = 0;
                foreach (var dir in Chess.AllDirectionsQueen)
                {
                    board |= Chess.PositionInDirection(sq, dir).Bitboard();
                }
                _attacks_from_king_lu[sq.GetIndex64()] = board;
            }

            //wpawn attacks
            foreach (var sq in Chess.AllPositions)
            {
                ChessBitboard board = 0;
                board |= Chess.PositionInDirection(sq, ChessDirection.DirNE).Bitboard();
                board |= Chess.PositionInDirection(sq, ChessDirection.DirNW).Bitboard();
                _attacks_from_wpawn_lu[sq.GetIndex64()] = board;
            }
            //wpawn attacks
            foreach (var sq in Chess.AllPositions)
            {
                ChessBitboard board = 0;
                board |= Chess.PositionInDirection(sq, ChessDirection.DirSE).Bitboard();
                board |= Chess.PositionInDirection(sq, ChessDirection.DirSW).Bitboard();
                _attacks_from_wpawn_lu[sq.GetIndex64()] = board;
            }
        }

        //used to help initialize attacks from static constructor.
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

        public static ChessBitboard HorizAttacks(ChessPosition position, ChessBitboard allPieces)
        {
            ulong pattern = ((((ulong)allPieces)>>_attacks_from_horiz_offset[position.GetIndex64()])&63);
            return _attacks_from_horiz_lu[position.GetIndex64(), (int)pattern];
        }
        public static ChessBitboard VertAttacks(ChessPosition position, ChessBitboardRotatedVert allPiecesVert)
        {
            ulong pattern = ((((ulong)allPiecesVert) >> _attacks_from_vert_offset[position.GetIndex64()]) & 63);
            return _attacks_from_vert_lu[position.GetIndex64(), (int)pattern];
        }
        public static ChessBitboard DiagA1H8Attacks(ChessPosition position, ChessBitboardRotatedA1H8 rotatedBoard)
        {
            int posIdx = position.GetIndex64();
            int shift = _attacks_from_diagh8_offset[posIdx];
            ulong pattern = ((((ulong)rotatedBoard) >> shift) & 63);
            return _attacks_from_diagh8_lu[posIdx, (int)pattern];
        }
        public static ChessBitboard DiagH1A8Attacks(ChessPosition position, ChessBitboardRotatedH1A8 rotatedBoard)
        {
            int posIdx = position.GetIndex64();
            int shift = _attacks_from_diaga8_offset[posIdx];
            ulong pattern = ((((ulong)rotatedBoard) >> shift) & 63);
            return _attacks_from_diaga8_lu[posIdx, (int)pattern];
        }

        public static ChessBitboardRotatedVert RotateVert(ChessBitboard bitboard)
        {
            ChessBitboard retval = 0;
            foreach (var pos in bitboard.ToPositions())
            {
                int vertIndex = _position_translate_vert[pos.GetIndex64()];
                ChessPosition transVert = Chess.AllPositions[vertIndex];
                retval |= (ChessBitboard)Chess.AllPositions[transVert.GetIndex64()].Bitboard();
            }
            return (ChessBitboardRotatedVert)retval;
        }

        public static ChessBitboard RotateVertReverse(ChessBitboardRotatedVert rotatedBitboard)
        {
            ChessBitboard retval = 0;
            foreach (var pos in ((ChessBitboard)rotatedBitboard).ToPositions())
            {
                int posidx = 0;
                for (posidx = 0; posidx < 65; posidx++)
                {
                    if (pos.GetIndex64() == _position_translate_vert[posidx]) { break; }
                }
                retval |= Chess.AllPositions[posidx].Bitboard();
            }
            return retval;
        }

        public static ChessBitboardRotatedA1H8 RotateDiagA1H8(ChessBitboard bitboard)
        {
            ChessBitboard retval = 0;
            foreach (var pos in bitboard.ToPositions())
            {
                int vertIndex = _position_translate_diagh8[pos.GetIndex64()];
                ChessPosition transVert = Chess.AllPositions[vertIndex];
                retval |= (ChessBitboard)Chess.AllPositions[transVert.GetIndex64()].Bitboard();
            }
            return (ChessBitboardRotatedA1H8)retval;
        }
        public static ChessBitboard RotateDiagA1H8Reverse(ChessBitboardRotatedA1H8 rotatedBitboard)
        {
            ChessBitboard retval = 0;
            foreach (var pos in ((ChessBitboard)rotatedBitboard).ToPositions())
            {
                int posidx=0;
                for(posidx = 0; posidx < 65; posidx++)
                {
                    if (pos.GetIndex64() == _position_translate_diagh8[posidx]) { break; }
                }
                retval |= Chess.AllPositions[posidx].Bitboard();
            }
            return retval;
        }

        public static ChessBitboardRotatedH1A8 RotateDiagH1A8(ChessBitboard bitboard)
        {
            ChessBitboard retval = 0;
            foreach (var pos in bitboard.ToPositions())
            {
                int vertIndex = _position_translate_diaga8[pos.GetIndex64()];
                ChessPosition transVert = Chess.AllPositions[vertIndex];
                retval |= (ChessBitboard)Chess.AllPositions[transVert.GetIndex64()].Bitboard();
            }
            return (ChessBitboardRotatedH1A8)retval;
        }
        public static ChessBitboard RotateDiagH1A8Reverse(ChessBitboardRotatedH1A8 rotatedBitboard)
        {
            ChessBitboard retval = 0;
            foreach (var pos in ((ChessBitboard)rotatedBitboard).ToPositions())
            {
                int posidx = 0;
                for (posidx = 0; posidx < 65; posidx++)
                {
                    if (pos.GetIndex64() == _position_translate_diaga8[posidx]) { break; }
                }
                retval |= Chess.AllPositions[posidx].Bitboard();
            }
            return retval;
        }
        
        
    }
}
