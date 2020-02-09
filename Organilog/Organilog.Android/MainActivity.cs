using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Microsoft.AppCenter.Push;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

namespace Organilog.Droid
{
    [Activity(Label = "Organilog", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);


            Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this, savedInstanceState);

            Forms.Init(this, savedInstanceState);

            // UserDialogs
            Acr.UserDialogs.UserDialogs.Init(this);

            ZXing.Net.Mobile.Forms.Android.Platform.Init();
            ZXing.Mobile.MobileBarcodeScanner.Initialize(Application);

            FFImageLoading.Forms.Platform.CachedImageRenderer.Init(enableFastRenderer: true);

            Plugin.LocalNotification.Platform.Droid.LocalNotificationService.NotifyNotificationTapped(Intent);
            Plugin.LocalNotification.Platform.Droid.LocalNotificationService.NotificationIconId = Resource.Drawable.icon;
            
            //Plugin.LocalNotification.Platform.Droid.LocalNotificationService.NotificationIconId.

            // IO.Fabric.Sdk.Android.Fabric.With(this, new Com.Crashlytics.Android.Crashlytics());

            LoadApplication(new App());



            Xamarin.Forms.Application.Current.On<Xamarin.Forms.PlatformConfiguration.Android>().UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Resize);
        }

        protected override void OnNewIntent(Intent intent)
        {
            Plugin.LocalNotification.Platform.Droid.LocalNotificationService.NotifyNotificationTapped(intent);

            base.OnNewIntent(intent);

            Push.CheckLaunchedFromNotification(this, intent);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Plugin.Permissions.PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            ZXing.Net.Mobile.Android.PermissionsHandler.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == 10000)
            {
                /*
                if (data != null && data.getComponent() != null && !TextUtils.isEmpty(data.getComponent().flattenToShortString()))
                {
                    String appName = data.getComponent().flattenToShortString();
                    // Now you know the app being picked.
                    // data is a copy of your launchIntent with this important extra info added.

                    // Start the selected activity
                    startActivity(data);
                }*/
            }
        }
    }
}