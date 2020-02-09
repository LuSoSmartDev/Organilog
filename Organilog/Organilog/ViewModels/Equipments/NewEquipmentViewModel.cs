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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TinyMVVM;
using Xamarin.Forms;
using Xamarin.Forms.Extensions;

namespace Organilog.ViewModels.Equipments
{
    public class NewEquipmentViewModel : BaseViewModel
    {
        private int mode; // 0: Add 1: Edit
        public int Mode { get => mode; set => SetProperty(ref mode, value); }

        private Equipment equipment;
        public Equipment Equipment { get => equipment; set => SetProperty(ref equipment, value); }

        private DateTime buyDate = DateTime.Now;
        public DateTime BuyDate { get => buyDate; set => SetProperty(ref buyDate, value); }

        private DateTime installDate = DateTime.Now;
        public DateTime InstallDate { get => installDate; set => SetProperty(ref installDate, value); }

        private DateTime guaranteeStartDate = DateTime.Now;
        public DateTime GuaranteeStartDate { get => guaranteeStartDate; set => SetProperty(ref guaranteeStartDate, value); }

        private DateTime guaranteeEndDate = DateTime.Now;
        public DateTime GuaranteeEndDate { get => guaranteeEndDate; set => SetProperty(ref guaranteeEndDate, value); }

        private List<MediaLink> NewMedias = new List<MediaLink>();
        private List<MediaLink> DeletedMedias = new List<MediaLink>();


        public ICommand OnClientFocusedCommand { get; private set; }
        public ICommand OnAddresseFocusedCommand { get; private set; }
        public ICommand BuyDateUnFocusedCommand { get; private set; }
        public ICommand InstallDateUnFocusedCommand { get; private set; }

        public ICommand GuaranteeStartDateUnFocusedCommand { get; private set; }
        public ICommand GuaranteeEndDateUnFocusedCommand { get; private set; }

        public ICommand AddProductCommand { get; private set; }
        public ICommand DeleteProductCommand { get; private set; }
        #region media 
        public ICommand TakePhotoCommand { get; private set; }
        public ICommand PickPhotoCommand { get; private set; }
        public ICommand SignatureCommand { get; private set; }
        public ICommand ViewMediaCommand { get; private set; }
        public ICommand DeleteMediaCommand { get; private set; }
        #endregion 

        public ICommand SaveEquipmentCommand { get; private set; }

        public NewEquipmentViewModel()
        {
            OnClientFocusedCommand = new AwaitCommand(OnClientFocused);
            OnAddresseFocusedCommand = new AwaitCommand(OnAddresseFocused);
            BuyDateUnFocusedCommand = new AwaitCommand(BuyDateUnFocused);
            InstallDateUnFocusedCommand = new AwaitCommand(InstallDateUnFocused);

            GuaranteeStartDateUnFocusedCommand = new AwaitCommand(GuaranteeStartDateUnFocused);
            GuaranteeEndDateUnFocusedCommand = new AwaitCommand(GuaranteeEndDateUnFocused);


            TakePhotoCommand = new AwaitCommand(TakePhoto);
            PickPhotoCommand = new AwaitCommand(PickPhoto);
            SignatureCommand = new AwaitCommand(Signature);
            ViewMediaCommand = new AwaitCommand<MediaLink>(ViewMedia);
            DeleteMediaCommand = new AwaitCommand<MediaLink>(DeleteMedia);


            SaveEquipmentCommand = new AwaitCommand(OnSaveEquipment);
        }

      

        public override void InitAsync(NavigationParameters parameters)
        {
            base.InitAsync(parameters);

            Mode = parameters?.GetValue<int>(ContentKey.EQUIPMENT_MODE) ?? 0;

            if (Mode == 1)
            {
                Equipment = parameters?.GetValue<Equipment>(ContentKey.SELECTED_EQUIPMENT)?.DeepCopy();
                if (Equipment.MediaLinks == null)
                    Equipment.MediaLinks = new ObservableCollection<MediaLink>();
                Equipment.OnPropertyChanged(nameof(Equipment.MediaLinks));

                Title = string.Format("{0}-{1}", "Q", (Equipment?.CodeId ?? (Equipment?.ServerId > 0 ? Equipment?.ServerId.ToString() : null)) ?? TranslateExtension.GetValue("page_title_edit_Equipment"));
            }
            else
            {
               

                Equipment = new Equipment()
                {
                    Id = Guid.NewGuid(),
                    UserId = CurrentUser.Id,
                    AccountId = CurrentUser.FkAccountId,
                    //IsInvoice = invoiceType == InvoiceType.Equipment ? 0 : 1,
                    //IsDraft = 1,
                    IsActif = 1,
             
                    MediaLinks = new ObservableCollection<MediaLink>(),
                    IsToSync = true
                };
                
                Title = TranslateExtension.GetValue("page_title_new_equipment");

            }

            RegisterMessagingCenter<ClientLookUpViewModel, Client>(this, MessageKey.CLIENT_SELECTED, OnClientSelected);
            RegisterMessagingCenter<AddressLookUpViewModel, Address>(this, MessageKey.ADDRESS_SELECTED, OnAddressSelected);
           
            RegisterMessagingCenter<MediaDetailViewModel, MediaLink>(this, MessageKey.MEDIA_SAVED, OnMediaSaved);
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
                Type = "Equipment",
                IsToSync = true
            };

