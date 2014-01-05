using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{
    public enum ChessDirectionValue
    {
        DirN = -8, DirE = 1, DirS = 8, DirW = -1,
        DirNE = DirN + DirE, DirSE = DirS + DirE, DirSW = DirS + DirW, DirNW = DirN + DirW,
        DirNNE = DirN + DirNE, DirEEN = DirE + DirNE, DirEES = DirE + DirSE, DirSSE = DirS + DirSE, DirSSW = DirS + DirSW, DirWWS = DirW + DirSW, DirWWN = DirW + DirNW, DirNNW = DirN + DirNW
    }

    public struct ChessDirection
    {
        public static readonly ChessDirection DirN = new ChessDirection(ChessDirectionValue.DirN);
        public static readonly ChessDirection DirE = new ChessDirection(ChessDirectionValue.DirE);
        public static readonly ChessDirection DirS = new ChessDirection(ChessDirectionValue.DirS);
        public static readonly ChessDirection DirW = new ChessDirection(ChessDirectionValue.DirW);

        public static readonly ChessDirection DirNE = new ChessDirection(ChessDirectionValue.DirNE);
        public static readonly ChessDirection DirNW = new ChessDirection(ChessDirectionValue.DirNW);
        public static readonly ChessDirection DirSE = new ChessDirection(ChessDirectionValue.DirSE);
        public static readonly ChessDirection DirSW = new ChessDirection(ChessDirectionValue.DirSW);

        public static readonly ChessDirection DirNNE = new ChessDirection(ChessDirectionValue.DirNNE);
        public static readonly ChessDirection DirEEN = new ChessDirection(ChessDirectionValue.DirEEN);
        public static readonly ChessDirection DirEES = new ChessDirection(ChessDirectionValue.DirEES);
        public static readonly ChessDirection DirSSE = new ChessDirection(ChessDirectionValue.DirSSE);
        public static readonly ChessDirection DirSSW = new ChessDirection(ChessDirectionValue.DirSSW);
        public static readonly ChessDirection DirWWS = new ChessDirection(ChessDirectionValue.DirWWS);
        public static readonly ChessDirection DirWWN = new ChessDirection(ChessDirectionValue.DirWWN);
        public static readonly ChessDirection DirNNW = new ChessDirection(ChessDirectionValue.DirNNW);
        
        public readonly ChessDirectionValue Value;

        public ChessDirection(ChessDirectionValue value)
        {
            Value = value;
        }

        #region operators

        public static implicit operator ChessDirectionValue(ChessDirection dir)
        {
            return dir.Value;
        }
        public static implicit operator ChessDirection(ChessDirectionValue value)
        {
            return new ChessDirection(value);
        }

        public static explicit operator int(ChessDirection dir)
        {
            return (int)dir.Value;
        }
        public static explicit operator ChessDirection(int i)
        {
            return new ChessDirection((ChessDirectionValue)i);
        }
        public static bool operator ==(ChessDirection a, ChessDirection b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(ChessDirection a, ChessDirection b)
        {
            return !(a == b);
        }

        #endregion

        public bool IsDirectionRook()
        {
            //AssertDirection(dir);
            switch (this.Value)
            {
                case ChessDirectionValue.DirN:
                case ChessDirectionValue.DirE:
                case ChessDirectionValue.DirS:
                case ChessDirectionValue.DirW:
                    return true;
                default:
                    return false;
            }
        }
        public bool IsDirectionBishop()
        {
            //AssertDirection(dir);
            switch (this.Value)
            {
                case ChessDirectionValue.DirNW:
                case ChessDirectionValue.DirNE:
                case ChessDirectionValue.DirSW:
                case ChessDirectionValue.DirSE:
                    return true;
                default:
                    return false;
            }
        }
        public bool IsDirectionKnight()
        {
            //AssertDirection(dir);
            switch (this.Value)
            {
                case ChessDirectionValue.DirNNE:
                case ChessDirectionValue.DirEEN:
                case ChessDirectionValue.DirEES:
                case ChessDirectionValue.DirSSE:
                case ChessDirectionValue.DirSSW:
                case ChessDirectionValue.DirWWS:
                case ChessDirectionValue.DirWWN:
                case ChessDirectionValue.DirNNW:
                    return true;
                default:
                    return false;
            }
        }

        public ChessDirection Opposite()
        {
            return (ChessDirection)(-(int)this);
        }

    }
}
