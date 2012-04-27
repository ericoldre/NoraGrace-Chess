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

    

        public BoardVM()
        {
            Squares = new ObservableCollection<BoardSquareVM>(BoardSquareVM.AllBoardSquares());
        }

        public static BoardVM GetDesignBoardVM()
        {
            return new BoardVM();
        }
    }
}
