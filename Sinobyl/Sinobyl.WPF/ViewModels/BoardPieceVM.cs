using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinobyl.Engine;
namespace Sinobyl.WPF.ViewModels
{
    public class BoardPieceVM: ViewModelBase
    {
        private ChessPiece _piece;
        private ChessPosition _position;
        private RelayCommand _dragStartCommand;
        private bool _isDraggable;
        
        public ChessPiece Piece
        {
            get
            {
                return _piece;
            }
            set
            {
                if (value == _piece) { return; }
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
                if (value == _position) { return; }
                _position = value;
                OnPropertyChanged("Position");
            }
        }

        public bool IsDraggable
        {
            get
            {
                return _isDraggable;
            }
            set
            {
                if (value == _isDraggable) { return; }
                _isDraggable = value;
                OnPropertyChanged("IsDraggable");
            }
        }
        public RelayCommand DragStartCommand
        {
            get
            {
                if (_dragStartCommand == null)
                {
                    _dragStartCommand = new RelayCommand(param => DragStartAction(param));
                        
                }
                return _dragStartCommand;
            }
        }
        private object DragStartAction(object param)
        {
            int i = 1;
            return null;
        }

        

    }
}
