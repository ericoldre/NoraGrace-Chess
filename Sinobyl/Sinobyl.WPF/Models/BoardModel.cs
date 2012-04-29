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
        private class BoardChangedException : Exception { }
        private readonly ChessBoard _board;
        private readonly RangeObservableCollection<IPieceModel> _pieces = new RangeObservableCollection<IPieceModel>();
        private readonly RangeObservableCollection<ChessMove> _moves = new RangeObservableCollection<ChessMove>();
        private readonly ReadOnlyObservableCollection<IPieceModel> _piecesReadonly;
        private readonly ReadOnlyObservableCollection<ChessMove> _movesReadonly;

        public ReadOnlyObservableCollection<IPieceModel> Pieces
        {
            get { return _piecesReadonly; }
        }

        public ReadOnlyObservableCollection<ChessMove> Moves
        {
            get { return _movesReadonly; }
        }

        public void ApplyMove(ChessMove move)
        {
            _board.MoveApply(move);

        }

        public BoardModel(ChessBoard board)
        {
            _board = board;
            _movesReadonly = new ReadOnlyObservableCollection<ChessMove>(_moves);
            _piecesReadonly = new ReadOnlyObservableCollection<IPieceModel>(_pieces);
            _board.BoardChanged += new EventHandler<ChessBoard.BoardChangedEventArgs>(Board_BoardChanged);
            RefreshPieces();
            _moves.AddRange(ChessMove.GenMovesLegal(_board));
        }

        void Board_BoardChanged(object sender, ChessBoard.BoardChangedEventArgs e)
        {
            try
            {
                foreach (var removed in e.Removed)
                {
                    _pieces.Where(p => p.Position == removed.Position).ToList().ForEach(ip =>
                    {
                        _pieces.Remove(ip);
                    });
                }
                foreach (var moved in e.Moved)
                {
                    _pieces.Where(p => p.Position == moved.OldPosition).ToList().ForEach(ip =>
                    {
                        ((PieceModel)ip).Position = moved.NewPosition;
                    });
                }
                foreach (var added in e.Added)
                {
                    _pieces.Add(new PieceModel() { Piece = added.Piece, Position = added.Position });
                }
                foreach (var changed in e.Changed)
                {
                    _pieces.Where(p => p.Position == changed.Position).ToList().ForEach(ip =>
                    {
                        ((PieceModel)ip).Piece = changed.NewPiece;
                    });
                }
                if (!VerifyPieces()) { throw new BoardChangedException(); }
            }
            catch (BoardChangedException ex)
            {
                RefreshPieces();
                throw ex;
            }

            _moves.RemoveRange(_moves.ToArray());
            _moves.AddRange(ChessMove.GenMovesLegal(_board));

        }

        private bool VerifyPieces()
        {
            if (Pieces.Count != _board.PieceLocationsAll.BitCount()) { return false; }
            foreach (var p in Pieces)
            {
                if (_board.PieceAt(p.Position) != p.Piece) { return false; }
            }
            return true;
        }
        private void RefreshPieces()
        {
            _pieces.RemoveRange(this._pieces.ToArray());
            var x = Chess.AllPositions.Where(pos => _board.PieceAt(pos) != ChessPiece.EMPTY).Select(pos => new PieceModel() { Piece = _board.PieceAt(pos), Position = pos });
            _pieces.AddRange(x.ToArray());
        }

    }
}
