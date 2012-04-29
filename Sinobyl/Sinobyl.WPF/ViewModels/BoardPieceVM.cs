using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinobyl.Engine;
namespace Sinobyl.WPF.ViewModels
{
    public class BoardPieceVM: BoardElementVM
    {
        private readonly BoardVM _boardVM;
        private readonly Sinobyl.WPF.Models.IPieceModel _pieceModel;
        private ChessPiece _piece;
        private RelayCommand _dragStartCommand;
        private RelayCommand _dragVectorCommand;
        private RelayCommand _dragEndCommand;
        private bool _isDragging = false;
        

        public BoardPieceVM(BoardVM boardVM, Sinobyl.WPF.Models.IPieceModel pieceModel): base(boardVM)
        {
            _boardVM = boardVM;
            _pieceModel = pieceModel;
            Piece = pieceModel.Piece;
            Position = pieceModel.Position;
            _pieceModel.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(PieceModel_PropertyChanged);
            _boardVM.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(BoardVM_PropertyChanged);
        }

        void BoardVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

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

        

        

        public RelayCommand DragStartCommand
        {
            get
            {
                if (_dragStartCommand == null)
                {
                    _dragStartCommand = new RelayCommand(param => IsDragging=true, param => _boardVM.MoveDestinations(this.Position).Count > 0);
                        
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
                    _dragVectorCommand = new RelayCommand(param =>
                    {
                        this.RenderOffset = (System.Windows.Vector)param;
                        var pos = PointToPosition(this.BoardViewModel.BoardSize, this.VisualCenter);
                        System.Diagnostics.Debug.WriteLine(string.Format("Over: {0} Point:{1}", pos.ToString(), this.VisualCenter));
                    }, param => IsDragging);
                }
                return _dragVectorCommand;
            }
        }
        public RelayCommand DragEndCommand
        {
            get
            {
                if (_dragEndCommand == null)
                {
                    _dragEndCommand = new RelayCommand(param => 
                    {
                        this.IsDragging = false;
                        this.RenderOffset = new System.Windows.Vector(0, 0);
                    }, 
                    param => IsDragging);
                }
                return _dragEndCommand;
            }
        }

    }
}
