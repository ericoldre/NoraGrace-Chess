using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Sinobyl.Engine;
using System.Windows.Input;
using Sinobyl.WPF.Models;
using System.Collections.Specialized;

namespace Sinobyl.WPF.ViewModels
{
    public class BoardVM: ViewModelBase
    {
        private readonly IBoardModel _model;
        private readonly Dictionary<ChessPosition, ObservableCollection<ChessPosition>> _moves = new Dictionary<ChessPosition, ObservableCollection<ChessPosition>>();
        public ObservableCollection<BoardSquareVM> Squares { get; private set; }
        public ObservableCollection<BoardPieceVM> Pieces { get; private set; }
        private double _boardWidth;
        private double _boardHeight;

        public IBoardModel Model
        {
            get
            {
                return _model;
            }
        }

        public double BoardWidth
        {
            get
            {
                return _boardWidth;
            }
            set
            {
                if (_boardWidth == value) { return; }
                _boardWidth = value;
                OnPropertyChanged("BoardWidth");
            }
        }
        public double BoardHeight
        {
            get
            {
                return _boardHeight;
            }
            set
            {
                if (_boardHeight == value) { return; }
                _boardHeight = value;
                OnPropertyChanged("BoardHeight");
            }
        }
        public BoardVM(IBoardModel model)
        {
            _model = model;

            Squares = new ObservableCollection<BoardSquareVM>();
            Pieces = new ObservableCollection<BoardPieceVM>();

            foreach (ChessPosition pos in Chess.AllPositions)
            {
                Squares.Add(new BoardSquareVM(this, pos));
            }

            foreach (var piece in _model.Pieces)
            {
                Pieces.Add(new BoardPieceVM(this, piece));
            }
            foreach (var move in _model.Moves)
            {
                MoveDestinations(move.From).Add(move.To);
            }

            ((INotifyCollectionChanged)(_model.Pieces)).CollectionChanged += new NotifyCollectionChangedEventHandler(ModelPieces_CollectionChanged);
            ((INotifyCollectionChanged)(_model.Moves)).CollectionChanged += new NotifyCollectionChangedEventHandler(ModelMoves_CollectionChanged);
        }

        void ModelMoves_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var oldMove in e.OldItems.Cast<ChessMove>())
                {
                    MoveDestinations(oldMove.From).Remove(oldMove.To);
                }
            }
            
            if (e.NewItems != null)
            {
                foreach (var newMove in e.NewItems.Cast<ChessMove>())
                {
                    MoveDestinations(newMove.From).Add(newMove.To);
                }
            }
            
        }

        void ModelPieces_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var oldPiece in e.OldItems.Cast<IPieceModel>())
                {
                    foreach (var existing in this.Pieces.Where(p => p.Position == oldPiece.Position).ToArray())
                    {
                        this.Pieces.Remove(existing);
                    }
                }
            }
            if (e.NewItems != null)
            {
                foreach (var newPiece in e.NewItems.Cast<IPieceModel>())
                {
                    Pieces.Add(new BoardPieceVM(this, newPiece));
                }
            }
        }


        public static BoardVM GetDesignBoardVM()
        {
            return new BoardVM(new BoardModel(new ChessBoard(new ChessFEN(ChessFEN.FENStart))));
        }

        public ObservableCollection<ChessPosition> MoveDestinations(ChessPosition fromPosition)
        {
            if(!_moves.ContainsKey(fromPosition))
            {
                _moves.Add(fromPosition, new ObservableCollection<ChessPosition>());
            }
            return _moves[fromPosition];
        }

        

    }
}
