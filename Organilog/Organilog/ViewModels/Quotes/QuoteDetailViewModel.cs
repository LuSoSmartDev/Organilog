using Organilog.Common;
using Organilog.Constants;
using Organilog.Helpers;
using Organilog.IServices;
using Organilog.Models;
using Organilog.ViewModels.Shared;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TinyMVVM;
using Xamarin.Forms;
using Xamarin.Forms.Extensions;

namespace Organilog.ViewModels.Quotes
{
    public class QuoteDetailViewModel : BaseViewModel
    {
        private readonly IInvoiceService invoiceService;

        private Guid QuoteId;

        private string invoiceID;
        public string InvoiceID { get => invoiceID; set => SetProperty(ref invoiceID, value); }

        private Invoice quote;
        public Invoice Quote { get => quote; set => SetProperty(ref quote, value); }

        private bool syncToWeb;
        public bool SyncToWeb { get => syncToWeb; set => SetProperty(ref syncToWeb, value); }

        public ICommand EditQuoteCommand { get; private set; }
        public ICommand DeleteQuoteCommand { get; private set; }
        public ICommand TakePhotoCommand { get; private set; }
        public ICommand PickPhotoCommand { get; private set; }
        public ICommand SignatureCommand { get; private set; }
        public ICommand DeleteMediaCommand { get; private set; }
        public ICommand ViewMediaCommand { get; private set; }
        public ICommand PaynowCommand { get; set; }


        public QuoteDetailViewModel(IInvoiceService invoiceService)
        {
            this.invoiceService = invoiceService;

            EditQuoteCommand = new AwaitCommand(EditQuote);
            DeleteQuoteCommand = new AwaitCommand(DeleteQuote);
            TakePhotoCommand = new AwaitCommand(TakePhoto);
            PickPhotoCommand = new AwaitCommand(PickPhoto);
            SignatureCommand = new AwaitCommand(Signature);
            ViewMediaCommand = new AwaitCommand<MediaLink>(ViewMedia);
            DeleteMediaCommand = new AwaitCommand<MediaLink>(DeleteMedia);
            PaynowCommand = new AwaitCommand(GotoWebpay);
        }

       

        public async override void InitAsync(NavigationParameters parameters)
        {
            base.InitAsync(parameters);

            QuoteId = parameters.GetValue<Invoice>(ContentKey.SELECTED_QUOTE)?.Id ?? Guid.Empty;

            Quote = await invoiceService.GetInvoiceDetail(QuoteId);

            Title = Quote.IsInvoice == 0 ? TranslateExtension.GetValue("page_title_quote_detail") : TranslateExtension.GetValue("page_title_invoice_detail");

            InvoiceID = string.Format("{0}: {1}", "ID", Quote?.InvoiceNumber ?? (Quote?.ServerId > 0 ? Quote?.ServerId.ToString() : "Vide"));

            SyncToWeb = !string.IsNullOrEmpty(Quote.Nonce);
            
            RegisterMessagingCenter<NewQuoteViewModel>(this, MessageKey.QUOTE_CHANGED, OnQuoteChanged);
            RegisterMessagingCenter<MediaDetailViewModel, MediaLink>(this, MessageKey.MEDIA_SAVED, OnMediaSaved);

            Quote.PropertyChanged += Quote_PropertyChanged;

        }

        private void Quote_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Quote.PropertyChanged -= Quote_PropertyChanged;

            if (e.PropertyName == nameof(Quote.MediaLinks))
            {
                Quote.IsToSync = true;
                MessagingCenter.Send(this, MessageKey.INVOICE_CHANGED);
            }

            Quote.PropertyChanged += Quote_PropertyChanged;
        }

