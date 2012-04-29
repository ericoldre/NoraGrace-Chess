using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinobyl.Engine;
using System.ComponentModel;
namespace Sinobyl.WPF.Models
{
    public interface IPieceModel: INotifyPropertyChanged
    {
        ChessPiece Piece { get; }
        ChessPosition Position { get; }
    }
}
