using System;
using Xamarin.Forms;
using System.Collections;
using System.Linq;

namespace Happimeter.Converter
{
    public class AnyValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is ICollection valueAsCollection)
            {
                return valueAsCollection.Count != 0;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
