using System;
using System.Globalization;
using Android.Content;
using Android.Content.PM;
using Organilog.IServices;
using Xamarin.Forms;

[assembly: Dependency(typeof(Organilog.Droid.Services.OpenMapIntentService))]
namespace Organilog.Droid.Services
{

    public class OpenMapIntentService: IOpenMapIntentService 
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        [Obsolete]
        public void Open( double lat, double lon)
        {
            try {
                //var geoUri = Android.Net.Uri.Parse(String.Format(CultureInfo.InvariantCulture, "http://maps.google.com/maps?daddr={0},{1}", lat, lon));
                ////var geoUri = Android.Net.Uri.Parse("google.streetview:cbll=21.027763,105.834160");
                //var mapIntent = new Intent(Intent.ActionView, geoUri);
                //mapIntent.AddFlags(ActivityFlags.NewTask);

                ////StartActivity(mapIntent);
                //Forms.Context.StartActivity(mapIntent);
               // var wazeURL = $"https://waze.com/ul?ll={lat},{lon}&navigate=yes";
                string wazeURL = string.Format("https://waze.com/ul?ll={0},{1}&navigate=yes", lat.ToString("0.0000000").Replace(",", "."), lon.ToString("0.0000000").Replace(",", "."));
                var test = string.Format("geo:{0},{1}",  lat.ToString("0.0000000").Replace(",",".") ,lon.ToString("0.0000000").Replace(",", "."));
                var geoUri = Android.Net.Uri.Parse(test);
                var mapIntent = new Intent(Intent.ActionView, geoUri);
                mapIntent.AddFlags(ActivityFlags.NewTask);
                Intent it = Intent.CreateChooser(mapIntent, "Select Map Application");
                //Intent intentChecking = new Intent(Intent.ActionView, Android.Net.Uri.Parse(wazeURL));
                //var resolveInfo = PackageManager.ResolveActivity(intentChecking, 0);
                
                //it.SetData(Android.Net.Uri.Parse(wazeURL));

                Forms.Context.StartActivity(it);
                
                /*
                const string wazeAppURL = "waze://";
                //var wazeURL = $"https://waze.com/ul?ll={loc[0]},{loc[1]}&navigate=yes";
                string wazeURL = string.Format("https://waze.com/ul?ll={0},{1}&navigate=yes",lat,lon);

                var intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(wazeAppURL));
                var resolveInfo = ResolveActivity(intent, 0);
                var wazeUri = resolveInfo != null ? Android.Net.Uri.Parse(wazeURL) : Android.Net.Uri.Parse("market://details?id=com.waze");
                intent.SetData(wazeUri);
                Forms.Context.StartActivity(intent);
                */
            }
            catch(Exception )
            {

            }
        }
    }
}
