using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoraGrace.Engine;
using NoraGrace.Engine.Evaluation;

namespace NoraGrace.Engine.Tests
{
    [TestClass]
    public class EvalMaterialTests
    {
        #region sample generation
        public class SidePieceCount
        {
            public int p;
            public int n;
            public int b;
            public int r;
            public int q;
            public int BasicScore
            {
                get
                {
                    return (p * 100) + (n * 300) + (b * 300) + (r * 500) + (q * 900) + (b > 1 ? 50 : 0);
                }
            }
        }

        public class PieceCounts
        {
            public SidePieceCount white;
            public SidePieceCount black;
            public int BasicScore
            {
                get
                {
                    return white.BasicScore - black.BasicScore;
                }
            }

            public int PDiff
            {
                get { return white.p - black.p; }
            }

            public int NDiff
            {
                get { return white.n - black.n; }
            }

            public int BDiff
            {
                get { return white.b - black.b; }
            }
            
            public int RDiff
            {
                get { return white.r - black.r; }
            }

            public int QDiff
            {
                get { return white.q - black.q; }
            }
        }

        public static IEnumerable<SidePieceCount> SampleSides()
        {
            for (int p = 0; p <= 8; p++)
            {
                for (int n = 0; n <= 2; n++)
                {
                    for (int b = 0; b <= 2; b++)
                    {
                        for (int r = 0; r <= 2; r++)
                        {
                            for (int q = 0; q <= 1; q++)
                            {
                                yield return new SidePieceCount() { p = p, n = n, b = b, r = r, q = q };
                            }
                        }
                    }
                }
            }
        }

        public static IEnumerable<PieceCounts> Samples()
        {
            foreach (var w in SampleSides())
            {
                foreach (var b in SampleSides())
                {
                    var s = new PieceCounts() { white = w, black = b };
                    if (Math.Abs(s.PDiff) <= 4 && Math.Abs(s.BasicScore) < 600)
                    {
                        yield return s;
                    }
                }
            }
        }

        #endregion

        [TestMethod]
        public void Within150OfBasic()
        {
            MaterialEvaluator eval = new MaterialEvaluator(Settings.Default());

            foreach (var s in Samples())
            {
                var r = eval.EvalMaterial(0, s.white.p, s.white.n, s.white.b, s.white.r, s.white.q, s.black.p, s.black.n, s.black.b, s.black.r, s.black.q);
                Assert.IsTrue(Math.Abs(r.Score - s.BasicScore) < 150);
            }
            //var r = evalBasic.EvalMaterial(0, ss.white.p, ss.white.n, ss.white.b, ss.white.r, ss.white.q, ss.black.p, ss.black.n, ss.black.b, ss.black.r, ss.black.q);
        }
    }
}
