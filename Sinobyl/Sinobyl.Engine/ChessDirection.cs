using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{
    public enum ChessDirection
    {
        DirN = -8, DirE = 1, DirS = 8, DirW = -1,
        DirNE = DirN + DirE, DirSE = DirS + DirE, DirSW = DirS + DirW, DirNW = DirN + DirW,
        DirNNE = DirN + DirNE, DirEEN = DirE + DirNE, DirEES = DirE + DirSE, DirSSE = DirS + DirSE, DirSSW = DirS + DirSW, DirWWS = DirW + DirSW, DirWWN = DirW + DirNW, DirNNW = DirN + DirNW
    }

    public static class ChessDirectionInfo
    {

        public static readonly ChessDirection[] AllDirections = new ChessDirection[]{
			ChessDirection.DirN, ChessDirection.DirE, ChessDirection.DirS, ChessDirection.DirW, ChessDirection.DirNE, ChessDirection.DirSE, ChessDirection.DirSW, ChessDirection.DirNW,
			ChessDirection.DirNNE, ChessDirection.DirEEN, ChessDirection.DirEES, ChessDirection.DirSSE, ChessDirection.DirSSW, ChessDirection.DirWWS, ChessDirection.DirWWN, ChessDirection.DirNNW};

        public static readonly ChessDirection[] AllDirectionsKnight = new ChessDirection[]{
			ChessDirection.DirNNE, ChessDirection.DirEEN, ChessDirection.DirEES, ChessDirection.DirSSE, ChessDirection.DirSSW, ChessDirection.DirWWS, ChessDirection.DirWWN, ChessDirection.DirNNW};

        public static readonly ChessDirection[] AllDirectionsRook = new ChessDirection[]{
			ChessDirection.DirN, ChessDirection.DirE, ChessDirection.DirS, ChessDirection.DirW};

        public static readonly ChessDirection[] AllDirectionsBishop = new ChessDirection[]{
			ChessDirection.DirNE, ChessDirection.DirSE, ChessDirection.DirSW, ChessDirection.DirNW};

        public static readonly ChessDirection[] AllDirectionsQueen = new ChessDirection[]{
			ChessDirection.DirN, ChessDirection.DirE, ChessDirection.DirS, ChessDirection.DirW, ChessDirection.DirNE, ChessDirection.DirSE, ChessDirection.DirSW, ChessDirection.DirNW};


        public static bool IsDirectionRook(this ChessDirection dir)
        {
            //AssertDirection(dir);
            switch (dir)
            {
                case ChessDirection.DirN:
                case ChessDirection.DirE:
                case ChessDirection.DirS:
                case ChessDirection.DirW:
                    return true;
                default:
                    return false;
            }
        }
        public static bool IsDirectionBishop(this ChessDirection dir)
        {
            //AssertDirection(dir);
            switch (dir)
            {
                case ChessDirection.DirNW:
                case ChessDirection.DirNE:
                case ChessDirection.DirSW:
                case ChessDirection.DirSE:
                    return true;
                default:
                    return false;
            }
        }
        public static bool IsDirectionKnight(this ChessDirection dir)
        {
            //AssertDirection(dir);
            switch (dir)
            {
                case ChessDirection.DirNNE:
                case ChessDirection.DirEEN:
                case ChessDirection.DirEES:
                case ChessDirection.DirSSE:
                case ChessDirection.DirSSW:
                case ChessDirection.DirWWS:
                case ChessDirection.DirWWN:
                case ChessDirection.DirNNW:
                    return true;
                default:
                    return false;
            }
        }

        public static ChessDirection Opposite(this ChessDirection dir)
        {
            return (ChessDirection)(-(int)dir);
        }

    }
}
