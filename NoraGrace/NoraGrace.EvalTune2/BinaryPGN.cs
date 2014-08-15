using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NoraGrace.Engine;

namespace NoraGrace.EvalTune2
{
    public class BinaryPGN
    {
        public GameResult Result { get; private set; }
        public Move[] Moves { get; private set; }
        public bool[] Exclude { get; private set; }

        public int MoveCount { get { return Moves.Length; } }

        public BinaryPGN(GameResult result, Move[] moves, bool[] exclude)
        {
            Result = result;
            Moves = moves;
            Exclude = exclude;
        }

        const int STARTMARKER = -2;
        const int ENDMARKER = -3;



        public static IEnumerable<BinaryPGN> Read(string fileName, Action<int> progressCallback = null)
        {
            using (var reader = new System.IO.BinaryReader(System.IO.File.OpenRead(fileName)))
            {
                foreach (var pgn in Read(reader, progressCallback))
                {
                    yield return pgn;
                }
            }
        }

        public static IEnumerable<BinaryPGN> Read(System.IO.BinaryReader reader, Action<int> progressCallback = null)
        {
            int count = 0;
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                yield return ReadNext(reader);
                count++;
                if (progressCallback != null) { progressCallback(count); }
            }
            if (progressCallback != null) 
            { 
                progressCallback(-1); 
            }
        }

        public static BinaryPGN ReadNext(System.IO.BinaryReader reader)
        {
            int start = reader.ReadInt32();
            if (start != STARTMARKER) { throw new ArgumentException(); }
            GameResult result = (GameResult)reader.ReadInt32();
            int movecount = reader.ReadInt32();

            Move[] moves = new Move[movecount];
            bool[] exclude = new bool[movecount];
            
            for (int i = 0; i < movecount; i++)
            {
                moves[i] = (Move)reader.ReadInt32();
                exclude[i] = reader.ReadBoolean();
            }
            
            int end = reader.ReadInt32();
            if (end != ENDMARKER) { throw new ArgumentException(); }

            return new BinaryPGN(result, moves, exclude);

        }

        public static void ConvertToBinary(string inputFile, string outputFile)
        {
            using (System.IO.StreamReader reader = new System.IO.StreamReader(inputFile))
            {
                using (var bw = new System.IO.BinaryWriter(new System.IO.FileStream(outputFile, System.IO.FileMode.Create)))
                {
                    int c = 0;
                    foreach (var pgn in NoraGrace.Engine.PGN.AllGames(reader))
                    {
                        BinaryPGN.Write(pgn, bw);
                        c++;
                        if (c % 1000 == 0) { Console.WriteLine(c); }
                    }
                }
            }
        }

        public static void Write(IEnumerable<NoraGrace.Engine.PGN> pgns, System.IO.BinaryWriter writer)
        {
            foreach (var pgn in pgns)
            {
                Write(pgn, writer);
            }
        }

        public static void Write(NoraGrace.Engine.PGN pgn, System.IO.BinaryWriter writer)
        {
            var result = pgn.Result.Value;

            writer.Write(STARTMARKER);
            writer.Write((int)result);
            writer.Write((int)pgn.Moves.Count);

            Board board = new Board();

            for (int iMove = 0; iMove < pgn.Moves.Count; iMove++)
            {
                Move move = pgn.Moves[iMove];
                string comment = pgn.Comments.ContainsKey(iMove) ? pgn.Comments[iMove] : "";
                double ddepth = 0;
                double dscore = 0;
                double thinkSeconds = 0;
                string[] split = comment.Split(new char[] { '/', ' ' });
                if (split.Length == 3 && 
                    double.TryParse(split[0],out dscore) 
                    && double.TryParse(split[1], out ddepth))
                {
                    if (split[2].EndsWith("s"))
                    {
                        double.TryParse(split[2].Replace("s", ""), out thinkSeconds);
                    }
                }

                board.MoveApply(move);

                bool exclude =
                    board.IsCheck()
                    || (result == GameResult.Draw && board.FiftyMovePlyCount >= 10)
                    || ddepth == 0
                    || MoveUtil.GenMoves(board).Any(m => m.CapturedPieceType().BasicVal() > m.MovingPieceType().BasicVal() && m.IsLegal(board));

                writer.Write((int)move);
                writer.Write((bool)exclude);
                //writer.Write((int)dscore * 100);
                //writer.Write((int)thinkSeconds / 1000);
                
            }
            writer.Write(ENDMARKER);
        }



    }
}
