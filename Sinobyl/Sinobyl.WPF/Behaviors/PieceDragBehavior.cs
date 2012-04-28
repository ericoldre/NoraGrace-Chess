using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interactivity;
using Sinobyl.WPF.ViewModels;
using System.Windows.Controls;

namespace Sinobyl.WPF.Behaviors
{
    public class PieceDragBehavior : Behavior<FrameworkElement>
    {

        private bool _mouseDown = false;

        protected override void OnAttached()
        {
            base.OnAttached();
            var o = AssociatedObject;
            var d = o.DataContext;
            var t = AssociatedType;

            var canvas = AssociatedObject.Parent as Canvas;

            BoardPieceVM vm = (BoardPieceVM)AssociatedObject.DataContext;


            AssociatedObject.MouseLeftButtonDown += (s, e) =>
            {
                _mouseDown = true;
                vm.DragStartCommand.Execute(null);
                AssociatedObject.CaptureMouse();
            };
            AssociatedObject.MouseMove += (s, e) =>
            {
                if (!_mouseDown) { return; }
                int i = 1;
            };
            AssociatedObject.MouseLeftButtonUp += (s, e) =>
            {
                _mouseDown = false;
                AssociatedObject.ReleaseMouseCapture();
            };

        }
    }
}
