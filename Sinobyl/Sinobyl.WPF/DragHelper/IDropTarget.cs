using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.WPF.DragHelper
{
    public class DropTargetHitTestArgs: EventArgs
    {
        private List<IDropTarget> _hits = new List<IDropTarget>();
        
        public DropTargetHitTestArgs()
        {

        }
        public List<IDropTarget> Hits
        {
            get
            {
                return _hits;
            }
        }
    }

    public interface IDropTarget
    {
        DragDropContext DragDropContext
        {
            get;
        }
    }
}
