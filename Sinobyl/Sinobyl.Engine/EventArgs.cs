﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{
    public class FENEventArgs : EventArgs
    {
        public FEN FEN { get; private set; }
        public FENEventArgs(FEN fen)
        {
            FEN = fen;
        }

    }

    public class MoveEventArgs : EventArgs
    {
        public Move Move { get; private set; }
        public MoveEventArgs(Move move)
        {
            Move = move;
        }
    }

    public class SearchProgressEventArgs : EventArgs
    {
        public ChessSearch.Progress Progress { get; private set; }
        public SearchProgressEventArgs(ChessSearch.Progress progress)
        {
            if (progress == null) { throw new ArgumentNullException("progress"); }
            Progress = progress;
        }
    }
    public class SearchProgressPlayerEventArgs : SearchProgressEventArgs
    {
        public Player Player { get; private set; }
        public SearchProgressPlayerEventArgs(ChessSearch.Progress progress, Player player)
            : base(progress)
        {
            if (player != Player.White && player != Player.Black) { throw new ArgumentOutOfRangeException("player"); }
            Player = player;
        }
    }
}
