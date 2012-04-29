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

        public void ApplyMove(ChessMove move)
        {
            _board.MoveApply(move);

            while (_moves.Count > 0) { _moves.RemoveAt(_moves.Count - 1); }
            foreach (var newmove in ChessMove.GenMovesLegal(_board))
            {
                _moves.Add(newmove);
            }

        }

        public BoardModel(ChessBoard board)
        {
            _board = board;
            _board.BoardChanged += new EventHandler<ChessBoard.BoardChangedEventArgs>(Board_BoardChanged);
            RefreshPieces();
            foreach (var move in ChessMove.GenMovesLegal(board))
            {
                _moves.Add(move);
            }
        }

        void Board_BoardChanged(object sender, ChessBoard.BoardChangedEventArgs e)
        {
            try
            {
                foreach (var removed in e.Removed)
                {
                    this.Pieces.Where(p => p.Position == removed.Position).ToList().ForEach(ip =>
                    {
                        this.Pieces.Remove(ip);
                    });
                }
                foreach (var moved in e.Moved)
                {
                    this.Pieces.Where(p => p.Position == moved.OldPosition).ToList().ForEach(ip =>
                    {
                        ((PieceModel)ip).Position = moved.NewPosition;
                    });
                }
                foreach (var added in e.Added)
                {
                    this.Pieces.Add(new PieceModel() { Piece = added.Piece, Position = added.Position });
                }
                foreach (var changed in e.Changed)
                {
                    this.Pieces.Where(p => p.Position == changed.Position).ToList().ForEach(ip =>
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
            this.Pieces.Clear();
            foreach (var position in Chess.AllPositions)
            {
                if (_board.PieceAt(position) != ChessPiece.EMPTY)
                {
                    _pieces.Add(new PieceModel() { Piece = _board.PieceAt(position), Position = position });
                }
            }
        }

    }
}
