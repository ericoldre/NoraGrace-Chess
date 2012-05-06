using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Sinobyl.WPF.DragHelper
{
    public interface IDragSource: INotifyPropertyChanged
    {
        bool CanDrag { get; }

        IEnumerable<IDropTarget> DragTargetPotential
        {
            get;
        }
        IEnumerable<IDropTarget> DragTargetValid
        {
            get;
        }
        DragDropContext DragDropContext
        {
            get;
        }

        void DragComplete(IDropTarget target);

        string DisplayName { get; }
    }
}
