using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{
    public class ChessTimeControl
    {
        [System.Xml.Serialization.XmlIgnore]
        public TimeSpan InitialTime = TimeSpan.FromMinutes(1);
        [System.Xml.Serialization.XmlIgnore]
        public TimeSpan BonusAmount = TimeSpan.FromSeconds(1);

        public int BonusEveryXMoves = 1;

        public double SerializationInitTimeSeconds
        {
            get
            {
                return InitialTime.TotalSeconds;
            }
            set
            {
                InitialTime = TimeSpan.FromSeconds(value);
            }
        }
        public double SerializationBonusAmt
        {
            get
            {
                return BonusAmount.TotalSeconds;
            }
            set
            {
                BonusAmount = TimeSpan.FromSeconds(value);
            }
        }

        public ChessTimeControl()
        {

        }


        public ChessTimeControl(TimeSpan a_InitialTime, TimeSpan a_BonusAmount, int a_BonusEveryXMoves)
        {
            InitialTime = a_InitialTime;
            BonusEveryXMoves = a_BonusEveryXMoves;
            BonusAmount = a_BonusAmount;
        }
        public static ChessTimeControl Blitz(int a_Minutes, int a_Seconds)
        {
            return new ChessTimeControl(TimeSpan.FromMinutes(a_Minutes), TimeSpan.FromSeconds(a_Seconds), 1);
        }
        public static ChessTimeControl TotalGame(int a_Minutes)
        {
            return new ChessTimeControl(TimeSpan.FromMinutes(a_Minutes), TimeSpan.FromSeconds(0), 0);
        }
        public static ChessTimeControl MovesInMinutes(int a_Minutes, int a_Moves)
        {
            return new ChessTimeControl(TimeSpan.FromMinutes(a_Minutes), TimeSpan.FromMinutes(a_Minutes), a_Moves);
        }


        public TimeSpan CalcNewTimeLeft(TimeSpan beforeMove, TimeSpan amountUsed, int moveNum)
        {
            TimeSpan retval = beforeMove - amountUsed;
            if (BonusEveryXMoves != 0 && moveNum % BonusEveryXMoves == 0)
            {
                retval += BonusAmount;
            }
            return retval;
        }

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
}
