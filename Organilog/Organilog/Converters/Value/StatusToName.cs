using System;
using System.Globalization;
using Xamarin.Forms;

namespace Xamarin.Forms.Converters
{
    public class StatusToName : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case 1:
                    return "Nouveau";

                case 2:
                    return "En cours";

                case 3:
                    return "Résolu";
               case 4:
                    return "En attente de réponse";
                case 5:
                    return "Fermé";

                case 6:
                    return "Rejeté";

                default:
                    return "";
            }
        }
        //status <- it's an ID 1=new / 2=in progress / 3=waiting for answer / 4=resolved / 5=closed / 6=rejected
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case "Nouveau":
                    return 1;

                case "En cours":
                    return 2;

                case "En attente de réponse":
                    return 3;

                case "Résolu":
                    return 4;

                case "Fermé":
                    return 5;

                case "Rejeté":
                    return 6;

                default:
                    return 0;
            }
        }
    }
}
