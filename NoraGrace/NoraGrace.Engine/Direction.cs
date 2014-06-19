using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoraGrace.Engine
{
    public enum Direction
    {
        DirN = -8, DirE = 1, DirS = 8, DirW = -1,
        DirNE = DirN + DirE, DirSE = DirS + DirE, DirSW = DirS + DirW, DirNW = DirN + DirW,
        DirNNE = DirN + DirNE, DirEEN = DirE + DirNE, DirEES = DirE + DirSE, DirSSE = DirS + DirSE, DirSSW = DirS + DirSW, DirWWS = DirW + DirSW, DirWWN = DirW + DirNW, DirNNW = DirN + DirNW
    }

    public static class DirectionInfo
    {

        public static readonly Direction[] AllDirections = new Direction[]{
			Direction.DirN, Direction.DirE, Direction.DirS, Direction.DirW, Direction.DirNE, Direction.DirSE, Direction.DirSW, Direction.DirNW,
			Direction.DirNNE, Direction.DirEEN, Direction.DirEES, Direction.DirSSE, Direction.DirSSW, Direction.DirWWS, Direction.DirWWN, Direction.DirNNW};

        public static readonly Direction[] AllDirectionsKnight = new Direction[]{
			Direction.DirNNE, Direction.DirEEN, Direction.DirEES, Direction.DirSSE, Direction.DirSSW, Direction.DirWWS, Direction.DirWWN, Direction.DirNNW};

        public static readonly Direction[] AllDirectionsRook = new Direction[]{
			Direction.DirN, Direction.DirE, Direction.DirS, Direction.DirW};

        public static readonly Direction[] AllDirectionsBishop = new Direction[]{
			Direction.DirNE, Direction.DirSE, Direction.DirSW, Direction.DirNW};

        public static readonly Direction[] AllDirectionsQueen = new Direction[]{
			Direction.DirN, Direction.DirE, Direction.DirS, Direction.DirW, Direction.DirNE, Direction.DirSE, Direction.DirSW, Direction.DirNW};


        public static bool IsDirectionRook(this Direction dir)
        {
            //AssertDirection(dir);
            switch (dir)
            {
                case Direction.DirN:
                case Direction.DirE:
                case Direction.DirS:
                case Direction.DirW:
                    return true;
                default:
                    return false;
            }
        }
        public static bool IsDirectionBishop(this Direction dir)
        {
            //AssertDirection(dir);
            switch (dir)
            {
                case Direction.DirNW:
                case Direction.DirNE:
                case Direction.DirSW:
                case Direction.DirSE:
                    return true;
                default:
                    return false;
            }
        }
        public static bool IsDirectionKnight(this Direction dir)
        {
            //AssertDirection(dir);
            switch (dir)
            {
                case Direction.DirNNE:
                case Direction.DirEEN:
                case Direction.DirEES:
                case Direction.DirSSE:
                case Direction.DirSSW:
                case Direction.DirWWS:
                case Direction.DirWWN:
                case Direction.DirNNW:
                    return true;
                default:
                    return false;
            }
        }

        public static Direction Opposite(this Direction dir)
        {
            return (Direction)(-(int)dir);
        }

    }
}
