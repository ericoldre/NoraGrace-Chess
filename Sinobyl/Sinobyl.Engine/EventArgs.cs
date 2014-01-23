﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{
    public class FENEventArgs : EventArgs
    {
        public ChessFEN FEN { get; private set; }
        public FENEventArgs(ChessFEN fen)
        {
            FEN = fen;
        }

    }

    public class MoveEventArgs : EventArgs
    {
        public ChessMove Move { get; private set; }
        public MoveEventArgs(ChessMove move)
        {
            Move = move;
        }
    }

    public class SearchProgressEventArgs : EventArgs
    {
        public ChessSearch.Progress Progress { get; private set; }
        public SearchProgressEventArgs(ChessSearch.Progress progress)
        {
            Progress = progress;
        }
    }
    public class SearchProgressPlayerEventArgs : SearchProgressEventArgs
    {
        public ChessPlayer Player { get; private set; }
        public SearchProgressPlayerEventArgs(ChessSearch.Progress progress, ChessPlayer player)
            : base(progress)
        {
            Player = player;
        }
    }
}
