using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace SensorMap.Converters
{
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
            => value is int n && n <= 0 ? Visibility.Visible : Visibility.Collapsed;

       

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Visibility.Visible;
        }
    }
}
