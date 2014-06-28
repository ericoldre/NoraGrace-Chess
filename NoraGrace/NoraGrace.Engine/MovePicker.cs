using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NoraGrace.Engine.Evaluation;

namespace NoraGrace.Engine
{


    public class MovePicker
    {

        public class Stack
        {
            List<MovePicker> _plyBuffers = new List<MovePicker>();

            public Stack(int plyCapacity = 50)
            {
                while (_plyBuffers.Count < plyCapacity)
                {
                    _plyBuffers.Add(new MovePicker());
                }
            }

            public MovePicker this[int ply]
            {
                get
                {
                    System.Diagnostics.Debug.Assert(ply >= 0);
                    if (ply > _plyBuffers.Count)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            _plyBuffers.Add(new MovePicker());
                        }
                    }
                    return _plyBuffers[ply];
                }
            }

        }

        private class MoveHistory
        {
            private readonly int[][] _history = new int[16][];
            public MoveHistory()
            {
                for (int i = 0; i <= _history.GetUpperBound(0); i++) { _history[i] = new int[64]; }
            }

            public void RegisterKiller(Board board, Move move)
            {

                if (board.PieceAt(move.To()) == Piece.EMPTY)
                {
                    //save to history table
                    var piece = (int)board.PieceAt(move.From());
                    var to = (int)move.To();
                    var newscore = Math.Min(1000000, _history[piece][to] + 1000);
                    _history[piece][to] = newscore;
                }

            }

            public void RegisterFailLow(Board board, Move move)
            {
                //decrease value in history table
                var piece = (int)board.PieceAt(move.From());
                var to = (int)move.To();
                _history[piece][to] = _history[piece][to] / 2;
            }

            public int HistoryScore(Board board, Move move)
            {
                var piece = (int)board.PieceAt(move.From());
                var to = (int)move.To();
                var score = _history[piece][to];
                return score;
            }



        }

        private enum GeneratorStep
        {
            ttMove,
            InitCaps,
            GoodCaps,
            Killers,
            BadCaps,
            InitQuiet,
            Quiet,
            Done,
        }

        private readonly ChessMoveData[] _captures = new ChessMoveData[192];
        private readonly ChessMoveData[] _nonCaptures = new ChessMoveData[192];

        private readonly MoveHistory _history = new MoveHistory();

        private readonly Move[][] _killers = new Move[2][] { new Move[2], new Move[2] };

        private Move _ttMove;
        private Board _board;
        private long _boardZob;
        private bool _capsOnly;
        private GeneratorStep _currStep;
        private ChessMoveData _tmpData;
        private int _capsCount;
        private int _capsGoodCount;
        private int _currIndex;
        private int _quietCount;

        private readonly Move[] _exclude = new Move[20];
        private int _excludeCount = 0;

        public MovePicker()
        {


        }

        public void Initialize(Board board, Move ttMove = Move.EMPTY, bool capsOnly = false)
        {
            _board = board;
            _boardZob = board.ZobristBoard;
            _ttMove = ttMove;
            _capsOnly = capsOnly;
            _currStep = 0;
            _capsCount = 0;
            _capsGoodCount = 0;
            _currIndex = 0;
            _quietCount = 0;
            _excludeCount = 0;
            //_moveCount = ChessMoveInfo.GenMovesArray(_array, board, capsOnly);
            //_moveCurrent = 0;
        }

        public void Sort(Board board, bool useSEE, Move ttMove)
        {

        }

        private ChessMoveData StepTTMove()
        {
            if (_ttMove != Move.EMPTY)
            {
                System.Diagnostics.Debug.Assert(_ttMove.IsPsuedoLegal(_board));
                _currStep++;
                _tmpData.Move = _ttMove;
                _tmpData.Flags = MoveFlags.TransTable;
                _exclude[_excludeCount++] = _ttMove;
                return _tmpData;
            }
            else
            {
                _currIndex = 0;
                _currStep++;
                return NextMoveData();
            }
        }

        private ChessMoveData StepInitCaps()
        {
            var array = _captures;

            _capsCount = MoveInfo.GenCapsNonCaps(array, _board, true, 0); //generate capture
            _capsCount = ExcludeFrom(array, 0, _capsCount, _exclude, _excludeCount); //remove trans table move from array

            for (int i = 0; i < _capsCount; i++)
            {
                array[i].SEE = StaticExchange.CalculateScore(array[i].Move, _board); //calculate if winning capture.
                array[i].Flags = MoveFlags.Capture;

                if (array[i].SEE >= 0) { _capsGoodCount++; } //incr good cap count.

                Move move = array[i].Move;
                Piece piece = _board.PieceAt(move.From());

                array[i].Score = array[i].SEE + PcSqChange(piece, move.From(), move.To());

            }
            SortMoveData(array, 0, _capsCount);

            _currIndex = 0;
            _currStep++;
            return NextMoveData();
        }

        private ChessMoveData StepGoodCaps()
        {
            if (_currIndex < _capsGoodCount)
            {
                return _captures[_currIndex++];
            }
            else
            {
                _currStep++;
                _currIndex = 0;
                return NextMoveData();
            }
        }

        private ChessMoveData StepKillers()
        {
            if (_capsOnly)
            {
                _currIndex = 0;
                _currStep++;
                return NextMoveData();
            }

            var killerInfo = _history;
            if (_currIndex < 2)
            {
                Move move = _killers[(int)_board.WhosTurn][_currIndex];
                if (move.IsPsuedoLegal(_board) && _board.PieceAt(move.To()) == Piece.EMPTY)
                {
                    _tmpData.Move = move;
                    _tmpData.Flags = MoveFlags.Killer;
                    _currIndex++;
                    _exclude[_excludeCount++] = move;
                    return _tmpData;
                }
                else
                {
                    _currIndex++;
                    return NextMoveData();
                }
            }
            else
            {
                _currIndex = 0;
                _currStep++;
                return NextMoveData();
            }
        }

        public ChessMoveData StepBadCaps()
        {
            int badCapIndex = _currIndex + _capsGoodCount;
            if (badCapIndex < _capsCount)
            {
                _currIndex++;
                return _captures[badCapIndex];
            }
            else
            {
                _currStep++;
                _currIndex = 0;
                return NextMoveData();
            }
        }

        public ChessMoveData StepInitQuiet()
        {
            if (_capsOnly)
            {
                _currIndex = 0;
                _currStep++;
                return NextMoveData();
            }
            var array = _nonCaptures;

            _quietCount = MoveInfo.GenCapsNonCaps(array, _board, false, 0);
            _quietCount = ExcludeFrom(array, 0, _quietCount, _exclude, _excludeCount);
            for (int i = 0; i < _quietCount; i++)
            {
                Move move = array[i].Move;
                int see = StaticExchange.CalculateScore(move, _board);
                array[i].SEE = see;
                array[i].Flags = 0;

                
                Piece piece = _board.PieceAt(move.From());


                array[i].Score =
                    _history.HistoryScore(_board, move)
                    + PcSqChange(piece, move.From(), move.To())
                    + (see < 0 ? -1000 : 0);

            }

            SortMoveData(array, 0, _quietCount);

            _currIndex = 0;
            _currStep++;
            return NextMoveData();

        }

        private ChessMoveData StepQuiet()
        {
            if (_capsOnly)
            {
                _currIndex = 0;
                _currStep++;
                return NextMoveData();
            }

            if (_currIndex < _quietCount)
            {
                return _nonCaptures[_currIndex++];
            }
            else
            {
                _currIndex = 0;
                _currStep++;
                return NextMoveData();
            }

        }

        private ChessMoveData StepDone()
        {
            _tmpData.Move = Move.EMPTY;
            return _tmpData;
        }
        public ChessMoveData NextMoveData()
        {
            System.Diagnostics.Debug.Assert(_board.ZobristBoard == _boardZob);
            switch (_currStep)
            {
                case GeneratorStep.ttMove:
                    return StepTTMove();

                case GeneratorStep.InitCaps:
                    return StepInitCaps();
                case GeneratorStep.GoodCaps:
                    return StepGoodCaps();

                case GeneratorStep.Killers:
                    return StepKillers();

                case GeneratorStep.BadCaps:
                    return StepBadCaps();

                case GeneratorStep.InitQuiet:
                    return StepInitQuiet();

                case GeneratorStep.Quiet:
                    return StepQuiet();

                case GeneratorStep.Done:
                    return StepDone();
                default:
                    System.Diagnostics.Debug.Assert(false);
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static int ExcludeFrom(ChessMoveData[] source, int sourceStart, int sourceEnd, Move[] exclude, int excludeCount)
        {
            int foundCount = 0;
            for (int i = sourceStart; i < sourceEnd - foundCount; i++)
            {
                bool excludeThis = false;
                for (int ii = 0; ii < excludeCount; ii++)
                {
                    if (source[i].Move == exclude[ii])
                    {
                        excludeThis = true;
                        break;
                    }
                }

                if (excludeThis)
                {
                    Array.Copy(source, i + 1, source, i, sourceEnd - i);
                    foundCount++;
                    i--;
                }
            }
            return sourceEnd - foundCount;
        }

        private int PcSqChange(Piece piece, Position from, Position to)
        {
            //calc pcsq value;
            Evaluation.PhasedScore pcSq = 0;
            _board.PcSqEvaluator.PcSqValuesRemove(piece, from, ref pcSq);
            _board.PcSqEvaluator.PcSqValuesAdd(piece, to, ref pcSq);
            if (_board.WhosTurn == Player.Black) { pcSq = pcSq.Negate(); }
            return pcSq.Opening();
        }

        private static void SortMoveData(ChessMoveData[] array, int index, int length)
        {
            ChessMoveData temp;
            for (int i = index; i < length; i++)
            {
                for (int ii = i; ii > index; ii--)
                {
                    if (array[ii].Score > array[ii - 1].Score)
                    {
                        temp = array[ii];
                        array[ii] = array[ii - 1];
                        array[ii - 1] = temp;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        public IEnumerable<Move> SortedMoves()
        {
            ChessMoveData moveData;

            while ((moveData = NextMoveData()).Move != Move.EMPTY)
            {
                yield return moveData.Move;
            }
        }

        public void RegisterCutoff(Board board, Move move)
        {
            //store killer moves in MovePicker
            var killers = _killers[(int)board.WhosTurn];
            if (board.PieceAt(move.To()) == Piece.EMPTY)
            {
                if (move != killers[0])
                {
                    killers[1] = killers[0];
                    killers[0] = move;
                }
            }
            //store to history object.
            _history.RegisterKiller(board, move);
        }

        public void RegisterFailLow(Board board, Move move)
        {
            _history.RegisterFailLow(board, move);
        }


    }


    [System.Diagnostics.DebuggerDisplay(@"{Move.Description()} SEE:{SEE} PcSq:{PcSq} Flags:{Flags}")]
    public struct ChessMoveData
    {
        public Move Move;
        public int SEE;
        public int Score;
        public MoveFlags Flags;

    }

    [Flags]
    public enum MoveFlags
    {
        Killer = (1 << 0),
        Capture = (1 << 1),
        Promote = (1 << 2),
        TransTable = (1 << 3),
    }
}
