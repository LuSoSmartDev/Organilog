using System;
using System.Globalization;

namespace Xamarin.Forms.Converters
{
    public class PriorityToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case 1:
                    return Color.FromHex("#5cb85c");

                case 2:
                    return Color.Default;

                case 3:
                    return Color.FromHex("#337ab7");

                case 4:
                    return Color.FromHex("#f0ad4e");

                case 5:
                    return Color.FromHex("#d9534f");

                default:
                    return Color.Default;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return default;
        }
    }
}