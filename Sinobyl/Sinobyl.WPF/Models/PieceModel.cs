using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinobyl.Engine;
using System.ComponentModel;

namespace Sinobyl.WPF.Models
{
    public class PieceModel: IPieceModel
    {
        private ChessPiece _piece;
        private ChessPosition _position;

        public event PropertyChangedEventHandler PropertyChanged;

        public ChessPiece Piece
        {
            get
            {
                return _piece;
            }
            set
            {
                if (_piece == value) { return; }
                _piece = value;
                OnPropertyChanged("Piece");
            }
        }
        public ChessPosition Position
        {
            get
            {
                return _position;
            }
            set
            {
                if (_position == value) { return; }
                _position = value;
                OnPropertyChanged("Position");
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

    }
}
