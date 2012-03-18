using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{
    public enum ChessDirection
    {
        DirN = 8, DirE = 1, DirS = -8, DirW = -1,
        DirNE = DirN + DirE, DirSE = DirS + DirE, DirSW = DirS + DirW, DirNW = DirN + DirW,
        DirNNE = DirN + DirNE, DirEEN = DirE + DirNE, DirEES = DirE + DirSE, DirSSE = DirS + DirSE, DirSSW = DirS + DirSW, DirWWS = DirW + DirSW, DirWWN = DirW + DirNW, DirNNW = DirN + DirNW
    }

    public static class ExtensionsChessDirection
    {
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
