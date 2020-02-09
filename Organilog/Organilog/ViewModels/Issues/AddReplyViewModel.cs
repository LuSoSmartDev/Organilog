using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Organilog.Constants;
using TinyMVVM;
using Xamarin.Forms;

namespace Organilog.ViewModels.Issues
{
    public class AddReplyViewModel : BaseViewModel
    {
        public ICommand CancelCommand { get; set; }
        public ICommand AddCommand { get; set; }

        public string textinput { get; set; }
        public AddReplyViewModel()
        {
            CancelCommand = new AwaitCommand(CancelHandle);
            AddCommand = new AwaitCommand(AddHandle);
        }

        private async void AddHandle(object obj, TaskCompletionSource<bool> tcs)
        {
            MessagingCenter.Send(this, MessageKey.REPLY_CHANGED, textinput);
           //MessagingCenter.Send(this, MessageKey.REPLY_CHANGED);

            await CoreMethods.PopViewModel(modal: true);

            tcs.TrySetResult(true);

        }

        private async void CancelHandle(object obj, TaskCompletionSource<bool> tcs)
        {
            await CoreMethods.PopViewModel(modal: IsModal);

            tcs.TrySetResult(true);
        }
    }
}
