using Organilog.Common;
using Organilog.Constants;
using Organilog.Helpers;
using Organilog.Models;
using Organilog.ViewModels.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TinyMVVM;

using Xamarin.Forms;
using Xamarin.Forms.Extensions;

namespace Organilog.ViewModels.Quotes
{
    public class NewQuoteViewModel : BaseViewModel
    {
        public int SelectedIndex { get; set; }
        public string ProTitle { get; set; }
        public string ProComment { get; set; }

        private List<InvoiceProduct> DeletedProducts = new List<InvoiceProduct>();

        private EditMode editMode;
        public EditMode EditMode { get => editMode; set => SetProperty(ref editMode, value); }

        private Invoice quote;
        public Invoice Quote { get => quote; set => SetProperty(ref quote, value); }

        private bool visibleComment ;
        public bool VisibleComment { get => visibleComment; set => SetProperty(ref visibleComment, value); }

        private bool visibleTitle;
        public bool VisibleTitle { get => visibleTitle; set => SetProperty(ref visibleTitle, value); }

        private DateTime iDate = DateTime.Now;
        public DateTime IDate { get => iDate; set => SetProperty(ref iDate, value); }

        private DateTime iDueDate = DateTime.Now;
        public DateTime IDueDate { get => iDueDate; set => SetProperty(ref iDueDate, value); }

        private List<MediaLink> NewMedias = new List<MediaLink>();
        private List<MediaLink> DeletedMedias = new List<MediaLink>();

        private List<Status> listStatus = new List<Status>();
        public List<Status> ListStatus { get => listStatus; set => SetProperty(ref listStatus, value); }

        public class Status
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public Status(int id, string name)
            {
                Id = id;
                Name = name;
            }

        }

        public ICommand OnClientFocusedCommand { get; private set; }
        public ICommand OnAddresseFocusedCommand { get; private set; }
        public ICommand IDateUnFocusedCommand { get; private set; }
        public ICommand IDueDateUnFocusedCommand { get; private set; }
        public ICommand AddProductCommand { get; private set; }

        public ICommand AddTitleCommand { get; private set; }
        public ICommand AddCommentCommand { get; private set; }

        public ICommand DeleteProductCommand { get; private set; }
        #region media 
        public ICommand TakePhotoCommand { get; private set; }
        public ICommand PickPhotoCommand { get; private set; }
        public ICommand SignatureCommand { get; private set; }
        public ICommand ViewMediaCommand { get; private set; }
        public ICommand DeleteMediaCommand { get; private set; }
        #endregion 

        public ICommand SaveQuoteCommand { get; private set; }

        public NewQuoteViewModel()
        {
            OnClientFocusedCommand = new AwaitCommand(OnClientFocused);
            OnAddresseFocusedCommand = new AwaitCommand(OnAddresseFocused);
            IDateUnFocusedCommand = new AwaitCommand(IDateUnFocused);
            IDueDateUnFocusedCommand = new AwaitCommand(IDueDateUnFocused);
            AddProductCommand = new AwaitCommand(AddProduct);
            AddTitleCommand = new AwaitCommand(AddTitle);
            AddCommentCommand = new AwaitCommand(AddComment);

            DeleteProductCommand = new AwaitCommand<InvoiceProduct>(DeleteProduct);

            TakePhotoCommand = new AwaitCommand(TakePhoto);
            PickPhotoCommand = new AwaitCommand(PickPhoto);
            SignatureCommand = new AwaitCommand(Signature);
            ViewMediaCommand = new AwaitCommand<MediaLink>(ViewMedia);
            DeleteMediaCommand = new AwaitCommand<MediaLink>(DeleteMedia);


            SaveQuoteCommand = new AwaitCommand(OnSaveQuote);
        }

       

