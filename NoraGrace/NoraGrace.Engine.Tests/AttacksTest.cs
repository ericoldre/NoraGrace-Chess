using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NoraGrace.Engine.Tests
{
    [TestClass]
    public class AttacksTest
    {
        static Random rand = new Random(0);

        private static Bitboard bitboard_to_attacks_calc(Bitboard piecelocations, Direction dir, Position position)
        {
            Bitboard retval = 0;
            for (; ; )
            {
                position = position.PositionInDirection(dir);
                if (!position.IsInBounds()) { break; }
                retval |= position.ToBitboard();
                if (!(piecelocations & position.ToBitboard()).Empty()) { break; }
            }
            return retval;
        }

        private static Bitboard RandomBitboard(int pctFill)
        {
            Bitboard retval = 0;
            foreach (var pos in PositionUtil.AllPositions)
            {
                if (rand.Next(0, 99) < pctFill)
                {
                    retval |= pos.ToBitboard();
                }
            }
            return retval;
        }

        
    }
}
