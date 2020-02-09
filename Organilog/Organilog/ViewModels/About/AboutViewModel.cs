using TinyMVVM;
using Xamarin.Essentials;

namespace Organilog.ViewModels.About
{
    public class AboutViewModel : TinyViewModel
    {
        public string Version { get; set; }

        public override void InitAsync(NavigationParameters parameters)
        {
            base.InitAsync(parameters);
            Version = AppInfo.VersionString;

        }

    }
}