using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoraGrace.Engine
{
    public abstract class TimeControlGeneric<T>
    {
        public T InitialAmount { get; set; }
        public T BonusAmount { get; set; }
        public int MovesPerControl { get; set; }

        public T CalcNewTimeLeft(T beforeMove, T amountUsed, int moveNum)
        {
            T retval = Subtract(beforeMove, amountUsed);
            if (MovesPerControl != 0 && moveNum % MovesPerControl == 0)
            {
                retval = Add(retval, BonusAmount);
            }
            return retval;
        }

        public abstract T Subtract(T x, T y);
        public abstract T Add(T x, T y);
        public abstract T Multiply(T x, double y);

        public override bool Equals(object obj)
        {
            TimeControlGeneric<T> other = obj as TimeControlGeneric<T>;
            return other != null
                && this.InitialAmount.Equals(other.InitialAmount)
                && this.BonusAmount.Equals(other.BonusAmount)
                && this.MovesPerControl.Equals(other.MovesPerControl);
        }

        public static bool operator ==(TimeControlGeneric<T> c1, TimeControlGeneric<T> c2)
        {
            if(c1== null)
            {
                return c2 == null;
            }
            else
            {
                return c2 != null && c1.Equals(c2);
            }
        }
        public static bool operator !=(TimeControlGeneric<T> c1, TimeControlGeneric<T> c2)
        {
            return !(c1 == c2);
        }
    }

    public class TimeControl: TimeControlGeneric<TimeSpan>
    {


        public static TimeControl Parse(string s)
        {
            string[] args = s.Split(' ');
            
            int movesPerTimeControl = int.Parse(args[0]);

            if (!args[1].Contains(":")) { args[1] = args[1] + ":00"; }
            string[] argsInitTime = args[1].Split(':');
            TimeSpan initialTime = TimeSpan.FromMinutes(int.Parse(argsInitTime[0])) + TimeSpan.FromSeconds(double.Parse(argsInitTime[1]));

            TimeSpan bonusAmount = TimeSpan.FromSeconds(double.Parse(args[2]));

            return new TimeControl(initialTime, bonusAmount, movesPerTimeControl);
        }

        public TimeControl()
        {
            InitialAmount = TimeSpan.FromMinutes(1);
            BonusAmount = TimeSpan.FromSeconds(1);
            MovesPerControl = 0;

        }


        public TimeControl(TimeSpan a_InitialTime, TimeSpan a_BonusAmount, int movesPerControl)
        {
            InitialAmount = a_InitialTime;
            MovesPerControl = movesPerControl;
            BonusAmount = a_BonusAmount;
        }
        public static TimeControl Blitz(int a_Minutes, int a_Seconds)
        {
            return new TimeControl(TimeSpan.FromMinutes(a_Minutes), TimeSpan.FromSeconds(a_Seconds), 0);
        }
        public static TimeControl TotalGame(int a_Minutes)
        {
            return new TimeControl(TimeSpan.FromMinutes(a_Minutes), TimeSpan.FromSeconds(0), 0);
        }
        public static TimeControl MovesInMinutes(int a_Minutes, int a_Moves)
        {
            return new TimeControl(TimeSpan.FromMinutes(a_Minutes), TimeSpan.FromSeconds(0), a_Moves);
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
