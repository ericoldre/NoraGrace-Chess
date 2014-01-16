using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinobyl.Engine;
namespace Sinobyl.WPF.ViewModels
{
    public class BoardPieceVM: BoardElementVM, Sinobyl.WPF.DragHelper.IDragSource, IDisposable
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



        public string DisplayName
        {
            get
            {
                return _piece.PieceToString();
            }
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

        public DragHelper.DragDropContext DragDropContext
        {
            get
            {
                return this.BoardViewModel.DragDropContext;
            }
        }

        #endregion

        #region IDragSource Members


        public IEnumerable<DragHelper.IDropTarget> DragTargetPotential
        {
            get 
            {
                foreach (ChessPosition pos in ChessPositionInfo.AllPositions)
                {
                    var sq = this.BoardViewModel.SquareDictionary[pos];
                    yield return sq;
                }
            }
        }
        public void DragComplete(DragHelper.IDropTarget target)
        {
            BoardSquareVM sqTo = (BoardSquareVM)target;
            if (this.Piece == ChessPiece.WPawn && sqTo.Position.GetRank() == ChessRank.Rank8)
            {
                this.BoardViewModel.Model.ApplyMove(new ChessMove(this.Position, sqTo.Position, ChessPiece.WQueen));
            }
            else if (this.Piece == ChessPiece.BPawn && sqTo.Position.GetRank() == ChessRank.Rank1)
            {
                this.BoardViewModel.Model.ApplyMove(new ChessMove(this.Position, sqTo.Position, ChessPiece.BQueen));
            }
            else
            {
                this.BoardViewModel.Model.ApplyMove(new ChessMove(this.Position, sqTo.Position));
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

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