        public override void InitAsync(NavigationParameters parameters)
        {
            base.InitAsync(parameters);
            
            EditMode = parameters?.GetValue<EditMode>(ContentKey.QUOTE_MODE) ?? EditMode.New;

            if (EditMode == EditMode.Modify)
            {
                Quote = parameters?.GetValue<Invoice>(ContentKey.SELECTED_QUOTE)?.DeepCopy();
                if (Quote.MediaLinks == null)
                    Quote.MediaLinks = new ObservableCollection<MediaLink>();
                //Quote.OnPropertyChanged(nameof(Quote.MediaLinks));

                Title = string.Format("{0}-{1}", "Q", (Quote?.InvoiceNumber ?? (Quote?.ServerId > 0 ? Quote?.ServerId.ToString() : null)) ?? TranslateExtension.GetValue("page_title_edit_quote"));
            }
            else
            {
                var invoiceType = parameters?.GetValue<InvoiceType>(ContentKey.INVOICE_TYPE) ?? InvoiceType.Quote;

                Quote = new Invoice()
                {
                    Id = Guid.NewGuid(),
                    UserId = CurrentUser.Id,
                    AccountId = CurrentUser.FkAccountId,
                    IsInvoice = invoiceType == InvoiceType.Quote ? 0 : 1,
                    IsDraft = 1,
                    IsActif = 1,
                    LinkInvoiceProducts = new ObservableCollection<InvoiceProduct>(),
                    MediaLinks = new ObservableCollection<MediaLink>(),
                    IsToSync = true
                };
                if(invoiceType == InvoiceType.Quote)
                    Title = TranslateExtension.GetValue("page_title_new_quote");
                else
                    Title = TranslateExtension.GetValue("page_title_new_invoice");
                
            }

            initStatus();

            RegisterMessagingCenter<ClientLookUpViewModel, Client>(this, MessageKey.CLIENT_SELECTED, OnClientSelected);
            RegisterMessagingCenter<AddressLookUpViewModel, Address>(this, MessageKey.ADDRESS_SELECTED, OnAddressSelected);
            RegisterMessagingCenter<AddProductViewModel, InvoiceProduct>(this, MessageKey.PRODUCT_SELECTED, OnProductSelected);
            RegisterMessagingCenter<MediaDetailViewModel, MediaLink>(this, MessageKey.MEDIA_SAVED, OnMediaSaved);
        }
        private void initStatus()
        {
            listStatus = new List<Status>();
            
            if (Quote.IsInvoice == 0)
            {
                listStatus.Add(new Status(0, "En cours"));
                listStatus.Add(new Status(1, "Rejeté"));
                listStatus.Add(new Status(2, "Accepté"));
                SelectedIndex = (Quote.Status.Equals("En cours") ? 0 : (Quote.Status.Equals("Rejeté") ? 1 : (Quote.Status.Equals("Accepté") ? 2 : -1)));
                
            }
            else
            {
                listStatus.Add(new Status(0, "En cours"));
                listStatus.Add(new Status(1, "Brouillon"));
                listStatus.Add(new Status(2, "Payé"));

                SelectedIndex = (Quote.Status.Equals("En cours") ? 0 : (Quote.Status.Equals("Brouillon") ? 1 : (Quote.Status.Equals("Payé") ? 2 : -1)));
            }
            
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
                AddDate = DateTime.Now,
                EditDate = DateTime.Now,
                Type = "invoice",
                IsToSync = true
            };

            var mediaLink = new MediaLink()
            {
                Id = Guid.NewGuid(),
                UserId = CurrentUser.Id,
                FkColumnAppliId = Quote.Id, //invoice 
                FkColumnServerId = Quote.ServerId,//invoice 
                FkMediaAppliId = media.Id,
                FkMediaServerId = media.ServerId,
                LinkTable = "Invoice",
                IsActif = 1,
                AddDate = DateTime.Now,
                EditDate = DateTime.Now,
                Media = media,
                IsToSync = true
            };
            //TODO add media to Invoice 
            Quote.MediaLinks.Add(mediaLink);
            NewMedias.Add(mediaLink);

