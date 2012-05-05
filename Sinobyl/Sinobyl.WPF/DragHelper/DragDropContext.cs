using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using Sinobyl.WPF.Models;

namespace Sinobyl.WPF.DragHelper
{
    public class DragDropContext
    {
        private readonly List<DragHandler> _drags = new List<DragHandler>();
        private readonly List<DropHandler> _drops = new List<DropHandler>();

        private readonly RangeObservableCollection<DropHandler> _potentials = new RangeObservableCollection<DropHandler>();
        private readonly RangeObservableCollection<DropHandler> _valids = new RangeObservableCollection<DropHandler>();
        private DropHandler _active = null;

        public DragDropContext()
        {
            _potentials.CollectionChanged += potentials_CollectionChanged;
            _valids.CollectionChanged += valids_CollectionChanged;
        }

        void valids_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var old in e.OldItems.Cast<DropHandler>())
                {
                    DragDropProperties.SetIsActiveValidDropTarget(old.Element, false);
                }
            }
            if (e.NewItems != null)
            {
                foreach (var newItem in e.NewItems.Cast<DropHandler>())
                {
                    DragDropProperties.SetIsActiveValidDropTarget(newItem.Element, true);
                }
            }
        }

        void potentials_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var old in e.OldItems.Cast<DropHandler>())
                {
                    DragDropProperties.SetIsActivePotentialDropTarget(old.Element, false);
                    DragDropProperties.SetIsActiveSelectedDropTarget(old.Element, false);
                }
            }
            if (e.NewItems != null)
            {
                foreach (var newItem in e.NewItems.Cast<DropHandler>())
                {
                    DragDropProperties.SetIsActivePotentialDropTarget(newItem.Element, true);
                }
            }
        }

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

        public void handleMouse(DragHandler sender, System.Windows.Input.MouseEventArgs e, Vector startRelToCenter)
        {
            foreach (var potential in _potentials)
            {
                var p = e.GetPosition(potential.Element) - startRelToCenter;
                FrameworkElement fe = potential.Element as FrameworkElement;
                bool inEle = (p.X > 0 && p.Y > 0 && p.X < fe.ActualWidth && p.Y < fe.ActualHeight);
                DragDropProperties.SetIsActiveSelectedDropTarget(potential.Element, inEle);                
            }
        }
        

        public void SetValidAndPotental(IEnumerable<IDropTarget> potentials, IEnumerable<IDropTarget> valids)
        {
            while (_potentials.Count > 0)
            {
                _potentials.RemoveAt(_potentials.Count-1);
            }
            while (_valids.Count > 0)
            {
                _valids.RemoveAt(_valids.Count - 1);
            }

            Dictionary<IDropTarget,DropHandler> dic = new Dictionary<IDropTarget,DropHandler>();
            foreach (var drop in this._drops)
            {
                dic.Add(drop.Target,drop);
            }
            if(potentials!=null)
            {
                foreach(var potential in potentials)
                {
                    if(dic.ContainsKey(potential))
                    {
                        var h = dic[potential];
                        _potentials.Add(h);
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
                        _valids.Add(h);
                    }
                }
            }
        }

    }
}
