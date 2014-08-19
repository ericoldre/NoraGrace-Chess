using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NoraGrace.Engine;

namespace NoraGrace.EvalTune2
{

    public class BinaryPGNSource
    {
        private readonly IEnumerable<BinaryPGN> _origSource;
        private readonly int _origCount;
        private readonly int _subsetCount;
        private readonly BinaryPGN[] _subset;

        public BinaryPGNSource(IEnumerable<BinaryPGN> origSource, int subsetCount)
        {
            _origSource = origSource;
            _origCount = origSource.Count();
            _subsetCount = subsetCount;
            _subset = new BinaryPGN[_subsetCount];
        }

        private static int[] GetRandomIndexes(int count, int maxIdx, Random rand = null)
        {
            
            if (rand == null) { rand = new Random(); }
            List<int> sourceIndexes = Enumerable.Range(0, maxIdx).ToList();
            List<int> indexes = new List<int>();
            
            while(indexes.Count < count)
            {
                int sourceIdx = rand.Next(0, sourceIndexes.Count);
                indexes.Add(sourceIndexes[sourceIdx]);
                sourceIndexes.RemoveAt(sourceIdx);
            }

            indexes.Sort();
            return indexes.ToArray();
            
        }
        
        private void SelectRandomSample()
        {

        }

    }

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
                        var bpgn = BinaryPGN.FromPgn(pgn);
                        BinaryPGN.Write(bpgn, bw);
                        c++;
                        if (c % 1000 == 0) { Console.WriteLine(c); }
                    }
                }
            }
        }

        public static void ConvertToBinary(IEnumerable<PGN> pgns, string outputFile)
        {
            
            using (var bw = new System.IO.BinaryWriter(new System.IO.FileStream(outputFile, System.IO.FileMode.Create)))
            {
                int c = 0;
                foreach (var pgn in pgns)
                {
                    BinaryPGN.Write(BinaryPGN.FromPgn(pgn), bw);
                    c++;
                    if (c % 1000 == 0) { Console.WriteLine(c); }
                }
            }
            
        }

        public static void Randomize(System.IO.FileInfo fileIn, System.IO.FileInfo fileOut)
        {
            var source = Read(fileIn.FullName).ToList();

            List<BinaryPGN> dest = new List<BinaryPGN>();

            Random rand = new Random();

            while (source.Count > 0)
            {
                int idx = rand.Next(source.Count);
                dest.Add(source[idx]);
                source.RemoveAt(idx);
            }

            using (var bw = new System.IO.BinaryWriter(new System.IO.FileStream(fileOut.FullName, System.IO.FileMode.Create)))
            {
                foreach (var bpgn in dest)
                {
                    Write(bpgn, bw);
                }
            }
        }

        public static void Write(IEnumerable<NoraGrace.Engine.PGN> pgns, System.IO.BinaryWriter writer)
        {
            foreach (var pgn in pgns)
            {
                Write(FromPgn(pgn), writer);
            }
        }

        public static BinaryPGN FromPgn(NoraGrace.Engine.PGN pgn)
        {

            GameResult result = pgn.Result.Value;
            Move[] moves = new Move[pgn.Moves.Count];
            bool[] excludes = new bool[pgn.Moves.Count];
            Board board = new Board();

            for (int iMove = 0; iMove < pgn.Moves.Count; iMove++)
            {
                moves[iMove] = pgn.Moves[iMove];

                string comment = pgn.Comments.ContainsKey(iMove) ? pgn.Comments[iMove] : "";
                double ddepth = 0;
                double dscore = 0;
                double thinkSeconds = 0;
                string[] split = comment.Split(new char[] { '/', ' ' });
                if (split.Length == 3 &&
                    double.TryParse(split[0], out dscore)
                    && double.TryParse(split[1], out ddepth))
                {
                    if (split[2].EndsWith("s"))
                    {
                        double.TryParse(split[2].Replace("s", ""), out thinkSeconds);
                    }
                }

                bool exclude =
                    board.IsCheck()
                    || (result == GameResult.Draw && board.FiftyMovePlyCount >= 10)
                    || ddepth == 0
                    || Math.Abs(dscore) > 300
                    || !PositionQuiet(board);

                excludes[iMove] = exclude;

            }

            return new BinaryPGN(result, moves, excludes);
            
        }
        
        public static void Write(BinaryPGN pgn, System.IO.BinaryWriter writer)
        {
            var result = pgn.Result;

            writer.Write(STARTMARKER);
            writer.Write((int)result);
            writer.Write((int)pgn.Moves.Length);

            Board board = new Board();

            for (int iMove = 0; iMove < pgn.Moves.Length; iMove++)
            {
                Move move = pgn.Moves[iMove];
                bool exclude = pgn.Exclude[iMove];

                writer.Write((int)move);
                writer.Write((bool)exclude);
                
            }
            writer.Write(ENDMARKER);
        }



        private static NoraGrace.Engine.MovePicker.Stack _mpsQuiet;
        public static bool PositionQuiet(Board board)
        {
            if (_mpsQuiet == null) { _mpsQuiet = new MovePicker.Stack(); }
            var x = QSearchQuiet(board, _mpsQuiet);
            return x <= 0;
        }

        private static int QSearchQuiet(Board board, MovePicker.Stack moveStack, int ply = 0)
        {
            if (ply >= 10) { return 0; }
            var plyMoves = moveStack[ply];
            plyMoves.Initialize(board, Move.EMPTY, !board.IsCheck());
            ChessMoveData moveData;
            //Bitboard taken = Bitboard.Empty;

            int best = board.IsCheck() ? int.MinValue : 0;

            while ((moveData = plyMoves.NextMoveData()).Move != Move.EMPTY)
            {
                Move move = moveData.Move;

                if (moveData.SEE < 0) { continue; }

                board.MoveApply(move);
                
                if (board.IsCheck(board.WhosTurn.PlayerOther()))
                {
                    board.MoveUndo();
                    continue;
                }
                int capScore = move.CapturedPieceType().BasicVal();
                int responseScore = -QSearchQuiet(board, moveStack, ply + 1);
                int moveScore = capScore + responseScore;

                board.MoveUndo();

                best = Math.Max(best, moveScore);
            }
            return best;
        }

    }
}
