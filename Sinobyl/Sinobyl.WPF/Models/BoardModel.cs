using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinobyl.Engine;
using System.Collections.ObjectModel;

namespace Sinobyl.WPF.Models
{
    public class BoardModel: IBoardModel
    {

        private readonly ObservableCollection<KeyValuePair<ChessPiece, ChessPosition>> _pieces = new ObservableCollection<KeyValuePair<ChessPiece, ChessPosition>>();
        private readonly ObservableCollection<ChessMove> _moves = new ObservableCollection<ChessMove>();
        #region IBoardModel Members

        ObservableCollection<KeyValuePair<ChessPiece, ChessPosition>> IBoardModel.Pieces
        {
            get { return _pieces; }
        }

        ObservableCollection<ChessMove> IBoardModel.Moves
        {
            get { return _moves; }
        }

        #endregion

        public BoardModel(ChessBoard board)
        {
            foreach (var position in Chess.AllPositions)
            {
                if (board.PieceAt(position) != ChessPiece.EMPTY)
                {
                    _pieces.Add(new KeyValuePair<ChessPiece, ChessPosition>(board.PieceAt(position), position));
                }
            }
            foreach (var move in ChessMove.GenMovesLegal(board))
            {
                _moves.Add(move);
            }
        }
    }
}