        private void OnMediaSaved(object sender, MediaLink mediaLink)
        {
            try
            {
                if (mediaLink.IsDelete)
                {
                    if (mediaLink.ServerId > 0)
                    {
                        if (mediaLink.Media != null && mediaLink.Media.ServerId > 0)
                        {
                            mediaLink.Media.SetProperty(nameof(mediaLink.Media.IsActif), 0);
                            mediaLink.Media.IsToSync = true;

                            App.LocalDb.Update(mediaLink.Media);
                        }
                        else
                        {
                            App.LocalDb.Delete(mediaLink.Media);
                        }

                        mediaLink.SetProperty(nameof(mediaLink.IsActif), 0);
                        mediaLink.IsToSync = true;

                        App.LocalDb.Update(mediaLink);
                    }
                    else
                    {
                        App.LocalDb.Delete(mediaLink.Media);
                        App.LocalDb.Delete(mediaLink);
                    }

                    Quote.MediaLinks.Remove(mediaLink);
                }
                else
                {
                    mediaLink.Media.EditDate = DateTime.Now;
                    mediaLink.EditDate = DateTime.Now;

                    mediaLink.Media.IsToSync = true;
                    mediaLink.IsToSync = true;

                    App.LocalDb.Update(mediaLink.Media);
                    App.LocalDb.Update(mediaLink);
                }

                if (Quote.MediaLinks.FirstOrDefault(m => m.Id == mediaLink.Id) is MediaLink mediaLnk)
                {
                    Quote.MediaLinks[Quote.MediaLinks.IndexOf(mediaLnk)] = mediaLink;
                }
            }
            catch (Exception ex)
            {
                CoreMethods.DisplayAlert(TranslateExtension.GetValue("error"), ex.Message, TranslateExtension.GetValue("ok"));
            }
        }


        private async void OnQuoteChanged(object sender)
        {
            Quote = await invoiceService.GetInvoiceDetail(QuoteId);
            SyncToWeb = !string.IsNullOrEmpty(Quote.Nonce);
        }

        private async void EditQuote(object sender, TaskCompletionSource<bool> tcs)
        {
            try
            {
                var parameters = new NavigationParameters()
                {
                    { ContentKey.SELECTED_QUOTE,  Quote},
                    { ContentKey.QUOTE_MODE, EditMode.Modify}
                };

                await CoreMethods.PushViewModel<NewQuoteViewModel>(parameters);
            }
            catch (Exception ex)
            {
                await CoreMethods.DisplayAlert(TranslateExtension.GetValue("error"), ex.Message, TranslateExtension.GetValue("ok"));
            }
            finally
            {
                tcs.TrySetResult(true);
            }
        }

        private async void DeleteQuote(object sender, TaskCompletionSource<bool> tcs)
        {
            if (await CoreMethods.DisplayAlert(TranslateExtension.GetValue("alert_title_quote"), TranslateExtension.GetValue("alert_message_delete_quote_confirm"), TranslateExtension.GetValue("alert_message_yes"), TranslateExtension.GetValue("alert_message_no")))
            {
                Quote.IsActif = 0;
                Quote.IsToSync = true;

                App.LocalDb.Update(Quote);

                MessagingCenter.Send(this, MessageKey.QUOTE_CHANGED);

                await CoreMethods.PopViewModel();
            };

            tcs.TrySetResult(true);
        }

        private async void TakePhoto(object sender, TaskCompletionSource<bool> tcs)
        {
            var photo = await PhotoHelper.TakePhotoStreamAsync();

            if (photo != null)
                SetMedia(photo, false);

            tcs.TrySetResult(true);
        }

        private async void PickPhoto(object sender, TaskCompletionSource<bool> tcs)
        {
            var photo = await PhotoHelper.PickPhotoStreamAsync();

            if (photo != null)
                SetMedia(photo, false);

            tcs.TrySetResult(true);
        }

        private async void Signature(object sender, TaskCompletionSource<bool> tcs)
        {
            var signaturePad = new Views.Popups.SignaturePad();
            signaturePad.ContentSaved += SignaturePad_ContentSaved;

            await CoreMethods.PushPage(signaturePad, modal: true);

            tcs.TrySetResult(true);
        }