            var mediaLink = new MediaLink()
            {
                Id = Guid.NewGuid(),
                UserId = CurrentUser.Id,
                FkColumnAppliId = Equipment.Id,  
                FkColumnServerId = Equipment.ServerId,
                FkMediaAppliId = media.Id,
                FkMediaServerId = media.ServerId,
                LinkTable = "Equipment",
                IsActif = 1,
                AddDate = DateTime.Now,
                EditDate = DateTime.Now,
                Media = media,
                IsToSync = true
            };
           
            Equipment.MediaLinks.Add(mediaLink);
            NewMedias.Add(mediaLink);

            Equipment.OnPropertyChanged(nameof(Equipment.MediaLinks));
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

                    Equipment.MediaLinks.Remove(mediaLink);
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
                
                if (Equipment.MediaLinks.FirstOrDefault(m => m.Id == mediaLink.Id) is MediaLink mediaLnk)
                {
                    Equipment.MediaLinks[Equipment.MediaLinks.IndexOf(mediaLnk)] = mediaLink;
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
                Equipment.MediaLinks.Remove(mediaLink);
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
                    Equipment.MediaLinks.Remove(mediaLink);
                    DeletedMedias.Add(mediaLink);
                }
            }

            Equipment.OnPropertyChanged(nameof(Equipment.MediaLinks));

            tcs.TrySetResult(true);
            
        }


        private async void OnClientFocused(object sender, TaskCompletionSource<bool> tcs)
        {
            await CoreMethods.PushViewModel<ClientLookUpViewModel>(modal: true);

            tcs.TrySetResult(true);
        }

        private void OnClientSelected(object sender, Client client)
        {
            Equipment.ClientAppliId = client.Id;
            Equipment.ClientServerId = client.ServerId;
            Equipment.Client = client;

            if (App.LocalDb.Table<Address>().ToList().FindAll(a => ((a.FkClientServerId > 0 && a.FkClientServerId == Equipment.Client.ServerId) || (!a.FkClientAppliId.Equals(Guid.Empty) && a.Id.Equals(Equipment.Client.Id)))) is List<Address> addresses && addresses.Count > 0)
            {
                OnAddressSelected(null, addresses.First());
            }
            else
            {
                Equipment.Address = null;
            }

            Equipment.OnPropertyChanged(nameof(Equipment.Client));
        }

        private async void OnAddresseFocused(object sender, TaskCompletionSource<bool> tcs)
        {
            var @params = new NavigationParameters
            {
                { ContentKey.SELECTED_CUSTOMER, Equipment.Client }
            };

            await CoreMethods.PushViewModel<AddressLookUpViewModel>(@params, modal: true);

            tcs.TrySetResult(true);
        }

        private void OnAddressSelected(object sender, Address address)
        {
            Equipment.AdresseAppliId = address.Id;
            Equipment.AdresseServerId = address.ServerId;
            Equipment.Address = address;

            Equipment.OnPropertyChanged(nameof(Equipment.Address));
        }

        private void BuyDateUnFocused(object value, TaskCompletionSource<bool> tcs)
        {
            Equipment.DateBuy = BuyDate;
            Equipment.OnPropertyChanged(nameof(Equipment.DateBuy));
            tcs.TrySetResult(true);
        }

        private void InstallDateUnFocused(object value, TaskCompletionSource<bool> tcs)
        {
            Equipment.DateInstall = InstallDate;

            Equipment.OnPropertyChanged(nameof(Equipment.DateInstall));

            tcs.TrySetResult(true);
        }

        private void GuaranteeEndDateUnFocused(object arg1, TaskCompletionSource<bool> tcs)
        {
            Equipment.DateGuaranteeEnd = GuaranteeEndDate;
            Equipment.OnPropertyChanged(nameof(Equipment.DateGuaranteeEnd));
            tcs.TrySetResult(true);
        }

        private void GuaranteeStartDateUnFocused(object arg1, TaskCompletionSource<bool> tcs)
        {
            Equipment.DateGuaranteeStart = GuaranteeStartDate;
            Equipment.OnPropertyChanged(nameof(Equipment.DateGuaranteeStart));
            tcs.TrySetResult(true);
        }



        private async void OnSaveEquipment(object sender, TaskCompletionSource<bool> tcs)
        {
            try
            {
                if (Mode == 0)
                {
                    if (Equipment.ClientAppliId.Equals(Guid.Empty) && Equipment.ClientServerId == 0)
                    {
                        await CoreMethods.DisplayAlert(TranslateExtension.GetValue("alert_title_equipment"), TranslateExtension.GetValue("alert_message_intervention_no_client"), TranslateExtension.GetValue("ok"));

                        tcs.TrySetResult(true);
                        return;
                    }

                    if (Equipment.AdresseAppliId.Equals(Guid.Empty) && Equipment.AdresseServerId == 0)
                    {
                        await CoreMethods.DisplayAlert(TranslateExtension.GetValue("alert_title_equipment"), TranslateExtension.GetValue("alert_message_intervention_no_address"), TranslateExtension.GetValue("ok"));

                        tcs.TrySetResult(true);
                        return;
                    }
                }

                App.LocalDb.BeginTransaction();

                if (Mode == 0)
                {
                    Equipment.EditDate = DateTime.Now;
                    Equipment.IsToSync = true;
                    App.LocalDb.InsertOrReplace(Equipment);
                }
                else
                {
                    Equipment.EditDate = DateTime.Now;
                    Equipment.IsToSync = true;
                    App.LocalDb.Update(Equipment);
                }

               
                
                foreach (var medl in Equipment.MediaLinks)
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

             
                MessagingCenter.Send(this, MessageKey.EQUIPMENT_CHANGED);

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
