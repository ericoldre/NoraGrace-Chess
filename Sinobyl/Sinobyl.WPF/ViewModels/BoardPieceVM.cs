using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinobyl.Engine;
namespace Sinobyl.WPF.ViewModels
{
    public class BoardPieceVM: BoardElementVM, Sinobyl.WPF.DragHelper.IDragSource
    {
        private readonly BoardVM _boardVM;
        private readonly Sinobyl.WPF.Models.IPieceModel _pieceModel;
        private ChessPiece _piece;
        

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


        #region IDragSource Members

        public bool CanDrag
        {
            get { return true; }
        }

        #endregion

        #region IDragSource Members


        public IEnumerable<DragHelper.IDropTarget> DragTargetPotential
        {
            get 
            {
                foreach(ChessPosition pos in Chess.AllPositions)
                {
                    var sq = this.BoardViewModel.SquareDictionary[pos];
                    yield return sq;
                }
            }
        }

        public IEnumerable<DragHelper.IDropTarget> DragTargetValid
        {
            get 
            {
                foreach (var to in this.BoardViewModel.MoveDestinations(this.Position))
                {
                    var sq = this.BoardViewModel.SquareDictionary[to];
                    yield return sq;
                }
            }
        }

        #endregion
    }
}
