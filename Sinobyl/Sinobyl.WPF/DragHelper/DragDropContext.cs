using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using Sinobyl.WPF.Models;
using System.ComponentModel;
using System.Windows.Input;

namespace Sinobyl.WPF.DragHelper
{
    public class DragDropContext
    {
        //private readonly List<DragHandler> _drags = new List<DragHandler>();
        //private readonly List<DropHandler> _drops = new List<DropHandler>();

        private readonly RangeObservableCollection<IDropTarget> _potentials = new RangeObservableCollection<IDropTarget>();
        private readonly RangeObservableCollection<IDropTarget> _valids = new RangeObservableCollection<IDropTarget>();
        private DropHandler _active = null;

        public event EventHandler<DragMoveEventArgs> DragMove;

        public DragDropContext()
        {
            
        }

        public RangeObservableCollection<IDropTarget> PotentialDropTargets
        {
            get
            {
                return _potentials;
            }
        }

        public RangeObservableCollection<IDropTarget> ValidDropTargets
        {
            get
            {
                return _valids;
            }
        }

        

        public void DoDrag(IDragSource dragSource)
        {
            while (_potentials.Count > 0)
            {
                _potentials.RemoveAt(_potentials.Count - 1);
            }
            while (_valids.Count > 0)
            {
                _valids.RemoveAt(_valids.Count - 1);
            }
            _potentials.AddRange(dragSource.DragTargetPotential.ToList());
            _valids.AddRange(dragSource.DragTargetValid.ToList());
        }

        public void DoDragMove(IDragSource dragSource, MouseEventArgs e, Vector startRelToCenter)
        {
            var ev = this.DragMove;
            if (ev != null)
            {
                DragMoveEventArgs args = new DragMoveEventArgs(e, startRelToCenter);
                ev(this, args);
            }
        }

        public void EndDrag(IDragSource dragSource)
        {
            while (_potentials.Count > 0)
            {
                _potentials.RemoveAt(_potentials.Count - 1);
            }
            while (_valids.Count > 0)
            {
                _valids.RemoveAt(_valids.Count - 1);
            }
        }

        public class DragMoveEventArgs: EventArgs
        {
            public MouseEventArgs MouseArgs { get; private set; }
            public Vector DraggedElementCenterRelativeToMouse { get; private set; }

            public DragMoveEventArgs(MouseEventArgs mouseArgs, Vector draggedElementCenterRelativeToMouse)
            {
                MouseArgs = mouseArgs;
                DraggedElementCenterRelativeToMouse = draggedElementCenterRelativeToMouse;
            }
        }

    }
}
