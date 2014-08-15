using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoraGrace.Engine
{
    public enum GameResult
    {
        Draw = 0, WhiteWins = 1, BlackWins = -1
    }

    public enum GameResultReason
    {
        NotDecided = 0,
        Checkmate = 1, Resign = 2, OutOfTime = 3, Adjudication = 4, //win reasons
        Stalemate = 5, FiftyMoveRule = 6, InsufficientMaterial = 7, MutualAgreement = 8, Repetition = 9, //draw reasons
        Unknown = 10, IllegalMove = 11
    }

    public static class GameResultUtil
    {

        public static string Description(this GameResult? gameResult)
        {
            if (gameResult.HasValue)
            {
                switch (gameResult.Value)
                {
                    case GameResult.WhiteWins:
                        return "1-0";
                    case GameResult.BlackWins:
                        return "0-1";
                    case GameResult.Draw:
                        return "1/2-1/2";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                return "*";
            }
        }

        public static GameResult? Parse(string token)
        {
            if (token.Contains(' ')) { throw new ArgumentException(""); }
            switch (token)
            {
                case "1-0":
                    return GameResult.WhiteWins;
                case "0-1":
                    return GameResult.BlackWins;
                case "1/2-1/2":
                    return GameResult.Draw;
                case "*":
                    return null;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static bool TryParse(string token, out GameResult? result)
        {
            token = token.Replace(" ", "");
            result = null;
            switch (token)
            {
                case "1-0":
                    result = GameResult.WhiteWins;
                    return true;
                case "0-1":
                    result = GameResult.BlackWins;
                    return true;
                case "1/2-1/2":
                    result = GameResult.Draw;
                    return true;
                case "*":
                    result = null;
                    return true;
                default:
                    return false;
            }
        }
    }

}
