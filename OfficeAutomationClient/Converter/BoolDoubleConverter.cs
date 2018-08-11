using System;
using System.Globalization;
using System.Windows.Data;

namespace OfficeAutomationClient.Converter
{
    [ValueConversion(typeof(bool), typeof(double))]
    public class BoolDoubleConverter : IValueConverter
    {
        /// <summary>
        /// if true return maxValue
        /// else return minValue
        /// if parameter is null, return 100
        /// </summary>
        /// <param name="value">true/false</param>
        /// <param name="targetType"></param>
        /// <param name="parameter">minValue/maxValue</param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string[] arr;
            if (null == parameter || (arr = parameter.ToString().Split('/')).Length != 2 ||
                !double.TryParse(arr[0], out double minVal) || !double.TryParse(arr[1], out double maxVal))
                return 100;
            else if (null != value && value.ToString().ToLower().Equals("true"))
                return maxVal;
            else
                return minVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
