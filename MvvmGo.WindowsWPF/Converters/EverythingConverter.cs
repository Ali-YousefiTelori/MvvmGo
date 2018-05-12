using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace MvvmGo.Converters
{
    public class EverythingConverterValue
    {
        public object ConditionValue { get; set; }
        public object ResultValue { get; set; }
    }

    public class EverythingConverterList : List<EverythingConverterValue>
    {

    }

    public class EverythingConverter : IValueConverter
    {
        public EverythingConverterList Conditions { get; set; } = new EverythingConverterList();

        public object NullResultValue { get; set; }
        public object NullBackValue { get; set; }

        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (parameter != null)
            {
                if (parameter.ToString() == "1")
                {
                    return Conditions.Select(x => x.ResultValue).ToList();
                }
                else if (parameter.ToString() == "2")
                {
                    return Conditions.Select(x => x.ConditionValue).ToList();
                }
            }
            return Conditions.Where(x => x.ConditionValue.Equals(value)).Select(x => x.ResultValue).FirstOrDefault() ?? NullResultValue;
        }
        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            var find = Conditions.Where(x => x.ConditionValue.Equals(value)).Select(x => x.ConditionValue).FirstOrDefault() ?? Conditions.Where(x => x.ResultValue.Equals(value)).Select(x => x.ConditionValue).FirstOrDefault();
            return find ?? NullBackValue;
        }
    }
}
