using System;
using System.Globalization;
using Xamarin.Forms.Extensions;

namespace Xamarin.Forms.Converters
{
    public class PriorityToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case 1:
                    return  TranslateExtension.GetValue("LOW");

                case 2:
                    return TranslateExtension.GetValue("NORMAL");

                case 3:
                    return TranslateExtension.GetValue("HIGH");

                case 4:
                    return TranslateExtension.GetValue("URGENT");

                case 5:
                    return TranslateExtension.GetValue("IMMEDIATE");

                default:
                    return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case "LOW":
                    return 1;

                case "NORMAL":
                    return 2;

                case "HIGH":
                    return 3;

                case "URGENT":
                    return 4;

                case "IMMEDIATE":
                    return 5;

                default:
                    return 0;
            }
        }
    }
}