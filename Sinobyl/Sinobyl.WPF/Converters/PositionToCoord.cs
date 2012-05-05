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

        protected static ChessPosition PointToPosition(Size boardSize, Point point)
        {
            if (point.X > boardSize.Width || point.X < 0) { return ChessPosition.OUTOFBOUNDS; }
            if (point.Y > boardSize.Height || point.Y < 0) { return ChessPosition.OUTOFBOUNDS; }

            var sqSize = BoardSizeToSquareSize(boardSize);

            double X = point.X;
            ChessFile f = ChessFile.FileA;
            while (X > sqSize.Width)
            {
                X -= sqSize.Width;
                f += 1;
            }
            double Y = point.Y;
            ChessRank r = ChessRank.Rank8;
            while (Y > sqSize.Height)
            {
                Y -= sqSize.Height;
                r += 1;
            }
            return f.ToPosition(r);
        }
        protected static Point PositionToPoint(Size boardSize, ChessPosition pos)
        {
            var size = BoardSizeToSquareSize(boardSize);
            return new Point(
                size.Width * Math.Abs((ChessFile.FileA - pos.GetFile())),
                size.Height * Math.Abs((ChessRank.Rank8 - pos.GetRank()))
                );
        }
        protected static Size BoardSizeToSquareSize(Size boardSize)
        {
            return new Size(boardSize.Width / 8, boardSize.Height / 8);
        }

    }

}
