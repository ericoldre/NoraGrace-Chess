using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.WPF.Converters
{
    public class PercentConverter: System.Windows.Data.IValueConverter
    {

        public PercentConverter()
        {

        }
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double pct = double.Parse(parameter.ToString());
            double input = double.Parse(value.ToString());
            double retval =  input * (pct / 100);
            return retval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return 0;
        }

        #endregion
    }
}
