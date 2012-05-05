using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Sinobyl.WPF.DragHelper
{
    public class DragDropContext
    {
        private Dictionary<UIElement, DragHandler> _drags = new Dictionary<UIElement, DragHandler>();
        private Dictionary<UIElement, DropHandler> _drops = new Dictionary<UIElement, DropHandler>();

        public void AddDragSource(UIElement element, IDragSource dragSource)
        {
            _drags.Add(element, new DragHandler(element, dragSource, this));
        }

        public void RemoveDragSource(UIElement element)
        {
            if (_drags.ContainsKey(element))
            {
                var handler = _drags[element];
                //handler.dispose;
                _drags.Remove(element);
            }
        }

        public void AddDropTarget(UIElement element, IDropTarget dropTarget)
        {
            _drops.Add(element, new DropHandler(element, dropTarget, this));
        }

        public void RemoveDropTarget(UIElement element)
        {
            if (_drops.ContainsKey(element))
            {
                var handler = _drags[element];
                //handler.dispose;
                _drops.Remove(element);
            }
        }

    }
}
