using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if (PORTABLE || NETSTANDARD1_6 || NETCOREAPP1_1 || __MOBILE__)
using Xamarin.Forms;
#endif

namespace MvvmGo.Converters
{
#if (PORTABLE || NETSTANDARD1_6 || NETCOREAPP1_1 || __MOBILE__)
    public class InverseBooleanValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                return !(bool)value;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
#endif
}
