using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoraGrace.Engine
{
    public static class Helpers
    {
        private static T[][] ArrayInit<T>(int x, int y)
        {
            T[][] retval = new T[x][];
            for (int i = 0; i < retval.Length; i++)
            {
                retval[i] = new T[y];
            }
            return retval;
        }

        private static T[][][] ArrayInit<T>(int x, int y, int z)
        {
            T[][][] retval = new T[x][][];
            for (int xi = 0; xi < retval.Length; xi++)
            {
                retval[xi] = new T[y][];
                for (int yi = 0; yi < retval[xi][yi].Length; yi++)
                {
                    retval[xi][yi] = new T[z];
                }
            }
            return retval;
        }

    }
}
