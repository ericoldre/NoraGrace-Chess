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
    public class PhasedScoreTests
    {
        private class SampleInfo
        {
            public int Opening { get; set; }
            public int Endgame { get; set; }
            public PhasedScore Score { get; set; }
        }
        private static IEnumerable<SampleInfo> Samples()
        {
            int[] vals = new int[] { 0, 1, 10, 34, 56, 120, 2032, -1, -4, -50, -103 };
            foreach (var opening in vals)
            {
                foreach (var endgame in vals)
                {
                    yield return new SampleInfo() { Opening = opening, Endgame = endgame, Score = PhasedScoreUtil.Create(opening, endgame) };
                }
            }
        }

        [TestMethod]
        public void CreateTest()
        {
            foreach (var sample in Samples())
            {
                int opening = sample.Score.Opening();
                int endgame = sample.Score.Endgame();

                Assert.AreEqual<int>(sample.Opening, opening);
                Assert.AreEqual<int>(sample.Endgame, endgame);

            }
        }

        [TestMethod]
        public void AddTest()
        {
            foreach (var s1 in Samples())
            {
                foreach (var s2 in Samples())
                {
                    var result = s1.Score.Add(s2.Score);
                    Assert.AreEqual<int>(s1.Opening + s2.Opening, result.Opening());
                    Assert.AreEqual<int>(s1.Endgame + s2.Endgame, result.Endgame());
                }
            }
        }

        [TestMethod]
        public void SubtractTest()
        {
            foreach (var s1 in Samples())
            {
                foreach (var s2 in Samples())
                {
                    var result = s1.Score.Subtract(s2.Score);
                    Assert.AreEqual<int>(s1.Opening - s2.Opening, result.Opening());
                    Assert.AreEqual<int>(s1.Endgame - s2.Endgame, result.Endgame());
                }
            }
        }

        [TestMethod]
        public void NegateTest()
        {
            foreach (var s1 in Samples())
            {
                var result = s1.Score.Negate();
                Assert.AreEqual<int>(-s1.Opening, result.Opening());
                Assert.AreEqual<int>(-s1.Endgame, result.Endgame());
            }
        }

        [TestMethod]
        public void MultiplyTest()
        {
            foreach (var s1 in Samples())
            {
                foreach (var multiplier in new int[] { 0, 1, -1, 10, 20, -24 })
                {
                    var result = s1.Score.Multiply(multiplier);
                    Assert.AreEqual<int>(s1.Opening * multiplier, result.Opening());
                    Assert.AreEqual<int>(s1.Endgame * multiplier, result.Endgame());
                }
            }
        }

        [TestMethod]
        public void ApplyWeightsTest()
        {
            foreach (var s1 in Samples())
            {
                Assert.AreEqual<int>(s1.Opening, s1.Score.ApplyScaleFactor(ScaleFactor.FULL));
                Assert.AreEqual<int>(s1.Endgame, s1.Score.ApplyScaleFactor(ScaleFactor.NONE));


            }
        }

        
    }
}
