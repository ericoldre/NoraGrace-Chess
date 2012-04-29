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

        private readonly ObservableCollection<IPieceModel> _pieces = new ObservableCollection<IPieceModel>();
        private readonly ObservableCollection<ChessMove> _moves = new ObservableCollection<ChessMove>();

        public ObservableCollection<IPieceModel> Pieces
        {
            get { return _pieces; }
        }

        public ObservableCollection<ChessMove> Moves
        {
            get { return _moves; }
        }


        public BoardModel(ChessBoard board)
        {
            foreach (var position in Chess.AllPositions)
            {
                if (board.PieceAt(position) != ChessPiece.EMPTY)
                {
                    _pieces.Add(new PieceModel() { Piece = board.PieceAt(position), Position = position });
                }
            }
            foreach (var move in ChessMove.GenMovesLegal(board))
            {
                _moves.Add(move);
            }
        }
    }
}
