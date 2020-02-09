using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Organilog.Droid.Services;
using Organilog.IServices;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(AndroidDevice))]
namespace Organilog.Droid.Services
{
    class AndroidDevice: IDevice
    {
        [Obsolete]
        public string GetIdentifier()
        {

            return Android.Provider.Settings.Secure.GetString(Android.App.Application.Context.ContentResolver, Android.Provider.Settings.Secure.AndroidId);

            //return "acf019Of9000faaf01032101affd";
        }
    }
}