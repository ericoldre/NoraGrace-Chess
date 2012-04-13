using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.EvalTune
{
    class Program
    {
        static void Main(string[] args)
        {
            Random rand = new Random(0);

            for (int i = 0; i < 100; i++)
            {
                var x = MutatorFactory.Create(rand);
                Console.WriteLine(x.ToString());
            }

            Console.ReadLine();
        }
    }
}
