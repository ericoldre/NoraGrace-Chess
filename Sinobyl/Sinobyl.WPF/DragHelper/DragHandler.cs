using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Sinobyl.WPF.DragHelper
{
    public class DragHandler
    {
        private readonly UIElement _element;
        private readonly IDragSource _source;

        private bool _canDrag = false;
        private Point? _startDragPoint;
        private Vector _startDragRelToCenter;

        public DragHandler(UIElement element, IDragSource source)
        {
            _element = element;
            _source = source;

            CanDrag = source.CanDrag;
            source.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(source_PropertyChanged);
        }

        public UIElement Element
        {
            get { return _element; }
        }
        public IDragSource Source
        {
            get { return _source; }
        }
        public DragDropContext Context
        {
            get { return _source.DragDropContext; }
        }

        void source_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CanDrag":
                    this.CanDrag = _source.CanDrag;
                    break;
            }
        }

        #region properties

        private Point? StartDragPoint
        {
            get
            {
                return _startDragPoint;
            }
            set
            {
                if (_startDragPoint == value)
                {
                    return;
                }
                if (!_startDragPoint.HasValue && value.HasValue)
                {
                    //from null to value
                    _element.MouseMove += element_PreviewMouseMove;
                    _element.MouseUp += element_PreviewMouseUp;
                }
                else if (_startDragPoint.HasValue && !value.HasValue)
                {
                    //from having value to null
                    _element.MouseMove -= element_PreviewMouseMove;
                    _element.MouseUp -= element_PreviewMouseUp;
                }
                _startDragPoint = value;
            }
        }

        private bool CanDrag
        {
            get { return _canDrag; }
            set
            {
                if (_canDrag == value) { return; }

                _canDrag = value;

                if (StartDragPoint.HasValue)
                {
                    //shouldn't change this property during a drag.
                    throw new Exception("Should not change CanDrag during a drag operation");
                }

                if (_canDrag)
                {
                    //add handlers
                    _element.MouseDown += element_PreviewMouseDown;
                }
                else
                {
                    //remove handlers.
                    _element.MouseDown -= element_PreviewMouseDown;
                }

            }
        }

        #endregion

        void element_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            StartDragPoint = e.GetPosition(null);

            if (this.Element is FrameworkElement)
            {
                FrameworkElement frame = this.Element as FrameworkElement;
                _startDragRelToCenter = e.GetPosition(this.Element) - new Point(frame.ActualWidth / 2, frame.ActualHeight / 2);
                //System.Diagnostics.Debug.WriteLine(string.Format("start: {0} {1}", _startDragRelToCenter.X, _startDragRelToCenter.Y));
            }
            else
            {
                _startDragRelToCenter = e.GetPosition(this.Element) - new Point(0, 0);
            }

            this.Context.DoDrag(this.Source);

            _element.CaptureMouse();
        }

        void element_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!StartDragPoint.HasValue) { return; }
            var currentPoint = e.GetPosition(null);
            var delta = currentPoint - StartDragPoint.Value;
            
            DragDropProperties.SetDragOffset(this.Element, delta);

            this.Context.DoDragMove(this.Source, e, _startDragRelToCenter);


        }

        void element_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _element.ReleaseMouseCapture();
            this.Context.EndDrag(this.Source);

            //Vector v = DragDropProperties.GetDragOffset(this.Element);
            
            DragDropProperties.SetDragOffset(this.Element, new Vector(0, 0));
            this.StartDragPoint = null;
        }


    }
}
