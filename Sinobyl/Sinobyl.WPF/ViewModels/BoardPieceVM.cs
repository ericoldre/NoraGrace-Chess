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
        private RelayCommand _dragVectorCommand;
        private bool _isDragging = false;
        private System.Windows.Vector _dragOffset = new System.Windows.Vector();

        public BoardPieceVM(ChessPiece piece, ChessPosition pos, BoardVM boardVM)
        {
            _piece = piece;
            _position = pos;
            _boardVM = boardVM;
            //_boardVM.MoveDestinations(pos).CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(MovesChanged);
            ////IsDraggable = _boardVM.MoveDestinations(_position).Count > 0;
            
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

        public bool IsDragging
        {
            get
            {
                return _isDragging;
            }
            private set
            {
                if (_isDragging == value) { return; }
                _isDragging = value;
                OnPropertyChanged("IsDragging");
            }
        }

        public System.Windows.Vector DragOffset
        {
            get
            {
                return _dragOffset;
            }
            private set
            {
                if (_dragOffset == value) { return; }
                _dragOffset = value;
                OnPropertyChanged("DragOffset");
            }

        }
        public RelayCommand DragStartCommand
        {
            get
            {
                if (_dragStartCommand == null)
                {
                    _dragStartCommand = new RelayCommand(param => IsDragging=true, param => _boardVM.MoveDestinations(_position).Count > 0);
                        
                }
                return _dragStartCommand;
            }
        }

        public RelayCommand DragVectorCommand
        {
            get
            {
                if (_dragVectorCommand == null)
                {
                    _dragVectorCommand = new RelayCommand(param => this.DragOffset = (System.Windows.Vector)param, param => IsDragging);
                }
                return _dragVectorCommand;
            }
        }

    }
}
