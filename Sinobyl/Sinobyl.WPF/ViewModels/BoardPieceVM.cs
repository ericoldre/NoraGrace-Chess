using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinobyl.Engine;
namespace Sinobyl.WPF.ViewModels
{
    public class BoardPieceVM: ViewModelBase
    {
        private readonly BoardVM _boardVM;
        private readonly ChessPiece _piece;
        private readonly ChessPosition _position;
        private RelayCommand _dragStartCommand;
        private bool _isDraggable;

        public BoardPieceVM(ChessPiece piece, ChessPosition pos, BoardVM boardVM)
        {
            _piece = piece;
            _position = pos;
            _boardVM = boardVM;
            _boardVM.MoveDestinations(pos).CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(MovesChanged);
            IsDraggable = _boardVM.MoveDestinations(_position).Count > 0;
        }

        void MovesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            IsDraggable = _boardVM.MoveDestinations(_position).Count > 0;
        }

        public ChessPiece Piece
        {
            get
            {
                return _piece;
            }
        }
        public ChessPosition Position
        {
            get
            {
                return _position;
            }
        }

        public bool IsDraggable
        {
            get
            {
                return _isDraggable;
            }
            private set
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
