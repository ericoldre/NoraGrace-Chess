using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Sinobyl.Engine;
namespace Sinobyl.WPF.ViewModels
{
    public class BoardVM: ViewModelBase
    {
        public ObservableCollection<BoardSquareVM> Squares { get; private set; }

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
