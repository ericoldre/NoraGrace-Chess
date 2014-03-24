﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinobyl.Engine;
namespace Sinobyl.WPF.ViewModel
{
    public class BoardSquareVM: BoardElementVM, Sinobyl.WPF.DragHelper.IDropTarget
    {
        
        public string DisplayName
        {
            get { return this.Position.PositionToString(); }
            
        }
        public bool IsLight
        {
            get { return this.Position.IsLight(); }
        }

        public BoardSquareVM(BoardVM boardVM, Sinobyl.Engine.ChessPosition pos): base(boardVM)
        {
            this.Position = pos;
        }

        public override string ToString()
        {
            return DisplayName;
        }

        public DragHelper.DragDropContext DragDropContext
        {
            get
            {
                return this.BoardViewModel.DragDropContext;
            }
        }

    }
}