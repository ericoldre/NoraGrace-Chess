using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinobyl.Engine;
using System.Windows;

namespace Sinobyl.WPF.ViewModels
{
    public abstract class BoardElementVM: ViewModelBase
    {
        private readonly BoardVM _boardVM;
        private ChessPosition _position;

        public BoardElementVM(BoardVM boardViewModel)
        {
            _boardVM = boardViewModel;
            _boardVM.PropertyChanged+=new System.ComponentModel.PropertyChangedEventHandler(BoardVM_PropertyChanged);
        }

        void  BoardVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
 	        switch (e.PropertyName)
            {
                case "BoardWidth":
                    OnPropertyChanged("Size");
                    OnPropertyChanged("Location");
                    break;
                case "BoardHeight":
                    OnPropertyChanged("Size");
                    OnPropertyChanged("Location");
                    break;
            }
        }

        public BoardVM BoardViewModel
        {
            get
            {
                return _boardVM;
            }
        }

        public ChessPosition Position
        {
            get
            {
                return _position;
            }
            protected set
            {
                if (_position == value) { return; }
                _position = value;
                OnPropertyChanged("Position");
                OnPropertyChanged("Size");
                OnPropertyChanged("Location");
            }
        }

        public Size Size
        {
            get
            {
                return BoardSizeToSquareSize(new Size(_boardVM.BoardWidth, _boardVM.BoardHeight));
            }
        }
        public Point Location
        {
            get
            {
                return PositionToPoint(new Size(_boardVM.BoardWidth, _boardVM.BoardHeight), this.Position);
            }
        }

        protected static Point PositionToPoint(Size boardSize, ChessPosition pos)
        {
            var size = BoardSizeToSquareSize(boardSize);
            return new Point(
                size.Width * Math.Abs((ChessFile.FileA - pos.GetFile())),
                size.Height * Math.Abs((ChessRank.Rank8 - pos.GetRank()))
                );
        }
        protected static Size BoardSizeToSquareSize(Size boardSize)
        {
            return new Size(boardSize.Width / 8, boardSize.Height / 8);
        }
    }
}
