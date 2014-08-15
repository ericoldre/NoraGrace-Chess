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
            public readonly MoveHistory History = new MoveHistory();
            StaticExchange _see = new StaticExchange();
            public Stack(int plyCapacity = 50)
            {
                while (_plyBuffers.Count < plyCapacity)
                {
                    _plyBuffers.Add(new MovePicker(History, _see));
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
                            _plyBuffers.Add(new MovePicker(History, _see));
                        }
                    }
                    return _plyBuffers[ply];
                }
            }

        }

        public class MoveHistory
        {
            private readonly int[][] _history = Helpers.ArrayInit<int>(16, 64);
            private readonly int[][] _maxPositionalGain = Helpers.ArrayInit<int>(16, 64);
            private int _count = 0;
            private int _maxCount = 50000;

            public MoveHistory()
            {

            }

            private void Age()
            {
                if (_count > _maxCount)
                {
                    for (int ipiece = 0; ipiece < 16; ipiece++)
                    {
                        for (int ipos = 0; ipos < 64; ipos++)
                        {
                            _history[ipiece][ipos] = _history[ipiece][ipos] / 2;
                            _maxPositionalGain[ipiece][ipos] = _maxPositionalGain[ipiece][ipos] / 2;
                        }
                    }
                    _count = 0;
                }
                else
                {
                    _count++;
                }
            }

            public void RegisterPositionalGain(Move move, int positionalGain)
            {
                var piece = move.MovingPiece();
                var to = move.To();
                _maxPositionalGain[(int)piece][(int)to] = Math.Max(positionalGain, _maxPositionalGain[(int)piece][(int)to]);
            }

            public int ReadMaxPositionalGain(Move move)
            {
                var piece = move.MovingPiece();
                var to = move.To();
                return _maxPositionalGain[(int)piece][(int)to];
            }

            public void RegisterCutoff(Move move, SearchDepth depth)
            {
                System.Diagnostics.Debug.Assert(!move.IsCapture());

                Age(); //every so often, reduce scores.

                Piece piece = move.MovingPiece();
                Position to = move.To();

                

                int ply = depth.ToPly();
                int value = (ply * 2) * (ply * 2);
                _history[(int)piece][(int)to] += value;

            }

            public void RegisterFailLow(Board board, Move move, SearchDepth depth)
            {
                System.Diagnostics.Debug.Assert(!move.IsCapture() && !move.IsEnPassant());

                Age(); //every so often, reduce scores.

                Piece piece = move.MovingPiece();
                Position to = move.To();

                

                int ply = depth.ToPly();
                int value = (ply * 2) * (ply * 2);
                _history[(int)piece][(int)to] -= value;
            }

            public int HistoryScore(Move move)
            {
                var piece = (int)move.MovingPiece();
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
        private readonly Move[] _failLows = new Move[192];
        private int _failLowCount = 0;

        private readonly MoveHistory _history;
        private readonly StaticExchange _see;

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

        public MovePicker(MoveHistory history, StaticExchange see)
        {
            _history = history;
            _see = see;
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
            _failLowCount = 0;
            //_moveCount = ChessMoveInfo.GenMovesArray(_array, board, capsOnly);
            //_moveCurrent = 0;
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

            _capsCount = MoveUtil.GenCapsNonCaps(array, _board, true, 0); //generate capture
            _capsCount = ExcludeFrom(array, 0, _capsCount, _exclude, _excludeCount); //remove trans table move from array

            for (int i = 0; i < _capsCount; i++)
            {
                array[i].SEE = _see.CalculateScore(_board, array[i].Move); //calculate if winning capture.
                array[i].Flags = MoveFlags.Capture;

                if (array[i].SEE >= 0) { _capsGoodCount++; } //incr good cap count.

                Move move = array[i].Move;

                array[i].Score = array[i].SEE + PcSqChange(move.MovingPiece(), move.From(), move.To());

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
                if (move != Move.EMPTY && move.IsPsuedoLegal(_board))
                {
                    System.Diagnostics.Debug.Assert(!move.IsCapture());
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

            _quietCount = MoveUtil.GenCapsNonCaps(array, _board, false, 0);
            _quietCount = ExcludeFrom(array, 0, _quietCount, _exclude, _excludeCount);
            for (int i = 0; i < _quietCount; i++)
            {
                Move move = array[i].Move;
                int see = _see.CalculateScore(_board, move);
                array[i].SEE = see;
                array[i].Flags = 0;

                
                int pcsq = PcSqChange(move.MovingPiece(), move.From(), move.To()) / 4;

                array[i].Score =
                    _history.HistoryScore(move)
                    + pcsq
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

        
        public void RegisterCutoff(Board board, ChessMoveData moveData, SearchDepth depth)
        {
            Move move = moveData.Move;
            if (!move.IsCapture())
            {
                //store killer moves in MovePicker
                var killers = _killers[(int)board.WhosTurn];

                //store as killer
                if (move != killers[0])
                {
                    killers[1] = killers[0];
                    killers[0] = move;
                }

                //store to history object.
                _history.RegisterCutoff(move, depth);

                //loop through all previously tried quiet moves and reduce history score.
                for (int i = 0; i < _failLowCount; i++)
                {
                    _history.RegisterFailLow(board, _failLows[i], depth);
                }
            }


            
        }

        public void RegisterFailLow(Board board, ChessMoveData moveData, SearchDepth depth)
        {
            if (!moveData.Move.IsCapture())
            {
                if (moveData.SEE >= 0)
                {
                    //do not mark it in history table UNTIL we get a fail-high. otherwise will pollute table with too many negatives.
                    _failLows[_failLowCount++] = moveData.Move;
                }
                
                //store to history object.
                //_history.RegisterFailLow(board, move, depth); 
            }
        }


    }


    [System.Diagnostics.DebuggerDisplay(@"{Move.Description()} SEE:{SEE} Score:{Score} Flags:{Flags}")]
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
