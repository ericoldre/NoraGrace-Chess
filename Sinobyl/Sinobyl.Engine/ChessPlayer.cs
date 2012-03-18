using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{
    public enum ChessPlayer
    {
        None = -1,
        White = 0, Black = 1
    }

    public static class ExtensionsChessPlayer
    {
        public static ChessPlayer PlayerOther(this ChessPlayer player)
        {
            //AssertPlayer(player);
            if (player == ChessPlayer.White) { return ChessPlayer.Black; }
            else { return ChessPlayer.White; }
        }
    }
}
