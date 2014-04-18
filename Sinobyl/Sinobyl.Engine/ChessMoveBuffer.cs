﻿using System;
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
            private ChessMove move1;
            private ChessMove move2;

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
            private readonly MoveInfo[] _array = new MoveInfo[192];
            private int moveCount;

            private readonly KillerInfo[] _playerKillers = new KillerInfo[2];

            public PlyBuffer()
            {
                _playerKillers[0] = new KillerInfo();
                _playerKillers[1] = new KillerInfo();
            }

            public void Initialize(ChessBoard board, bool capsOnly = false)
            {
                moveCount = 0;

                foreach (ChessMove genMove in ChessMoveInfo.GenMoves(board, capsOnly))
                {
                    _array[moveCount++].Move = genMove;// = new MoveInfo() { Move = genMove };
                }
            }

            public int MoveCount
            {
                get { return moveCount; }
            }

            public void RegisterCutoff(ChessBoard board, ChessMove move)
            {
                if (board.PieceAt(move.To()) != ChessPiece.EMPTY)
                {
                    _playerKillers[(int)board.WhosTurn].RegisterKiller(move);
                }
            }

            public void Sort(ChessBoard board, bool useSEE, ChessMove ttMove)
            {
                //useSEE = true;
                var killers = _playerKillers[(int)board.WhosTurn];
                //first score moves
                for (int i = 0; i < moveCount; i++)
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

                    //if (killers.IsKiller(move)) { flags |= MoveFlags.Killer; }

                    _array[i].SEE = see;
                    _array[i].PcSq = pcSq;
                    _array[i].Flags = flags;
                    _array[i].Score = _array[i].ScoreCalc();

                }

                //now sort array.
                for (int i = 1; i < moveCount; i++)
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
                for (int i = 0; i < moveCount; i++)
                {
                    yield return _array[i].Move;
                }
            }

        }

        [System.Diagnostics.DebuggerDisplay(@"{Move} SEE:{SEE} PcSq:{PcSq} Flags:{Flags}")]
        public struct MoveInfo
        {
            public ChessMove Move;
            public int SEE;
            public int PcSq;
            public MoveFlags Flags;
            public int Score;

            public int ScoreCalc()
            {
                return SEE + PcSq
                    + ((Flags & MoveFlags.TransTable) != 0 ? 10000 : 0)
                    + ((Flags & MoveFlags.Capture) != 0 ? 1000 : 0);
            }
            public static bool operator >(MoveInfo x, MoveInfo y)
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
            public static bool operator <(MoveInfo x, MoveInfo y)
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