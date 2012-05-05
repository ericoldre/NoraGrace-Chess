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
    }
}
