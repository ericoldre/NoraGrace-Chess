using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Sinobyl.WPF.DragHelper
{
    public static class DragDropProperties
    {

        #region DragSource


        public static readonly DependencyProperty DragSourceProperty =
            DependencyProperty.RegisterAttached("DragSource", typeof(IDragSource), typeof(DragDropProperties),
                new UIPropertyMetadata(null, DragSourceChanged));

        public static IDragSource GetDragSource(UIElement target)
        {
            return (IDragSource)target.GetValue(DragSourceProperty);
        }

        public static void SetDragSource(UIElement target, IDragSource value)
        {
            target.SetValue(DragSourceProperty, value);
        }

        static void DragSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UIElement uiElement = (UIElement)d;


            if (e.OldValue != null)
            {
                IDragSource oldValue = (IDragSource)e.OldValue;
                oldValue.DragDropContext.RemoveDragSource(uiElement);
                //context.RemoveDragSource(uiElement);
            }
            if (e.NewValue != null)
            {
                IDragSource newValue = (IDragSource)e.NewValue;
                newValue.DragDropContext.AddDragSource(uiElement, newValue);
            }
            
        }

        #endregion

        #region DropTarget

        public static readonly DependencyProperty DropTargetProperty =
            DependencyProperty.RegisterAttached("DropTarget", typeof(IDropTarget), typeof(DragDropProperties),
            new UIPropertyMetadata(null, DropTargetChanged));

        public static IDropTarget GetDropTarget(UIElement target)
        {
            return (IDropTarget)target.GetValue(DropTargetProperty);
        }

        public static void SetDropTarget(UIElement target, IDropTarget value)
        {
            target.SetValue(DropTargetProperty, value);
        }

        static void DropTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UIElement uiElement = (UIElement)d;

            if (e.OldValue != null)
            {
                IDropTarget oldValue = (IDropTarget)e.OldValue;
                oldValue.DragDropContext.RemoveDropTarget(uiElement);
                //context.RemoveDragSource(uiElement);
            }
            if (e.NewValue != null)
            {
                IDropTarget newValue = (IDropTarget)e.NewValue;
                newValue.DragDropContext.AddDropTarget(uiElement, newValue);
            }
        }

        #endregion


        public static readonly DependencyProperty IsActivePotentialDropTargetProperty =
            DependencyProperty.RegisterAttached("IsActivePotentialDropTarget", typeof(bool), typeof(DragDropProperties),
            new UIPropertyMetadata(false));

        public static bool GetIsActivePotentialDropTarget(UIElement target)
        {
            return (bool)target.GetValue(IsActivePotentialDropTargetProperty);
        }

        public static void SetIsActivePotentialDropTarget(UIElement target, bool value)
        {
            target.SetValue(IsActivePotentialDropTargetProperty, value);
        }

        public static readonly DependencyProperty IsActiveValidDropTargetProperty =
            DependencyProperty.RegisterAttached("IsActiveValidDropTarget", typeof(bool), typeof(DragDropProperties),
            new UIPropertyMetadata(false));

        public static bool GetIsActiveValidDropTarget(UIElement target)
        {
            return (bool)target.GetValue(IsActiveValidDropTargetProperty);
        }

        public static void SetIsActiveValidDropTarget(UIElement target, bool value)
        {
            target.SetValue(IsActiveValidDropTargetProperty, value);
        }


        public static readonly DependencyProperty IsActiveSelectedDropTargetProperty =
            DependencyProperty.RegisterAttached("IsActiveSelectedDropTarget", typeof(bool), typeof(DragDropProperties),
            new UIPropertyMetadata(false));

        public static bool GetIsActiveSelectedDropTarget(UIElement target)
        {
            return (bool)target.GetValue(IsActiveSelectedDropTargetProperty);
        }

        public static void SetIsActiveSelectedDropTarget(UIElement target, bool value)
        {
            target.SetValue(IsActiveSelectedDropTargetProperty, value);
        }



        public static readonly DependencyProperty DragOffsetProperty =
            DependencyProperty.RegisterAttached("DragOffset", typeof(Vector), typeof(DragDropProperties));

        public static Vector GetDragOffset(UIElement target)
        {
            return (Vector)target.GetValue(DragOffsetProperty);
        }

        public static void SetDragOffset(UIElement target, Vector value)
        {
            target.SetValue(DragOffsetProperty, value);
        }


    }
}
