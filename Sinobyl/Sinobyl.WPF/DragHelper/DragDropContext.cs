using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Sinobyl.WPF.DragHelper
{
    public class DragDropContext
    {
        private List<DragHandler> _drags = new List<DragHandler>();
        private List<DropHandler> _drops = new List< DropHandler>();

        public void AddDragSource(UIElement element, IDragSource dragSource)
        {
            _drags.Add(new DragHandler(element, dragSource, this));
        }

        public void RemoveDragSource(UIElement element)
        {
            var rs = _drags.Where(d=>d.Element==element).ToArray();
            foreach(var r in rs)
            {
                _drags.Remove(r);
            }
        }

        public void AddDropTarget(UIElement element, IDropTarget dropTarget)
        {
            _drops.Add(new DropHandler(element, dropTarget, this));
        }

        public void RemoveDropTarget(UIElement element)
        {
            var rs = _drops.Where(d=>d.Element==element).ToArray();
            foreach(var r in rs)
            {
                _drops.Remove(r);
            }
        }

        public void SetValidAndPotental(IEnumerable<IDropTarget> potentials, IEnumerable<IDropTarget> valids)
        {
            Dictionary<IDropTarget,DropHandler> dic = new Dictionary<IDropTarget,DropHandler>();
            foreach (var drop in this._drops)
            {
                dic.Add(drop.Target,drop);
                DragDropProperties.SetIsActivePotentialDropTarget(drop.Element,false);
                DragDropProperties.SetIsActiveValidDropTarget(drop.Element,false);
            }
            if(potentials!=null)
            {
                foreach(var potential in potentials)
                {
                    if(dic.ContainsKey(potential))
                    {
                        var h = dic[potential];
                        DragDropProperties.SetIsActivePotentialDropTarget(h.Element,true);
                    }
                }
            }
            if (valids != null)
            {
                foreach (var v in valids)
                {
                    if (dic.ContainsKey(v))
                    {
                        var h = dic[v];
                        DragDropProperties.SetIsActiveValidDropTarget(h.Element, true);
                    }
                }
            }
        }

    }
}
