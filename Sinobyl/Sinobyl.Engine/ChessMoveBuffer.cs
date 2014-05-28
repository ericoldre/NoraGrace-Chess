using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinobyl.Engine
{
    public class ChessMoveBuffer
    {
        List<PlyBuffer> _plyBuffers = new List<PlyBuffer>();

        public ChessMoveBuffer(int plyCapacity = 50)
        {
            while (_plyBuffers.Count < plyCapacity)
            {
                _plyBuffers.Add(new PlyBuffer());
            }
        }

        public PlyBuffer this[int ply]
        {
            get
            {
                if (ply > _plyBuffers.Count)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        _plyBuffers.Add(new PlyBuffer());
                    }
                }
                return _plyBuffers[ply];
            }
        }

    }

    public class PlyBuffer
    {

        private class KillerInfo
        {
            public ChessMove move1 { get; private set; }
            public ChessMove move2 { get; private set; }

            public void RegisterKiller(ChessMove move)
            {
                if (move != move1)
                {
                    move2 = move1;
                    move1 = move;
                }
            }

            public bool IsKiller(ChessMove move)
            {
                return move == move1 || move == move2;
            }
        }

        private readonly ChessMoveData[] _array = new ChessMoveData[192];
        private int _moveCount;
        private int _moveCurrent;
        public readonly PlyBuffer2 Buffer2 = new PlyBuffer2();

        private readonly KillerInfo[] _playerKillers = new KillerInfo[2];
        private readonly ChessMove[][] _counterMoves = new ChessMove[16][]; //[16][64];
        public PlyBuffer()
        {
            _playerKillers[0] = new KillerInfo();
            _playerKillers[1] = new KillerInfo();
            for (int i = 0; i <= _counterMoves.GetUpperBound(0); i++) { _counterMoves[i] = new ChessMove[64]; }
        }

        public void Initialize(ChessBoard board, bool capsOnly = false)
        {
            _moveCount = ChessMoveInfo.GenMovesArray(_array, board, capsOnly);
            _moveCurrent = 0;
        }

        public int MoveCount
        {
            get { return _moveCount; }
        }

        public ChessMove NextMove()
        {
            if (_moveCurrent < _moveCount)
            {
                return _array[_moveCurrent++].Move;
            }
            else
            {
                return ChessMove.EMPTY;
            }
        }

        public ChessMoveData NextMoveData()
        {
            if (_moveCurrent < _moveCount)
            {
                return _array[_moveCurrent++];
            }
            else
            {
                return new ChessMoveData() { Move = ChessMove.EMPTY };
            }

        }

        public void RegisterCutoff(ChessBoard board, ChessMove move)
        {
            if (board.PieceAt(move.To()) == ChessPiece.EMPTY)
            {
                _playerKillers[(int)board.WhosTurn].RegisterKiller(move);

                if (board.HistoryCount > 0 && board.MovesSinceNull > 0)
                {
                    //register counter move hueristic.
                    ChessMove prevMove = board.HistMove(1);
                    ChessPiece prevPiece = board.PieceAt(prevMove.To());
                    System.Diagnostics.Debug.Assert(prevPiece.PieceToPlayer() == board.WhosTurn.PlayerOther());
                    _counterMoves[(int)prevPiece][(int)prevMove.To()] = move;
                }

            }
        }

        public void Sort(ChessBoard board, bool useSEE, ChessMove ttMove)
        {
            //useSEE = true;
            var killers = _playerKillers[(int)board.WhosTurn];

            ChessMove killer1 = killers.move1;
            ChessMove killer2 = killers.move2;

            //find previous move and last quiet move used to counter it.
            ChessMove counterMove = ChessMove.EMPTY;
            if (board.HistoryCount > 0)
            {
                ChessMove prevMove = board.HistMove(1);
                ChessPiece prevPiece = board.PieceAt(prevMove.To());
                //System.Diagnostics.Debug.Assert(prevPiece.PieceToPlayer() == board.WhosTurn.PlayerOther());
                counterMove = _counterMoves[(int)prevPiece][(int)prevMove.To()];
            }


            //first score moves
            for (int i = 0; i < _moveCount; i++)
            {
                ChessMove move = _array[i].Move;
                ChessPiece piece = board.PieceAt(move.From());
                bool isCap = board.PieceAt(move.To()) != ChessPiece.EMPTY;

                System.Diagnostics.Debug.Assert(move != ChessMove.EMPTY);

                //calc pcsq value;
                PhasedScore pcSq = 0;
                board.PcSqEvaluator.PcSqValuesRemove(piece, move.From(), ref pcSq);
                board.PcSqEvaluator.PcSqValuesAdd(piece, move.To(), ref pcSq);
                if (board.WhosTurn == ChessPlayer.Black) { pcSq = pcSq.Negate(); }

                int see = 0;

                //calc flags
                MoveFlags flags = 0;
                if (move == ttMove) { flags |= MoveFlags.TransTable; }
                if (move.Promote() != ChessPiece.EMPTY) { flags |= MoveFlags.Promote; }

                if (isCap)
                {
                    flags |= MoveFlags.Capture;
                    see = ChessMoveSEE.CompEstScoreSEE(move, board);
                    //if (see > 0) { flags |= MoveFlags.CapturePositive; }
                    //if (see == 0) { flags |= MoveFlags.CaptureEqual; }
                }

                //if(piece.ToPieceType() == ChessPieceType.Pawn && (move.To.GetRank() == ChessRank.Rank7 || move.To.GetRank() == ChessRank.Rank2))
                //{
                //    flags |= MoveFlags.Pawn7th;
                //}

                if (move == counterMove || move == killer1 || move == killer2)
                {
                    flags |= MoveFlags.Killer;
                }

                //if (killers.IsKiller(move)) { flags |= MoveFlags.Killer; }

                _array[i].SEE = see;
                _array[i].PcSq = pcSq.Opening();
                _array[i].Flags = flags;
                _array[i].Score = _array[i].ScoreCalc();

            }

            //now sort array.
            for (int i = 1; i < _moveCount; i++)
            {
                for (int ii = i; ii > 0; ii--)
                {
                    if (_array[ii].Score > _array[ii - 1].Score)
                    {
                        var tmp = _array[ii];
                        _array[ii] = _array[ii - 1];
                        _array[ii - 1] = tmp;
                    }
                    else
                    {
                        break;
                    }
                }
            }

        }

        public IEnumerable<ChessMove> SortedMoves()
        {
            for (int i = 0; i < _moveCount; i++)
            {
                yield return _array[i].Move;
            }
        }

    }

    public class PlyBuffer2
    {

        private class KillerInfo
        {
            public ChessMove move1 { get; private set; }
            public ChessMove move2 { get; private set; }


            public void RegisterKiller(ChessMove move)
            {
                if (move != move1)
                {
                    move2 = move1;
                    move1 = move;
                }
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
        private int _killerCount;
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
            _killerCount = 0;
            _quietCount = 0;
            _excludeCount = 0;
            //_moveCount = ChessMoveInfo.GenMovesArray(_array, board, capsOnly);
            //_moveCurrent = 0;
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
                        for (int ii = i; ii > 0; ii--)
                        {
                            if (_array[ii].SEE > _array[ii - 1].SEE)
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
                    if (_currIndex < _killerCount)
                    {
                        ChessMove move = killerInfo[_currIndex];
                        if (move.IsPsuedoLegal(_board))
                        {
                            _tmpData.Move = move;
                            _tmpData.Flags = MoveFlags.Killer;
                            _currIndex++;
                            _exclude[_excludeCount++] = _ttMove;
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
                        //if (_array[i].SEE >= 0) { _capsGoodCount++; } //incr good cap count.
                        //for (int ii = i; ii > 0; ii--)
                        //{
                        //    if (_array[ii].SEE > _array[ii - 1].SEE)
                        //    {
                        //        _tmpData = _array[ii];
                        //        _array[ii] = _array[ii - 1];
                        //        _array[ii - 1] = _tmpData;
                        //    }
                        //    else
                        //    {
                        //        break;
                        //    }
                        //}
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

                        if (!_playerKillers[(int)_board.WhosTurn].IsKiller(m))
                        {
                            return _array[_currIndex++];
                        }
                        else
                        {
                            _currIndex++;
                            return NextMoveData();
                        }
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
            if (board.PieceAt(move.To()) == ChessPiece.EMPTY)
            {
                _playerKillers[(int)board.WhosTurn].RegisterKiller(move);

                //if (board.HistoryCount > 0 && board.MovesSinceNull > 0)
                //{
                //    //register counter move hueristic.
                //    ChessMove prevMove = board.HistMove(1);
                //    ChessPiece prevPiece = board.PieceAt(prevMove.To());
                //    System.Diagnostics.Debug.Assert(prevPiece.PieceToPlayer() == board.WhosTurn.PlayerOther());
                //    _counterMoves[(int)prevPiece][(int)prevMove.To()] = move;
                //}

            }
        }


    }


    [System.Diagnostics.DebuggerDisplay(@"{Move.Description()} SEE:{SEE} PcSq:{PcSq} Flags:{Flags}")]
    public struct ChessMoveData
    {
        public ChessMove Move;
        public int SEE;
        public int PcSq;
        public MoveFlags Flags;
        public int Score;

        public int ScoreCalc()
        {
            return SEE + PcSq
                + ((Flags & MoveFlags.TransTable) != 0 ? 1000000 : 0)
                + ((Flags & MoveFlags.Capture) != 0 && SEE >= 0 ? 100000 : 0)
                + ((Flags & MoveFlags.Killer) != 0 ? 10000 : 0)
                + ((Flags & MoveFlags.Capture) != 0 ? 1000 : 0);
        }


        //public void SetScores(ChessMove move, int see, int pcSq, MoveFlags flags)
        //{
        //    Move = move;
        //    SEE = see;
        //    PcSq = pcSq;
        //    Flags = flags;
        //}

        //private const MoveFlags _orderFlags = MoveFlags.TransTable | MoveFlags.Promote | MoveFlags.CapturePositive | MoveFlags.CaptureEqual | MoveFlags.Killer;
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
