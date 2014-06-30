using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoraGrace.Engine
{
    public static class Helpers
    {
        public static T[][] ArrayInit<T>(int x, int y)
        {
            T[][] retval = new T[x][];
            for (int i = 0; i < retval.Length; i++)
            {
                retval[i] = new T[y];
            }
            return retval;
        }

        public static T[][][] ArrayInit<T>(int x, int y, int z)
        {
            T[][][] retval = new T[x][][];
            for (int xi = 0; xi < retval.Length; xi++)
            {
                retval[xi] = ArrayInit<T>(y, z);
            }
            return retval;
        }

        public static void ArrayReset<T>(T[] array, T defaultValue)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = defaultValue;
            }
        }

        public static void ArrayReset<T>(T[][] array, T defaultValue)
        {
            for (int i = 0; i < array.Length; i++)
            {
                ArrayReset<T>(array[i], defaultValue);
            }
        }

        public static void ArrayReset<T>(T[][][] array, T defaultValue)
        {
            for (int i = 0; i < array.Length; i++)
            {
                ArrayReset<T>(array[i], defaultValue);
            }
        }

    }
}
