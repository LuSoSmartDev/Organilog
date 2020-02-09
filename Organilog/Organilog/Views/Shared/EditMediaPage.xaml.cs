using System;
using System.Collections.Generic;
using System.IO;
using Organilog.Constants;
using Organilog.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Organilog.Views.Shared
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditMediaPage : ContentPage
    {

        public event EventHandler<Stream> ContentSaved;

        public EditMediaPage(MediaLink mediaLink)
        {
            InitializeComponent();
            editorImage.Source = mediaLink.Media.ImageDisplay;
           
        }

       public  async void Handle_ImageSaving(object sender, Syncfusion.SfImageEditor.XForms.ImageSavingEventArgs args)
        {
            args.Cancel = true;
            var data = args.Stream;
            ContentSaved?.Invoke(this, data);
            MessagingCenter.Send(this, MessageKey.MEDIA_EDIT_SAVED, data);
            await Navigation.PopModalAsync();
        }

        void Handle_ImageSaved(object sender, Syncfusion.SfImageEditor.XForms.ImageSavedEventArgs args)
        {
            var data = args.Location;
        }

        private async void CancelButton_Clicked(object sender, System.EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}
