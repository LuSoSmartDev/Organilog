using Foundation;
using UIKit;
using Xamarin.Forms;

namespace Organilog.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            Forms.Init();

            // Init Plugin
            ZXing.Net.Mobile.Forms.iOS.Platform.Init();

            FFImageLoading.Forms.Platform.CachedImageRenderer.Init();
            /*
            if #available(iOS 13.0, *) {
                #if swift(>=5.1)
                if let statusBarManager = UIApplication.shared.keyWindow?.windowScene?.statusBarManager,
                    let localStatusBar = statusBarManager.perform(Selector(("createLocalStatusBar")))?.takeRetainedValue()
                        as? UIView,
                    let statusBar = localStatusBar.perform(Selector(("statusBar")))?.takeRetainedValue() as? UIView,
                    let _statusBar = statusBar.value(forKey: "_statusBar") as? UIView {
                    print(localStatusBar, statusBar, _statusBar)
                }
                #endif
            } else {
                // Fallback on earlier versions
                if let statusBarWindow = UIApplication.shared.value(forKey: "statusBarWindow") as? UIWindow {
                    statusBarWindow.alpha = 1 - statusBarWindow.alpha
                }
            }*/
            if (!UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
            {
                // Status Bar Style
                UIView statusBar = UIApplication.SharedApplication.ValueForKey(new NSString("statusBarWindow")).ValueForKey(new NSString("statusBar")) as UIView;
                statusBar.BackgroundColor = UIColor.FromRGB((float)Color.DodgerBlue.R, (float)Color.DodgerBlue.G, (float)Color.DodgerBlue.B);
                app.StatusBarStyle = UIStatusBarStyle.LightContent;
            }
            else
            {
                UIView statusBar = new UIView(UIApplication.SharedApplication.StatusBarFrame);
                statusBar.BackgroundColor = UIColor.FromRGB((float)Color.DodgerBlue.R, (float)Color.DodgerBlue.G, (float)Color.DodgerBlue.B);
                app.StatusBarStyle = UIStatusBarStyle.LightContent;
                //app.KeyWindow.AddSubview(statusBar);

            }
            UINavigationBar.Appearance.BarTintColor = UIColor.FromRGB((float)Color.DodgerBlue.R, (float)Color.DodgerBlue.G, (float)Color.DodgerBlue.B);
            UINavigationBar.Appearance.TintColor = UIColor.White;
            UIBarButtonItem.Appearance.TintColor = UIColor.White;

            Plugin.LocalNotification.Platform.iOS.LocalNotificationService.Init();

            Syncfusion.SfImageEditor.XForms.iOS.SfImageEditorRenderer.Init();

            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }

        public override void ReceivedLocalNotification(UIApplication application, UILocalNotification notification)
        {
            if (!UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                //Change UIApplicationState to suit different situations
                if (UIApplication.SharedApplication.ApplicationState != UIApplicationState.Active)
                {
                    Plugin.LocalNotification.Platform.iOS.LocalNotificationService.NotifyNotificationTapped(notification);
                }
                else
                {
                    base.ReceivedLocalNotification(application, notification);
                }
            }
            else
            {
                base.ReceivedLocalNotification(application, notification);
            }
        }
    }
}
