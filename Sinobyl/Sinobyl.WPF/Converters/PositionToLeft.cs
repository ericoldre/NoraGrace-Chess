using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Globalization;
using Sinobyl.Engine;

namespace Sinobyl.WPF.Converters
{
    public class PositionToLeftMulti : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Size size = new Size();
            Point point = new Point();

            var position = (ChessPosition)values[0];
            var layoutRoot = values[1] as System.Windows.FrameworkElement;
            var boardvm = layoutRoot.DataContext;

            size.Width = layoutRoot.ActualWidth / 8;
            size.Height = layoutRoot.ActualHeight / 8;

            point.Y = size.Height * Math.Abs((ChessRank.Rank8 - position.GetRank()));
            point.X = size.Width * Math.Abs((ChessFile.FileA - position.GetFile()));

            switch (parameter.ToString())
            {
                case "Top":
                    return point.Y;
                case "Left":
                    return point.X;
                case "Width":
                    return size.Width;
                case "Height":
                    return size.Height;
                default:
                    throw new ArgumentException("invalid convert parameter");
            }
            
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }

}
