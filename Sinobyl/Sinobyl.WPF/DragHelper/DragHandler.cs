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
        private readonly DragDropContext _context;

        private bool _canDrag = false;
        private Point? _startDragPoint;

        public DragHandler(UIElement element, IDragSource source, DragDropContext context)
        {
            _element = element;
            _source = source;
            _context = context;

            CanDrag = source.CanDrag;
            source.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(source_PropertyChanged);
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
                    _element.PreviewMouseDown += element_PreviewMouseDown;
                    _element.PreviewMouseMove += element_PreviewMouseMove;
                    _element.PreviewMouseUp += element_PreviewMouseUp;
                }
                else
                {
                    //remove handlers.
                    _element.PreviewMouseDown -= element_PreviewMouseDown;
                    _element.PreviewMouseMove -= element_PreviewMouseMove;
                    _element.PreviewMouseUp -= element_PreviewMouseUp;
                }

            }
        }

        #endregion

        void element_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            StartDragPoint = e.GetPosition(_element);
            _element.CaptureMouse();
        }

        void element_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {

        }

        void element_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _element.ReleaseMouseCapture();
        }


    }
}
