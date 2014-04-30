﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinobyl.Engine
{
    [System.Diagnostics.DebuggerDisplay(@"{Sinobyl.Engine.PhasedScoreInfo.Description(this),nq}")]
    public enum PhasedScore: long
    {
    }

    public static class PhasedScoreInfo
    {
        public static PhasedScore Create(int opening, int endgame)
        {
            //long high = (long)mg << 32;
            //long low = (long)eg;
            //long retval = high + low;
            //return (TestEnum)retval;
            return (PhasedScore)(((long)opening << 32) + (long)endgame);
        }

        public static int Opening(this PhasedScore phasedScore)
        {
            //long s1 = (long)s + 0x80000000L;
            //long a = ~0xffffffffL;
            //long s2 = s1 & a;
            //long s3 = s2 / 0x100000000L;
            //return (int)s3;

            return (int)((((long)phasedScore + 0x80000000L) & ~0xffffffffL) / 0x100000000L);
        }


        public static int Endgame(this PhasedScore phasedScore)
        {
            //var s1 = (long)s & 0xffffffff;
            //return (int)s1;

            return (int)((long)phasedScore & 0xffffffffL);
        }

        public static string Description(this PhasedScore score)
        {
            return string.Format("{0}, {1}", score.Opening(), score.Endgame());
        }

        public static PhasedScore Add(this PhasedScore a, PhasedScore b)
        {
            return (PhasedScore)((long)a + (long)b);
        }

        public static PhasedScore Subtract(this PhasedScore a, PhasedScore b)
        {
            return (PhasedScore)((long)a - (long)b);
        }

        public static PhasedScore Negate(this PhasedScore phasedScore)
        {
            return (PhasedScore)(-(long)phasedScore);
        }

        public static PhasedScore Multiply(this PhasedScore phasedScore, int multiplier)
        {
            return (PhasedScore)((long)phasedScore * multiplier);
        }

        public static int ApplyWeights(this PhasedScore phasedScore, int StageStartWeight)
        {
            return (
                    (phasedScore.Opening() * StageStartWeight) 
                    + (phasedScore.Endgame() * (100 - StageStartWeight)
                    )) / 100;
        }



    }
}