            Quote.OnPropertyChanged(nameof(Quote.MediaLinks));
        }

        private async void ViewMedia(MediaLink mediaLink, TaskCompletionSource<bool> tcs)
        {
            try
            {
                var parameters = new NavigationParameters()
                {
                    { ContentKey.SELECTED_MEDIA, mediaLink}
                };

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

        private void DeleteMedia(MediaLink mediaLink, TaskCompletionSource<bool> tcs)
        {
            if (NewMedias.Contains(mediaLink))
            {
                Quote.MediaLinks.Remove(mediaLink);
                NewMedias.Remove(mediaLink);
            }
            else
            {
                if (mediaLink.ServerId > 0)
                {
                    if (mediaLink.Media != null && mediaLink.Media.ServerId > 0)
                    {
                        mediaLink.Media.IsActif = 0;
                        mediaLink.Media.IsToSync = true;
                    }

                    mediaLink.IsActif = 0;
                    mediaLink.IsToSync = true;
                }
                else
                {
                    Quote.MediaLinks.Remove(mediaLink);
                    DeletedMedias.Add(mediaLink);
                }
            }

            Quote.OnPropertyChanged(nameof(Quote.MediaLinks));

            tcs.TrySetResult(true);
        }


        private async void OnClientFocused(object sender, TaskCompletionSource<bool> tcs)
        {
            await CoreMethods.PushViewModel<ClientLookUpViewModel>(modal: true);

            tcs.TrySetResult(true);
        }

        private void OnClientSelected(object sender, Client client)
        {
            Quote.FkClientAppId = client.Id;
            Quote.FkClientServerId = client.ServerId;
            Quote.Client = client;

            if (App.LocalDb.Table<Address>().ToList().FindAll(a => ((a.FkClientServerId > 0 && a.FkClientServerId == Quote.Client.ServerId) || (!a.FkClientAppliId.Equals(Guid.Empty) && a.Id.Equals(Quote.Client.Id)))) is List<Address> addresses && addresses.Count > 0)
            {
                OnAddressSelected(null, addresses.First());
            }
            else
            {
                Quote.Address = null;
            }

            Quote.OnPropertyChanged();
        }

        private async void OnAddresseFocused(object sender, TaskCompletionSource<bool> tcs)
        {
            var @params = new NavigationParameters
            {
                { ContentKey.SELECTED_CUSTOMER, Quote.Client }
            };

            await CoreMethods.PushViewModel<AddressLookUpViewModel>(@params, modal: true);

            tcs.TrySetResult(true);
        }

        private void OnAddressSelected(object sender, Address address)
        {
            Quote.FkAddressAppId = address.Id;
            Quote.FkAddressServerId = address.ServerId;
            Quote.Address = address;

            Quote.OnPropertyChanged();
        }

        private void IDateUnFocused(object value, TaskCompletionSource<bool> tcs)
        {
            Quote.IDate = IDate;

            tcs.TrySetResult(true);
        }

        private void IDueDateUnFocused(object value, TaskCompletionSource<bool> tcs)
        {
            Quote.IDueDate = IDueDate;

            tcs.TrySetResult(true);
        }
        private void AddComment(object arg1, TaskCompletionSource<bool> tcs)
        {
            VisibleComment = !VisibleComment;

            tcs.TrySetResult(true);
        }
        private async void AddTitle(object sender, TaskCompletionSource<bool> tcs)
        {

            VisibleTitle = !VisibleTitle;
            tcs.TrySetResult(true);
        }
        private async void AddProduct(object sender, TaskCompletionSource<bool> tcs)
        {
            await CoreMethods.PushViewModel<AddProductViewModel>(modal: true);

            tcs.TrySetResult(true);
        }

        private void OnProductSelected(object sender, InvoiceProduct product)
        {
            product.FKInvoiceAppId = Quote.Id;
            product.FkInvoiceServerId = Quote.ServerId;
            product.Position = Quote.LinkInvoiceProducts?.Count ?? 0;
            product.AddDate = DateTime.Now;
            product.EditDate = DateTime.Now;

            Quote.LinkInvoiceProducts.Add(product.DeepCopy());

            Quote.CachePtHt = Quote.LinkInvoiceProducts?.Sum(ip => ip.TotalPrice);
            Quote.CachePtTax = Quote.LinkInvoiceProducts?.Sum(ip => ip.AmountOfTax);
            Quote.CachePtTtcToPay = Quote.LinkInvoiceProducts?.Sum(ip => ip.TotalPriceWithTax) ?? 0 - Quote.AmountPaid ?? 0;
        }

        private void DeleteProduct(InvoiceProduct value, TaskCompletionSource<bool> tcs)
        {
            if (EditMode == EditMode.New)
            {
                Quote.LinkInvoiceProducts.Remove(value);
            }
            else
            {
                if (value.ServerId > 0)
                {
                    Public.SetProperty(value, nameof(value.IsActif), 0);
                    value.IsToSync = true;
                }
                else
                {
                    Quote.LinkInvoiceProducts.Remove(value);
                    DeletedProducts.Add(value);
                }
            }
            Quote.CachePtHt = Quote.LinkInvoiceProducts?.Sum(ip => ip.TotalPrice);
            Quote.CachePtTax = Quote.LinkInvoiceProducts?.Sum(ip => ip.AmountOfTax);
            Quote.CachePtTtcToPay = Quote.LinkInvoiceProducts?.Sum(ip => ip.TotalPriceWithTax) ?? 0 - Quote.AmountPaid ?? 0;

            tcs.TrySetResult(true);
        }

        private async void OnSaveQuote(object sender, TaskCompletionSource<bool> tcs)
        {
            try
            {
                if (EditMode == EditMode.New)
                {
                    if (Quote.FkClientAppId.Equals(Guid.Empty) && Quote.FkClientServerId == 0)
                    {
                        await CoreMethods.DisplayAlert(TranslateExtension.GetValue("alert_title_quote"), TranslateExtension.GetValue("alert_message_intervention_no_client"), TranslateExtension.GetValue("ok"));

                        tcs.TrySetResult(true);
                        return;
                    }

                    if (Quote.FkAddressAppId.Equals(Guid.Empty) && Quote.FkAddressServerId == 0)
                    {
                        await CoreMethods.DisplayAlert(TranslateExtension.GetValue("alert_title_quote"), TranslateExtension.GetValue("alert_message_intervention_no_address"), TranslateExtension.GetValue("ok"));

                        tcs.TrySetResult(true);
                        return;
                    }
                }

                App.LocalDb.BeginTransaction();

                if (Quote.IsInvoice == 0) //QUOTE 
                {
                    if (SelectedIndex == 0)
                    {
                        Quote.IsDraft = 1;
                        if (Quote.IsPaid !=1 && Quote.IsPaid!=0)
                            Quote.IsPaid = 0;
                    }
                    else if (SelectedIndex == 1)
                    {
                        Quote.IsDraft = Quote.IsPaid = 0;
                    }
                    else if(SelectedIndex==2)
                    {
                        Quote.IsDraft = 0;
                        Quote.IsPaid = 1;
                    }
                }
                else//invoice 
                {
                    if (SelectedIndex == 0)
                    {
                        Quote.IsDraft = Quote.IsPaid = 0;

                    }
                    else if (SelectedIndex == 1)
                    {
                        Quote.IsDraft = 1;
                        if (Quote.IsPaid != 1 && Quote.IsPaid != 0)
                            Quote.IsPaid = 0;
                    }
                    else
                    {
                        Quote.IsDraft = 0;
                        Quote.IsPaid = 1;
                    }
                }
                if (EditMode == EditMode.New)
                {
                    Quote.AddDate = DateTime.Now;
                    Quote.EditDate = DateTime.Now;
                    Quote.IsToSync = true;
                    App.LocalDb.InsertOrReplace(Quote);
                    if (Quote.LinkInvoiceProducts == null)
                        Quote.LinkInvoiceProducts = new ObservableCollection<InvoiceProduct>();
                    if (!string.IsNullOrEmpty(ProTitle))
                    {
                        Quote.LinkInvoiceProducts.Add(new InvoiceProduct
                        {
                            Id = new Guid(),
                            IsTitle = 1, 
                            Label = ProTitle
                        }) ;

                    }
                    if (!string.IsNullOrEmpty(ProComment))
                    {
                        Quote.LinkInvoiceProducts.Add(new InvoiceProduct
                        {
                            Id = new Guid(),
                            IsTitle = 1,
                            Label = ProComment

                        });
                    }
                   
                }
                else
                {
                    Quote.EditDate = DateTime.Now;
                    Quote.IsToSync = true;
                    App.LocalDb.Update(Quote);
                }
                
                foreach (var ip in Quote.LinkInvoiceProducts)
                {
                    App.LocalDb.InsertOrReplace(ip);
                }

                foreach (var ip in DeletedProducts)
                {
                    App.LocalDb.Delete(ip);
                }

                foreach (var medl in Quote.MediaLinks)
                {
                    if (medl.Media != null)
                    {
                        App.LocalDb.InsertOrReplace(medl.Media);
                    }
                    App.LocalDb.InsertOrReplace(medl);
                }

                foreach (var medl in DeletedMedias)
                {
                    if (medl.Media != null)
                    {
                        App.LocalDb.Delete(medl.Media);
                    }
                    App.LocalDb.Delete(medl);
                }

                App.LocalDb.Commit();

                MessagingCenter.Send(this, MessageKey.INVOICE_CHANGED);
                MessagingCenter.Send(this, MessageKey.QUOTE_CHANGED);

                await CoreMethods.PopViewModel();
            }
            catch (Exception ex)
            {
                App.LocalDb.Rollback();
                await CoreMethods.DisplayAlert(TranslateExtension.GetValue("error"), ex.Message, TranslateExtension.GetValue("ok"));
            }
            finally
            {
                tcs.TrySetResult(true);
            }
        }
    }
}