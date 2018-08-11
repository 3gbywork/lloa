using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace OfficeAutomationClient.Converter
{
    /// <summary>
    /// value   result
    /// --------------------------
    /// true    visible
    /// null    hidden
    /// false   collapsed
    /// </summary>
    [ValueConversion(typeof(Nullable<bool>), typeof(Visibility))]
    public class NullableBoolVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (null == value)
                return Visibility.Hidden;
            else if (value.ToString().ToLower().Equals("true"))
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
