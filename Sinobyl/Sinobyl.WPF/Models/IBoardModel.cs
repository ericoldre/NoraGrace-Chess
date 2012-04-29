using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinobyl.Engine;
using System.Collections.ObjectModel;

namespace Sinobyl.WPF.Models
{
    public interface IBoardModel
    {
        ObservableCollection<IPieceModel> Pieces { get; }
        ObservableCollection<ChessMove> Moves { get; }
        void ApplyMove(ChessMove move);
    }
}
