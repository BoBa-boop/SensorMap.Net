using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SensorMap.Converters
{
    public class ContainsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // values[0] - коллекция SensorTypes
            // values[1] - текущий элемент ListBoxItem

            if (values.Length < 2)
                return false;

            var collection = values[0] as IEnumerable;
            var item = values[1];

            if (collection == null || item == null)
                return false;

            // Проверяем наличие элемента в коллекции
            foreach (var collectionItem in collection)
            {
                if (Equals(collectionItem, item))
                    return true;
            }

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
