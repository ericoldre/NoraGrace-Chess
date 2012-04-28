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
    public class PositionToCoord : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Size size = new Size();
            Point point = new Point();

            var position = (ChessPosition)values[0];
            var canvasSize = new Size((double)values[1], (double)values[2]);

            size.Width = canvasSize.Width / 8;
            size.Height = canvasSize.Height / 8;

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
