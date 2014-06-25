using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private class KillerInfo
        {
            public Move move1 { get; private set; }
            public Move move2 { get; private set; }
            private readonly Move[][] _counterMoves = new Move[7][]; //[7][64];
            private readonly int[][] _history = new int[7][];
            public KillerInfo()
            {
                for (int i = 0; i <= _counterMoves.GetUpperBound(0); i++) { _counterMoves[i] = new Move[64]; }
                for (int i = 0; i <= _history.GetUpperBound(0); i++) { _history[i] = new int[64]; }
            }

            public void RegisterKiller(Board board, Move move)
            {
                //record as killer
                if (move != move1 && board.PieceAt(move.To()) == Piece.EMPTY)
                {
                    move2 = move1;
                    move1 = move;
                }

                if (board.PieceAt(move.To()) == Piece.EMPTY)
                {
                    //save to history table
                    var ptype = (int)board.PieceAt(move.From()).ToPieceType();
                    var to = (int)move.To();
                    var newscore = Math.Min(1000000, _history[ptype][to] + 1000);
                    _history[ptype][to] = newscore;
                }

            }

            public void RegisterFailLow(Board board, Move move)
            {
                //decrease value in history table
                var ptype = (int)board.PieceAt(move.From()).ToPieceType();
                var to = (int)move.To();
                _history[ptype][to] = _history[ptype][to] / 2;
            }

            public int HistoryScore(Board board, Move move)
            {
                var ptype = (int)board.PieceAt(move.From()).ToPieceType();
                var to = (int)move.To();
                var score = _history[ptype][to];
                return score;
            }

            public bool IsKiller(Move move)
            {
                return move == move1 || move == move2;
            }

            public Move this[int index]
            {
                get
                {
                    if (index == 0) { return move1; }
                    if (index == 1) { return move2; }
                    throw new ArgumentOutOfRangeException();
                }
            }

            public int Count
            {
                get { return 2; }
            }

        }

        private enum steps
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

        private readonly ChessMoveData[] _array = new ChessMoveData[192];

        private readonly KillerInfo[] _playerKillers = new KillerInfo[2];

        private readonly Move[] _killers = new Move[3];
        private Move _ttMove;
        private Board _board;
        private long _boardZob;
        private bool _capsOnly;
        private steps _currStep;
        private ChessMoveData _tmpData;
        private int _capsCount;
        private int _capsGoodCount;
        private int _currIndex;
        private int _quietCount;

        private readonly Move[] _exclude = new Move[20];
        private int _excludeCount = 0;

        public MovePicker()
        {
            _playerKillers[0] = new KillerInfo();
            _playerKillers[1] = new KillerInfo();

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
            _capsCount = MoveInfo.GenCapsNonCaps(_array, _board, true, 0);
            _capsCount = ExcludeFrom(_array, 0, _capsCount, _exclude, _excludeCount);
            for (int i = 0; i < _capsCount; i++)
            {
                _array[i].SEE = StaticExchange.CalculateScore(_array[i].Move, _board); //calculate if winning capture.
                _array[i].Flags = MoveFlags.Capture;
                if (_array[i].SEE >= 0) { _capsGoodCount++; } //incr good cap count.

                Move move = _array[i].Move;
                Piece piece = _board.PieceAt(move.From());

                _array[i].Score = _array[i].SEE + PcSqChange(piece, move.From(), move.To());

            }
            SortMoveData(_array, 0, _capsCount);

            _currIndex = 0;
            _currStep++;
            return NextMoveData();
        }

        private ChessMoveData StepGoodCaps()
        {
            if (_currIndex < _capsGoodCount)
            {
                return _array[_currIndex++];
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
            var killerInfo = _playerKillers[(int)_board.WhosTurn];
            if (_currIndex < killerInfo.Count)
            {
                Move move = killerInfo[_currIndex];
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
                return _array[badCapIndex];
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

            _quietCount = MoveInfo.GenCapsNonCaps(_array, _board, false, 0);
            _quietCount = ExcludeFrom(_array, 0, _quietCount, _exclude, _excludeCount);
            for (int i = 0; i < _quietCount; i++)
            {
                Move move = _array[i].Move;
                int see = StaticExchange.CalculateScore(move, _board);
                _array[i].SEE = see;
                _array[i].Flags = 0;

                
                Piece piece = _board.PieceAt(move.From());


                _array[i].Score =
                    _playerKillers[(int)_board.WhosTurn].HistoryScore(_board, move)
                    + PcSqChange(piece, move.From(), move.To())
                    + (see < 0 ? -1000 : 0);

            }

            SortMoveData(_array, 0, _quietCount);

            _currIndex = 0;
            _currStep++;
            return NextMoveData();

        }

        private ChessMoveData StepQuiet()
        {
            if (_capsOnly)
            {
                _tmpData.Move = Move.EMPTY;
                return _tmpData;
            }

            if (_currIndex < _quietCount)
            {
                var m = _array[_currIndex].Move;

                return _array[_currIndex++];


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
                case steps.ttMove:
                    return StepTTMove();

                case steps.InitCaps:
                    return StepInitCaps();
                case steps.GoodCaps:
                    return StepGoodCaps();

                case steps.Killers:
                    return StepKillers();

                case steps.BadCaps:
                    return StepBadCaps();

                case steps.InitQuiet:
                    return StepInitQuiet();

                case steps.Quiet:
                    return StepQuiet();

                case steps.Done:
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
            PhasedScore pcSq = 0;
            _board.PcSqEvaluator.PcSqValuesRemove(piece, from, ref pcSq);
            _board.PcSqEvaluator.PcSqValuesAdd(piece, to, ref pcSq);
            if (_board.WhosTurn == Player.Black) { pcSq = pcSq.Negate(); }
            return pcSq.Opening();
        }

        private void SortMoveData(ChessMoveData[] array, int index, int length)
        {
            ChessMoveData temp;
            for (int i = index; i < length; i++)
            {
                for (int ii = i; ii > index; ii--)
                {
                    if (_array[ii].Score > _array[ii - 1].Score)
                    {
                        temp = _array[ii];
                        _array[ii] = _array[ii - 1];
                        _array[ii - 1] = temp;
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
            _playerKillers[(int)board.WhosTurn].RegisterKiller(board, move);
        }

        public void RegisterFailLow(Board board, Move move)
        {
            _playerKillers[(int)board.WhosTurn].RegisterFailLow(board, move);
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
