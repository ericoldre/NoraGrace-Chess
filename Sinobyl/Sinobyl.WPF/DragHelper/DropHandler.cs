using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Sinobyl.WPF.DragHelper
{
    class DropHandler
    {
        private readonly UIElement _element;
        private readonly IDropTarget _target;
        private readonly DragDropContext _context;

        public DropHandler(UIElement element, IDropTarget target, DragDropContext context)
        {
            _element = element;
            _target = target;
            _context = context;
        }
    }
}
