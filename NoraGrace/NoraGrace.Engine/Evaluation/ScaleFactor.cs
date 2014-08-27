using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoraGrace.Engine.Evaluation
{
    [System.Diagnostics.DebuggerDisplay(@"{NoraGrace.Engine.Evaluation.ScaleFactorUtil.Description(this),nq}")]
    public enum ScaleFactor
    {
        NONE = 0,
        FULL = 255
    }

    public static class ScaleFactorUtil
    {

        public static ScaleFactor FromDouble(double d)
        {
            return (ScaleFactor)((double)ScaleFactor.FULL * d);
        }

        public static double ToDouble(this ScaleFactor sf)
        {
            return (double)sf / (double)ScaleFactor.FULL;
        }

        public static string Description(this ScaleFactor sf)
        {
            return "Scale:" + sf.ToDouble().ToString("D3");
        }

        public static int ScaleValue(this ScaleFactor sf, int value)
        {
            return (value * (int)sf) >> 8;
        }

    }
}
