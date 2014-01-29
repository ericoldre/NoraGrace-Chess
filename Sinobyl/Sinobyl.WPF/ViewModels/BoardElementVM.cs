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
            }
        }





        
    }
}
