using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoraGrace.Engine
{
    public class EPD
    {

        public FEN FEN { get; private set; }
        public Move? BestMove {get; private set;}
        public Dictionary<Move, int> MoveScores { get; private set; }
        public string ID { get; private set; }


        /// <summary>
        /// Not a fully compliant EPD parser, specifically designed to work with epd of the strategic test suite. 
        /// example: 4k3/r2bbprp/3p1p1N/2qBpP2/ppP1P1P1/1P1R3P/P7/1KR1Q3 w - - bm a3; id "Undermine.056"; c0 "a3=10, Qd2=3, Rc2=3, h4=3";
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static EPD Parse(string line)
        {
            string[] semiSplit = line.Split(';');
            var sFen = string.Join(" ", semiSplit[0].Split(' ').Take(4)) + " 1 1";
            semiSplit[0] = string.Join(" ", semiSplit[0].Split(' ').Skip(4));

            EPD retval = new EPD();
            retval.FEN = new FEN(sFen);
            Board board = new Board(retval.FEN);

            foreach (string option in semiSplit.Select(s => s.Trim()))
            {
                if (option.IndexOf(" ") > 0)
                {
                    string key = option.Substring(0, option.IndexOf(" ")).Trim();
                    string val = option.Substring(option.IndexOf(" ")).Replace(@"""","").Trim();

                    switch (key)
                    {
                        case "bm":
                            retval.BestMove = MoveInfo.Parse(board, val);
                            break;
                        case "id":
                            retval.ID = val;
                            break;
                        case "c0":

                            foreach (var moveScore in val.Split(new string[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                if (moveScore.Contains("="))
                                {
                                    string smove = moveScore.Split('=')[0].Trim();
                                    string sscore = moveScore.Split('=')[1].Trim();
                                    if (retval.MoveScores == null) { retval.MoveScores = new Dictionary<Move, int>(); }
                                    Move move = MoveInfo.Parse(board, smove);
                                    if(!retval.MoveScores.ContainsKey(move))
                                    {
                                        retval.MoveScores.Add(move, int.Parse(sscore));
                                    }
                                    
                                }
                            }
                            break;
                    }
                }
            }

            return retval;

        }

        public static IEnumerable<EPD> ParseMultiple(System.IO.TextReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                yield return Parse(line);
            }
        }

    }
}
