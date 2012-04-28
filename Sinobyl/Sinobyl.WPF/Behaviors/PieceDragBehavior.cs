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
        private BoardPieceVM ViewModel;

        protected override void OnAttached()
        {
            base.OnAttached();

            ViewModel = (BoardPieceVM)AssociatedObject.DataContext;

            AssociatedObject.MouseLeftButtonDown += (s, e) =>
            {
                _mouseDown = true;
                System.Diagnostics.Debug.WriteLine(string.Format("Down: {0}", ViewModel.Position.ToString()));
                if (ViewModel.DragStartCommand.CanExecute(null))
                {
                    ViewModel.DragStartCommand.Execute(null);
                }
                AssociatedObject.CaptureMouse();
            };
            AssociatedObject.MouseMove += (s, e) =>
            {
                if (!_mouseDown) { return; }
                System.Diagnostics.Debug.WriteLine(string.Format("move: {0}", ViewModel.Position.ToString()));
                int i = 1;
            };
            AssociatedObject.MouseLeftButtonUp += (s, e) =>
            {
                _mouseDown = false;
                System.Diagnostics.Debug.WriteLine(string.Format("Up: {0}", ViewModel.Position.ToString()));
                AssociatedObject.ReleaseMouseCapture();
            };

        }
    }
}
