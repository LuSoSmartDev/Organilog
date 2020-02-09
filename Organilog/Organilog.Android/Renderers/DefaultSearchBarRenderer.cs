using Android.Content;
using Android.OS;
using Organilog.Droid.Renderers;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(SearchBar), typeof(DefaultSearchBarRenderer))]

namespace Organilog.Droid.Renderers
{
    public class DefaultSearchBarRenderer : SearchBarRenderer
    {
        public DefaultSearchBarRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<SearchBar> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement == null && Element != null)
            {
                // WorkAround to searchBar not appearing in newer android versions
                if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                    Element.HeightRequest = 40;
            }
        }
    }
}