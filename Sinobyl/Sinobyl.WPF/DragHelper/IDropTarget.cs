using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.WPF.DragHelper
{
    public interface IDropTarget
    {
        DragDropContext DragDropContext
        {
            get;
        }
    }
}
