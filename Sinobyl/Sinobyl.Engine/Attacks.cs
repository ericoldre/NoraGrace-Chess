using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{
    public static class Attacks
    {

        private static readonly ChessBitboard[] _attacks_from_knight_lu = new ChessBitboard[65];
        private static readonly ChessBitboard[] _attacks_from_king_lu = new ChessBitboard[65];
        private static readonly ChessBitboard[][] _attacks_from_pawn_lu = new ChessBitboard[2][];
        private static readonly ChessBitboard[][] _attacks_from_pawn_flood_lu = new ChessBitboard[2][];
        //private static readonly ChessBitboard[] _attacks_from_bpawn_lu = new ChessBitboard[65];

        


        static Attacks()
        {
            
            //knight attacks
            foreach (var sq in ChessPositionInfo.AllPositions)
            {
                ChessBitboard board = 0;
                foreach (var dir in ChessDirectionInfo.AllDirectionsKnight)
                {
                    board |= sq.PositionInDirection(dir).Bitboard();
                }
                _attacks_from_knight_lu[sq.GetIndex64()] = board;
            }
            
            //king attacks
            foreach (var sq in ChessPositionInfo.AllPositions)
            {
                ChessBitboard board = 0;
                foreach (var dir in ChessDirectionInfo.AllDirectionsQueen)
                {
                    board |= sq.PositionInDirection(dir).Bitboard();
                }
                _attacks_from_king_lu[sq.GetIndex64()] = board;
            }

            //pawn attacks
            foreach(ChessPlayer player in ChessPlayerInfo.AllPlayers)
            {
                _attacks_from_pawn_lu[(int)player] = new ChessBitboard[65];
                _attacks_from_pawn_flood_lu[(int)player] = new ChessBitboard[65];
                foreach (var sq in ChessPositionInfo.AllPositions)
                {
                    ChessBitboard board = 0;
                    board |= sq.PositionInDirection(player.MyNorth()).PositionInDirection(ChessDirection.DirE).Bitboard();
                    board |= sq.PositionInDirection(player.MyNorth()).PositionInDirection(ChessDirection.DirW).Bitboard();

                    _attacks_from_pawn_lu[(int)player][sq.GetIndex64()] = board;
                    board = board.Flood(player.MyNorth());
                    _attacks_from_pawn_flood_lu[(int)player][sq.GetIndex64()] = board;
                }
            }
        }


        public static ChessBitboard KnightAttacks(ChessPosition from)
        {
            return _attacks_from_knight_lu[from.GetIndex64()];
        }

        public static ChessBitboard KingAttacks(ChessPosition from)
        {
            return _attacks_from_king_lu[from.GetIndex64()];
        }

        public static ChessBitboard PawnAttacks(ChessPosition from, ChessPlayer player)
        {
            return _attacks_from_pawn_lu[(int)player][(int)from];
        }

        public static ChessBitboard PawnAttacksFlood(ChessPosition from, ChessPlayer player)
        {
            return _attacks_from_pawn_flood_lu[(int)player][(int)from];
        }
        
        
    }
}
