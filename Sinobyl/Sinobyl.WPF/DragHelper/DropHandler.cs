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

        #region constructor destructor

        public DropHandler(UIElement element, IDropTarget target)
        {
            _element = element;
            _target = target;
            _target.DragDropContext.PotentialDropTargets.CollectionChanged += PotentialDropTargets_CollectionChanged;
            _target.DragDropContext.ValidDropTargets.CollectionChanged += ValidDropTargets_CollectionChanged;
        }
        ~DropHandler()
        {
            //IsPotential = false;
            //IsValid = false;
            _target.DragDropContext.PotentialDropTargets.CollectionChanged -= PotentialDropTargets_CollectionChanged;
            _target.DragDropContext.ValidDropTargets.CollectionChanged -= ValidDropTargets_CollectionChanged;
        }

        #endregion

        #region properties

        public UIElement Element
        {
            get { return _element; }
        }
        public IDropTarget Target
        {
            get { return _target; }
        }
        public DragDropContext Context
        {
            get { return _target.DragDropContext; }
        }

        bool IsValid
        {
            get
            {
                return DragDropProperties.GetIsActiveValidDropTarget(this.Element);
            }
            set
            {
                if (value == IsValid) { return; }
                DragDropProperties.SetIsActiveValidDropTarget(this.Element, value);
            }
        }

        bool IsPotential
        {
            get
            {
                return DragDropProperties.GetIsActivePotentialDropTarget(this.Element);
            }
            set
            {
                if (value == this.IsPotential)
                {
                    return;
                }
                DragDropProperties.SetIsActivePotentialDropTarget(this.Element, value);

                if (value)
                {
                    this.Context.DragMove += Context_DragMove;
                }
                else
                {
                    this.Context.DragMove -= Context_DragMove;
                    this.IsSelected = false;
                }
            }
        }

        bool IsSelected
        {
            get
            {
                return DragDropProperties.GetIsActiveSelectedDropTarget(this.Element);
            }
            set
            {
                if (IsSelected == value) { return; }
                DragDropProperties.SetIsActiveSelectedDropTarget(this.Element, value);

                var contains = this.Context.SelectedDropTargets.Contains(this.Target);
                if (contains != value)
                {
                    if (value)
                    {
                        this.Context.SelectedDropTargets.Add(this.Target);
                    }
                    else
                    {
                        this.Context.SelectedDropTargets.Remove(this.Target);
                    }
                }
            }
        }

        #endregion

        #region event handlers

        void ValidDropTargets_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.IsValid = this.Context.ValidDropTargets.Contains(this.Target);
        }

        void PotentialDropTargets_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.IsPotential = this.Context.PotentialDropTargets.Contains(this.Target);
        }

        void Context_DragMove(object sender, DragDropContext.DragMoveEventArgs e)
        {
            var MousePosition = e.MouseArgs.GetPosition(this.Element);
            var DraggedElementCenter = MousePosition - e.DraggedElementCenterRelativeToMouse;

            FrameworkElement fe = this.Element as FrameworkElement;

            this.IsSelected = (
                DraggedElementCenter.X > 0 
                && DraggedElementCenter.Y > 0
                && DraggedElementCenter.X < fe.ActualWidth
                && DraggedElementCenter.Y < fe.ActualHeight);

        }

        #endregion




    }
}
