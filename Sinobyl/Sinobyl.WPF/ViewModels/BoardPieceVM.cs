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
        private readonly Sinobyl.WPF.Models.IPieceModel _pieceModel;
        private ChessPiece _piece;
        private ChessPosition _position;
        private RelayCommand _dragStartCommand;
        private RelayCommand _dragVectorCommand;
        private bool _isDragging = false;
        private System.Windows.Vector _dragOffset = new System.Windows.Vector();

        public BoardPieceVM(BoardVM boardVM, Sinobyl.WPF.Models.IPieceModel pieceModel)
        {
            _boardVM = boardVM;
            _pieceModel = pieceModel;
            Piece = pieceModel.Piece;
            Position = pieceModel.Position;
            _pieceModel.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(PieceModel_PropertyChanged);
        }

        void PieceModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Piece = _pieceModel.Piece;
            Position = _pieceModel.Position;
        }


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
