using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Sinobyl.Engine;
using System.Windows.Input;

namespace Sinobyl.WPF.ViewModels
{
    public class BoardVM: ViewModelBase
    {
        public ObservableCollection<BoardSquareVM> Squares { get; private set; }
        public ObservableCollection<BoardPieceVM> Pieces { get; private set; }

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

    

        public BoardVM(ChessFEN fen)
        {
            Squares = new ObservableCollection<BoardSquareVM>(BoardSquareVM.AllBoardSquares());
            Pieces = new ObservableCollection<BoardPieceVM>();

            ChessBoard board = new ChessBoard(fen);
            var moves = ChessMove.GenMovesLegal(board);

            foreach (var position in Chess.AllPositions)
            {
                if (fen.pieceat[(int)position] != ChessPiece.EMPTY)
                {
                    Pieces.Add(new BoardPieceVM() { 
                        Piece = fen.pieceat[(int)position], 
                        Position = position,
                        IsDraggable = moves.Any(m=>m.From==position)
                    });
                }
            }
        }

        public static BoardVM GetDesignBoardVM()
        {
            return new BoardVM(new ChessFEN(ChessFEN.FENStart));
        }
    }
}
