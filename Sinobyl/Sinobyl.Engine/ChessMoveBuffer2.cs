using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinobyl.Engine
{
    public class PlyBuffer2
    {

        private class KillerInfo
        {
            public ChessMove move1 { get; private set; }
            public ChessMove move2 { get; private set; }
            private readonly ChessMove[][] _counterMoves = new ChessMove[7][]; //[7][64];
            private readonly int[][] _history = new int[7][];
            public KillerInfo()
            {
                for (int i = 0; i <= _counterMoves.GetUpperBound(0); i++) { _counterMoves[i] = new ChessMove[64]; }
                for (int i = 0; i <= _history.GetUpperBound(0); i++) { _history[i] = new int[64]; }
            }

            public void RegisterKiller(ChessBoard board, ChessMove move)
            {
                //record as killer
                if (move != move1 && board.PieceAt(move.To()) == ChessPiece.EMPTY)
                {
                    move2 = move1;
                    move1 = move;
                }

                if (board.PieceAt(move.To()) == ChessPiece.EMPTY)
                {
                    //save to history table
                    var ptype = (int)board.PieceAt(move.From()).ToPieceType();
                    var to = (int)move.To();
                    var newscore = Math.Min(1000000, _history[ptype][to] + 1000);
                    _history[ptype][to] = newscore;
                }

            }

            public void RegisterFailLow(ChessBoard board, ChessMove move)
            {
                //decrease value in history table
                var ptype = (int)board.PieceAt(move.From()).ToPieceType();
                var to = (int)move.To();
                _history[ptype][to] = _history[ptype][to] / 2;
            }

            public int HistoryScore(ChessBoard board, ChessMove move)
            {
                var ptype = (int)board.PieceAt(move.From()).ToPieceType();
                var to = (int)move.To();
                var score = _history[ptype][to];
                return score;
            }

            public bool IsKiller(ChessMove move)
            {
                return move == move1 || move == move2;
            }

            public ChessMove this[int index]
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
        }

        private readonly ChessMoveData[] _array = new ChessMoveData[192];

        private readonly KillerInfo[] _playerKillers = new KillerInfo[2];

        private readonly ChessMove[] _killers = new ChessMove[3];
        private ChessMove _ttMove;
        private ChessBoard _board;
        private long _boardZob;
        private bool _capsOnly;
        private steps _currStep;
        private ChessMoveData _tmpData;
        private int _capsCount;
        private int _capsGoodCount;
        private int _currIndex;
        private int _quietCount;

        private readonly ChessMove[] _exclude = new ChessMove[20];
        private int _excludeCount = 0;

        public PlyBuffer2()
        {
            _playerKillers[0] = new KillerInfo();
            _playerKillers[1] = new KillerInfo();

        }

        public void Initialize(ChessBoard board, ChessMove ttMove = ChessMove.EMPTY, bool capsOnly = false)
        {
            _board = board;
            _boardZob = board.Zobrist;
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

        public void Sort(ChessBoard board, bool useSEE, ChessMove ttMove)
        {

        }


        public ChessMoveData NextMoveData()
        {
            System.Diagnostics.Debug.Assert(_board.Zobrist == _boardZob);
            switch (_currStep)
            {
                case steps.ttMove:
                    if (_ttMove != ChessMove.EMPTY)
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

                case steps.InitCaps:
                    _capsCount = ChessMoveInfo.GenCapsNonCaps(_array, _board, true, 0);
                    _capsCount = ExcludeFrom(_array, 0, _capsCount, _exclude, _excludeCount);
                    for (int i = 0; i < _capsCount; i++)
                    {
                        _array[i].SEE = ChessMoveSEE.CompEstScoreSEE(_array[i].Move, _board); //calculate if winning capture.
                        _array[i].Flags = MoveFlags.Capture;
                        if (_array[i].SEE >= 0) { _capsGoodCount++; } //incr good cap count.

                        ChessMove move = _array[i].Move;
                        ChessPiece piece = _board.PieceAt(move.From());

                        //calc pcsq value;
                        PhasedScore pcSq = 0;
                        _board.PcSqEvaluator.PcSqValuesRemove(piece, move.From(), ref pcSq);
                        _board.PcSqEvaluator.PcSqValuesAdd(piece, move.To(), ref pcSq);
                        if (_board.WhosTurn == ChessPlayer.Black) { pcSq = pcSq.Negate(); }
                        _array[i].PcSq = pcSq.Opening();

                        for (int ii = i; ii > 0; ii--)
                        {
                            if (_array[ii].SEE + _array[ii].PcSq > _array[ii - 1].SEE + _array[ii - 1].PcSq)
                            {
                                _tmpData = _array[ii];
                                _array[ii] = _array[ii - 1];
                                _array[ii - 1] = _tmpData;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    _currIndex = 0;
                    _currStep++;
                    return NextMoveData();
                case steps.GoodCaps:
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

                case steps.Killers:
                    if (_capsOnly)
                    {
                        _currIndex = 0;
                        _currStep++;
                        return NextMoveData();
                    }
                    var killerInfo = _playerKillers[(int)_board.WhosTurn];
                    if (_currIndex < killerInfo.Count)
                    {
                        ChessMove move = killerInfo[_currIndex];
                        if (move.IsPsuedoLegal(_board))
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
                case steps.BadCaps:
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
                case steps.InitQuiet:
                    if (_capsOnly)
                    {
                        _currIndex = 0;
                        _currStep++;
                        return NextMoveData();
                    }

                    _quietCount = ChessMoveInfo.GenCapsNonCaps(_array, _board, false, 0);
                    _quietCount = ExcludeFrom(_array, 0, _quietCount, _exclude, _excludeCount);
                    for (int i = 0; i < _quietCount; i++)
                    {
                        _array[i].SEE = 0;
                        _array[i].Flags = 0;
                        _array[i].PcSq = 0;

                        ChessMove move = _array[i].Move;
                        ChessPiece piece = _board.PieceAt(move.From());

                        //calc pcsq value;
                        PhasedScore pcSq = 0;
                        _board.PcSqEvaluator.PcSqValuesRemove(piece, move.From(), ref pcSq);
                        _board.PcSqEvaluator.PcSqValuesAdd(piece, move.To(), ref pcSq);
                        if (_board.WhosTurn == ChessPlayer.Black) { pcSq = pcSq.Negate(); }

                        _array[i].PcSq = pcSq.Opening();

                        _array[i].PcSq = _playerKillers[(int)_board.WhosTurn].HistoryScore(_board, move) +pcSq.Opening();

                        //if (_array[i].SEE >= 0) { _capsGoodCount++; } //incr good cap count.
                        for (int ii = i; ii > 0; ii--)
                        {
                            if (_array[ii].PcSq > _array[ii - 1].PcSq)
                            {
                                _tmpData = _array[ii];
                                _array[ii] = _array[ii - 1];
                                _array[ii - 1] = _tmpData;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    _currIndex = 0;
                    _currStep++;
                    return NextMoveData();
                case steps.Quiet:
                    if (_capsOnly)
                    {
                        _tmpData.Move = ChessMove.EMPTY;
                        return _tmpData;
                    }

                    if (_currIndex < _quietCount)
                    {
                        var m = _array[_currIndex].Move;

                        return _array[_currIndex++];


                    }
                    else
                    {
                        _tmpData.Move = ChessMove.EMPTY;
                        return _tmpData;
                    }
                    break;
                default:
                    System.Diagnostics.Debug.Assert(false);
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static int ExcludeFrom(ChessMoveData[] source, int sourceStart, int sourceEnd, ChessMove[] exclude, int excludeCount)
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

        public IEnumerable<ChessMove> SortedMoves()
        {
            ChessMoveData moveData;

            while ((moveData = NextMoveData()).Move != ChessMove.EMPTY)
            {
                yield return moveData.Move;
            }
        }

        public void RegisterCutoff(ChessBoard board, ChessMove move)
        {
            _playerKillers[(int)board.WhosTurn].RegisterKiller(board, move);
        }

        public void RegisterFailLow(ChessBoard board, ChessMove move)
        {
            _playerKillers[(int)board.WhosTurn].RegisterFailLow(board, move);
        }


    }

}
