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
        private Vector _renderOffset = new Vector();

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
                    OnPropertyChanged("VisualCenter");
                    break;
                case "BoardHeight":
                    OnPropertyChanged("Size");
                    OnPropertyChanged("Location");
                    OnPropertyChanged("VisualCenter");
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
                OnPropertyChanged("VisualCenter");
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

        public System.Windows.Vector RenderOffset
        {
            get
            {
                return _renderOffset;
            }
            set
            {
                if (_renderOffset == value) { return; }
                _renderOffset = value;
                OnPropertyChanged("RenderOffset");
                OnPropertyChanged("VisualCenter");
            }
        }

        public System.Windows.Point VisualCenter
        {
            get
            {
                return new System.Windows.Point()
                {
                    X = this.Location.X + this.RenderOffset.X + (this.Size.Width / 2),
                    Y = this.Location.Y + this.RenderOffset.Y + (this.Size.Height / 2)
                };
            }
        }
        
        protected static ChessPosition PointToPosition(Size boardSize, Point point)
        {
            if (point.X > boardSize.Width || point.X < 0) { return ChessPosition.OUTOFBOUNDS; }
            if (point.Y > boardSize.Height || point.Y < 0) { return ChessPosition.OUTOFBOUNDS; }

            var sqSize = BoardSizeToSquareSize(boardSize);
           
            double X = point.X;
            ChessFile f = ChessFile.FileA;
            while (X > sqSize.Width)
            {
                X -= sqSize.Width;
                f += 1;
            }
            double Y = point.Y;
            ChessRank r = ChessRank.Rank8;
            while (Y > sqSize.Height)
            {
                Y -= sqSize.Height;
                r += 1;
            }
            return f.ToPosition(r);
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
