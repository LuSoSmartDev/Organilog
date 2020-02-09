using Organilog.Constants;
using Organilog.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using TinyMVVM;

using Xamarin.Forms;
using Xamarin.Forms.Extensions;

namespace Organilog.ViewModels.Shared
{
    public class MediaDetailViewModel : TinyViewModel
    {
        private MediaLink mediaLink;
        public MediaLink MediaLink { get => mediaLink; set => SetProperty(ref mediaLink, value); }

        public ICommand CancelCommand { get; private set; }
        public ICommand DeleteMediaCommand { get; private set; }
        public ICommand SaveMediaCommand { get; private set; }
        public ICommand ShareMediaCommand { get; private set; }
        public ICommand EditMediaCommand { get; private set; }

        public MediaDetailViewModel()
        {
            CancelCommand = new AwaitCommand(Cancel);
            DeleteMediaCommand = new AwaitCommand(DeleteMedia);
            SaveMediaCommand = new AwaitCommand(SaveMedia);
            ShareMediaCommand = new AwaitCommand(ShareMedia);
            EditMediaCommand = new AwaitCommand(EditMedia);
        }

     

        public override void InitAsync(NavigationParameters parameters)
        {
            base.InitAsync(parameters);

            MediaLink = parameters?.GetValue<MediaLink>(ContentKey.SELECTED_MEDIA)?.DeepCopy();

            Title = MediaLink.Media.FileName;
        }

        private async void EditMedia(object sender, TaskCompletionSource<bool> tcs)
        {
            var editMedia = new Views.Shared.EditMediaPage(mediaLink);
            editMedia.ContentSaved += editMedia_ContentSaved;

            await CoreMethods.PushPage(editMedia, modal: true);

            tcs.TrySetResult(true);
        }

       
        private void editMedia_ContentSaved(object sender, Stream e)
        {
            if (e != null)
            {
                byte[] bytes;
                using (var memoryStream = new MemoryStream())
                {
                    e.CopyTo(memoryStream);
                    bytes = memoryStream.ToArray();
                }
                mediaLink.Media.FileData = Convert.ToBase64String(bytes);
                mediaLink.Media.IsToSync = true;
                mediaLink.IsEdited = true;
                mediaLink.Media.EditDate = DateTime.Now;
                mediaLink.Media.PropertyChanged += Media_PropertyChanged;
            }

        }

        void Media_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

        }


        private async void DeleteMedia(object sender, TaskCompletionSource<bool> tcs)
        {
            if (await CoreMethods.DisplayAlert(TranslateExtension.GetValue("alert_title_media"), TranslateExtension.GetValue("alert_message_delete_media_confirm"), TranslateExtension.GetValue("alert_message_yes"), TranslateExtension.GetValue("alert_message_no")))
            {
                MediaLink.IsDelete = true;

                MessagingCenter.Send(this, MessageKey.MEDIA_SAVED, MediaLink);

                await CoreMethods.PopViewModel(modal: IsModal);
            };

            tcs.SetResult(true);
        }

        private async void SaveMedia(object sender, TaskCompletionSource<bool> tcs)
        {
            MessagingCenter.Send(this, MessageKey.MEDIA_SAVED, MediaLink);

            await CoreMethods.PopViewModel(modal: IsModal);

            tcs.SetResult(true);
        }

        private void ShareMedia(object sender, TaskCompletionSource<bool> tcs)
        {
            tcs.SetResult(true);
        }

        private async void Cancel(object sender, TaskCompletionSource<bool> tcs)
        {
            await CoreMethods.PopViewModel(modal: IsModal);

            tcs.TrySetResult(true);
        }
    }
}