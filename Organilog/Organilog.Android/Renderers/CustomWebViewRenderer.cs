using System;
using System.Net;
using Organilog.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Controls;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CustomWebView), typeof(CustomWebViewRenderer))]
namespace Organilog.Droid.Renderers
{
#pragma warning disable CS0618 // Type or member is obsolete
    public class CustomWebViewRenderer : WebViewRenderer
        {
            protected override void OnElementChanged (ElementChangedEventArgs<WebView> e)
            {
                base.OnElementChanged (e);

                if (e.NewElement != null) {
                    var customWebView = Element as CustomWebView;
                    Control.Settings.AllowUniversalAccessFromFileURLs = true;
                    Control.Settings.JavaScriptEnabled = true;
                    Control.SetWebViewClient(new Android.Webkit.WebViewClient());
                    Control.LoadUrl(string.Format("https://docs.google.com/viewer?url={0}", WebUtility.UrlEncode(customWebView.Uri)));
            }
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete
}
