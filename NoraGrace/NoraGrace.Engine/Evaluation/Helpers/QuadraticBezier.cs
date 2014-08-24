using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoraGrace.Engine.Evaluation.Helpers
{
    public class Point
    {
        public double X { get; set; }
        public double Y { get; set; }
        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }
        public Point()
        {

        }
    }

    public class QuadBezierCurve
    {
        public Point Start { get; set; }
        public Point Gravity { get; set; }
        public Point End { get; set; }

        public QuadBezierCurve(Point start, Point gravity, Point end)
        {
            Start = start;
            Gravity = gravity;
            End = end;
        }

        public Point PointAtT(double t)
        {
            //Point[] p = new Point[] { Start, Gravity, End };
            //double x = (1 - t) * (1 - t) * p[0].X + 2 * (1 - t) * t * p[1].X + t * t * p[2].X;
            // y = (1 - t) * (1 - t) * p[0].Y + 2 * (1 - t) * t * p[1].Y + t * t * p[2].Y;
            return new Point(XAtT(t), YAtT(t));
        }


        public double XAtT(double t)
        {
            return (1 - t) * (1 - t) * Start.X + 2 * (1 - t) * t * Gravity.X + t * t * End.X;
        }

        public double YAtT(double t)
        {
            return (1 - t) * (1 - t) * Start.Y + 2 * (1 - t) * t * Gravity.Y + t * t * End.Y;
        }

        

        public IEnumerable<Point> Select(IEnumerable<double> xCoords, double incr = .005)
        {
            double t = 0;
            foreach (double x in xCoords)
            {
                while (true)
                {
                    double xAtT = XAtT(t);
                    if (xAtT >= x) { break; }
                    if (t > 1) { break; }
                    t += incr;
                }
                double y = YAtT(t);
                yield return new Point(x, y);
            }
        }
    }
}