        private void GotoWebpay(object sender, TaskCompletionSource<bool> tcs)
        {
            //https://subdomain_account.organilog.com/page/invoice_view.php?id=123456&nonce=45a5024a0062b8082d4bb5e1da7ddf16
            var url = ApiURI.URL_BASE_ROOT(Settings.CurrentAccount) + string.Format("page/invoice_view.php?id={0}&nonce={1}",Quote.ServerId,Quote.Nonce);

            Device.OpenUri(new Uri(url));

            tcs.TrySetResult(true);
        }

        private void SignaturePad_ContentSaved(object sender, Stream content)
        {
            if (content != null)
                SetMedia(content, true);
        }

        private async void SetMedia(Stream content, bool isSignature)
        {
            string fileName;

            if (isSignature)
                fileName = "SIG_" + DateTime.Now.ToString("ddMMyyyyHHmmss");
            else
                fileName = "IMG_" + DateTime.Now.ToString("ddMMyyyyHHmmss");

            var media = new Media()
            {
                Id = Guid.NewGuid(),
                UserId = CurrentUser.Id,
                AccountId = CurrentUser.Id,
                FileName = fileName,
                Year = DateTime.Today.Year.ToString(),
                Month = DateTime.Today.Month.ToString(),
                FileData = await content.ToBase64(),
                IsActif = 1,
                Type = "invoice",
                AddDate = DateTime.Now,
                EditDate = DateTime.Now,
                IsToSync = true
            };

            var mediaLink = new MediaLink()
            {
                Id = Guid.NewGuid(),
                UserId = CurrentUser.Id,
                FkColumnAppliId = Quote.Id,
                FkColumnServerId = Quote.ServerId,
                FkMediaAppliId = media.Id,
                FkMediaServerId = media.ServerId,
                LinkTable = "intervention",
                IsActif = 1,
                AddDate = DateTime.Now,
                EditDate = DateTime.Now,
                Media = media,

                IsToSync = true
            };

            Quote.MediaLinks.Add(mediaLink);

            App.LocalDb.InsertOrReplace(media);
            App.LocalDb.InsertOrReplace(mediaLink);

            Quote.IsToSync = true;

            Quote.OnPropertyChanged("MediaLinks");
        }

        private async void ViewMedia(MediaLink mediaLink, TaskCompletionSource<bool> tcs)
        {
            try
            {
                var parameters = new NavigationParameters()
                {
                    { ContentKey.SELECTED_MEDIA, mediaLink}
                };
                if (mediaLink.Media.isPDF && Device.RuntimePlatform == Device.Android)
                    Device.OpenUri(new Uri(mediaLink.Media.ImageUri));
                else
                    await CoreMethods.PushViewModel<MediaDetailViewModel>(parameters, modal: true);
            }
            catch (Exception ex)
            {
                await CoreMethods.DisplayAlert(TranslateExtension.GetValue("error"), ex.Message, TranslateExtension.GetValue("ok"));
            }
            finally
            {
                tcs.TrySetResult(true);
            }
        }

        private void DeleteMedia(MediaLink mediaLink, TaskCompletionSource<bool> tcs)
        {
            if (mediaLink.ServerId > 0)
            {
                if (mediaLink.Media != null && mediaLink.Media.ServerId > 0)
                {
                    mediaLink.Media.IsActif = 0;
                    mediaLink.Media.IsToSync = true;

                    App.LocalDb.Update(mediaLink.Media);
                }
                else
                {
                    App.LocalDb.Delete(mediaLink.Media);
                }

                mediaLink.IsActif = 0;
                mediaLink.IsToSync = true;

                App.LocalDb.Update(mediaLink);
            }
            else
            {
                if (mediaLink.Media != null)
                    App.LocalDb.Delete(mediaLink.Media);
                App.LocalDb.Delete(mediaLink);
            }

            Quote.MediaLinks.Remove(mediaLink);

            Quote.OnPropertyChanged("MediaLinks");

            tcs.TrySetResult(true);
        }
    }
}