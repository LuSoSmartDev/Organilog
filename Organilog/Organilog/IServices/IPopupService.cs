using Xamarin.Forms;

namespace Organilog.IServices
{
    public interface IPopupService
    {
        void ShowContent(View content, bool mathParent = true);

        void HideContent();
    }
}