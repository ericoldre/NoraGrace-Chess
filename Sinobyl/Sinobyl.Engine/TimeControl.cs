using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{
    public abstract class TimeControlGeneric<T>
    {
        public T InitialAmount { get; set; }
        public T BonusAmount { get; set; }
        public int BonusEveryXMoves { get; set; }

        public T CalcNewTimeLeft(T beforeMove, T amountUsed, int moveNum)
        {
            T retval = Subtract(beforeMove, amountUsed);
            if (BonusEveryXMoves != 0 && moveNum % BonusEveryXMoves == 0)
            {
                retval = Add(retval, BonusAmount);
            }
            return retval;
        }

        public abstract T Subtract(T x, T y);
        public abstract T Add(T x, T y);
        public abstract T Multiply(T x, double y);
    }

    public class TimeControl: TimeControlGeneric<TimeSpan>
    {


        public TimeControl()
        {
            InitialAmount = TimeSpan.FromMinutes(1);
            BonusAmount = TimeSpan.FromSeconds(1);
            BonusEveryXMoves = 1;

        }


        public TimeControl(TimeSpan a_InitialTime, TimeSpan a_BonusAmount, int a_BonusEveryXMoves)
        {
            InitialAmount = a_InitialTime;
            BonusEveryXMoves = a_BonusEveryXMoves;
            BonusAmount = a_BonusAmount;
        }
        public static TimeControl Blitz(int a_Minutes, int a_Seconds)
        {
            return new TimeControl(TimeSpan.FromMinutes(a_Minutes), TimeSpan.FromSeconds(a_Seconds), 1);
        }
        public static TimeControl TotalGame(int a_Minutes)
        {
            return new TimeControl(TimeSpan.FromMinutes(a_Minutes), TimeSpan.FromSeconds(0), 0);
        }
        public static TimeControl MovesInMinutes(int a_Minutes, int a_Moves)
        {
            return new TimeControl(TimeSpan.FromMinutes(a_Minutes), TimeSpan.FromMinutes(a_Minutes), a_Moves);
        }

        public override TimeSpan Add(TimeSpan x, TimeSpan y)
        {
            return x + y;
        }

        public override TimeSpan Subtract(TimeSpan x, TimeSpan y)
        {
            return x - y;
        }

        public override TimeSpan Multiply(TimeSpan x, double y)
        {
            return TimeSpan.FromMilliseconds(x.TotalMilliseconds * y);
        }

        //public TimeSpan CalcNewTimeLeft(TimeSpan beforeMove, TimeSpan amountUsed, int moveNum)
        //{
        //    TimeSpan retval = beforeMove - amountUsed;
        //    if (BonusEveryXMoves != 0 && moveNum % BonusEveryXMoves == 0)
        //    {
        //        retval += BonusAmount;
        //    }
        //    return retval;
        //}

        //public TimeSpan RecommendSearchTime(TimeSpan a_Remaining, int a_MoveNum)
        //{

        //    TimeSpan ExtraPerMove = TimeSpan.FromSeconds(0);
        //    if (this.BonusEveryXMoves > 0)
        //    {
        //        ExtraPerMove = TimeSpan.FromMilliseconds(this.BonusAmount.TotalMilliseconds / this.BonusEveryXMoves);
        //    }

        //    return TimeSpan.FromMilliseconds(a_Remaining.TotalMilliseconds / 30) + ExtraPerMove;
        //}


    }

    public class TimeControlNodes : TimeControlGeneric<int>
    {
        public override int Add(int x, int y)
        {
            return x + y;
        }
        public override int Subtract(int x, int y)
        {
            return x - y;
        }
        public override int Multiply(int x, double y)
        {
            return (int)Math.Round((double)x * y);
        }
    }
}
