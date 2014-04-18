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

        public class KillerInfo
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

        public class PlyBuffer
        {
            private readonly MoveData[] _array = new MoveData[192];
            private int _moveCount;
            private int _moveCurrent;

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
                    int pcSq = 0;
                    int dummyPcSq = 0;
                    board.PcSqEvaluator.PcSqValuesRemove(piece, move.From(), ref pcSq, ref dummyPcSq);
                    board.PcSqEvaluator.PcSqValuesAdd(piece, move.To(), ref pcSq, ref dummyPcSq);
                    if (board.WhosTurn == ChessPlayer.Black) { pcSq = -pcSq; }

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
                    _array[i].PcSq = pcSq;
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

        [System.Diagnostics.DebuggerDisplay(@"{Move.Description()} SEE:{SEE} PcSq:{PcSq} Flags:{Flags}")]
        public struct MoveData
        {
            public ChessMove Move;
            public int SEE;
            public int PcSq;
            public MoveFlags Flags;
            public int Score;

            public int ScoreCalc()
            {
                return SEE + PcSq
                    + ((Flags & MoveFlags.TransTable) != 0 ? 100000 : 0)
                    + ((Flags & MoveFlags.Capture) != 0 ? 10000 : 0)
                    + ((Flags & MoveFlags.Killer) != 0 ? 1000 : 0);
            }
            public static bool operator >(MoveData x, MoveData y)
            {
                return x.Score > y.Score;
                //first check if any of the flags used for ordering are different, if they are, order using the relevant flags value.
                //var xOrderFlags = x.Flags & _orderFlags;
                //var yOrderFlags = y.Flags & _orderFlags;
                if (x.Flags != y.Flags)
                {
                    //int x2 = (int)xOrderFlags;
                    //int y2 = (int)yOrderFlags;
                    return x.Flags > y.Flags;
                }
                else if (x.SEE != y.SEE)
                {
                    return x.SEE > y.SEE;
                }
                else
                {
                    //return false;
                    return x.PcSq > y.PcSq;
                }
            }
            public static bool operator <(MoveData x, MoveData y)
            {
                return !(x > y);
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
}
