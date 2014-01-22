using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{
    public class EventArgsFEN : EventArgs
    {
        public ChessFEN FEN { get; private set; }
        public EventArgsFEN(ChessFEN fen)
        {
            FEN = fen;
        }

    }

    public class EventArgsMove : EventArgs
    {
        public ChessMove Move { get; private set; }
        public EventArgsMove(ChessMove move)
        {
            Move = move;
        }
    }

    public class EventArgsSearchProgress : EventArgs
    {
        public ChessSearch.Progress Progress { get; private set; }
        public EventArgsSearchProgress(ChessSearch.Progress progress)
        {
            Progress = progress;
        }
    }
    public class EventArgsSearchProgressPlayer : EventArgsSearchProgress
    {
        public ChessPlayer Player { get; private set; }
        public EventArgsSearchProgressPlayer(ChessSearch.Progress progress, ChessPlayer player)
            : base(progress)
        {
            Player = player;
        }
    }
}
