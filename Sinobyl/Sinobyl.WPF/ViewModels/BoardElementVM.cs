using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinobyl.Engine;

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
                    OnPropertyChanged("Width");
                    OnPropertyChanged("Left");
                    break;
                case "BoardHeight":
                    OnPropertyChanged("Height");
                    OnPropertyChanged("Top");
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
                OnPropertyChanged("Top");
                OnPropertyChanged("Left");
            }
        }

        public double Width
        {
            get
            {
                return _boardVM.BoardWidth / 8;
            }
        }
        public double Height
        {
            get
            {
                return _boardVM.BoardHeight / 8;
            }
        }
        public double Top
        {
            get
            {
                return Height * Math.Abs((ChessRank.Rank8 - this.Position.GetRank()));
            }
        }
        public double Left
        {
            get
            {
                return Width * Math.Abs((ChessFile.FileA - this.Position.GetFile()));
            }
        }
    }
}
