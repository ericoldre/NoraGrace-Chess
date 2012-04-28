using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Sinobyl.Engine;
using System.Windows.Input;
using Sinobyl.WPF.Models;

namespace Sinobyl.WPF.ViewModels
{
    public class BoardVM: ViewModelBase
    {
        public ObservableCollection<BoardSquareVM> Squares { get; private set; }
        public ObservableCollection<BoardPieceVM> Pieces { get; private set; }
        private readonly IBoardModel _model;

        private int _width;
        public int Width
        {
            get 
            { 
                return _width; 
            }
            set 
            { 
                _width = value; 
                OnPropertyChanged("Width"); 
            }
        }

    

        public BoardVM(IBoardModel model)
        {
            _model = model;

            Squares = new ObservableCollection<BoardSquareVM>(BoardSquareVM.AllBoardSquares());
            Pieces = new ObservableCollection<BoardPieceVM>();

            foreach (var piece in _model.Pieces)
            {
                Pieces.Add(new BoardPieceVM()
                {
                    Piece = piece.Key,
                    Position = piece.Value,
                    IsDraggable = _model.Moves.Any(m=>m.From==piece.Value)
                });
            }
        }

        public static BoardVM GetDesignBoardVM()
        {
            return new BoardVM(new BoardModel(new ChessBoard(new ChessFEN(ChessFEN.FENStart))));
        }
    }
}
