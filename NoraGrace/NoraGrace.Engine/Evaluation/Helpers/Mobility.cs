
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoraGrace.Engine.Evaluation.Helpers
{
    public class Mobility
    {

        public int Amplitude { get; set; }
        public Point BezControlPct { get; set; }
        public int ExpectedAttacksAvailable { get; set; }

        public int[] GetValues(int maxMobility)
        {
            Point start = new Point(0, 0);
            Point end = new Point(maxMobility, Amplitude);
            Point control = new Point((double)maxMobility * BezControlPct.X, (double)Amplitude * BezControlPct.Y);
            QuadBezierCurve curve = new QuadBezierCurve(start, control, end);
            double[] xCoordsFloat = Enumerable.Range(0, maxMobility + 1).Select(i => (double)i).ToArray();
            int[] yCoords = curve.Select(xCoordsFloat).Select(p => (int)p.Y).ToArray();
            int offset = yCoords[ExpectedAttacksAvailable];
            int[] retval = yCoords.Select(y => y - offset).ToArray();
            return retval;
        }
    }
}
