using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Sinobyl.WPF.DragHelper
{
    public static class DragDropProperties
    {

        #region DragContext

        public static readonly DependencyProperty DragDropContextProperty =
            DependencyProperty.RegisterAttached("DragDropContext", typeof(DragDropContext), typeof(DragDropProperties),
            new UIPropertyMetadata(null, DragDropContextChanged));

        public static DragDropContext GetDragDropContext(UIElement target)
        {
            return (DragDropContext)target.GetValue(DragDropContextProperty);
        }

        public static void SetDragDropContext(UIElement target, DragDropContext value)
        {
            target.SetValue(DragDropContextProperty, value);
        }

        static void DragDropContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UIElement uiElement = (UIElement)d;

            var dragSource = GetDragSource(uiElement);
            var dropTarget = GetDropTarget(uiElement);

            if (e.OldValue != null)
            {
                DragDropContext context = (DragDropContext)e.OldValue;
                
                if (dragSource != null)
                {
                    context.RemoveDragSource(uiElement);
                }

                if (dropTarget != null)
                {
                    context.RemoveDropTarget(uiElement);
                }
            }
            if (e.NewValue != null)
            {
                DragDropContext context = (DragDropContext)e.NewValue;

                if (dragSource != null)
                {
                    context.AddDragSource(uiElement, dragSource);
                }

                if (dropTarget != null)
                {
                    context.AddDropTarget(uiElement, dropTarget);
                }

            }
        }

        #endregion

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

            var context = GetDragDropContext(uiElement);

            if (context != null)
            {
                if (e.OldValue != null)
                {
                    context.RemoveDragSource(uiElement);
                }
                if (e.NewValue != null)
                {
                    context.AddDragSource(uiElement, (IDragSource)e.NewValue);
                }
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
            var context = GetDragDropContext(uiElement);

            if (context != null)
            {
                if (e.OldValue != null)
                {
                    context.RemoveDropTarget(uiElement);
                }
                if (e.NewValue != null)
                {
                    context.AddDropTarget(uiElement, (IDropTarget)e.NewValue);
                }
            }
        }

        #endregion


    }
}
