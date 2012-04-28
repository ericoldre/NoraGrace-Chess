﻿using System;
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
        

        public IBoardModel Model
        {
            get
            {
                return _model;
            }
        }
    

        public BoardVM(IBoardModel model)
        {
            _model = model;

            Squares = new ObservableCollection<BoardSquareVM>(BoardSquareVM.AllBoardSquares());
            Pieces = new ObservableCollection<BoardPieceVM>();

            foreach (var piece in _model.Pieces)
            {
                Pieces.Add(new BoardPieceVM(piece.Key, piece.Value, this));
            }
            foreach (var move in _model.Moves)
            {
                MoveDestinations(move.From).Add(move.To);
            }

            _model.Pieces.CollectionChanged += new NotifyCollectionChangedEventHandler(ModelPieces_CollectionChanged);
            _model.Moves.CollectionChanged += new NotifyCollectionChangedEventHandler(ModelMoves_CollectionChanged);
        }

        void ModelMoves_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var oldMove in e.OldItems.Cast<ChessMove>())
            {
                MoveDestinations(oldMove.From).Remove(oldMove.To);
            }
            foreach (var newMove in e.NewItems.Cast<ChessMove>())
            {
                MoveDestinations(newMove.From).Add(newMove.To);
            }
        }

        void ModelPieces_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var oldPiece in e.OldItems.Cast<KeyValuePair<ChessPiece, ChessPosition>>())
            {
                foreach (var existing in this.Pieces.Where(p => p.Position == oldPiece.Value).ToArray())
                {
                    this.Pieces.Remove(existing);
                }
            }
            foreach (var newPiece in e.NewItems.Cast<KeyValuePair<ChessPiece, ChessPosition>>())
            {
                Pieces.Add(new BoardPieceVM(newPiece.Key, newPiece.Value, this));
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